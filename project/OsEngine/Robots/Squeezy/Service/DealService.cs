using OkonkwoOandaV20.Framework;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OsEngine.Alerts;
using OsEngine.Charts.CandleChart.Elements;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Market.Servers.GateIo.Futures.Response;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static protobuf.ws.TradesRequest;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Position = OsEngine.Entity.Position;

namespace OsEngine.Robots.SqueezyBot
{
    internal class DealService
    {

        private BotTabSimple tab;
        private GeneralParametersTester generalParametersTester;
        private LogService logService;

        public DealService(BotTabSimple tab, GeneralParametersTester generalParametersTester, LogService logService)
        {
            this.tab = tab;
            this.generalParametersTester = generalParametersTester;
            this.logService = logService;
        }

        private const int COUNT_TRY_OPEN_DEAL = 10;

        public Position getBuyPosition()
        {
            List<Position> positions = tab.PositionOpenLong;
            if (positions.Count > 0)
            {
                return positions[0];
            }
            return null;
        }
        public Position getSellPosition()
        {
            List<Position> positions = tab.PositionOpenShort;
            if(positions.Count > 0)
            {
                return positions[0];
            }
            return null;
        }
        private void sendLogErrorLocal(string text, Position position = null)
        {
            StringBuilder sb = new StringBuilder();
            string positionInfo = "";
            if (position != null)
            {
                sb.Append("#0000").Append(position.Number).Append(" ");
                positionInfo = logService.getPositionInfo(position);
            }
            sb.Append(text).Append(" ");
            sb.Append(positionInfo).Append(" ");
            logService.sendLogSystem(sb.ToString());
        }
        private void sendLogSystemLocal(string text, Position position = null)
        {
            StringBuilder sb = new StringBuilder();
            string positionInfo = "";
            if (position != null)
            {
                sb.Append("#0000").Append(position.Number).Append(" ");
                positionInfo = logService.getPositionInfo(position);
            }
            sb.Append(text).Append(" ");
            sb.Append(positionInfo).Append(" ");
            logService.sendLogSystem(sb.ToString());
        }

        public void setTpSl(Position position, decimal tp, decimal sl, int orderSleepage)
        {
            if (position.Direction == Side.Buy)
            {
                decimal stopOrderPriceTp = tp - tab.Securiti.PriceStep * orderSleepage;
                tab.CloseAtProfit(position, tp, stopOrderPriceTp, "TpSl");
                decimal stopOrderPriceSl = sl - tab.Securiti.PriceStep * orderSleepage;
                tab.CloseAtStop(position, sl, stopOrderPriceSl, "TpSl");
            }
            else if (position.Direction == Side.Sell)
            {
                decimal stopOrderPrice = tp + tab.Securiti.PriceStep * orderSleepage;
                tab.CloseAtProfit(position, tp, stopOrderPrice, "TpSl");
                decimal stopOrderPriceSl = sl + tab.Securiti.PriceStep * orderSleepage;
                tab.CloseAtStop(position, sl, stopOrderPriceSl, "TpSl");
            }
        }

        public Position openSellDeal(string signalType, string comment, decimal volumeSum = 0)
        {
            decimal volume;
            if (volumeSum == 0)
            {
                volume = Math.Round(generalParametersTester.getVolumePercent() / 100.0m * tab.Portfolio.ValueCurrent, 0);
            }
            else
            {
                volume = volumeSum;
            }
             
            Position position = null;
            for (int i = 0; i < COUNT_TRY_OPEN_DEAL; ++i)
            {
                position = tab.SellAtMarket(volume, signalType);
                if (position != null)
                {
                    position.Comment = comment;
                    sendLogSystemLocal("Заведена заявка SellAtMarket:", position);
                    break;
                }
                else
                {
                    sendLogErrorLocal("Не удалось открыть SellAtMarket позицию " + " volume:" + volume + ", signalType:" + signalType + " попытка:" + i);
                }
            }
            if (position == null)
            {
                sendLogErrorLocal("Не смогли открыть SellAtMarket позицию volume:" + volume + ", signalType:" + signalType + " за " + COUNT_TRY_OPEN_DEAL + " попыток");
            }
            return position;
        }

        public Position openDeal(Side side, string signalType, string comment, decimal volumeSum = 0)
        {
            if(side == Side.Buy)
            {
                return openBuyDeal(signalType, comment, volumeSum);
            }
            return openSellDeal(signalType, comment, volumeSum);
        }

