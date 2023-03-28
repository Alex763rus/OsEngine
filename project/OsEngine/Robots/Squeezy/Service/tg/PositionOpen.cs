using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.tg
{
    public class PositionOpen
    {
        public String stand;
        public String currency;
        public String number;
        public String dateStart;
        public String side;
        public string volume;
        public String groupType;
        public String signalTypeOpen;

        public PositionOpen(string stand, string currency, string number, string dateStart, string side, string volume, string groupType, string signalTypeOpen)
        {
            this.stand = stand;
            this.currency = currency;
            this.number = number;
            this.dateStart = dateStart;
            this.side = side;
            this.volume = volume;
            this.groupType = groupType;
            this.signalTypeOpen = signalTypeOpen;
        }
    }
}
