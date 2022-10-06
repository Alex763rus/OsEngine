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
    internal class DealService
    {

        private BotTabSimple tab;
        private GeneralParameters generalParameters;

        public DealService(BotTabSimple tab, GeneralParameters generalParameters)
        {
            this.tab = tab;
            this.generalParameters = generalParameters;
        }

        private const int COUNT_TRY_OPEN_DEAL = 10;

        public Position openBuyDeal(string signalType)
        {
            decimal volume = generalParameters.getVolumePercent()/100.0m * tab.Portfolio.ValueCurrent;
            Position position = null;
            for (int i = 0; i < COUNT_TRY_OPEN_DEAL; ++i)
            {
                position = tab.BuyAtMarket(volume, signalType);
                if (position != null)
                {
                    break;
                } else {
                    int test = 0;
                }
            }

            return position;
        }

        public Position openSellDeal(string signalType)
        {
            decimal volume = generalParameters.getVolumePercent()/ 100.0m * tab.Portfolio.ValueCurrent;
            Position position = null;
            for (int i = 0; i < COUNT_TRY_OPEN_DEAL; ++i)
            {
                position = tab.SellAtMarket(volume, signalType);
                if (position != null)
                {
                    break;
                }
                else
                {
                    //todo ERROR
                }
            }
            return position;
        }

        public bool hasOpendeal(TrendType groupType)
        {
            if(groupType == TrendType.Short)
            {
                return tab.PositionOpenShort.Count > 0;
            }
            if (groupType == TrendType.Long)
            {
                return tab.PositionOpenLong.Count > 0;
            }
            return false;
        }

        public void closeAllDeals(Side direction)
        {
            List<Position> positions = tab.PositionsOpenAll;
            foreach (Position position in positions)
            {
                if(position.Direction == direction)
                {
                    tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по барам");
                    return;
                }
            }

        }
        public Position getPositionWithSlTpNotExists()
        {
            List<Position> positions = tab.PositionsOpenAll;
            foreach(Position position in positions)
            {
                if(!position.ProfitOrderIsActiv || !position.StopOrderIsActiv)
                {
                    return position;
                }
            }
            return null;
        }

        public void setSlTp(Position position, decimal sl, decimal tp)
        {
            position.StopOrderRedLine = sl;
            position.ProfitOrderPrice = tp;
            position.ProfitOrderIsActiv = true;
            position.StopOrderIsActiv = true;
        }

        internal void checkSlTpAndClose(decimal lastCandleClose)
        {
            List<Position> positions = tab.PositionsOpenAll;
            foreach (Position position in positions)
            {
                if (position.Direction == Side.Buy)
                {
                    if(lastCandleClose < position.StopOrderRedLine)
                    {
                        position.SignalTypeClose = position.SignalTypeClose + "Закрылись по SL";
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по SL");

                    }
                    else if(lastCandleClose > position.ProfitOrderPrice)
                    {
                        position.SignalTypeClose = position.SignalTypeClose + "Закрылись по TP";
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по TP");
                    }
                }
                else if (position.Direction == Side.Sell)
                {
                    if (lastCandleClose > position.StopOrderRedLine)
                    {
                        position.SignalTypeClose = position.SignalTypeClose + "Закрылись по SL";
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по SL");
                    }
                    else if (lastCandleClose < position.ProfitOrderPrice)
                    {
                        position.SignalTypeClose = position.SignalTypeClose + "Закрылись по TP";
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по TP");
                    }
                }
            }

        }
    }
}
