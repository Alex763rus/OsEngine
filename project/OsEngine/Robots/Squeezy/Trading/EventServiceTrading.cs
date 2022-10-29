using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
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

namespace OsEngine.Robots.Squeezy.Trading
{
    public class EventServiceTrading
    {

        private GeneralParametersTrading generalParameters;
        private GroupParametersTradingService groupParametersService;

        private MovingAverageService movingAverageService;
        private DealService dealService;
        private CountBarService countBarService;
        private PaintService paintService;
        private LogService logService;
        private DealSupportService dealSupportService;

        private Candle lastCandle;
        private GroupParametersTrading lastCandleGroupParameters; //группа для текущей свечи
        private decimal candleTriggerStartBid; //триггер того что есть сквиз. Изменяется с завершением бара
        private decimal candleTriggerStartAsc; //триггер того что есть сквиз. Изменяется с завершением бара
        private decimal priceOpenLimitSell;    //цена открытия sell лимитки. Изменяется с завершением бара
        private decimal priceOpenLimitBuy;    //цена открытия buy лимитки. Изменяется с завершением бара
        private decimal lastBestAsc;          //последняя известная лучшая цена продажи
        private decimal lastBestBid;          //последняя известная лучшая цена покупки

        public EventServiceTrading(BotTabSimple tab, GeneralParametersTrading generalParameters, GroupParametersTradingService groupParametersService, LogService logService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;
            this.logService = logService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);
            countBarService = new CountBarService();
            paintService = new PaintService(tab);
            dealSupportService = new DealSupportService();
        }

