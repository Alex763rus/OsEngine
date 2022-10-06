using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using System.Collections.Generic;

namespace OsEngine.Robots.SqueezyBot
{
    public class EventService
    {
        private GeneralParameters generalParameters;
        private GroupParametersService groupParametersService;
        private MovingAverageService movingAverageService;
        private DealService dealService;
        private bool needSetSlTp; //Необходимость выполнить поиск сделки без SL TP и сделать расчет
        private CountBarService countBarService;
        public EventService(BotTabSimple tab, GeneralParameters generalParameters, GroupParametersService groupParametersService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters);
            needSetSlTp = false;
            countBarService = new CountBarService();
        }

        public void finishedEventLogic(List<Candle> candles)
        {
            if(candles.Count < 2 || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }
            decimal candleClose1 = candles[candles.Count - 1].Close;
            decimal candleClose2 = candles[candles.Count - 2].Close;
            setSlTpIfNotExists();
            dealService.checkSlTpAndClose(candleClose1);

            GroupType groupType = getGroupType(candleClose1);
            GroupParameters groupParameters = groupParametersService.getGroupParameters(groupType);

            if (!groupParameters.getGroupOn())
            {
                return;
            }

            //Sell:
            if (dealService.hasOpendeal(TrendType.Short))
            {
                countBarService.addCounterBarSell();
                if (countBarService.getCounterBarSell() > generalParameters.getCountBarForClose())
                {
                    dealService.closeAllDeals(Side.Sell);
                    countBarService.resetCountBarSell();//todo перенести на евент закрытия позиции
                }
            } else if (candleClose1 > (candleClose2 + candleClose2 * (groupParameters.getTriggerCandleDiff() / 100)))
            {
                if (dealService.openSellDeal(groupType.ToString()) != null)
                {
                    needSetSlTp = true;
                }
            }
            
            //Buy:
            if (dealService.hasOpendeal(TrendType.Long))
            {
                countBarService.addCounterBarBuy();
                if(countBarService.getCounterBarBuy() > generalParameters.getCountBarForClose())
                {
                    dealService.closeAllDeals(Side.Buy);
                    countBarService.resetCountBarBuy();//todo перенести на евент закрытия позиции
                }
            } else if(candleClose1 < (candleClose2 - candleClose2 * (groupParameters.getTriggerCandleDiff() / 100))) {
                if (dealService.openBuyDeal(groupType.ToString()) != null)
                {
                    needSetSlTp = true;
                }
            }
        }

        private void setSlTpIfNotExists()
        {
            Position position = dealService.getPositionWithSlTpNotExists();
            if(position == null || !needSetSlTp)
            {
                return;
            }
            
            GroupParameters groupParameters = groupParametersService.getGroupParameters(position.SignalTypeOpen);
            decimal sl = position.EntryPrice;
            decimal tp = position.EntryPrice;
            if(position.Direction == Side.Buy)
            {
                tp = tp + tp * groupParameters.getTakeProfit()/100.0m;
                sl = sl - sl * groupParameters.getStopLoss() / 100.0m;
            }
            else if (position.Direction == Side.Sell)
            {
                tp = tp - tp * groupParameters.getTakeProfit() / 100.0m;
                sl = sl + sl * groupParameters.getStopLoss() / 100.0m;
            }
            dealService.setSlTp(position, sl, tp);
            needSetSlTp = false;
        }

        private GroupType getGroupType(decimal lastCandleClose)
        {
            GroupType groupType;
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
    }
}
