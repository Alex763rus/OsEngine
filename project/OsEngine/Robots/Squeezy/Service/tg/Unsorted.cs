using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.tg
{
    public class Unsorted
    {
        public String stand;
        public String currency;
        public string message;

        public Unsorted(string stand, string currency, string message)
        {
            this.stand = stand;
            this.currency = currency;
            this.message = message;
        }
    }
}
