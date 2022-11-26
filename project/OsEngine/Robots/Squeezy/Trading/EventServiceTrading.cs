using Kraken.WebSockets;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Pricing;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.Market.Servers.GateIo.Futures.Response;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static protobuf.ws.TradesRequest;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Position = OsEngine.Entity.Position;
using Side = OsEngine.Entity.Side;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class EventServiceTrading
    {

        private GeneralParametersTrading generalParameters;
        private GroupParametersTradingService groupParametersService;

        private MovingAverageService movingAverageService;
        private DealService dealService;
        private PaintService paintService;
        private LogService logService;
        private VolumeSumService volumeSumService;

        private Candle lastCandle;
        //private GroupParametersTrading lastCandleGroupParameters; //группа для текущей свечи
        private decimal candleTriggerStartBid; //триггер того что есть сквиз. Изменяется с завершением бара
        private decimal candleTriggerStartAsc; //триггер того что есть сквиз. Изменяется с завершением бара
        //private decimal priceOpenLimitSell;    //цена открытия sell лимитки. Изменяется с завершением бара
        //private decimal priceOpenLimitBuy;    //цена открытия buy лимитки. Изменяется с завершением бара
        private decimal lastBestAsc;          //последняя известная лучшая цена продажи
        private decimal lastBestBid;          //последняя известная лучшая цена покупки

        private DealSupport dealSupportBuy;     //сопровождение Buy сделок
        private DealSupport dealSupportSell;    //сопровождение Sell сделок



        //private GroupType groupTypeSmallCurrent; //Текущая группа
        private DirectionType directionTypeCurrent; //Направление текущего бара по МА. 
        private bool lockCurrentDirection; //признак блокировки текущего направления. Не открывать больше сделок, дождаться завершения текущих.

        private decimal priceForPaintGroup;         //Цена чтобы рисовать тренд. Заполняется единажды
        private DateTime timeStartGroup;            //Время начала действия группы

        public EventServiceTrading(BotTabSimple tab, GeneralParametersTrading generalParameters, GroupParametersTradingService groupParametersService, LogService logService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;
            this.logService = logService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);
            volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);

            paintService = new PaintService(tab);
            dealSupportBuy = new DealSupport(Side.Buy);
            dealSupportSell = new DealSupport(Side.Sell);

            directionTypeCurrent = DirectionType.None;
            lockCurrentDirection = false;
        }

        public void candleFinishedEventLogic(List<Candle> candles)
        {
            //Если мало баров или нет медленной, ничего не делаем:
            if (candles.Count < 2 || movingAverageService.getMaLastValueSlow() == 0)
            {
                if (candles.Count == 1)
                {
                    logBotSettings();
                    paintService.deleteAllChartElement();
                    dealSupportBuy.reset();
                    dealSupportSell.reset();
                    priceForPaintGroup = getValueSubtractPercent(candles[candles.Count - 1].Low, generalParameters.getPaintGroup());
                    timeStartGroup = candles[candles.Count - 1].TimeStart;
                }
                return;
            }

            lastCandle = candles[candles.Count - 1];
            candleTriggerStartBid = getValueSubtractPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());
            candleTriggerStartAsc = getValueAddPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());

            barCounterProcess(dealSupportBuy, dealSupportSell);
            barCounterProcess(dealSupportSell, dealSupportBuy);

            DirectionType directionTypeTmp = getDirectionType();

            if (directionTypeCurrent != directionTypeTmp)
            {
                //paintService.paintGroup(timeStartGroup, candles[candles.Count - 1].TimeStart, priceForPaintGroup, directionTypeTmp);
                timeStartGroup = candles[candles.Count - 1].TimeStart;
            }

            if (!lockCurrentDirection && directionTypeCurrent != directionTypeTmp
                && (   dealSupportSell.getProcessState() == ProcessState.WAIT_TP_SL 
                    || dealSupportBuy.getProcessState()  == ProcessState.WAIT_TP_SL))
            {
                lockCurrentDirection = true;
                sendLogSystemLocal("Заблокировали направление:" + directionTypeCurrent + " т.к. пришел новый бар с направлением:" + directionTypeTmp + " и есть незавершенные сделки");
            }
            directionTypeCurrent = directionTypeTmp;
            printEndBarInfo();
        }

        private void printEndBarInfo()
        {
            int countPosition = 0;
            if(dealSupportBuy.getPosition() != null)
            {
                ++countPosition;
            }
            if (dealSupportSell.getPosition() != null)
            {
                ++countPosition;
            }
            StringBuilder currentInfo = new StringBuilder();
            currentInfo.Append("******> Закрыт бар. Группа:").Append(directionTypeCurrent)
                .Append(" Позиций:").Append(countPosition)
                .Append(" Buy:").Append(dealSupportBuy.getProcessState())
                .Append(" Position:#0000").Append(dealSupportBuy.getPositionNumber())
                .Append(" Sell:").Append(dealSupportSell.getProcessState())
                .Append(" Position:#0000").Append(dealSupportSell.getPositionNumber())
                ;
            sendLogSystemLocal(currentInfo.ToString());
        }
        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            if(lastCandle == null || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }

            lastBestBid = bestBid;
            lastBestAsc = bestAsk;

            if (lockCurrentDirection)
            {
                return;
            }
            waitTriggerStartLogic(bestBid, dealSupportBuy, dealSupportSell, candleTriggerStartBid);
            waitTriggerStartLogic(bestAsk, dealSupportSell, dealSupportBuy, candleTriggerStartAsc);
        }
        private void waitTriggerStartLogic(decimal price, DealSupport dealSupport, DealSupport dealSupportAnother, decimal tgStart)
        {
            ProcessState processState = dealSupport.getProcessState();
            if (processState == ProcessState.WAIT_TP_SL || processState == ProcessState.OK_TRIGGER_START)
            {
                return;
            }
            Side side = dealSupport.getSide();
            Position position;
            if (  (side == Side.Sell && price > tgStart) //продать по цене дороже
                ||(side == Side.Buy && price < tgStart)  //купить по цене дешевле
                ) 
            {
                //Есть сквиз
                ProcessState processStateAnother = dealSupportAnother.getProcessState();
                Position positionAnother = dealSupportAnother.getPosition();
                string message = side + " Обнаружен сквиз. предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + tgStart;
                sendLogSystemLocal(message, null, dealSupport);
                if (processStateAnother == ProcessState.OK_TRIGGER_START)
                {
                    sendLogSystemLocal(side + " нашли неоткрытую зарегистрированную лимитку в противоположную сторону, будем ее закрывать" + logService.getPositionInfo(positionAnother), null, dealSupport);
                    dealService.closeAllOrderToPosition(positionAnother, "Новая лимитка");
                    resetSide(dealSupportAnother, dealSupport);
                }
                //Если нет активностей в противоположную сторону активности в противоположной стороне в той же группе
                //if (processStateAnother == ProcessState.FREE)
                //{
                    GroupType groupTypeCurrent = getGroupType(side);
                    GroupParametersTrading groupParameters = groupParametersService.getGroupParameters(groupTypeCurrent);

                    decimal priceOpenLimit = 0 ;
                    if(side == Side.Sell)
                    {
                        priceOpenLimit = getValueAddPercent(lastCandle.Close, groupParameters.getTriggerCandleDiff());
                    } else if(side == Side.Buy)
                    {
                        priceOpenLimit = getValueSubtractPercent(lastCandle.Close, groupParameters.getTriggerCandleDiff());
                    }
                    //Если уже пробили отслеживаемые величины, то открываемся по рынку
                    if (  (side == Side.Sell && price > priceOpenLimit)
                        ||(side == Side.Buy && price < priceOpenLimit)
                        )
                    {
                        position = dealService.openDeal(side, groupParameters.getGroupType().ToString(), "Вход по рынку", volumeSumService.getVolumeSum(side));
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START : выставили заявку по рынку:", position, dealSupport);
                            dealSupport.dealSupportUpdate(groupParameters, ProcessState.OK_TRIGGER_START, position);
                            dealSupport.addChartElement(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), tgStart, position.EntryPrice, groupTypeCurrent.ToString()));
                        }
                    }
                    else
                    {
                        position = dealService.openLimit(side, priceOpenLimit, groupTypeCurrent.ToString(), side + "AtLimit", volumeSumService.getVolumeSum(side));
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START выставили заявку на открытие лимитки:", position, dealSupport);
                            dealSupport.dealSupportUpdate(groupParameters, ProcessState.OK_TRIGGER_START, position);
                            dealSupport.addChartElement(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), tgStart, priceOpenLimit, groupTypeCurrent.ToString()));
                        }
                    }
                //}
            }
            
        }


        public void positionOpeningSuccesEventLogic(Position position)
        {
            decimal sl = position.EntryPrice;
            decimal tp = position.EntryPrice;
            if (position.Direction == Side.Buy)
            {
                tp = getValueAddPercent(tp, dealSupportBuy.getGroupParametersTrading().getTakeProfit());
                sl = getValueSubtractPercent(sl, dealSupportBuy.getGroupParametersTrading().getStopLoss());
                dealSupportBuy.setProcessState(ProcessState.WAIT_TP_SL);
                dealSupportBuy.addChartElement(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportBuy.getGroupType()));
            }
            else if (position.Direction == Side.Sell)
            {
                tp = getValueSubtractPercent(tp, dealSupportSell.getGroupParametersTrading().getTakeProfit());
                sl = getValueAddPercent(sl, dealSupportSell.getGroupParametersTrading().getStopLoss());
                dealSupportSell.setProcessState(ProcessState.WAIT_TP_SL);
                dealSupportSell.addChartElement(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportSell.getGroupType()));
            }
            position.ProfitOrderRedLine = tp;
            position.StopOrderRedLine = sl;
            dealService.setTpSl(position, tp, sl, 0);
            sendLogSystemLocal("-> WAIT_TP_SL Успешно открыта позиция:" + position.Comment, position, dealSupportSell);
            sendLogSystemLocal("Установлен TP =" + tp + ", SL =" + sl + " для позиции:", position, dealSupportSell);
        }

        public void positionClosingSuccesEventLogic(Position position)
        {
            bool isProfit = position.ProfitPortfolioPunkt > 0;
            volumeSumService.updateLevel(position.Direction, isProfit);
            paintService.paintClosedPosition(position, dealService.getTimeFrame(), isProfit);

            if (position.SignalTypeClose != null && position.SignalTypeClose.Equals("TpSl"))
            {
                if (isProfit)
                {
                    position.SignalTypeClose = "Закрылись по TP";
                }
                else
                {
                    position.SignalTypeClose = "Закрылись по SL";
                }
            }
            //Значит ждем эту позицию:
            if (dealSupportBuy.getPosition() != null && position.Number == dealSupportBuy.getPosition().Number)
            {
                sendLogSystemLocal("Успешно закрыта позиция:" + position.SignalTypeClose, position, dealSupportBuy);
                resetSide(dealSupportBuy, dealSupportSell);
            }
            else if (dealSupportSell.getPosition() != null && position.Number == dealSupportSell.getPosition().Number)
            {
                sendLogSystemLocal("Успешно закрыта позиция:" + position.SignalTypeClose, position, dealSupportSell);
                resetSide(dealSupportSell, dealSupportBuy);
            }
            else
            {
                sendLogSystemLocal("Успешно закрыта старая позиция:" + position.SignalTypeClose, position);
            }



        }

        public void positionOpeningFailEventLogic(Position position)
        {
            sendLogSystemLocal("Позиция переведена в статус Fail:" + lastBestAsc, position);
            //Значит ждем эту позицию:
            if (dealSupportBuy.getPosition() != null && position.Number == dealSupportBuy.getPosition().Number)
            {
                sendLogSystemLocal("Успешно удалилил Fail позицию:" + position.SignalTypeClose, position, dealSupportBuy);
                resetSide(dealSupportBuy, dealSupportSell);
            }
            else if (dealSupportSell.getPosition() != null && position.Number == dealSupportSell.getPosition().Number)
            {
                sendLogSystemLocal("Успешно удалилил Fail позицию:" + position.SignalTypeClose, position, dealSupportSell);
                resetSide(dealSupportSell, dealSupportBuy);
            }
            else
            {
                sendLogSystemLocal("Успешно удалили старую Fail позицию:" + position.SignalTypeClose, position);
            }
        }

        private GroupType getGroupType(Side side)
        {
            GroupType groupTypeWithSide = GroupType.TestTest;
            
            if (directionTypeCurrent == DirectionType.Flat && side == Side.Sell)
            {
                groupTypeWithSide = GroupType.FlatSell;
            } else if (directionTypeCurrent == DirectionType.Flat && side == Side.Buy)
            {
                groupTypeWithSide = GroupType.FlatBuy;
            } else if (directionTypeCurrent == DirectionType.Up && side == Side.Sell)
            {
                groupTypeWithSide = GroupType.UpSell;
            }
            else if (directionTypeCurrent == DirectionType.Up && side == Side.Buy)
            {
                groupTypeWithSide = GroupType.UpBuy;
            } else if (directionTypeCurrent == DirectionType.Down && side == Side.Sell)
            {
                groupTypeWithSide = GroupType.DownSell;
            }
            else if (directionTypeCurrent == DirectionType.Down && side == Side.Buy)
            {
                groupTypeWithSide = GroupType.DownBuy;
            }
            return groupTypeWithSide;
        }
        private DirectionType getDirectionType()
        {
            DirectionType directionType;
            if (generalParameters.getTestSettings() || movingAverageService.getMaLastValueSlow() == 0)
            {
                directionType = DirectionType.Test;
            }
            else if ((movingAverageService.getMaLastValueSlow() < movingAverageService.getMaLastValueFast() && movingAverageService.getMaLastValueFast() < getValueAddPercent(movingAverageService.getMaLastValueSlow(), generalParameters.getMaStrength()))
                    || (movingAverageService.getMaLastValueFast() < movingAverageService.getMaLastValueSlow() && movingAverageService.getMaLastValueSlow() < getValueAddPercent(movingAverageService.getMaLastValueFast(), generalParameters.getMaStrength())))
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
        private void resetSide(DealSupport dealSupport, DealSupport dealSupportAnother)
        {
            sendLogSystemLocal("Обнуляем " + dealSupport.getSide() + " нашли позицию:", dealSupport.getPosition(), dealSupport, -1);
            if (dealSupport.getChartElementCount() <= 2)
            {
                paintService.deleteChartElements(dealSupport.getChartElements());
            }
            if(dealSupportAnother != null && dealSupportAnother.getProcessState() == ProcessState.FREE)
            {
                //Если нет открытых сделок в противоположную сторону, можно разблокировать открытие новых сделок в рамках тренда
                lockCurrentDirection = false;
            }
            dealSupport.reset();
        }
        private void barCounterProcess(DealSupport dealSupport, DealSupport dealSupportAnother)
        {
            if (dealSupport.getProcessState() == ProcessState.OK_TRIGGER_START)
            {
                dealService.closeAllOrderToPosition(dealSupport.getPosition(), "Новая свеча");
                resetSide(dealSupport, dealSupportAnother);
            }
            if (dealSupport.getProcessState() == ProcessState.WAIT_TP_SL)
            {
                dealSupport.addCounterBar();
                if (dealSupport.getCounterBar() > dealSupport.getCountBarForClose())
                {
                    dealService.closeAllDeals(dealSupport.getSide(), "Закрылись по барам");
                    resetSide(dealSupport, dealSupportAnother);
                }
            }
        }

        private void sendLogSystemLocal(string text, Position position = null, DealSupport dealSupport = null, int level = 0)
        {
            StringBuilder sb = new StringBuilder();
            string positionInfo = "";
            string groupType = "";
            ProcessState processState = ProcessState.FREE;
            if (position != null)
            {
                sb.Append("#0000").Append(position.Number).Append(" ");
                positionInfo = logService.getPositionInfo(position);
                if (position.Direction == Side.Buy && dealSupportBuy.getPosition() != null && dealSupportBuy.getPosition().Number == position.Number)
                {
                    processState = dealSupportBuy.getProcessState();
                }
                else if (position.Direction == Side.Sell && dealSupportSell.getPosition() != null && dealSupportSell.getPosition().Number == position.Number)
                {
                    processState = dealSupportSell.getProcessState();
                }
            }
            if(dealSupport != null)
            {
                groupType = dealSupport.getGroupType();
            }
            if (level == 0)
            {
                level = (int)processState;
            }
            sb.Append(processState).Append(" ");
            sb.Append(groupType).Append(" ");
            sb.Append(text);
            sb.Append(positionInfo);
            sb.Append(" lastBestBid = ").Append(lastBestBid);
            sb.Append(" lastBestAsc = ").Append(lastBestAsc);
            logService.sendLogSystem(sb.ToString(), level);
        }

        private decimal getValueAddPercent(decimal value, decimal percent)
        {
            return value + (value * percent / 100);
        }
        private decimal getValueSubtractPercent(decimal value, decimal percent)
        {
            return value - (value * percent / 100);
        }

        private void logBotSettings()
        {
            logService.sendLogSystem("");
            logService.sendLogSystem("");
            logService.sendLogSystem(LogService.SEPARATE_PARAMETR_LINE);
            logService.sendLogSystem(LogService.SEPARATE_PARAMETR_LINE);
            logService.sendLogSystem(SqueezyTrading.BOT_NAME + " init successful, started version bot:" + SqueezyTrading.VERSION);
            logService.sendLogSystem(generalParameters.getAllSettings());
            List<GroupParametersTrading> listParameters = groupParametersService.getGroupsParameters();
            foreach (var groupParameters in listParameters)
            {
                logService.sendLogSystem(groupParameters.getAllGroupParameters());
            }
                
        }

        public void parametrsChangeByUserLogic()
        {
            movingAverageService.updateMaLen();
            paintService.deleteAllChartElement();
            volumeSumService.calculateAndSetVolumeSum(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey());
        }

    }

    public enum ProcessState
    {
        FREE                           //группа свободна, ожидаем достижения триггера старта
      , OK_TRIGGER_START               //сработал триггер старта, оформили заявку на открытие лимитки или по рынку. Ждем открытия
      , WAIT_TP_SL                     //пришло подтверждение, что лимитка заведена, открыта, выставили sl/tp ждем окончания сделки
    }

}
