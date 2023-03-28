using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.tg
{
    public class PositionOpen
    {
        public string stand;
        public string currency;
        public string number;
        public string dateStart;
        public string side;
        public string volume;
        public string groupType;
        public string signalTypeOpen;

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
