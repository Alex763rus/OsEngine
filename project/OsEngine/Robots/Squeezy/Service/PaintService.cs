using OsEngine.Charts.CandleChart.Elements;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Trading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
        private IChartElement paintLabel(decimal priceY, DateTime timeX)
        {
            PointElement point = new PointElement(getUniqName(), "Prime");

            point.Y = priceY;
            point.TimePoint = timeX;
            point.Label = "Тут могла быть ваша реклама!";
            point.Color = Color.Red;
            point.Style = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Star4;
            point.Size = 12;

            tab.SetChartElement(point);
            return point;
        }

        public IChartElement paintClosedPosition(Position position, TimeSpan timeFrame)
        {
            string label = "#" + position.Number + " " + position.SignalTypeOpen + " \n" + position.Direction;
            DateTime timeStart = position.TimeOpen.AddMinutes(-timeFrame.TotalMinutes);
            DateTime timeEnd = position.TimeClose.AddMinutes(-timeFrame.TotalMinutes);
            return paintLine(timeStart, position.EntryPrice, timeEnd, position.ClosePrice, label, Color.Yellow, 2);
        }

        public IChartElement paintClosedPosition(Position position)
        {
            string label = "#" + position.Number + " " + position.SignalTypeOpen + " \n" + position.Direction;
            return paintLine(position.TimeOpen, position.EntryPrice, position.TimeClose, position.ClosePrice, label, Color.Yellow, 2);
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
        public void paintLineHorisontal(DateTime timeStart, DateTime timeEnd, decimal yValue)
        {
            paintLineHorisontal(timeStart, timeEnd, yValue, "LABEL", Color.Red, 1);
        }
        private IChartElement paintLineHorisontal(DateTime timeStart, DateTime timeEnd, decimal yValue, string label, Color color, int width)
        {
            LineHorisontal lineHorizontal = new LineHorisontal(getUniqName(), "Prime", true);

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
