using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.statistic
{
    public class StatisticResult
    {
        private decimal maxDrawdown;            //максимальная просадка
        private Position positionMaxDrawdown;   //позиция с максимальной просадкой

        public StatisticResult()
        {
            maxDrawdown = 0;
            positionMaxDrawdown = null;
        }
        public void setMaxDrawdown(decimal maxDrawdown)
        {
            this.maxDrawdown = maxDrawdown;
        }

        public void setPositionMaxDrawdown(Position positionMaxDrawdown)
        {
            this.positionMaxDrawdown = positionMaxDrawdown;
        }


        public decimal getMaxDrawdown()
        {
            return this.maxDrawdown;
        }

        public Position getPositionMaxDrawdown()
        {
            return positionMaxDrawdown;
        }
    }
}
