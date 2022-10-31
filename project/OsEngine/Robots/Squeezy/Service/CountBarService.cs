using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    internal class CountBarService
    {
        private int counterBarSell;
        private int counterBarBuy;

        private int limitBarSell;
        private int limitBarBuy;

        public CountBarService()
        {
            resetCountBarSell();
            resetCountBarBuy();
        }
        public void resetCountBarSell()
        {
            counterBarSell = 0;
        }

        public void resetCountBarBuy()
        {
            counterBarBuy = 0;
        }

        public void addCounterBarSell()
        {
            counterBarSell = counterBarSell + 1;
        }
        public void addCounterBarBuy()
        {
            counterBarBuy = counterBarBuy + 1;
        }
        public int getCounterBarSell()
        {
            return counterBarSell;
        }

        public int getCounterBarBuy()
        {
            return counterBarBuy;
        }


        public void setLimitBarSell(int limitBarSell)
        {
            this.limitBarSell = limitBarSell;
        }
        public void setLimitBarBuy(int limitBarBuy)
        {
            this.limitBarBuy = limitBarBuy;
        }
        public int getLimitBarSell()
        {
            return limitBarSell;
        }

        public int getLimitBarBuy()
        {
            return limitBarBuy;
        }

    }
}
