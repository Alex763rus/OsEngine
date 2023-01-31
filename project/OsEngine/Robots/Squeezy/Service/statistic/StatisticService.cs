using OsEngine.Alerts;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Robots.Squeezy.Service.statistic;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service
{
    public class StatisticService
    {
        private bool isEnabled;
        private string filePathStatistic;
        public StatisticResult[] statisticResults;
        
        public StatisticService(string filePathStatistic, bool isEnabled)
        {
            this.filePathStatistic = filePathStatistic;
            this.isEnabled = isEnabled;
            statisticResults = new StatisticResult[Enum.GetValues(typeof(GroupType)).Length];
            for(int i = 0; i < statisticResults.Length; ++i)
            {
                statisticResults[i] = new StatisticResult();
            }
            
        }

        public void setIsEnabled(bool isEnabled)
        {
            this.isEnabled = isEnabled;
        }
        public void recalcSqueezyCounter(GroupType groupType, Side side, decimal lastCandleClose, decimal price)
        {
            if (!isEnabled)
            {
                return;
            }
            decimal percent = 0;
            if (side == Side.Sell)
            {
                percent = 100.0m - lastCandleClose / price * 100.0m;
            }
            else if (side == Side.Buy)
            {
                percent = 100.0m - price / lastCandleClose * 100.0m;
            }

            percent = Math.Round(percent, 0, MidpointRounding.AwayFromZero);
            SqueezCounter squeezCounter = new SqueezCounter(percent);
            statisticResults[(int)groupType].countSqueezy(squeezCounter);
            saveStatistic();
        }
        public void recalcMaxDrawdown(GroupType groupTypeBuy, GroupType groupTypeSell, DealService dealService)
        {
            if (!isEnabled)
            {
                return;
            }
            if (dealService.hasOpendeal(Side.Sell))
            {
                Position sellPosition = dealService.getSellPosition();
                if (sellPosition != null && sellPosition.ProfitPortfolioPunkt < statisticResults[(int)groupTypeSell].getMaxDrawdown())
                {
                    statisticResults[(int)groupTypeSell].setMaxDrawdown(sellPosition.ProfitPortfolioPunkt);
                    statisticResults[(int)groupTypeSell].setPositionMaxDrawdown(sellPosition);
                    saveStatistic();
                }
            }
            else if (dealService.hasOpendeal(Side.Buy))
            {
                Position buyPosition = dealService.getBuyPosition();
                if (buyPosition != null && buyPosition.ProfitPortfolioPunkt < statisticResults[(int)groupTypeBuy].getMaxDrawdown()) {
                    statisticResults[(int)groupTypeBuy].setMaxDrawdown(buyPosition.ProfitPortfolioPunkt);
                    statisticResults[(int)groupTypeBuy].setPositionMaxDrawdown(buyPosition);
                    saveStatistic();
                }
            }
           
        }

        private void saveStatistic()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Запуск: ").Append(DateTime.Now).Append("\r\n");
            for (int i = 0; i < statisticResults.Length; ++i)
            {
                sb.Append(Enum.GetName(typeof(GroupType), i)).Append(": ");
                Position pos = statisticResults[i].getPositionMaxDrawdown();
                if (pos != null)
                {
                    sb.Append(" [").Append(LogService.getPositionNumber(pos))
                    .Append(", ").Append(pos.Direction)
                    .Append(", ").Append(pos.State)
                    .Append(", tOpen:").Append(pos.TimeOpen)
                    .Append(", tClose:").Append(pos.TimeClose)
                    .Append(']');
                }
                else
                {
                    sb.Append("[no positions]");
                }
                sb.Append(", макс. просадка:").Append(statisticResults[i].getMaxDrawdown());
                sb.Append(", сквизы: ").Append(statisticResults[i].getCounterSqueezyList());
                sb.Append("\r\n");
            }
            saveMessageInFile(filePathStatistic, sb.ToString());
        }

        private void saveMessageInFile(string filePath, string data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(data);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
