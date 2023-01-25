using OsEngine.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy
{
    public interface Loggable
    {
        public void sendLog(string message, LogMessageType logMessageType);

        public bool loggingEnabled();
        public int getCountBufferLogLine();
        public string getFilePath();

        public string getUniqBotName();
        public DateTime getTimeServerCurrent();
    }
}
