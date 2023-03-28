using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.tg
{
    public class PositionClose : PositionOpen
    {
        public String dateClose;
        public String profit;
        public String comment;
        public String deposit;
        public String signalTypeClose;

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
