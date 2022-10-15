using OsEngine.Charts.CandleChart.Elements;
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
        private BotTabSimple tab;

        public PaintService(BotTabSimple tab)
        {
            this.tab = tab;
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
        public void paintLine(DateTime timeStart, DateTime timeEnd)
        {
            Line line = new Line("nameUniq", "Prime");

            line.TimeStart = timeStart;
            line.TimeEnd = timeEnd;
            line.ValueYStart = 0.6234m;
            line.ValueYEnd = 0.6534m;
            line.Label = "Тут могла быть ваша реклама!";
            line.Color = Color.YellowGreen;

            line.LineWidth = 5;
            /*
            line.ValueYStart = candles[candles.Count - 11].Close;
            line.TimeStart = candles[candles.Count - 11].TimeStart;

            line.ValueYEnd = candles[candles.Count - 1].Close;
            line.TimeEnd = candles[candles.Count - 1].TimeStart;
            */
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
    }
}
