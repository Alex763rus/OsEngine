using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsEngine.Robots.Squeezy.Trading;
using System.IO;
using System.Windows.Controls;
using System.Collections;
using ru.micexrts.cgate;
using System.Threading;
using OsEngine.Entity;
using OsEngine.Alerts;
using OsEngine.Charts.CandleChart.Indicators;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using Position = OsEngine.Entity.Position;
using OsEngine.Robots.Squeezy;

namespace OsEngine.Robots.SqueezyBot.Service
{
    public class LogService
    {
        private readonly Loggable squeezy;
        private ArrayList logList;
        private const int SAVE_LOG_TIME_OUT = 60000;

        public LogService(Loggable squeezy)
        {
            this.squeezy = squeezy;
            logList = new ArrayList();

            Thread thread = new Thread(new ThreadStart(checkAndSaveLog));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void sendLogSystem(string message, int level = 0)
        {
            sendLogMessage(message, LogMessageType.System, level);
        }

        public void sendLogError(string message, int level = 0)
        {
            sendLogMessage(message, LogMessageType.Error, level);
        }

        public string getCandleInfo(Candle candle)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TimeStart:").Append(candle.TimeStart)
            .Append(", Close:").Append(candle.Close)
            ;
            return sb.ToString();
        }
        public string getPositionInfo(Position position)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Number:#").Append(position.Number)
            .Append(", TimeOpen:").Append(position.TimeOpen)
            .Append(", TimeClose:").Append(position.TimeClose)
            .Append(", volume:").Append(position.OpenVolume)
            .Append(", SignalTypeOpen:").Append(position.SignalTypeOpen)
            .Append(", SignalTypeClose:").Append(position.SignalTypeClose)
            .Append(", TP:").Append(position.ProfitOrderPrice)
            .Append(", SL:").Append(position.StopOrderPrice)
            .Append(", Profit:").Append(position.ProfitPortfolioPunkt)
            ;
            return sb.ToString();
        }

        private void sendLogMessage(string message, LogMessageType logMessageType, int level = 0)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.Append(DateTime.Now).Append(" ")
                        .Append(getIndent(level))
                        .Append(logMessageType.ToString()).Append(":")
                        .Append(message);

            logList.Add(logMessage.ToString());
            squeezy.sendLog(logMessage.ToString(), logMessageType);
            if (logList.Count == squeezy.getCountBufferLogLine())
            {
                saveLogInFile(squeezy.getFilePath(), logList);
                logList.Clear();
            }
        }

        private void saveLogInFile(string filePath, ArrayList logList)
        {
            if(logList == null)
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
            } catch(Exception ex)
            {

            }
        }

        private void checkAndSaveLog()
        {
            while (true)
            {
                Thread.Sleep(SAVE_LOG_TIME_OUT);
                saveLogInFile(squeezy.getFilePath(), logList);
                logList.Clear();
            }
        }
        private string getIndent(int level)
        {
            string indent = "";
            switch (level)
            {
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
