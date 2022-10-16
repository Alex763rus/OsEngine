using OsEngine.Charts.CandleChart.Elements;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using System;
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

        public void paintLabel(decimal priceY, DateTime timeX)
        {
            PointElement point = new PointElement("Some label", "Prime");

            point.Y = priceY;
            point.TimePoint = timeX;
            point.Label = "Тут могла быть ваша реклама!";
            point.Color = Color.Red;
            point.Style = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Star4;
            point.Size = 12;

            tab.SetChartElement(point);
        }

        public void paintClosedPosition(Position position)
        {
            string label = "#" + position.Number + " " + position.SignalTypeOpen + " \n" + position.Direction;
            paintLine(position.TimeOpen, position.EntryPrice, position.TimeClose, position.ClosePrice, label, Color.Yellow, 2);
        }
        public void paintLine(DateTime timeStart, decimal valueYStart, DateTime timeEnd, decimal valueYEnd, string label, Color color, int width)
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
        }

        public void paintLineHorisontal(DateTime timeStart, DateTime timeEnd)
        {
            LineHorisontal lineHorizontal = new LineHorisontal("nameUniqLH", "Prime", true);

            lineHorizontal.TimeStart = timeStart;
            lineHorizontal.TimeEnd = timeEnd;
            lineHorizontal.Label = "Тут могла быть ваша реклама!";
            lineHorizontal.Color = Color.Red;
            lineHorizontal.Value = 0.6234m;

            lineHorizontal.LineWidth = 5;

            tab.SetChartElement(lineHorizontal);
        }

        public string getUniqName(string mainPartName = "")
        {
            string uniqName = mainPartName + "_" + uniqNameIncrement;
            ++uniqNameIncrement;
            return uniqName;
        }
    }
}
