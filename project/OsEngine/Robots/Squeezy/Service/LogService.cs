using OsEngine.Logging;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using OsEngine.Entity;
using Position = OsEngine.Entity.Position;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Market.Servers.OKX.Entity;
using OsEngine.Robots.Squeezy.Service;

namespace OsEngine.Robots.SqueezyBot.Service
{
    public class LogService
    {
        public static string SEPARATE_PARAMETR_LINE = "==========================================================================================================";
        private const int SAVE_LOG_TIME_OUT = 60000;

        private BotTabSimple tab;
        private string filePath;
        private bool isEnabled;
        private int countBufferLogLine;
        private ArrayList logList;
        private Thread threadSaver;
        
        public LogService(string filePath, bool isEnabled, int countBufferLogLine, BotTabSimple tab)
        {
            this.tab = tab;
            this.filePath = filePath;
            this.isEnabled = isEnabled;
            this.countBufferLogLine = countBufferLogLine;
            logList = new ArrayList();
            if (!isEnabled)
            {
                return;
            }
            threadSaver = new Thread(new ThreadStart(checkAndSaveLog));
            threadSaver.IsBackground = true;
            threadSaver.SetApartmentState(ApartmentState.STA);
            threadSaver.Start();
        }

        public void setup(bool isEnabled, int countBufferLogLine)
        {
            this.isEnabled = isEnabled;
            this.countBufferLogLine = countBufferLogLine;
        }

        public void sendLogUser(string message, int level = 0)
        {
            sendLogMessage(message, LogMessageType.User, level);
        }

        public void sendLogSystem(string message, int level = 0)
        {
            sendLogMessage(message, LogMessageType.System, level);
        }

        public void sendLogError(string message, int level = 0)
        {
            sendLogMessage(message, LogMessageType.Error, level);
        }

        public static string getCandleInfo(Candle candle)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TimeStart:").Append(candle.TimeStart)
            .Append(", Close:").Append(candle.Close)
            ;
            return sb.ToString();
        }
        public static string getPositionInfo(Position position)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" [").Append(getPositionNumber(position))
            .Append(", ").Append(position.Direction)
            .Append(", ").Append(position.State)
            .Append(", tOpen:").Append(position.TimeOpen)
            .Append(", tClose:").Append(position.TimeClose)
            .Append(", vol:").Append(position.OpenVolume)
            .Append(", textOpen:").Append(position.SignalTypeOpen)
            .Append(", textClose:").Append(position.SignalTypeClose)
            .Append(", TP:").Append(position.ProfitOrderPrice)
            .Append(", SL:").Append(position.StopOrderPrice)
            .Append(", Profit:").Append(position.ProfitPortfolioPunkt)
            .Append(", Comment:").Append(position.Comment)
            .Append(']');
            ;
            return sb.ToString();
        }

        public static string getPositionNumber(Position position)
        {
            if(position == null)
            {
                return "";
            }
            string positionNumber = position.Number.ToString();
            char[] m = { '0', '0', '0', '0', '0', '0' };
            StringBuilder positionNumberSb = new StringBuilder("#").Append(new string(m, 0, m.Length - positionNumber.Length)).Append(positionNumber);
            return positionNumberSb.ToString();
        }

        public void sendLogMessage(string message, LogMessageType logMessageType, int level = 0)
        {
            if (!isEnabled)
            {
                return;
            }
            StringBuilder logMessage = new StringBuilder();
            logMessage.Append(DateTime.Now).Append(" ");
            if (tab != null)
            {
                logMessage.Append(tab.TimeServerCurrent).Append(" ");
            }
            logMessage.Append(getIndent(level))
            .Append(logMessageType.ToString()).Append(":")
            .Append(message);

            logList.Add(logMessage.ToString());
            if (logList.Count == countBufferLogLine)
            {
                saveLogInFile(filePath, logList);
                logList.Clear();
            }
        }

        public void saveLogInFile(string filePath, ArrayList logList)
        {
            if (logList == null)
            {
                return;
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    foreach (var logLine in logList)
                    {
                        writer.WriteLine(logLine);
                    }
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                //skip
            }
        }

        private void checkAndSaveLog()
        {
            while (true)
            {
                Thread.Sleep(SAVE_LOG_TIME_OUT);
                saveLogInFile(filePath, logList);
                logList.Clear();
            }
        }
        private string getIndent(int level)
        {
            string indent = "";
            switch (level)
            {
                case -1: indent = "------------> "; break;
                case 1: indent = "==> "; break;
                case 2: indent = "====> "; break;
                case 3: indent = "======> "; break;
                case 4: indent = "========> "; break;
                case 5: indent = "==========> "; break;
                case 6: indent = "============> "; break;
                case 7: indent = "==============> "; break;
            }
            return indent;
        }
    }
}
