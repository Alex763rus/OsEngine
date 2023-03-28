using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab.Internal;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Service.tg;
using OsEngine.Robots.SqueezyBot.Service;
using RestSharp;
using RestSharp.Serializers;
using ru.micexrts.cgate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace OsEngine.Robots.Squeezy.Trading
{
    public class TgService
    {
        private bool isEnabled;
        private int messageId;
        private string stand;
        private string botName;
 
        public TgService(bool isEnabled, string stand, string botName) {
            this.isEnabled = isEnabled;
            this.stand = stand;
            this.botName = botName;
            messageId = 0;
        }

        public void sendUnsorted(string message)
        {
            if (!isEnabled)
            {
                return;
            }
            Unsorted unsorted = new Unsorted(stand, botName, message);
            SendTestPostRequest("http://localhost:8080/squeezy/unsorted", SimpleJson.SerializeObject(unsorted));
        }
        public void sendSqueezyStart(string version)
        {
            if (!isEnabled)
            {
                return;
            }
            SqueezyStart squeezyStart = new SqueezyStart(version);
            SendTestPostRequest("http://localhost:8080/squeezy/start", SimpleJson.SerializeObject(squeezyStart));
        }
        public void sendPositionOpen(DealSupport dealSupport)
        {
            if (!isEnabled)
            {
                return;
            }
            Position position = dealSupport.getPosition();
            PositionOpen positionOpen = new PositionOpen(stand, botName, LogService.getPositionNumber(position)
                , position.TimeOpen.ToString(), position.Direction.ToString(), getDecimalValue(position.PortfolioValueOnOpenPosition), dealSupport.getGroupType()
                , position.SignalTypeOpen
                );
            SendTestPostRequest("http://localhost:8080/position/open", SimpleJson.SerializeObject(positionOpen));
        }

        public void sendPositionClose(DealSupport dealSupport, string comment, decimal deposit)
        {
            if (!isEnabled)
            {
                return;
            }
            Position position = dealSupport.getPosition();
            PositionClose positionClose = new PositionClose(stand, botName, LogService.getPositionNumber(position)
                , position.TimeOpen.ToString(), position.Direction.ToString(), getDecimalValue(position.PortfolioValueOnOpenPosition), dealSupport.getGroupType()
                , position.SignalTypeOpen
                , position.TimeClose.ToString(), getDecimalValue(position.ProfitPortfolioPunkt), comment, getDecimalValue(deposit), position.SignalTypeClose
                );
            SendTestPostRequest("http://localhost:8080/position/close", SimpleJson.SerializeObject(positionClose));
        }

        void SendTestPostRequest(string url, string data)
        {
            string json = data;
            byte[] body = Encoding.UTF8.GetBytes(json);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = body.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(body, 0, body.Length);
                stream.Close();
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                response.Close();
            }
        }

        private string getDecimalValue(decimal value)
        {
            return Convert.ToString(value).Replace(",", ".");
        }

        public void setIsEnabled(bool isEnabled)
        {
            this.isEnabled = isEnabled;
        }

    }
}
