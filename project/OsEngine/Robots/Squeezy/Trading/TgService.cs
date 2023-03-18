using OsEngine.Robots.Squeezy.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class TgService
    {
        private bool isEnabled;
        private int messageId;
        private string filePath;
        private string stand;
        private string header;
        public TgService(string stand, bool isEnabled, string botName, string messageDir) {
            this.stand = stand;
            this.isEnabled = isEnabled;
            messageId = 0;
            filePath = messageDir + "\\" + botName + "_";
            header = stand + ", " + botName + ": ";
        }

        public void sendMessage(string data)
        {
            if (isEnabled)
            {
                string messageFileName = filePath + messageId + ".txt";
                FileService.saveMessageInFile(messageFileName, header + data, true);
                ++messageId;
            }
        }
        public void setIsEnabled(bool isEnabled)
        {
            this.isEnabled = isEnabled;
        }

    }
}
