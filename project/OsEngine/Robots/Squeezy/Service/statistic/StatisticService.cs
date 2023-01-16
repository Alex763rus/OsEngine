using OsEngine.Alerts;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Robots.Squeezy.Service.statistic;
using OsEngine.Robots.Squeezy.Tester;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service
{
    public class StatisticService
    {
        private Statisticable statisticable;
        public StatisticResult[] statisticResults;  //7 групп

        public StatisticService(Statisticable statisticable)
        {
            this.statisticable = statisticable;
            statisticResults = new StatisticResult[Enum.GetValues(typeof(GroupType)).Length];
            for(int i = 0; i < statisticResults.Length; ++i)
            {
                statisticResults[i] = new StatisticResult();
            }
            
        }

        public void recalculateStatistic(GroupType groupType, Position position)
        {
            if (!statisticable.statisticEnabled())
            {
                return;
            }
            if (position.ProfitPortfolioPunkt < statisticResults[(int)groupType].getMaxDrawdown())
            {
                
                statisticResults[(int)groupType].setMaxDrawdown(position.ProfitPortfolioPunkt);
                statisticResults[(int)groupType].setPositionMaxDrawdown(position);
                
                StringBuilder sb = new StringBuilder();
                sb.Append("Запуск: ").Append(DateTime.Now).Append("\r\n");
                for (int i = 0; i < statisticResults.Length; ++i)
                {
                    sb.Append(Enum.GetName(typeof(GroupType), i)).Append(": ");
                    Position pos = statisticResults[i].getPositionMaxDrawdown();
                    if (pos != null)
                    {
                        sb.Append(" [#0000").Append(pos.Number)
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
                    sb.Append("\r\n");
                }
                saveMessageInFile(statisticable.getFilePathStatistic(), sb.ToString());
            }
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
