using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Position = OsEngine.Entity.Position;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class EventServiceTrading
    {

        private GeneralParametersTrading generalParameters;
        private GroupParametersTradingService groupParametersTradingService;

        private MovingAverageService movingAverageService;
        private DealService dealService;
        private CountBarService countBarService;
        private PaintService paintService;
        private LogService logService;

        private Candle lastCandle;
        private decimal candleTriggerStartBid; //триггер цены открытия лимитки. Изменяется с завершением бара
        private decimal candleTriggerStartAsc; //триггер цены открытия лимитки. Изменяется с завершением бара
        public EventServiceTrading(BotTabSimple tab, GeneralParametersTrading generalParameters, GroupParametersTradingService groupParametersTradingService, LogService logService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersTradingService = groupParametersTradingService;
            this.logService = logService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);
            countBarService = new CountBarService();
            paintService = new PaintService(tab);
        }

        public void finishedEventLogic(List<Candle> candles)
        {

            if(candles.Count == 5)
            {
                logService.sendLogSystem("HELLO LOGGER!");
            }
            logService.sendLogSystem(candles[candles.Count-1].Low.ToString() + "  vs " + candles[candles.Count - 1].High.ToString());
           /* if (candles.Count == 10)
            {
                paintService.paintLineHorisontal(candles[5].TimeStart, candles[7].TimeStart);
            }
            if (candles.Count == 15)
            {
                paintService.paintLabel(candles[10].Open, candles[10].TimeStart);
            }*/
           //============================
            if (candles.Count < 2 || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }
            lastCandle = candles[candles.Count - 1];
            candleTriggerStartBid = getValueAddPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());
            candleTriggerStartAsc = getValueSubtractPercent(lastCandle.Close, generalParameters.getTriggerStartPercent());
            //todo завершить все незаконченное
        }

        public void positionClosingSuccesEventLogic(Position position)
        {
            /*
            if (position.Direction == Side.Buy)
            {
                countBarService.resetCountBarBuy();
            }
            if (position.Direction == Side.Sell)
            {
                countBarService.resetCountBarSell();
            }
            */
            //todo
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
            //throw new NotImplementedException();
        }

        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            if(lastCandle == null || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }
            List<GroupParametersTrading> groupsParameters = groupParametersTradingService.getGroupsParameters();
            foreach (GroupParametersTrading groupParameters in groupsParameters)
            {
                if (!groupParameters.getGroupOn())
                {
                    continue;
                }
                if(groupParameters.getGroupStatus() == GroupStatus.WAIT_TRIGGER_START) //группа свободна, ожидаем достижения триггер старта
                {
                    waitTriggerStartLogic(groupParameters, bestBid, bestAsk);//проверяем триггер старта, открываем лимитку
                } else if (groupParameters.getGroupStatus() == GroupStatus.WAIT_OPEN_POSITION) //выставили лимитку, ожидаем открытия позиции, мониторим на предмет сброса лимитки
                {
                    waitOpenPositionLogic(groupParameters);
                } else if (groupParameters.getGroupStatus() == GroupStatus.WAIT_TRIGGER_TP_SL_START) //открыли позицию, ждем триггер старта sl tp
                {
                    waitTriggerTpSlStartLogic(groupParameters);
                }
                else if (groupParameters.getGroupStatus() == GroupStatus.WAIT_TP_SL) //открыли sl/tp ждем окончания сделки
                {
                    waitTpSlLogic(groupParameters);
                }
            }
        }

        
        private void waitTriggerStartLogic(GroupParametersTrading groupParameters, decimal bestBid, decimal bestAsk)
        {
            //todo... проверить наличие открытой активности в том же направлении у другой группы
            if(bestAsk > candleTriggerStartBid)
            {
                decimal priceLimit = getValueAddPercent(bestAsk, groupParameters.getTriggerCandleDiff());
                dealService.openSellAtLimit(priceLimit);
                groupParameters.setGroupStatus(GroupStatus.WAIT_OPEN_POSITION);
            } else if (bestBid < candleTriggerStartAsc)
            {
                decimal priceLimit = getValueSubtractPercent(bestBid, groupParameters.getTriggerCandleDiff());
                dealService.openBuyAtLimit(priceLimit);
                groupParameters.setGroupStatus(GroupStatus.WAIT_OPEN_POSITION);
            }
        }
        private void waitTpSlLogic(GroupParametersTrading groupParameters)
        {
           //todo
        }

        private void waitTriggerTpSlStartLogic(GroupParametersTrading groupParameters)
        {
            //todo
        }

        private void waitOpenPositionLogic(GroupParametersTrading groupParameters)
        {
            //todo
        }


        private decimal getValueAddPercent(decimal value, decimal percent)
        {
            return value + (value * percent / 100);
        }
        private decimal getValueSubtractPercent(decimal value, decimal percent)
        {
            return value - (value * percent / 100);
        }
    }

    public enum GroupStatus
    {
        WAIT_TRIGGER_START,         //группа свободна, ожидаем достижения триггер старта
        WAIT_OPEN_POSITION,         //выставили лимитку, ожидаем открытия позиции, мониторим на предмет сброса лимитки
        WAIT_TRIGGER_TP_SL_START,   //открыли позицию, ждем триггер старта sl tp
        WAIT_TP_SL                  //открыли sl/tp ждем окончания сделки
    }

    /*
     * 1) Триггер старта:  (1%)				общий параметр 		//относительно цены закрытия последнего бара
        2) Триггер отложенного ордера (2,5%)	групповой параметр	//относительно цены закрытия последнего бара
3) Триггер старта tp:  (1%)				групповой параметр	//относительно цены открытия ордера
4) Триггер фактического tp: (10%)		групповой параметр	//относительно цены открытия ордера
5) Триггер старта sl:  (1%)				групповой параметр	//относительно цены открытия ордера
6) Триггер фактического sl: (10%)		групповой параметр	//относительно цены открытия ордера

     * Онлайн следим за изменением цены по тикам. Сравниваем ее с ценой закрытия последнего бара.
Если есть расхождение на % параметр1, то выставляем отложенный ордер на % параматр2
При открытой сделке если есть отклонение параметр5 от цены открытия, то ставим tp на %парметр4.
При открытой сделке если есть отклонение параметр3 от цены открытия, то ставим sl на %параметр6.
Откаты:
Если установлен SL, а потом отклонение становится меньше чем % параметр5, то удаляем sl
Если установлен TP, а потом отклонение становится меньше чем % параметр3, то удаляем tp
Если сделка не открыта, но выставлен отложенный ордер, а потом отклонение стало меньше чем параметр1, 
	то удаляем отложенный ордер начинаем расчет заново.
Если начала образовываться новая свеча, удаляем все неоткрытые отложенные ордера
     */
}
