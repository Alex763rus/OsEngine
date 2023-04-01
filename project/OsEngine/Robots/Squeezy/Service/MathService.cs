using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service
{
    public class MathService
    {

        public static decimal getValueAddPercent(decimal value, decimal percent)
        {
            return value + (value * percent / 100);
        }
        public static decimal getValueSubtractPercent(decimal value, decimal percent)
        {
            return value - (value * percent / 100);
        }

        public static decimal round(decimal value, decimal step)
        {
            decimal poluStep = step / 2;
            decimal roundValue = 0;
            int countStep = (int)(value / step);
            if (value > (countStep* step + poluStep))
            {
                roundValue = (countStep + 1) * step;
            }
            else
            {
                roundValue = countStep * step;
            }
            return roundValue;
        }
    }

}
