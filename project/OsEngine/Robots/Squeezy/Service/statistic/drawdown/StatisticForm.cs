using OsEngine.Alerts;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace OsEngine.Robots.Squeezy.Service.statistic
{
    public class StatisticForm 
    {

        public static void showStatistic()
        {
            StringBuilder sb = new StringBuilder();
            DirectoryInfo directoryInfo = new DirectoryInfo("C:\\1_LOGS");
            FileInfo [] fileinfo = directoryInfo.GetFiles();
            foreach (FileInfo file in fileinfo)
            {
                if (!file.Name.Contains("statistic"))
                {
                    continue;
                }
                sb.Append(file.Name).Append(":").Append("\r\n");
                sb.Append(getDataFromFile(file.FullName));
                sb.Append("\r\n");
                sb.Append("\r\n");
            }
            
            AlertMessageSimpleUi ui = new AlertMessageSimpleUi(sb.ToString());
            ui.SizeToContent = SizeToContent.WidthAndHeight;
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
