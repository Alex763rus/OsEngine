using OsEngine.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.statistic
{
    public class StatisticResult
    {
        private decimal maxDrawdown;                //максимальная просадка
        private Position positionMaxDrawdown;       //позиция с максимальной просадкой
        private ArrayList counterSqueezyList;       //лист со счетчиками сквизов
        public StatisticResult()
        {
            maxDrawdown = 0;
            positionMaxDrawdown = null;
            counterSqueezyList = new ArrayList();
        }
        public void setMaxDrawdown(decimal maxDrawdown)
        {
            this.maxDrawdown = maxDrawdown;
        }

        public string getCounterSqueezyList()
        {
            StringBuilder sb = new StringBuilder();
            counterSqueezyList.Sort();
            if(counterSqueezyList.Count == 0)
            {
                sb.Append("[no squeez]");
            }
            foreach (SqueezCounter squeezCounter in counterSqueezyList)
            {
                sb.Append(squeezCounter.getPercent()).Append("% - ")
                    .Append(squeezCounter.getCountSqueezy()).Append(" шт, ");
            }
            return sb.ToString();
        }

        public void countSqueezy(SqueezCounter squeezCounter)
        {
            int index = counterSqueezyList.IndexOf(squeezCounter);
            if(index == -1)
            {
                counterSqueezyList.Add(squeezCounter);
            }
            else
            {
                ((SqueezCounter)counterSqueezyList[index]).count();
            }
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
