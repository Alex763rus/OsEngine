using Kraken.WebSockets;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Pricing;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Text;
using Position = OsEngine.Entity.Position;
using Side = OsEngine.Entity.Side;

namespace OsEngine.Robots.Squeezy.Tester
{
    public class EventServiceTester:EventService
    {
        private GeneralParametersTester generalParameters;
        private GroupParametersTesterService groupParametersService;
        private MovingAverageService movingAverageService;
        private DealService dealService;
        private CountBarService countBarService;
        private LogService logService;
        private PaintService paintService;
        private VolumeSumService volumeSumService;
        private StatisticService statisticService;


        private DirectionType directionTypeCurrent; //Направление текущего бара по МА. 
        private bool lockCurrentDirection;          //признак блокировки текущего направления. Не открывать больше сделок, дождаться завершения текущих.
        private decimal priceForPaintGroup;         //Цена чтобы рисовать тренд. Заполняется единажды.
        private decimal priceForPaintSqueezy;       //Цена чтобы рисовать сквизы. Заполняется единажды
        DateTime timeStartPaintGroup;               //Время начала тренда
        private bool isStart;                       //Признак начала работы
        public EventServiceTester(BotTabSimple tab, GeneralParametersTester generalParameters, GroupParametersTesterService groupParametersService,  LogService logService, StatisticService statisticService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;
            this.logService = logService;
            this.statisticService = statisticService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);
            countBarService = new CountBarService();
            paintService = new PaintService(tab);
            lockCurrentDirection = false;
            isStart = true;
        }

        public void candleFinishedEventLogic(List<Candle> candles)
        {
            //Для тестовой среды: Если мало баров или нет медленной, ничего не делаем:
            if (candles.Count <= 2 || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }
            if (isStart)
            {
                volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);
                movingAverageService.updateMaLen();
                logBotSettings();
                paintService.deleteAllChartElement();
                priceForPaintGroup = MathService.getValueAddPercent(candles[candles.Count - 1].Low, generalParameters.getPaintGroup());
                timeStartPaintGroup = candles[candles.Count - 1].TimeStart;
                priceForPaintSqueezy = MathService.getValueAddPercent(candles[candles.Count - 1].Low, generalParameters.getPaintSqueezy());
                isStart = false;
            }

            SqueezyType squeezyType = SqueezyType.None;
            DirectionType directionTypeTmp = getDirectionType();
            if(generalParameters.getPaintGroupEnabled() && directionTypeCurrent != directionTypeTmp)
            {
                paintService.paintGroup(timeStartPaintGroup, candles[candles.Count - 1].TimeStart, priceForPaintGroup, directionTypeCurrent);
                timeStartPaintGroup = candles[candles.Count - 1].TimeStart;
            }
            if (!lockCurrentDirection && directionTypeCurrent != directionTypeTmp
                && (dealService.hasOpendeal(Side.Buy) || dealService.hasOpendeal(Side.Sell)))
            {
                lockCurrentDirection = true;
                logService.sendLogSystem("Заблокировали группу:" + directionTypeCurrent + " т.к. пришел новый бар с группой:" + directionTypeTmp + " и есть незавершенные сделки");
            }

            directionTypeCurrent = directionTypeTmp;

            decimal candleLow1 = candles[candles.Count - 1].Low;
            decimal candleHigh1 = candles[candles.Count - 1].High;
            decimal candleClose2 = candles[candles.Count - 2].Close;

            dealService.checkSlTpAndClose(candleLow1);
            dealService.checkSlTpAndClose(candleHigh1);

            GroupType groupTypeSell = getGroupType(Side.Sell);
            GroupType groupTypeBuy = getGroupType(Side.Buy);
            statisticService.recalcMaxDrawdown(groupTypeBuy, groupTypeSell, dealService);

