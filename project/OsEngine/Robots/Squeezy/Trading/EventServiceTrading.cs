using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OsEngine.Entity;
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
        private decimal candleTriggerStartBid; //триггер цены открытия лимитки. Изменяется с завершением бара
        private decimal candleTriggerStartAsc; //триггер цены открытия лимитки. Изменяется с завершением бара

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
                return;
            }
            lastCandle = candles[candles.Count - 1];
            lastCandleGroupParameters = groupParametersService.getGroupParameters(getGroupType(lastCandle.Close));   
            candleTriggerStartBid = getValueSubtractPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());
            candleTriggerStartAsc = getValueAddPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());

            if(dealSupportService.getProcessState(Side.Sell) == ProcessState.WAIT_OPEN_POSITION)
            {
                dealService.closeAllOrderToPosition(dealSupportService.getSellPosition(), "Новая свеча");
                resetSide(Side.Sell);
            }
            if (dealSupportService.getProcessState(Side.Buy) == ProcessState.WAIT_OPEN_POSITION)
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

        private void resetSide(Side side)
        {
            if (side == Side.Sell)
            {
                countBarService.resetCountBarSell();
                if (dealSupportService.getChartElementsSellCount() <= 4) {
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
        public void positionClosingSuccesEventLogic(Position position)
        {
            //logService.sendLogSystem("Подтверждение: Успешно закрыта позиция:" + logService.getPositionInfo(position));
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
            sendLogSystemLocal("Успешно закрыта позиция:", position);
            paintService.paintClosedPosition(position);
            resetSide(position.Direction);
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
            decimal sl = position.EntryPrice;
            decimal tp = position.EntryPrice;
            if (position.Direction == Side.Buy)
            {
                tp = getValueAddPercent(tp, lastCandleGroupParameters.getTakeProfit());
                sl = getValueSubtractPercent(sl, lastCandleGroupParameters.getStopLoss());
                dealSupportService.setProcessStateBuy(ProcessState.WAIT_TP_SL);
                dealSupportService.addChartElementBuy(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportService.getGroupTypeBuy()));
            }
            else if (position.Direction == Side.Sell)
            {
                tp = getValueSubtractPercent(tp, lastCandleGroupParameters.getTakeProfit());
                sl = getValueAddPercent(sl, lastCandleGroupParameters.getStopLoss());
                dealSupportService.setProcessStateSell(ProcessState.WAIT_TP_SL);
                dealSupportService.addChartElementSell(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportService.getGroupTypeSell()));
            }
            //logService.sendLogSystem("Успешно открыта позиция:" + logService.getPositionInfo(position));
            position.ProfitOrderRedLine = tp;
            position.StopOrderRedLine = sl;
            sendLogSystemLocal("Успешно открыта позиция:", position);
            dealService.setTpSl(position, tp, sl, 0);
        }

        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            if(lastCandle == null)
            {
                return;
            }
            waitTriggerStartLogic(Side.Buy, bestBid);
            waitTriggerStartLogic(Side.Sell, bestAsk);
        }
        private void sendLogSystemLocal(string text, Position position = null)
        {
            StringBuilder sb = new StringBuilder();
            string positionInfo = "";
            ProcessState processState = ProcessState.WAIT_TRIGGER_START;
            if(position != null)
            {
                sb.Append("#0000").Append(position.Number).Append(" ");
                positionInfo = logService.getPositionInfo(position);
                processState = dealSupportService.getProcessState(position.Direction);
            }
            sb.Append(processState).Append(" ");
            sb.Append(text).Append(" ");
            sb.Append(positionInfo).Append(" ");
            logService.sendLogSystem(sb.ToString(), (int)processState);
        }
        private void waitTriggerStartLogic(Side side, decimal price)
        {
            ProcessState processState = dealSupportService.getProcessState(side);
            if (processState != ProcessState.WAIT_TRIGGER_START)//группа свободна, ожидаем достижения триггер старта
            {
                return;
            }
            Position position;
            string groupType = lastCandleGroupParameters.getGroupType().ToString();
            if (side == Side.Sell && price > candleTriggerStartAsc) //продать по цене дороже
            {
                decimal priceLimit = getValueAddPercent(lastCandle.Close, lastCandleGroupParameters.getTriggerCandleDiff());
                //logService.sendLogSystem("WAIT_TRIGGER_START " + groupType + ":" + side + " предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + candleTriggerStartAsc, (int)ProcessState.WAIT_TRIGGER_START);
                string message = groupType + ":" + side + " предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + candleTriggerStartAsc;
                sendLogSystemLocal(message);
                //Если есть открытая лимитка в противоположную сторону:
                if (dealSupportService.getProcessState(Side.Buy) == ProcessState.WAIT_OPEN_POSITION)
                {
                    //logService.sendLogSystem("Нашли зарегистрированную лимитку в противоположную сторону, будем ее закрывать");
                    sendLogSystemLocal("Нашли зарегистрированную лимитку в противоположную сторону, будем ее закрывать", dealService.getBuyPosition());
                    dealService.closeAllDeals(Side.Buy, "Новая лимитка");
                    resetSide(Side.Buy);
                }
                //Если уже пробили отслеживаемые величины, то открываемся по рынку
                if (price > priceLimit)
                {
                    position = dealService.openSellDeal(groupType, "Продажа по рынку", generalParameters.getVolumeSum());
                    if (position != null)
                    {
                        //logService.sendLogSystem("WAIT_TRIGGER_START -> WAIT_TRIGGER_TP_SL_START " + groupType + ": успешно открыли сделку по рынку:" + logService.getPositionInfo(position), (int)ProcessState.WAIT_TRIGGER_START);
                        sendLogSystemLocal("-> WAIT_TRIGGER_TP_SL_START : успешно открыли сделку по рынку: ", position);
                        dealSupportService.saveNewLimitPosition(side, ProcessState.WAIT_TRIGGER_TP_SL_START, lastCandleGroupParameters, position);
                        dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartAsc, position.EntryPrice, groupType));
                    }
                }
                else {
                    position = dealService.openSellAtLimit(priceLimit, groupType, "SellAtLimit", generalParameters.getVolumeSum());
                    if (position != null)
                    {
                        //logService.sendLogSystem("WAIT_TRIGGER_START -> WAIT_OPEN_POSITION " + groupType + ": успешно открыли лимитку:" + logService.getPositionInfo(position), (int)ProcessState.WAIT_TRIGGER_START);
                        sendLogSystemLocal("-> WAIT_OPEN_POSITION успешно открыли лимитку:", position);
                        dealSupportService.saveNewLimitPosition(side, ProcessState.WAIT_OPEN_POSITION, lastCandleGroupParameters, position);
                        dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartAsc, priceLimit, groupType));
                    }
                }
            } else if (side == Side.Buy && price < candleTriggerStartBid) //купить по цене дешевле
            {
                decimal priceLimit = getValueSubtractPercent(lastCandle.Close, lastCandleGroupParameters.getTriggerCandleDiff());
                //logService.sendLogSystem("WAIT_TRIGGER_START " + groupType + ":" + side + " предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + candleTriggerStartBid, (int)ProcessState.WAIT_TRIGGER_START);
                string message = groupType + ":" + side + " предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + candleTriggerStartBid;
                sendLogSystemLocal(message);
                //Если есть открытая лимитка в противоположную сторону:
                if (dealSupportService.getProcessState(Side.Sell) == ProcessState.WAIT_OPEN_POSITION)
                {
                    //logService.sendLogSystem("Нашли зарегистрированную лимитку в противоположную сторону, будем ее закрывать");
                    sendLogSystemLocal("Нашли зарегистрированную лимитку в противоположную сторону, будем ее закрывать", dealService.getSellPosition());
                    dealService.closeAllDeals(Side.Sell, "Новая лимитка");
                    resetSide(Side.Buy);
                }
                //Если уже пробили отслеживаемые величины, то открываемся по рынку
                if (price < priceLimit)
                {
                    position = dealService.openBuyDeal(groupType, "Покупка по рынку", generalParameters.getVolumeSum());
                    if (position != null)
                    {
                        //logService.sendLogSystem("WAIT_TRIGGER_START -> WAIT_TRIGGER_TP_SL_START " + groupType + ": успешно открыли сделку по рынку:" + logService.getPositionInfo(position), (int)ProcessState.WAIT_TRIGGER_START);
                        sendLogSystemLocal("-> WAIT_TRIGGER_TP_SL_START успешно открыли сделку по рынку: ", position);
                        dealSupportService.saveNewLimitPosition(side, ProcessState.WAIT_TRIGGER_TP_SL_START, lastCandleGroupParameters, position);
                        dealSupportService.addChartElementSell(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartBid, position.EntryPrice, groupType));
                    }
                }
                else {
                    position = dealService.openBuyAtLimit(priceLimit, groupType, "BuyAtLimit", generalParameters.getVolumeSum());
                    if (position != null)
                    {
                        //logService.sendLogSystem("WAIT_TRIGGER_START -> WAIT_OPEN_POSITION " + groupType + ": успешно открыли лимитку:" + logService.getPositionInfo(position), (int)ProcessState.WAIT_TRIGGER_START);
                        sendLogSystemLocal("-> WAIT_OPEN_POSITION успешно открыли лимитку:", position);
                        dealSupportService.saveNewLimitPosition(side, ProcessState.WAIT_OPEN_POSITION, lastCandleGroupParameters, position);
                        dealSupportService.addChartElementBuy(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), candleTriggerStartBid, priceLimit, groupType));
                    }
                }    
            }
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

        private decimal getValueAddPercent(decimal value, decimal percent)
        {
            return value + (value * percent / 100);
        }
        private decimal getValueSubtractPercent(decimal value, decimal percent)
        {
            return value - (value * percent / 100);
        }

        internal void positionOpeningFailEventLogic(Position position)
        {
            //logService.sendLogError( + logService.getPositionInfo(position));
            sendLogSystemLocal("Позиция переведена в статус Fail: ", position);
            resetSide(position.Direction);
        }
    }

    public enum ProcessState
    {
          WAIT_TRIGGER_START         //группа свободна, ожидаем достижения триггер старта
        , WAIT_OPEN_POSITION         //выставили лимитку, ожидаем открытия позиции, мониторим на предмет сброса лимитки
        , WAIT_TRIGGER_TP_SL_START   //открыли позицию, ждем триггер старта sl tp
        , WAIT_TP_SL                 //открыли sl/tp ждем окончания сделки
    }

}
