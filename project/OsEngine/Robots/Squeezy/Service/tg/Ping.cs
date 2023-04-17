using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.tg
{
    public class Ping
    {
        public string stand;
        public string currency;

        public Ping(string stand, string currency)
        {
            this.stand = stand;
            this.currency = currency;
        }
    }
}
