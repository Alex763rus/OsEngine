using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.tg
{
    public class SqueezyBloking
    {
        public string stand;
        public string currency;
        public string number;
        public string state;
        public string comment;

        public SqueezyBloking(string stand, string currency, string number, string state, string comment)
        {
            this.stand = stand;
            this.currency = currency;
            this.number = number;
            this.state = state;
            this.comment = comment;
        }
    }
}
