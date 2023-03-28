using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.tg
{
    public class PositionClose : PositionOpen
    {
        public string dateClose;
        public string profit;
        public string comment;
        public string deposit;
        public string signalTypeClose;

        public PositionClose(string stand, string currency, string number, string dateStart, string side, string volume, string groupType, string signalTypeOpen
            , string dateClose, string profit, string comment, string deposit, string signalTypeClose)
            : base(stand, currency, number, dateStart, side, volume, groupType, signalTypeOpen)
        {
            this.dateClose = dateClose;
            this.profit = profit;
            this.comment = comment;
            this.deposit = deposit;
            this.signalTypeClose = signalTypeClose; 
        }
    }
}