            //Sell:
            GroupParametersTester groupParameters;
            groupParameters = groupParametersService.getGroupParameters(groupTypeSell);
            if (groupParameters.getGroupOn())
            {
                if (dealService.hasOpendeal(Side.Sell))
                {
                    countBarService.addCounterBarSell();
                    if (countBarService.getCounterBarSell() > countBarService.getLimitBarSell())
                    {
                        dealService.closeAllDeals(Side.Sell, "Закрылись по барам");
                    }
                } else if (candleHigh1 > MathService.getValueAddPercent(candleClose2, groupParameters.getTriggerCandleDiff()))
                {
                    if (lockCurrentDirection)
                    {
                        squeezyType = SqueezyType.SellMissed;
                    } else {
                        string message = "Обнаружен сквиз " + groupParameters.getGroupType().ToString() + ": предпоследний бар:" + LogService.getCandleInfo(candles[candles.Count - 2])
                                            + " последний бар:" + LogService.getCandleInfo(candles[candles.Count - 1])
                                            + " отношение:" + Math.Round((candleHigh1 - candleClose2) / candleClose2 * 100, 2) + "%"
                                            + " настройки:" + groupParameters.getTriggerCandleDiff() + "%";
                        logService.sendLogSystem(message);
                        dealService.openDeal(Side.Sell, groupParameters.getGroupType().ToString(), "Вход по рынку", volumeSumService.getVolumeSum(Side.Sell));
                        countBarService.setLimitBarSell(groupParameters.getCountBarForClose());
                        squeezyType = SqueezyType.Sell;
                        statisticService.recalcSqueezyCounter(groupTypeSell, Side.Sell, candleClose2, candleHigh1);
                    }
                }
            }

