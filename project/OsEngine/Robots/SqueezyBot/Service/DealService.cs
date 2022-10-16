using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.SqueezyBot.rulerVersion;
using OsEngine.Robots.SqueezyBot.Service;
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
        private LogService logService;


        public DealService(BotTabSimple tab, GeneralParametersRuler generalParametersRuler, LogService logService)
        {
            this.tab = tab;
            this.generalParametersRuler = generalParametersRuler;
            this.logService = logService;
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
                    logService.sendLogSystem("Успешно открыта BuyAtMarket позиция:" + logService.getPositionInfo(position));
                    break;
                } else {
                    logService.sendLogError("Не удалось открыть BuyAtMarket позицию. volume:" + volume + ", signalType:" + signalType + " попытка:" + i);
                }
            }
            if(position == null)
            {
                logService.sendLogError("Не смогли открыть BuyAtMarket позицию volume:" + volume + ", signalType:" + signalType + " за " + COUNT_TRY_OPEN_DEAL + " попыток");
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
                    logService.sendLogSystem("Успешно открыта SellAtMarket позиция:" + logService.getPositionInfo(position));
                    break;
                }
                else
                {
                    logService.sendLogError("Не удалось открыть SellAtMarket позицию " + " volume:" + volume + ", signalType:" + signalType + " попытка:" + i);
                }
            }
            if (position == null)
            {
                logService.sendLogError("Не смогли открыть SellAtMarket позицию volume:" + volume + ", signalType:" + signalType + " за " + COUNT_TRY_OPEN_DEAL + " попыток");
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
            logService.sendLogSystem("Хотим закрыть все позиции по направлению:" + direction + " по причине превышения допустимого количества баров");
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
                logService.sendLogSystem("Найдена позиция для закрывания:" + positions[0].Number + " в направлении: " + direction);
                tab.CloseAtMarket(positions[0], positions[0].MaxVolume, "Закрылись по барам");
            }
        }

        public void setSlTp(Position position, decimal sl, decimal tp)
        {
            position.StopOrderPrice = sl;
            position.ProfitOrderPrice = tp;
        }

        public void openSellAtLimit(decimal priceLimit)
        {
            decimal volume = generalParametersRuler.getVolumePercent() / 100.0m * tab.Portfolio.ValueCurrent;
            logService.sendLogSystem("Хотим открыть позицию SellAtLimit, priceLimit = " + priceLimit + ", volume = " + volume);
            tab.SellAtLimit(volume, priceLimit);
        }
        public void openBuyAtLimit(decimal priceLimit)
        {
            decimal volume = generalParametersRuler.getVolumePercent() / 100.0m * tab.Portfolio.ValueCurrent;
            logService.sendLogSystem("Хотим открыть позицию BuyAtLimit, priceLimit = " + priceLimit + ", volume = " + volume);
            tab.BuyAtLimit(volume, priceLimit);
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
