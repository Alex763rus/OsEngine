using OsEngine.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy
{
    public interface Statisticable
    {
        public bool statisticEnabled();
        public string getFilePathStatistic();
    }
}