            //Buy:
            groupParameters = groupParametersService.getGroupParameters(groupTypeBuy);
            if (groupParameters.getGroupOn())
            {
                if (dealService.hasOpendeal(Side.Buy))
                {
                    countBarService.addCounterBarBuy();
                    if (countBarService.getCounterBarBuy() > countBarService.getLimitBarBuy())
                    {
                        dealService.closeAllDeals(Side.Buy, "Закрылись по барам");
                    }
                }
                else if (candleLow1 < MathService.getValueSubtractPercent(candleClose2, groupParameters.getTriggerCandleDiff()))
                {
                    if (lockCurrentDirection)
                    {
                        squeezyType = SqueezyType.BuyMissed;
                    }
                    else
                    {
                        string message = "Обнаружен сквиз " + groupParameters.getGroupType().ToString() + ": предпоследний бар:" + LogService.getCandleInfo(candles[candles.Count - 2])
                                            + " последний бар:" + LogService.getCandleInfo(candles[candles.Count - 1])
                                            + " отношение:" + Math.Round((candleClose2 - candleLow1) / candleLow1 * 100, 2) + "%"
                                            + " настройки:" + groupParameters.getTriggerCandleDiff() + "%";
                        logService.sendLogSystem(message);
                        dealService.openDeal(Side.Buy, groupParameters.getGroupType().ToString(), "Вход по рынку", volumeSumService.getVolumeSum(Side.Buy));
                        countBarService.setLimitBarBuy(groupParameters.getCountBarForClose());
                        squeezyType = SqueezyType.Buy;
                        statisticService.recalcSqueezyCounter(groupTypeBuy, Side.Buy, candleClose2, candleLow1);
                    }
                }
            }
            if (generalParameters.getPaintSqueezyEnabled() && squeezyType != SqueezyType.None)
            {
               paintService.paintSqueezy(candles[candles.Count - 1].TimeStart, dealService.getTimeFrame(), priceForPaintSqueezy, squeezyType);
            }
            printEndBarInfo();
        }

        private void printEndBarInfo()
        {
            int countPosition = 0;
            if (dealService.hasOpendeal(Side.Sell))
            {
                ++countPosition;
            }
            if (dealService.hasOpendeal(Side.Buy))
            {
                ++countPosition;
            }
            StringBuilder currentInfo = new StringBuilder();
            currentInfo.Append("******> Закрыт бар. Группа:").Append(directionTypeCurrent)
                .Append(" Позиций:").Append(countPosition);
            logService.sendLogSystem(currentInfo.ToString());
        }

        private GroupType getGroupType(Side side)
        {
            GroupType groupTypeWithSide = GroupType.TestTest;

            if (directionTypeCurrent == DirectionType.Flat && side == Side.Sell)
            {
                groupTypeWithSide = GroupType.FlatSell;
            }
            else if (directionTypeCurrent == DirectionType.Flat && side == Side.Buy)
            {
                groupTypeWithSide = GroupType.FlatBuy;
            }
            else if (directionTypeCurrent == DirectionType.Up && side == Side.Sell)
            {
                groupTypeWithSide = GroupType.UpSell;
            }
            else if (directionTypeCurrent == DirectionType.Up && side == Side.Buy)
            {
                groupTypeWithSide = GroupType.UpBuy;
            }
            else if (directionTypeCurrent == DirectionType.Down && side == Side.Sell)
            {
                groupTypeWithSide = GroupType.DownSell;
            }
            else if (directionTypeCurrent == DirectionType.Down && side == Side.Buy)
            {
                groupTypeWithSide = GroupType.DownBuy;
            }
            return groupTypeWithSide;
        }

        public void positionClosingSuccesEventLogic(Position position)
        {
            logService.sendLogSystem("Подтверждение: Успешно закрыта позиция:" + LogService.getPositionInfo(position));

            bool isProfit = position.ProfitPortfolioPunkt > 0;
            paintService.paintClosedPosition(position, isProfit);
            volumeSumService.updateLevel(position.Direction, isProfit);
            if (position.Direction == Side.Buy)
            {
                countBarService.resetCountBarBuy();
            }
            if (position.Direction == Side.Sell)
            {
                countBarService.resetCountBarSell();
            }
            if (lockCurrentDirection && !dealService.hasOpendeal(Side.Sell) && !dealService.hasOpendeal(Side.Buy))
            {
                logService.sendLogSystem("Сняли блокировку на открытие сделки по группе, т.к.закрылась последняя сделка:" + LogService.getPositionInfo(position));
                lockCurrentDirection = false;
            }
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
            GroupParametersTester groupParameters = groupParametersService.getGroupParameters(position.SignalTypeOpen);
            decimal sl = position.EntryPrice;
            decimal tp = position.EntryPrice;
            if (position.Direction == Side.Buy)
            {
                tp = tp + tp * groupParameters.getTakeProfit() / 100.0m;
                sl = sl - sl * groupParameters.getStopLoss() / 100.0m;
            }
            else if (position.Direction == Side.Sell)
            {
                tp = tp - tp * groupParameters.getTakeProfit() / 100.0m;
                sl = sl + sl * groupParameters.getStopLoss() / 100.0m;
            }
            dealService.setSlTp(position, sl, tp);
        }

        private DirectionType getDirectionType()
        {
            DirectionType directionType;
            if (generalParameters.getTestSettings() || movingAverageService.getMaLastValueSlow() == 0)
            {
                directionType = DirectionType.Test;
            }
            else if ((     movingAverageService.getMaLastValueSlow() < movingAverageService.getMaLastValueFast() 
                        && movingAverageService.getMaLastValueFast() < MathService.getValueAddPercent(movingAverageService.getMaLastValueSlow(), generalParameters.getMaStrength()))
                    || (   movingAverageService.getMaLastValueFast() < movingAverageService.getMaLastValueSlow() 
                        && movingAverageService.getMaLastValueSlow() < MathService.getValueAddPercent(movingAverageService.getMaLastValueFast(), generalParameters.getMaStrength())))
            {
                directionType = DirectionType.Flat;
            }
            else if (movingAverageService.getMaLastValueFast() > movingAverageService.getMaLastValueSlow())
            {
                directionType = DirectionType.Up;
            }
            else
            {
                directionType = DirectionType.Down;
            }
            return directionType;
        }

        private void logBotSettings()
        {
            logService.sendLogSystem("");
            logService.sendLogSystem("");
            logService.sendLogSystem(LogService.SEPARATE_PARAMETR_LINE);
            logService.sendLogSystem(LogService.SEPARATE_PARAMETR_LINE);
            logService.sendLogSystem(SqueezyTester.BOT_NAME + " init successful, started version bot:" + SqueezyTrading.VERSION);
            logService.sendLogSystem(generalParameters.getAllSettings());
            List<GroupParametersTester> listParameters = groupParametersService.getGroupsParameters();
            foreach (var groupParameters in listParameters)
            {
                logService.sendLogSystem(groupParameters.getAllGroupParameters());
            }

        }

        public void parametrsChangeByUserLogic()
        {
            movingAverageService.updateMaLen();
            paintService.deleteAllChartElement();
            volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);
            statisticService.setIsEnabled(generalParameters.getStatisticEnabled());
            logService.setup(generalParameters.getLogEnabled(), generalParameters.getCountBufferLogLine());
        }
    }
}
