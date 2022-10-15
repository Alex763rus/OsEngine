using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsEngine.Robots.SqueezyBot.promVersion;
using System.IO;
using System.Windows.Controls;
using System.Collections;
using ru.micexrts.cgate;
using System.Threading;

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

        private void sendLogMessage(string message, LogMessageType logMessageType, int level = 0)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.Append(DateTime.Now).Append(" ")
                        .Append(getIndent(level))
                        .Append(logMessageType.ToString()).Append(":")
                        .Append(message).Append("\n");

            logList.Add(logMessage.ToString());
            squeezy.sendLog(logMessage.ToString(), logMessageType);
            if (logList.Count == squeezy.getCountBufferLogLine())
            {
                saveLogInFile(squeezy.getFilePath(), logList);
                logList.Clear();
            }
        }

        private void saveLogInFile(String filePath, ArrayList logList)
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
            }
            finally
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
