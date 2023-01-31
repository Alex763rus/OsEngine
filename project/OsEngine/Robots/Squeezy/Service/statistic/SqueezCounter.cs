using OsEngine.Market.Servers.Transaq.TransaqEntity;
using ru.micexrts.cgate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.statistic
{
    public class SqueezCounter : IComparable
    {
        private decimal percent;   //процент
        private int countSqueezy; //количество сквизов

        public SqueezCounter(decimal percent)
        {
            this.percent = percent;
            countSqueezy = 1;
        }

        public decimal getPercent()
        {
            return percent;
        }
        public int getCountSqueezy()
        {
            return countSqueezy;
        }

        public void setPercent(decimal percent) { 
            this.percent = percent;
        }

        public void count()
        {
            ++countSqueezy;
        }

        
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;
            return ((SqueezCounter)obj).getPercent() == this.getPercent();
        }

        public override int GetHashCode()
        {
            return 118159978 + percent.GetHashCode();
        }

        public int Compare(SqueezCounter x, SqueezCounter y)
        {
            if (x is null || y is null)
                throw new ArgumentException("Некорректное значение параметра");
            return (int)(x.getPercent() - y.getPercent());
        }

        public int CompareTo(object other)
        {
            SqueezCounter sq = (SqueezCounter)other;
            return (int)(this.getPercent() - sq.getPercent());
        }
    }
}