        public void candleFinishedEventLogic(List<Candle> candles)
        {
            if (candles.Count < 2)
            {
                if (candles.Count == 1)
                {
                    logBotSettings();
                    countBarService.resetCountBarSell();
                    countBarService.resetCountBarBuy();
                    paintService.deleteAllChartElement();
                    dealSupportService.dealSupportResetSell();
                    dealSupportService.dealSupportResetBuy();
                }
                return;
            }

            lastCandle = candles[candles.Count - 1];
            lastCandleGroupParameters = groupParametersService.getGroupParameters(getGroupType(lastCandle.Close));   
            candleTriggerStartBid = getValueSubtractPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());
            candleTriggerStartAsc = getValueAddPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());

            priceOpenLimitSell = getValueAddPercent(lastCandle.Close, lastCandleGroupParameters.getTriggerCandleDiff());
            priceOpenLimitBuy = getValueSubtractPercent(lastCandle.Close, lastCandleGroupParameters.getTriggerCandleDiff());

            if (dealSupportService.getProcessState(Side.Sell) == ProcessState.OK_TRIGGER_START)
            {
                dealService.closeAllOrderToPosition(dealSupportService.getSellPosition(), "Новая свеча");
                resetSide(Side.Sell);
            }
            if (dealSupportService.getProcessState(Side.Buy) == ProcessState.OK_TRIGGER_START)
            {
                dealService.closeAllOrderToPosition(dealSupportService.getBuyPosition(), "Новая свеча");
                resetSide(Side.Buy);
            }

            if (dealSupportService.getProcessState(Side.Sell) == ProcessState.WAIT_TP_SL)
            {
                countBarService.addCounterBarSell();
                if (countBarService.getCounterBarSell() > generalParameters.getCountBarForClose())
                {
                    dealService.closeAllDeals(Side.Sell, "Закрылись по барам");
                    resetSide(Side.Sell);
                }
            }
            if (dealSupportService.getProcessState(Side.Buy) == ProcessState.WAIT_TP_SL)
            {
                countBarService.addCounterBarBuy();
                if (countBarService.getCounterBarBuy() > generalParameters.getCountBarForClose())
                {
                    dealService.closeAllDeals(Side.Buy, "Закрылись по барам");
                    resetSide(Side.Buy);
                }
            }
        }

        public void positionClosingSuccesEventLogic(Position position)
        {
            if (position.SignalTypeClose!= null && position.SignalTypeClose.Equals("TpSl"))
            {
                if (position.ProfitPortfolioPunkt > 0)
                {
                    position.SignalTypeClose = "Закрылись по TP";
                }
                else
                {
                    position.SignalTypeClose = "Закрылись по SL";
                }
            }
            //Значит мы еще ждем эту позицию
            if (dealSupportService.checkBuyOrSellPosition(position))
            {
                sendLogSystemLocal("Успешно закрыта позиция:" + position.SignalTypeClose, position);
                resetSide(position.Direction);
            }
            else
            {
                sendLogSystemLocal("Успешно закрыта старая позиция:" + position.SignalTypeClose, position);
            }
            paintService.paintClosedPosition(position, dealService.getTimeFrame());
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
            sendLogSystemLocal("-> WAIT_TP_SL Успешно открыта позиция:" + position.Comment, position);
            decimal sl = position.EntryPrice;
            decimal tp = position.EntryPrice;
            if (position.Direction == Side.Buy)
            {
                tp = getValueAddPercent(tp, lastCandleGroupParameters.getTakeProfit());
                sl = getValueSubtractPercent(sl, lastCandleGroupParameters.getStopLoss());
                if (position.Comment.Equals("BuyAtLimit"))
                {
                    dealSupportService.setProcessStateBuy(ProcessState.WAIT_TP_SL);
                }
                else
                {
                    dealSupportService.setProcessStateBuy(ProcessState.WAIT_TP_SL); 
                }
                dealSupportService.addChartElementBuy(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportService.getGroupTypeBuy()));
            }
            else if (position.Direction == Side.Sell)
            {
                tp = getValueSubtractPercent(tp, lastCandleGroupParameters.getTakeProfit());
                sl = getValueAddPercent(sl, lastCandleGroupParameters.getStopLoss());
                if (position.Comment.Equals("SellAtLimit"))
                {
                    dealSupportService.setProcessStateSell(ProcessState.WAIT_TP_SL);
                }
                else
                {
                    dealSupportService.setProcessStateSell(ProcessState.WAIT_TP_SL);
                } 
                dealSupportService.addChartElementSell(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportService.getGroupTypeSell()));
            }
            position.ProfitOrderRedLine = tp;
            position.StopOrderRedLine = sl;
            dealService.setTpSl(position, tp, sl, 0);
            sendLogSystemLocal("Установлен TP =" + tp + ", SL =" + sl + " для позиции:", position);
            
        }

        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            if(lastCandle == null)
            {
                return;
            }

            lastBestBid = bestBid;
            lastBestAsc = bestAsk;

            waitTriggerStartLogic(Side.Buy, bestBid);
            waitTriggerStartLogic(Side.Sell, bestAsk);
        }
        private void waitTriggerStartLogic(Side side, decimal price)
        {
            ProcessState processState = dealSupportService.getProcessState(side);
            //группа свободна, или лимитка зарегистрирована
            if (processState != ProcessState.FREE)
            {
                return;
            }
            Position position;
            string groupType = lastCandleGroupParameters.getGroupType().ToString();
            if (side == Side.Sell && price > candleTriggerStartAsc) //продать по цене дороже
            {

                string message = groupType + ":" + side + " предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + candleTriggerStartAsc;
                sendLogSystemLocal(message);
                
                ProcessState processStateAnother = dealSupportService.getProcessState(Side.Buy);
                Position positionAnother = dealSupportService.getBuyPosition();
                if (processStateAnother == ProcessState.WAIT_TP_SL) {
                    sendLogSystemLocal("Открываться не будем, найдена открытая сделка в другую сторону:" + logService.getPositionInfo(positionAnother));
                    return;
                }
                if (processStateAnother == ProcessState.OK_TRIGGER_START)
                {
                    sendLogSystemLocal("Нашли неоткрытую зарегистрированную лимитку в противоположную сторону, будем ее закрывать" + logService.getPositionInfo(positionAnother));
                    dealService.closeAllOrderToPosition(positionAnother, "Новая лимитка");
                    resetSide(Side.Buy);
                }
                //Если нет активностей в противоположную сторону:
                if(dealSupportService.getProcessState(Side.Buy) == ProcessState.FREE)
                {
                    //Если уже пробили отслеживаемые величины, то открываемся по рынку
                    if (price > priceOpenLimitSell)
                    {
                        position = dealService.openSellDeal(groupType, "Продажа по рынку", generalParameters.getVolumeSum());
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START : выставили заявку по рынку:", position);
                            dealSupportService.saveNewLimitPosition(side, ProcessState.OK_TRIGGER_START, lastCandleGroupParameters, position);
                            dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartAsc, position.EntryPrice, groupType));
                            dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartBid, position.EntryPrice, groupType));
                        }
                    }
                    else
                    {
                        position = dealService.openSellAtLimit(priceOpenLimitSell, groupType, "SellAtLimit", generalParameters.getVolumeSum());
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START выставили заявку на открытие лимитки:", position);
                            dealSupportService.saveNewLimitPosition(side, ProcessState.OK_TRIGGER_START, lastCandleGroupParameters, position);
                            dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartAsc, priceOpenLimitSell, groupType));
                            dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartBid, priceOpenLimitBuy, groupType));
                        }
                    }
                }

            }
            if (side == Side.Buy && price < candleTriggerStartBid) //купить по цене дешевле
            {
                string message = groupType + ":" + side + " предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + candleTriggerStartBid;
                sendLogSystemLocal(message);

                ProcessState processStateAnother = dealSupportService.getProcessState(Side.Sell);
                Position positionAnother = dealSupportService.getSellPosition();
                if (processStateAnother == ProcessState.WAIT_TP_SL)
                {
                    sendLogSystemLocal("Открываться не будем, найдена открытая сделка в другую сторону:" + logService.getPositionInfo(positionAnother));
                    return;
                }
                if (processStateAnother == ProcessState.OK_TRIGGER_START)
                {
                    sendLogSystemLocal(side + " нашли неоткрытую зарегистрированную лимитку в противоположную сторону, будем ее закрывать" + logService.getPositionInfo(positionAnother));
                    dealService.closeAllOrderToPosition(positionAnother, "Новая лимитка");
                    resetSide(Side.Sell);
                }

                //Если нет активностей в противоположную сторону:
                if (dealSupportService.getProcessState(Side.Sell) == ProcessState.FREE)
                {
                    //Если уже пробили отслеживаемые величины, то открываемся по рынку
                    if (price < priceOpenLimitBuy)
                    {
                        position = dealService.openBuyDeal(groupType, "Покупка по рынку", generalParameters.getVolumeSum());
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START выставили заявку по рынку:", position);
                            dealSupportService.saveNewLimitPosition(side, ProcessState.OK_TRIGGER_START, lastCandleGroupParameters, position);
                            dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartBid, position.EntryPrice, groupType));
                            dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartAsc, position.EntryPrice, groupType));
                        }
                    }
                    else
                    {
                        position = dealService.openBuyAtLimit(priceOpenLimitBuy, groupType, "BuyAtLimit", generalParameters.getVolumeSum());
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START выставили заявку на открытие лимитки:", position);
                            dealSupportService.saveNewLimitPosition(side, ProcessState.OK_TRIGGER_START, lastCandleGroupParameters, position);
                            dealSupportService.addChartElementBuy(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartBid, priceOpenLimitBuy, groupType));
                            dealSupportService.addChartElementBuy(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartAsc, priceOpenLimitSell, groupType));
                        }
                    }
                }
            }
        }
        private void sendLogSystemLocal(string text, Position position = null, int level = 0)
        {
            StringBuilder sb = new StringBuilder();
            string positionInfo = "";
            ProcessState processState = ProcessState.FREE;
            if (position != null)
            {
                sb.Append("#0000").Append(position.Number).Append(" ");
                positionInfo = logService.getPositionInfo(position);
                processState = dealSupportService.getProcessState(position.Direction);
            }
            if (level == 0)
            {
                level = (int)processState;
            }
            sb.Append(processState).Append(" ");
            sb.Append(text);
            sb.Append(positionInfo);
            sb.Append(" lastBestBid = ").Append(lastBestBid);
            sb.Append(" lastBestAsc = ").Append(lastBestAsc);
            logService.sendLogSystem(sb.ToString(), level);
        }
        private GroupType getGroupType(decimal lastCandleClose)
        {
            if (generalParameters.getTestSettings())
            {
                return GroupType.TestTest;
            }
            GroupType groupType;
            //Группа по дефолту, пока нет медленной:
            if (movingAverageService.getMaLastValueSlow() == 0)
            {
                return GroupType.UpLong;
            }
            if (movingAverageService.getMaLastValueFast() > movingAverageService.getMaLastValueSlow())
            {
                //up:
                decimal maCorridor = movingAverageService.getMaLastValueSlow() + movingAverageService.getMaLastValueSlow() * (generalParameters.getMaCorridorHighSlow() / 100);
                if (lastCandleClose > maCorridor)
                {
                    groupType = GroupType.UpLong;
                }
                else
                {
                    groupType = GroupType.UpShort;
                }
            }
            else
            {
                //down:
                decimal maCorridor = movingAverageService.getMaLastValueSlow() - movingAverageService.getMaLastValueSlow() * (generalParameters.getMaCorridorHighSlow() / 100);
                if (lastCandleClose > maCorridor)
                {
                    groupType = GroupType.DownLong;
                }
                else
                {
                    groupType = GroupType.DownShort;
                }
            }
            return groupType;
        }
        public void positionOpeningFailEventLogic(Position position)
        {
            sendLogSystemLocal("Позиция переведена в статус Fail:" + lastBestAsc, position);
            resetSide(position.Direction);
        }

        private void resetSide(Side side)
        {
            sendLogSystemLocal("Обнуляем " + side + " нашли позицию:", dealSupportService.getPosition(side), -1);
            if (side == Side.Sell)
            {
                countBarService.resetCountBarSell();
                if (dealSupportService.getChartElementsSellCount() <= 4)
                {
                    paintService.deleteChartElements(dealSupportService.getChartElementsSell());
                }
                dealSupportService.dealSupportResetSell();
            }
            if (side == Side.Buy)
            {
                countBarService.resetCountBarBuy();
                if (dealSupportService.getChartElementsBuyCount() <= 4)
                {
                    paintService.deleteChartElements(dealSupportService.getChartElementsBuy());
                }
                dealSupportService.dealSupportResetBuy();
            }
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
            logService.sendLogSystem("/n/n");
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

    }

    public enum ProcessState
    {
        FREE                           //группа свободна, ожидаем достижения триггера старта
      , OK_TRIGGER_START               //сработал триггер старта, оформили заявку на открытие лимитки или по рынку. Ждем открытия
//      , OK_LIMIT_REGISTERED            //пришло подтверждение, что лимитка заведена, но еще не открыта
      , WAIT_TP_SL                     //пришло подтверждение, что лимитка заведена, открыта, выставили sl/tp ждем окончания сделки
    }

}
