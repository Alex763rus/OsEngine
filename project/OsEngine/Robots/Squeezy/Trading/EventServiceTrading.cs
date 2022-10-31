using Kraken.WebSockets;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Pricing;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.Market.Servers.GateIo.Futures.Response;
using OsEngine.OsTrader.Panels.Tab;
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

        private Candle lastCandle;
        private GroupParametersTrading lastCandleGroupParameters; //группа для текущей свечи
        private decimal candleTriggerStartBid; //триггер того что есть сквиз. Изменяется с завершением бара
        private decimal candleTriggerStartAsc; //триггер того что есть сквиз. Изменяется с завершением бара
        private decimal priceOpenLimitSell;    //цена открытия sell лимитки. Изменяется с завершением бара
        private decimal priceOpenLimitBuy;    //цена открытия buy лимитки. Изменяется с завершением бара
        private decimal lastBestAsc;          //последняя известная лучшая цена продажи
        private decimal lastBestBid;          //последняя известная лучшая цена покупки

        private DealSupport dealSupportBuy;     //сопровождение Buy сделок
        private DealSupport dealSupportSell;    //сопровождение Sell сделок
        public EventServiceTrading(BotTabSimple tab, GeneralParametersTrading generalParameters, GroupParametersTradingService groupParametersService, LogService logService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;
            this.logService = logService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);

            paintService = new PaintService(tab);
            dealSupportBuy = new DealSupport(Side.Buy);
            dealSupportSell = new DealSupport(Side.Sell);
        }

        public void candleFinishedEventLogic(List<Candle> candles)
        {
            if (candles.Count < 2)
            {
                if (candles.Count == 1)
                {
                    logBotSettings();
                    paintService.deleteAllChartElement();
                    dealSupportBuy.reset();
                    dealSupportSell.reset();
                }
                return;
            }
            //Если нет медленной, ничего ен делаем:
            if (movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }

            lastCandle = candles[candles.Count - 1];
            lastCandleGroupParameters = groupParametersService.getGroupParameters(getGroupType(lastCandle.Close));   
            candleTriggerStartBid = getValueSubtractPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());
            candleTriggerStartAsc = getValueAddPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());

            priceOpenLimitSell = getValueAddPercent(lastCandle.Close, lastCandleGroupParameters.getTriggerCandleDiff());
            priceOpenLimitBuy = getValueSubtractPercent(lastCandle.Close, lastCandleGroupParameters.getTriggerCandleDiff());

            newBarProcess(dealSupportBuy);
            newBarProcess(dealSupportSell);

            barCounterProcess(dealSupportBuy);
            barCounterProcess(dealSupportSell);
        }
        private void newBarProcess(DealSupport dealSupport)
        {
            if (dealSupport.getProcessState() != ProcessState.OK_TRIGGER_START)
            {
                return;
            }
            dealService.closeAllOrderToPosition(dealSupport.getPosition(), "Новая свеча");
            resetSide(dealSupport);
        }
        private void barCounterProcess(DealSupport dealSupport)
        {
            if (dealSupport.getProcessState() != ProcessState.WAIT_TP_SL)
            {
                return;
            }
            dealSupport.addCounterBar();
            if (dealSupport.getCounterBar() > dealSupport.getCountBarForClose())
            {
                dealService.closeAllDeals(dealSupport.getSide(), "Закрылись по барам");
                resetSide(dealSupport);
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
            //Значит ждем эту позицию:
            if(dealSupportBuy.getPosition() != null && position.Number == dealSupportBuy.getPosition().Number)
            {
                sendLogSystemLocal("Успешно закрыта позиция:" + position.SignalTypeClose, position);
                resetSide(dealSupportBuy);
            } else if(dealSupportSell.getPosition() != null && position.Number == dealSupportSell.getPosition().Number)
            {
                sendLogSystemLocal("Успешно закрыта позиция:" + position.SignalTypeClose, position);
                resetSide(dealSupportSell);
            } else
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
                dealSupportBuy.setProcessState(ProcessState.WAIT_TP_SL);
                dealSupportBuy.addChartElement(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportBuy.getGroupType()));
            }
            else if (position.Direction == Side.Sell)
            {
                tp = getValueSubtractPercent(tp, lastCandleGroupParameters.getTakeProfit());
                sl = getValueAddPercent(sl, lastCandleGroupParameters.getStopLoss());
                dealSupportSell.setProcessState(ProcessState.WAIT_TP_SL);
                dealSupportSell.addChartElement(paintService.paintSlTp(lastCandle, dealService.getTimeFrame(), sl, tp, dealSupportSell.getGroupType()));
            }
            position.ProfitOrderRedLine = tp;
            position.StopOrderRedLine = sl;
            dealService.setTpSl(position, tp, sl, 0);
            sendLogSystemLocal("Установлен TP =" + tp + ", SL =" + sl + " для позиции:", position);
        }

        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            if(lastCandle == null || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }

            lastBestBid = bestBid;
            lastBestAsc = bestAsk;

            waitTriggerStartLogic(bestBid, dealSupportBuy, dealSupportSell, candleTriggerStartBid, priceOpenLimitBuy);
            waitTriggerStartLogic(bestAsk, dealSupportSell, dealSupportBuy, candleTriggerStartAsc, priceOpenLimitSell);
        }
        private void waitTriggerStartLogic(decimal price, DealSupport dealSupport, DealSupport dealSupportAnother, decimal tgStart, decimal priceOpenLimit)
        {
            ProcessState processState = dealSupport.getProcessState();
            //группа свободна, или лимитка зарегистрирована
            if (processState != ProcessState.FREE)
            {
                return;
            }

            ProcessState processStateAnother = dealSupportAnother.getProcessState();
            Position positionAnother = dealSupportAnother.getPosition();
            Side side = dealSupport.getSide();

            Position position;
            string groupType = lastCandleGroupParameters.getGroupType().ToString();
            if (  (side == Side.Sell && price > tgStart) //продать по цене дороже
                ||(side == Side.Buy && price < tgStart)  //купить по цене дешевле
                ) 
            {
                if (processStateAnother == ProcessState.WAIT_TP_SL)
                {
                   //sendLogSystemLocal("Открываться не будем, найдена открытая сделка в другую сторону:" + logService.getPositionInfo(positionAnother));
                    return;
                }
                string message = groupType + ":" + side + " предыдущая свеча:" + logService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + tgStart;
                sendLogSystemLocal(message);
                if (processStateAnother == ProcessState.OK_TRIGGER_START)
                {
                    sendLogSystemLocal(side + " нашли неоткрытую зарегистрированную лимитку в противоположную сторону, будем ее закрывать" + logService.getPositionInfo(positionAnother));
                    dealService.closeAllOrderToPosition(positionAnother, "Новая лимитка");
                    resetSide(dealSupportAnother);
                }
                //Если нет активностей в противоположную сторону:
                if (processStateAnother == ProcessState.FREE)
                {
                    //Если уже пробили отслеживаемые величины, то открываемся по рынку
                    if (  (side == Side.Sell && price > priceOpenLimit)
                        ||(side == Side.Buy && price < priceOpenLimit)
                        )
                    {
                        position = dealService.openDeal(side, groupType, "Вход по рынку", generalParameters.getVolumeSum());
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START : выставили заявку по рынку:", position);
                            dealSupport.dealSupportUpdate(lastCandleGroupParameters, ProcessState.OK_TRIGGER_START, position);
                            dealSupport.addChartElement(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), tgStart, position.EntryPrice, groupType));
                        }
                    }
                    else
                    {
                        position = dealService.openLimit(side, priceOpenLimit, groupType, side + "AtLimit", generalParameters.getVolumeSum());
                        if (position != null)
                        {
                            sendLogSystemLocal("-> OK_TRIGGER_START выставили заявку на открытие лимитки:", position);
                            dealSupport.dealSupportUpdate(lastCandleGroupParameters, ProcessState.OK_TRIGGER_START, position);
                            dealSupport.addChartElement(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), tgStart, priceOpenLimit, groupType));
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
                if(position.Direction == Side.Buy && dealSupportBuy.getPosition() != null && dealSupportBuy.getPosition().Number == position.Number)
                {
                    processState = dealSupportBuy.getProcessState();
                }
                else if (position.Direction == Side.Sell && dealSupportSell.getPosition() != null && dealSupportSell.getPosition().Number == position.Number)
                {
                    processState = dealSupportSell.getProcessState();
                }
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
            //Группа по дефолту, пока нет медленной:
            if (movingAverageService.getMaLastValueSlow() == 0)
            {
                return GroupType.UpBuy;
            }
            GroupType groupType;
            if (movingAverageService.getMaLastValueFast() > movingAverageService.getMaLastValueSlow())
            {
                //up:
                decimal maCorridor = getValueAddPercent(movingAverageService.getMaLastValueSlow(), generalParameters.getMaCorridorHighSlow());
                if (lastCandleClose > maCorridor)
                {
                    groupType = GroupType.UpBuy;
                }
                else
                {
                    groupType = GroupType.UpSell;
                }
            }
            else
            {
                //down:
                decimal maCorridor = getValueSubtractPercent(movingAverageService.getMaLastValueSlow(), generalParameters.getMaCorridorHighSlow());
                if (lastCandleClose > maCorridor)
                {
                    groupType = GroupType.DownBuy;
                }
                else
                {
                    groupType = GroupType.DownSell;
                }
            }
            return groupType;
        }
        public void positionOpeningFailEventLogic(Position position)
        {
            sendLogSystemLocal("Позиция переведена в статус Fail:" + lastBestAsc, position);
            //Значит ждем эту позицию:
            if (dealSupportBuy.getPosition() != null && position.Number == dealSupportBuy.getPosition().Number)
            {
                sendLogSystemLocal("Успешно удалилил Fail позицию:" + position.SignalTypeClose, position);
                resetSide(dealSupportBuy);
            }
            else if (dealSupportSell.getPosition() != null && position.Number == dealSupportSell.getPosition().Number)
            {
                sendLogSystemLocal("Успешно удалилил Fail позицию:" + position.SignalTypeClose, position);
                resetSide(dealSupportSell);
            }
            else
            {
                sendLogSystemLocal("Успешно удалили старую Fail позицию:" + position.SignalTypeClose, position);
            }
        }

        private void resetSide(DealSupport dealSupport)
        {
            sendLogSystemLocal("Обнуляем " + dealSupport.getSide() + " нашли позицию:", dealSupport.getPosition(), -1);
            if (dealSupport.getChartElementCount() <= 2)
            {
                paintService.deleteChartElements(dealSupport.getChartElements());
            }
            dealSupport.reset();
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

    }

    public enum ProcessState
    {
        FREE                           //группа свободна, ожидаем достижения триггера старта
      , OK_TRIGGER_START               //сработал триггер старта, оформили заявку на открытие лимитки или по рынку. Ждем открытия
      , WAIT_TP_SL                     //пришло подтверждение, что лимитка заведена, открыта, выставили sl/tp ждем окончания сделки
    }

}
