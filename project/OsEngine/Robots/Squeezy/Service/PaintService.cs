using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Pricing;
using OsEngine.Charts.CandleChart.Elements;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Color = System.Drawing.Color;
using Position = OsEngine.Entity.Position;

namespace OsEngine.Robots.SqueezyBot.Service
{
    internal class PaintService
    {
        private int uniqNameIncrement;
        private BotTabSimple tab;
        
        public PaintService(BotTabSimple tab)
        {
            this.tab = tab;
            uniqNameIncrement = 0;
        }
        public void paintSqueezy(DateTime timeEnd, TimeSpan timeFrame, decimal price, SqueezyType squeezyType)
        {
            Color color;
            if (squeezyType == SqueezyType.None)
            {
                color = Color.Black;
            }
            else if (squeezyType == SqueezyType.Buy)
            {
                color = Color.Green;
            }
            else if (squeezyType == SqueezyType.Sell)
            {
                color = Color.Red;
            }
            else if (squeezyType == SqueezyType.BuyMissed)
            {
                color = Color.Blue;
            }
            else if (squeezyType == SqueezyType.SellMissed)
            {
                color = Color.Pink;
            }
            else
            {
                color = Color.White;
            }
            DateTime timeStart = timeEnd.AddMinutes(-timeFrame.TotalMinutes);
            paintLineHorisontal(timeStart, timeEnd, price, "", color, 3);
        }

        public void paintGroup(DateTime timeStart, DateTime timeEnd, decimal price, DirectionType directionType)
        {
            Color color;
            if(directionType == DirectionType.Up)
            {
                color = Color.Green;
            } else if(directionType == DirectionType.Down)
            {
                color = Color.Red;
            } else if(directionType == DirectionType.Flat)
            {
                color = Color.Yellow;
            }
            else
            { 
                color = Color.White;
            }
            paintLineHorisontal(timeStart, timeEnd, price, "", color, 3);
        }

        public void deleteAllChartElement()
        {
            tab.DeleteAllChartElement();
        }
        public void deleteChartElements(IChartElement[] chartElements)
        {
            if(chartElements == null)
            {
                return;
            }
            foreach (IChartElement chartElement in chartElements)
            {
                if(chartElement == null)
                {
                    break;
                }
                tab.DeleteChartElement(chartElement);
            }
        }

        public IChartElement paintClosedPosition(Position position, TimeSpan timeFrame, bool isProfit)
        {
            string label = "#" + position.Number + " " + position.SignalTypeOpen + " \n" + position.Direction;
            Color color;
            if (isProfit)
            {
                color = Color.White;
            }
            else
            {
                color = Color.Red;
            }
            DateTime timeStart = position.TimeOpen.AddMinutes(-timeFrame.TotalMinutes);
            DateTime timeEnd = position.TimeClose.AddMinutes(-timeFrame.TotalMinutes);
            return paintLine(timeStart, position.EntryPrice, timeEnd, position.ClosePrice, label, color, 3);
        }

        public IChartElement paintClosedPosition(Position position, bool isProfit)
        {
            string label = "#" + position.Number; //+ " " + position.SignalTypeOpen + " \n" + position.Direction;
            Color color;
            if (isProfit)
            {
                color = Color.White;
            }
            else
            { 
                color = Color.Red;
            }
            return paintLine(position.TimeOpen, position.EntryPrice, position.TimeClose, position.ClosePrice, label, color, 3);
        }
        private IChartElement paintLine(DateTime timeStart, decimal valueYStart, DateTime timeEnd, decimal valueYEnd, string label, Color color, int width)
        {
            Line line = new Line(getUniqName(), "Prime");

            line.TimeStart = timeStart;
            line.TimeEnd = timeEnd;
            line.ValueYStart = valueYStart;
            line.ValueYEnd = valueYEnd;
            line.Label = label;
            line.Color = color;
            line.LineWidth = width;
            tab.SetChartElement(line);
            return line;
        }

        public IChartElement[] paintSlTp(Candle lastCandle, TimeSpan timeFrame, decimal sl, decimal tp, string label)
        {
            IChartElement[] chartElements = new IChartElement[2];
            DateTime timeStart = lastCandle.TimeStart;
            DateTime timeEnd = lastCandle.TimeStart.AddMinutes(timeFrame.TotalMinutes);
            chartElements[0] = paintLineHorisontal(timeStart, timeEnd, sl, "sl " + label, Color.Red, 1);
            chartElements[1] = paintLineHorisontal(timeStart, timeEnd, tp, "tp " + label, Color.Green, 1);
            return chartElements;
        }
        public IChartElement[] paintLimitPosition(Candle lastCandle, TimeSpan timeFrame, decimal candleTriggerStart, decimal priceLimit, string label)
        {
            IChartElement[] chartElements = new IChartElement[2];
            DateTime timeStart = lastCandle.TimeStart;
            DateTime timeEnd = lastCandle.TimeStart.AddMinutes(timeFrame.TotalMinutes);
            chartElements[0] = paintLineHorisontal(timeStart, timeEnd, candleTriggerStart, "start " + label, Color.Red, 1);
            chartElements[1] = paintLineHorisontal(timeStart, timeEnd, priceLimit, "limit " + label, Color.Yellow, 1);
            return chartElements;
        }

        private IChartElement paintLineHorisontal(DateTime timeStart, DateTime timeEnd, decimal yValue, string label, Color color, int width)
        {
            LineHorisontal lineHorizontal = new LineHorisontal(getUniqName(), "Prime", false);

            lineHorizontal.TimeStart = timeStart;
            lineHorizontal.TimeEnd = timeEnd;
            lineHorizontal.Label = label;
            lineHorizontal.Color = color;
            lineHorizontal.Value = yValue;

            lineHorizontal.LineWidth = width;
            tab.SetChartElement(lineHorizontal);
            return lineHorizontal;
        }

        public string getUniqName(string mainPartName = "")
        {
            string uniqName = mainPartName + "_" + uniqNameIncrement;
            ++uniqNameIncrement;
            return uniqName;
        }
    }
}
