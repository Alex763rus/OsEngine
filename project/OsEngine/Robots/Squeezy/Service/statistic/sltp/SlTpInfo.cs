using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.statistic.statistic2
{
    public class SlTpInfo
    {
        private GroupType groupType;
        private Side side;                      //направление
        private decimal percent;                //процент сквиза
        private decimal entryPrice;             //цена после сквиза

        private decimal maxLoss;                //максимальная величина убыточного отклонения за n баров после сквиза
        private decimal maxProfit;              //максимальный величина профитного отклонения на n баров после сквиза
        private decimal percentLoss;            //максимальный % убыточного отклонения за n баров после сквиза
        private decimal percentProfit;          //максимальный % профитного отклонения на n баров после сквиза

        private int candleCounter;
        private bool isFinish;

        private decimal step;

        public SlTpInfo(GroupType groupType, Side side, decimal percent, decimal entryPrice, int candleCounter)
        {
            this.groupType = groupType;
            this.side = side;
            this.percent = percent;
            this.entryPrice = entryPrice;
            this.candleCounter = candleCounter;

            maxLoss = entryPrice;
            maxProfit = entryPrice;
            percentLoss = 0;
            percentProfit = 0;
            isFinish = false;
            step = 0.5m;
        }

        public void newCandleLogic(Candle lastCandle, decimal stepProfit, decimal stepLoss)
        {
            if (isFinish)
            {
                return;
            }
            if (candleCounter == 0)
            {
                decimal onePecent = entryPrice / 100.0m;
                percentProfit = MathService.round(Math.Abs(maxProfit / onePecent - 100.0m), stepProfit);
                percentLoss = MathService.round(Math.Abs(maxLoss / onePecent - 100.0m), stepLoss);
                isFinish = true;
                return;
            }
            --candleCounter;
            if (side == Side.Buy)
            {
                if (lastCandle.High > maxProfit)
                {
                    maxProfit = lastCandle.High;
                }
                if (lastCandle.Low < maxLoss)
                {
                    maxLoss = lastCandle.Low;
                }
            }
            if (side == Side.Sell)
            {
                if (lastCandle.Low < maxProfit)
                {
                    maxProfit = lastCandle.Low;
                }
                if (lastCandle.High > maxLoss)
                {
                    maxLoss = lastCandle.High;
                }
            }
        }

        public bool getIsFinish()
        {
            return isFinish;
        }

        public string getInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(groupType)
                .Append(";").Append(side)
                .Append(";").Append(percent)
                .Append(";").Append(percentProfit)
                .Append(";").Append(percentLoss);
            return sb.ToString();
        }
    }
}
