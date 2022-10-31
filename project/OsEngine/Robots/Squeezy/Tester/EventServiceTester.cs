﻿using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;

namespace OsEngine.Robots.Squeezy.Tester
{
    public class EventServiceTester
    {
        private GeneralParametersTester generalParameters;
        private GroupParametersTesterService groupParametersService;
        private MovingAverageService movingAverageService;
        private DealService dealService;
        private CountBarService countBarService;
        private LogService logService;
        private PaintService paintService;
        public EventServiceTester(BotTabSimple tab, GeneralParametersTester generalParameters, GroupParametersTesterService groupParametersService,  LogService logService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;
            this.logService = logService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);
            countBarService = new CountBarService();
            paintService = new PaintService(tab);
        }

        public void finishedEventLogic(List<Candle> candles)
        {
            if(candles.Count < 2)
            {
                return;
            }
            decimal candleClose1 = candles[candles.Count - 1].Close;
            decimal candleClose2 = candles[candles.Count - 2].Close;

            dealService.checkSlTpAndClose(candleClose1);

            GroupType groupType = getGroupType(candleClose1);
            GroupParametersTester groupParameters = groupParametersService.getGroupParameters(groupType);

            if (!groupParameters.getGroupOn())
            {
                return;
            }

            //Sell:
            if (dealService.hasOpendeal(Side.Sell))
            {
                countBarService.addCounterBarSell();
                if (countBarService.getCounterBarSell() > countBarService.getLimitBarSell())
                {
                    dealService.closeAllDeals(Side.Sell, "Закрылись по барам");
                }
            } else if (candleClose1 > (candleClose2 + candleClose2 * (groupParameters.getTriggerCandleDiff() / 100)))
            {
                string message = "Обнаружен сквиз " + groupType.ToString() + ": предпоследний бар:" + logService.getCandleInfo(candles[candles.Count - 2])
                                    + " последний бар:" + logService.getCandleInfo(candles[candles.Count - 1])
                                    + " отношение:" + Math.Round((candleClose1 - candleClose2) / candleClose2 * 100, 2) + "%"
                                    + " настройки:" + groupParameters.getTriggerCandleDiff() + "%";
                logService.sendLogSystem(message);
                dealService.openSellDeal(groupType.ToString(), "Продажа по рынку");
                countBarService.setLimitBarSell(groupParameters.getCountBarForClose());
            }
            
            //Buy:
            if (dealService.hasOpendeal(Side.Buy))
            {
                countBarService.addCounterBarBuy();
                if(countBarService.getCounterBarBuy() > countBarService.getLimitBarBuy())
                {
                    dealService.closeAllDeals(Side.Buy, "Закрылись по барам");
                }
            } else if(candleClose1 < (candleClose2 - candleClose2 * (groupParameters.getTriggerCandleDiff() / 100))) {
                string message = "Обнаружен сквиз " + groupType.ToString() + ": предпоследний бар:" + logService.getCandleInfo(candles[candles.Count - 2])
                                    + " последний бар:" + logService.getCandleInfo(candles[candles.Count - 1])
                                    + " отношение:" + Math.Round((candleClose2 - candleClose1) / candleClose1 * 100, 2) + "%"
                                    + " настройки:" + groupParameters.getTriggerCandleDiff() + "%";
                logService.sendLogSystem(message);
                dealService.openBuyDeal(groupType.ToString(), "Покупка по рынку");
                countBarService.setLimitBarBuy(groupParameters.getCountBarForClose());
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
                return GroupType.UpBuy;
            }
            if (movingAverageService.getMaLastValueFast() > movingAverageService.getMaLastValueSlow())
            {
                //up:
                decimal maCorridor = movingAverageService.getMaLastValueSlow() + movingAverageService.getMaLastValueSlow() * (generalParameters.getMaCorridorHighSlow() / 100);
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
                decimal maCorridor = movingAverageService.getMaLastValueSlow() - movingAverageService.getMaLastValueSlow() * (generalParameters.getMaCorridorHighSlow() / 100);
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

        public void positionClosingSuccesEventLogic(Position position)
        {
            logService.sendLogSystem("Подтверждение: Успешно закрыта позиция:" + logService.getPositionInfo(position));
            paintService.paintClosedPosition(position);
            if (position.Direction == Side.Buy)
            {
                countBarService.resetCountBarBuy();
            }
            if (position.Direction == Side.Sell)
            {
                countBarService.resetCountBarSell();
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
    }
}
