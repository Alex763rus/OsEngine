using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OsEngine.Robots.SqueezyBot
{
    public class EventService
    {
        private GeneralParameters generalParameters;
        private GroupParametersService groupParametersService;
        private MovingAverageService movingAverageService;
        private DealService dealService;
        private bool needSetSlTp; //Необходимость выполнить поиск сделки без SL TP и сделать расчет

        public EventService(BotTabSimple tab, GeneralParameters generalParameters, GroupParametersService groupParametersService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters);
            needSetSlTp = false;
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
            if(!dealService.hasOpendeal(TrendType.Short) && candleClose1 > (candleClose2 + candleClose2 * (groupParameters.getTriggerCandleDiff()/100)))
            {
                if (dealService.openSellDeal(groupType.ToString()) != null)
                {
                    needSetSlTp = true;
                }
            } else if(!dealService.hasOpendeal(TrendType.Long) && candleClose1 < (candleClose2 - candleClose2 * (groupParameters.getTriggerCandleDiff() / 100)))
            {
                if(dealService.openBuyDeal(groupType.ToString())!= null)
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
            decimal maCorridor = 0;
            GroupType groupType;
            if (movingAverageService.getMaLastValueFast() > movingAverageService.getMaLastValueSlow())
            {
                //up:
                maCorridor = movingAverageService.getMaLastValueSlow() + movingAverageService.getMaLastValueSlow() * (generalParameters.getMaCorridorHighSlow() / 100);
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
                maCorridor = movingAverageService.getMaLastValueSlow() - movingAverageService.getMaLastValueSlow() * (generalParameters.getMaCorridorHighSlow() / 100);
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
