using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.SqueezyBot.rulerVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Position = OsEngine.Entity.Position;

namespace OsEngine.Robots.SqueezyBot
{
    internal class DealService
    {

        private BotTabSimple tab;
        private GeneralParametersRuler generalParametersRuler;

        public DealService(BotTabSimple tab, GeneralParametersRuler generalParameters)
        {
            this.tab = tab;
            this.generalParametersRuler = generalParameters;
        }

        private const int COUNT_TRY_OPEN_DEAL = 10;

        public Position openBuyDeal(string signalType)
        {
            decimal volume = Math.Round(generalParametersRuler.getVolumePercent() / 100.0m * tab.Portfolio.ValueCurrent, 0);
            Position position = null;
            for (int i = 0; i < COUNT_TRY_OPEN_DEAL; ++i)
            {
                position = tab.BuyAtMarket(volume, signalType);
                if (position != null)
                {
                    break;
                } else {
                    //todo ERROR
                }
            }

            return position;
        }

        public Position openSellDeal(string signalType)
        {
            decimal volume = generalParametersRuler.getVolumePercent()/ 100.0m * tab.Portfolio.ValueCurrent;
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

        public bool hasOpendeal(Side direction)
        {
            if(direction == Side.Sell)
            {
                return tab.PositionOpenShort.Count > 0;
            }
            if (direction == Side.Buy)
            {
                return tab.PositionOpenLong.Count > 0;
            }
            return false;
        }

        public void closeAllDeals(Side direction)
        {
            List<Position> positions = null;
            if (direction == Side.Sell)
            {
                positions = tab.PositionOpenShort;
            } else if(direction == Side.Buy)
            {
                positions = tab.PositionOpenLong;
            }

            if (positions != null && positions.Count > 0)
            {
                tab.CloseAtMarket(positions[0], positions[0].MaxVolume, "Закрылись по барам");
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

        public void openSellAtLimit(decimal priceLimit)
        {
            decimal volume = generalParametersRuler.getVolumePercent() / 100.0m * tab.Portfolio.ValueCurrent;
            tab.SellAtLimit(volume, priceLimit);
        }
        public void openBuyAtLimit(decimal priceLimit)
        {
            decimal volume = generalParametersRuler.getVolumePercent() / 100.0m * tab.Portfolio.ValueCurrent;
            tab.BuyAtLimit(volume, priceLimit);
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