        public Position openBuyDeal(string signalType, string comment, decimal volumeSum = 0)
        {
            decimal volume;
            if (volumeSum == 0)
            {
                volume = Math.Round(generalParametersTester.getVolumePercent() / 100.0m * tab.Portfolio.ValueCurrent, 0);
            }
            else
            {
                volume = volumeSum;
            }
            Position position = null;
            for (int i = 0; i < COUNT_TRY_OPEN_DEAL; ++i)
            {
                position = tab.BuyAtMarket(volume, signalType);
                if (position != null)
                {
                    position.Comment = comment;
                    sendLogSystemLocal("Заведена заявка BuyAtMarket:", position);
                    break;
                }
                else
                {
                    sendLogErrorLocal("Не удалось открыть BuyAtMarket заявку. volume:" + volume + ", signalType:" + signalType + " попытка:" + i);
                }
            }
            if (position == null)
            {
                sendLogErrorLocal("Не смогли открыть BuyAtMarket позицию volume:" + volume + ", signalType:" + signalType + " за " + COUNT_TRY_OPEN_DEAL + " попыток");
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

        public void closeAllOrderToPosition(Position position, string signalType)
        {
            sendLogSystemLocal("Хотим закрыть позицию по причине:" + signalType, position);
            tab.CloseAllOrderToPosition(position, signalType);
        }
        public void closeAllDeals(Side direction, string signalCloseType)
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
                sendLogSystemLocal("Хотим закрыть позицию по направлению:" + direction + " по причине:" + signalCloseType + ". Найдена позиция:", positions[0]);
                tab.CloseAtMarket(positions[0], positions[0].MaxVolume, signalCloseType);
            }
        }

        public void setSlTp(Position position, decimal sl, decimal tp)
        {
            position.StopOrderPrice = sl;
            position.ProfitOrderPrice = tp;
            sendLogSystemLocal("Установлен TP =" + tp + ", SL =" + sl + " для позиции:", position);
        }

        public Position openLimit(Side side, decimal priceLimit, string signalType, string comment, decimal volumeSum)
        {
            if(side == Side.Buy)
            {
                return openBuyAtLimit(priceLimit, signalType, comment, volumeSum);
            }
            return openSellAtLimit(priceLimit, signalType, comment, volumeSum);
        }
        public Position openSellAtLimit(decimal priceLimit, string signalType, string comment, decimal volumeSum)
        {
            priceLimit = Convert.ToDecimal(priceLimit);
            Position position = tab.SellAtLimit(volumeSum, priceLimit, signalType);
            if (position != null)
            {
                sendLogSystemLocal("Заведена заявка на позицию SellAtLimit, priceLimit = " + priceLimit + ", volumeSum = " + volumeSum + ", comment = " + comment, position);
                position.Comment = comment;
            }
            return position;
        }
        public Position openBuyAtLimit(decimal priceLimit, string signalType, string comment, decimal volumeSum)
        {
            Position position = tab.BuyAtLimit(volumeSum, priceLimit, signalType);
            if (position != null)
            {
                sendLogSystemLocal("Заведена заявка на позицию BuyAtLimit, priceLimit = " + priceLimit + ", volumeSum = " + volumeSum + ", comment = " + comment, position);
                position.Comment = comment;
            }
            else
            {
                sendLogSystemLocal("ОШИБКА при заведении заявки на позицию BuyAtLimit, priceLimit = " + priceLimit + ", volumeSum = " + volumeSum + ", comment = " + comment, position);
            }
            return position;
        }

        public TimeSpan getTimeFrame()
        {
            return tab.TimeFrame;
        }

        internal void checkSlTpAndClose(decimal lastCandleClose)
        {
            List<Position> positions = tab.PositionsOpenAll;
            foreach (Position position in positions)
            {
                if (position.Direction == Side.Buy)
                {
                    if(lastCandleClose < position.StopOrderPrice)
                    {
                        logService.sendLogSystem("Пересечен виртуальный SL у позиции:" + position.Number + ", цена закрытия последнего бара:" + lastCandleClose + ", виртуальный SL:" + position.StopOrderPrice + " группа:" + position.SignalTypeOpen + ", направление:" + position.Direction);
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по SL");
                    }
                    else if(lastCandleClose > position.ProfitOrderPrice)
                    {
                        logService.sendLogSystem("Пересечен виртуальный TP у позиции:" + position.Number + ", цена закрытия последнего бара:" + lastCandleClose + ", виртуальный TP:" + position.ProfitOrderPrice + " группа:" + position.SignalTypeOpen + ", направление:" + position.Direction);
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по TP");
                    }
                }
                else if (position.Direction == Side.Sell)
                {
                    if (lastCandleClose > position.StopOrderPrice)
                    {
                        logService.sendLogSystem("Пересечен виртуальный SL у позиции:" + position.Number + ", цена закрытия последнего бара:" + lastCandleClose + ", виртуальный SL:" + position.StopOrderPrice + " группа:" + position.SignalTypeOpen + ", направление:" + position.Direction);
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по SL");
                    }
                    else if (lastCandleClose < position.ProfitOrderPrice)
                    {
                        logService.sendLogSystem("Пересечен виртуальный TP у позиции:" + position.Number + ", цена закрытия последнего бара:" + lastCandleClose + ", виртуальный TP:" + position.ProfitOrderPrice + " группа:" + position.SignalTypeOpen + ", направление:" + position.Direction);
                        tab.CloseAtMarket(position, position.MaxVolume, "Закрылись по TP");
                    }
                }
            }
        }
    }
}
