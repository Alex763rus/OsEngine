﻿using Kraken.WebSockets;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Position = OsEngine.Entity.Position;
using Side = OsEngine.Entity.Side;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class EventServiceTrading:EventService
    {

        private GeneralParametersTrading generalParameters;
        private GroupParametersTradingService groupParametersService;

        private MovingAverageService movingAverageService;
        private DealService dealService;
        private PaintService paintService;
        private LogService logService;
        private StatisticService statisticService;
        private VolumeSumService volumeSumService;

        private Candle lastCandle;
        private decimal candleTriggerStartBid; //триггер того что есть сквиз. Изменяется с завершением бара
        private decimal candleTriggerStartAsc; //триггер того что есть сквиз. Изменяется с завершением бара
        private decimal lastBestAsc;          //последняя известная лучшая цена продажи
        private decimal lastBestBid;          //последняя известная лучшая цена покупки

        private DealSupport dealSupportBuy;     //сопровождение Buy сделок
        private DealSupport dealSupportSell;    //сопровождение Sell сделок

        private DirectionType directionTypeCurrent; //Направление текущего бара по МА. 
        private bool lockCurrentDirection; //признак блокировки текущего направления. Не открывать больше сделок, дождаться завершения текущих.

        private decimal priceForPaintGroup;         //Цена чтобы рисовать тренд. Заполняется единажды
        private DateTime timeStartGroup;            //Время начала действия группы
        private bool isStart;                       //Признак начала работы

        public EventServiceTrading(BotTabSimple tab, GeneralParametersTrading generalParameters, GroupParametersTradingService groupParametersService, LogService logService, StatisticService statisticService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;
            this.logService = logService;
            this.statisticService = statisticService;
  
            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);

            paintService = new PaintService(tab);
            dealSupportBuy = new DealSupport(Side.Buy);
            dealSupportSell = new DealSupport(Side.Sell);

            directionTypeCurrent = DirectionType.None;
            lockCurrentDirection = false;
            isStart = true;
        }

        public void candleFinishedEventLogic(List<Candle> candles)
        {
            //Для тестов: Если мало баров или нет медленной, ничего не делаем:
            if (candles.Count < 2 || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }

            if(generalParameters.getClearJournal() && candles.Count%100 == 0 && !dealService.hasOpendeal(Side.Buy) && !dealService.hasOpendeal(Side.Sell)
                && dealSupportBuy.getProcessState() == ProcessState.FREE && dealSupportSell.getProcessState() == ProcessState.FREE)
            {
                sendLogSystemLocal("Выполнена очистка журнала");
                dealService.tabClear();
            }
            if (isStart)
            {
                logBotSettings();
                paintService.deleteAllChartElement();
                dealSupportBuy.reset();
                dealSupportSell.reset();
                priceForPaintGroup = MathService.getValueSubtractPercent(candles[candles.Count - 1].Low, generalParameters.getPaintGroup());
                timeStartGroup = candles[candles.Count - 1].TimeStart;
                volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);
                logService.sendLogSystem(LogService.SEPARATE_PARAMETR_LINE);
                isStart = false;
            }

            lastCandle = candles[candles.Count - 1];
            candleTriggerStartBid = MathService.getValueSubtractPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());
            candleTriggerStartAsc = MathService.getValueAddPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());

            barCounterProcess(dealSupportBuy, dealSupportSell);
            barCounterProcess(dealSupportSell, dealSupportBuy);

            DirectionType directionTypeTmp = getDirectionType();
            statisticService.recalcMaxDrawdown(getGroupType(Side.Buy), getGroupType(Side.Sell), dealService);

            if (directionTypeCurrent != directionTypeTmp)
            {
                sendLogSystemLocal("Поменялась группа тренда с " + directionTypeCurrent + " на " + directionTypeTmp + ", timeStartGroup = " 
                    + timeStartGroup + ", candles[candles.Count - 1].TimeStart = " + candles[candles.Count - 1].TimeStart + ", priceForPaintGroup = " + priceForPaintGroup);
                paintService.paintGroup(timeStartGroup, candles[candles.Count - 1].TimeStart, priceForPaintGroup, directionTypeTmp);
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
                .Append(" Position:").Append(LogService.getPositionNumber(dealSupportBuy.getPosition())).Append(" ")
                .Append(" Sell:").Append(dealSupportSell.getProcessState())
                .Append(" Position:").Append(LogService.getPositionNumber(dealSupportSell.getPosition()))
                ;
            sendLogSystemLocal(currentInfo.ToString());
        }

        private void checkDealUpdate(DealSupport dealSupport)
        {
            if (!dealSupport.hasPosition())
            {
                return;
            }
            Position position= dealSupport.getPosition();
            if(dealSupport.getProcessState() == ProcessState.OK_TRIGGER_START)
            {
                switch (position.State)
                {
                    case PositionStateType.Open: positionOpeningSuccesEventLogic(position); break;
                    case PositionStateType.OpeningFail: positionOpeningFailEventLogic(position); break;
                    case PositionStateType.Closing: positionClosingSuccesEventLogic(position); break;
                }
            } else if (dealSupport.getProcessState() == ProcessState.WAIT_TP_SL)
            {
                switch (position.State)
                {
                    case PositionStateType.ClosingFail: positionClosingSuccesEventLogic(position); break;
                    case PositionStateType.Closing: positionClosingSuccesEventLogic(position); break;
                }
            }
        }
        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            if(lastCandle == null || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }
            checkDealUpdate(dealSupportBuy);
            checkDealUpdate(dealSupportSell);

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
                string message = side + " Обнаружен сквиз. предыдущая свеча:" + LogService.getCandleInfo(lastCandle) + " price = " + price + " пересекли стартовый процент:" + tgStart;
                sendLogSystemLocal(message, null, dealSupport);
                if (processStateAnother == ProcessState.OK_TRIGGER_START)
                {
                    message = side + " нашли неоткрытую зарегистрированную лимитку в противоположную сторону, будем ее закрывать" + LogService.getPositionInfo(positionAnother);
                    sendLogSystemLocal(message, null, dealSupport, (int)processStateAnother);
                    dealService.closeAllOrderToPosition(positionAnother, "Новая лимитка");
                    resetSide(dealSupportAnother, dealSupport);
                }
                GroupType groupTypeCurrent = getGroupType(side);
                statisticService.recalcSqueezyCounter(groupTypeCurrent, side, lastCandle.Close, price);
                GroupParametersTrading groupParameters = groupParametersService.getGroupParameters(groupTypeCurrent);

                decimal priceOpenLimit = 0 ;
                if(side == Side.Sell)
                {
                    priceOpenLimit = MathService.getValueAddPercent(lastCandle.Close, groupParameters.getTriggerCandleDiff());
                } else if(side == Side.Buy)
                {
                    priceOpenLimit = MathService.getValueSubtractPercent(lastCandle.Close, groupParameters.getTriggerCandleDiff());
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
                        dealSupport.addChartElement(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), tgStart, position.EntryPrice, LogService.getPositionNumber(position)));
                    }
                }
                else
                {
                    position = dealService.openLimit(side, priceOpenLimit, groupTypeCurrent.ToString(), side + "AtLimit", volumeSumService.getVolumeSum(side));
                    if (position != null)
                    {
                        ProcessState psOkTriggerStart = ProcessState.OK_TRIGGER_START;
                        sendLogSystemLocal("-> " + psOkTriggerStart + " выставили заявку на открытие лимитки:", position, dealSupport, (int)psOkTriggerStart);
                        dealSupport.dealSupportUpdate(groupParameters, psOkTriggerStart, position);
                        dealSupport.addChartElement(paintService.paintLimitPosition(lastCandle, dealService.getTimeFrame(), tgStart, priceOpenLimit, LogService.getPositionNumber(position)));
                    }
                }
            }
            
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
            if(position.Number != dealSupportSell.getPositionNumber() && position.Number != dealSupportBuy.getPositionNumber())
            {
                logService.sendLogError("Открылась позиция, по которой у нас нет информации, будем ее закрывать!:" + LogService.getPositionInfo(position));
                dealService.closeAllOrderToPosition(position, "Ошибка. Неизвестная позиция");
                return;
            }
            decimal sl = position.EntryPrice;
            decimal tp = position.EntryPrice;
            DealSupport dealSupport = null;
            if (position.Direction == Side.Buy)
            {
                dealSupport = dealSupportBuy;
                tp = MathService.getValueAddPercent(tp, dealSupport.getGroupParametersTrading().getTakeProfit());
                sl = MathService.getValueSubtractPercent(sl, dealSupport.getGroupParametersTrading().getStopLoss());
            }
            else if (position.Direction == Side.Sell)
            {
                dealSupport = dealSupportSell;
                tp = MathService.getValueSubtractPercent(tp, dealSupport.getGroupParametersTrading().getTakeProfit());
                sl = MathService.getValueAddPercent(sl, dealSupport.getGroupParametersTrading().getStopLoss());
            }
            dealSupport.setProcessState(ProcessState.WAIT_TP_SL);
            position.ProfitOrderRedLine = tp;
            position.StopOrderRedLine = sl;
            dealService.setTpSl(position, tp, sl, 0);
            sendLogSystemLocal("Успешно открыта позиция:" + position.Comment, position, dealSupport, (int)dealSupport.getProcessState());
            sendLogSystemLocal("Установлен TP =" + tp + ", SL =" + sl + " для позиции:", position, dealSupport, (int)dealSupport.getProcessState());
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
            int finishState = -1;
            if (dealSupportBuy.getPosition() != null && position.Number == dealSupportBuy.getPosition().Number)
            {
                sendLogSystemLocal("Подтверждение 1: Успешно закрыта позиция:" + position.SignalTypeClose, position, dealSupportBuy, finishState);
                resetSide(dealSupportBuy, dealSupportSell);
            }
            else if (dealSupportSell.getPosition() != null && position.Number == dealSupportSell.getPosition().Number)
            {
                sendLogSystemLocal("Подтверждение 2: Успешно закрыта позиция:" + position.SignalTypeClose, position, dealSupportSell, finishState);
                resetSide(dealSupportSell, dealSupportBuy);
            }
            else
            {
                sendLogSystemLocal("Подтверждение 3: Успешно закрыта старая позиция:" + position.SignalTypeClose, position, null, finishState);
            }
        }

        public void positionOpeningFailEventLogic(Position position)
        {
            string comment = "Позиция переведена в статус Fail.";
            //Значит ждем эту позицию:
            if (dealSupportBuy.getPosition() != null && position.Number == dealSupportBuy.getPosition().Number)
            {
                sendLogSystemLocal(comment + "Успешно забыли Buy позицию, причина:" + position.SignalTypeClose, position, dealSupportBuy, -1);
                resetSide(dealSupportBuy, dealSupportSell);
            }
            else if (dealSupportSell.getPosition() != null && position.Number == dealSupportSell.getPosition().Number)
            {
                sendLogSystemLocal(comment + "Успешно забыли Sell позицию, причина:" + position.SignalTypeClose, position, dealSupportSell, -1);
                resetSide(dealSupportSell, dealSupportBuy);
            }
            else
            {
                sendLogSystemLocal("Мы о ней уже не помним:" + position.SignalTypeClose, position, null, -1);
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
            else if ((movingAverageService.getMaLastValueSlow() < movingAverageService.getMaLastValueFast() && movingAverageService.getMaLastValueFast() < MathService.getValueAddPercent(movingAverageService.getMaLastValueSlow(), generalParameters.getMaStrength()))
                    || (movingAverageService.getMaLastValueFast() < movingAverageService.getMaLastValueSlow() && movingAverageService.getMaLastValueSlow() < MathService.getValueAddPercent(movingAverageService.getMaLastValueFast(), generalParameters.getMaStrength())))
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
            //sendLogSystemLocal("Обнуляем " + dealSupport.getSide() + " нашли позицию:", dealSupport.getPosition(), dealSupport, -1);
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
                    //resetSide(dealSupport, dealSupportAnother);
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
                sb.Append(LogService.getPositionNumber(position)).Append(" ");
                positionInfo = LogService.getPositionInfo(position);
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
            volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);
            statisticService.setIsEnabled(generalParameters.getStatisticEnabled());
            logService.setup(generalParameters.getLogEnabled(), generalParameters.getCountBufferLogLine());
        }

    }

    public enum ProcessState
    {
        FREE                           //группа свободна, ожидаем достижения триггера старта
      , OK_TRIGGER_START               //сработал триггер старта, оформили заявку на открытие лимитки или по рынку. Ждем открытия
      , WAIT_TP_SL                     //пришло подтверждение, что лимитка заведена, открыта, выставили sl/tp ждем окончания сделки
      , FINISH                         //пришло событие об успешном закрытии сделки
    }

}
