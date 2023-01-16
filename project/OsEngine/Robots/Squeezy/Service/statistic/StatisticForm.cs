using OsEngine.Alerts;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OsEngine.Robots.Squeezy.Service.statistic
{
    public class StatisticForm 
    {

        public static void showStatistic()
        {
            string squeezyTesterStatistic = getDataFromFile("C:\\1_LOGS\\" + SqueezyTester.BOT_NAME + "_statistic.txt");
            string squeezyTradingStatistic = getDataFromFile("C:\\1_LOGS\\" + SqueezyTrading.BOT_NAME + "_statistic.txt");
            StringBuilder sb = new StringBuilder();
            sb.Append(SqueezyTester.BOT_NAME).Append(":").Append("\r\n");
            sb.Append(squeezyTesterStatistic);
            sb.Append("\r\n");
            sb.Append("\r\n");
            sb.Append(SqueezyTrading.BOT_NAME).Append(":").Append("\r\n");
            sb.Append(squeezyTradingStatistic);
            AlertMessageSimpleUi ui = new AlertMessageSimpleUi(sb.ToString());
            ui.Width = 1000;
            ui.Height = 400;
            ui.Title = "Статистика";
            ui.Show();
            
        }

        private static string getDataFromFile(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }
    }
}
