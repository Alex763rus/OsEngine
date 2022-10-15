using OsEngine.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    public interface Loggable
    {
        public void sendLog(string message, LogMessageType logMessageType);

        public int getCountBufferLogLine();
        public string getFilePath();
    }
}
