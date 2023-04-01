using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Service.statistic.statistic2;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.statistic.drawdown
{
    public class StSlTpService
    {
        private string filePathStatistic;
        private decimal squeezyStep;

        private List <SlTpInfo> slTpInfoList;
        private List<SlTpInfo> slTpInfoListOld;

        public StSlTpService(string filePathStatistic, decimal squeezyStep) {
            this.filePathStatistic = filePathStatistic;
            this.squeezyStep = squeezyStep;
            slTpInfoList = new List<SlTpInfo> ();
            slTpInfoListOld = new List<SlTpInfo>();
        }

        public void startTestInit(string message)
        {
            FileService.saveMessageInFile(filePathStatistic, message, true);
        }
        public void newSqueezyLogic(GroupType groupType, Side side, decimal lastCandleClose, decimal price, int candleCounter)
        {
            decimal percent = 0;
            if (side == Side.Sell)
            {
                percent = 100.0m - lastCandleClose / price * 100.0m;
            }
            else if (side == Side.Buy)
            {
                percent = 100.0m - price / lastCandleClose * 100.0m;
            }
            percent = MathService.round(percent, squeezyStep);
            SlTpInfo slTpInfo = new SlTpInfo(groupType, side, percent, price, candleCounter);
            slTpInfoList.Add(slTpInfo);
        }

        public void candleFinishedEventLogic(Candle lastCandle, decimal stepProfit, decimal stepLoss)
        {
            for(int i = 0; i < slTpInfoList.Count; ++i)
            {
                if (slTpInfoList[i].getIsFinish())
                {
                    slTpInfoListOld.Add(slTpInfoList[i]);
                    FileService.saveMessageInFile(filePathStatistic, slTpInfoList[i].getInfo(), true);
                    slTpInfoList.RemoveAt(i);
                    --i;
                }
                else
                {
                    slTpInfoList[i].newCandleLogic(lastCandle, stepProfit, stepLoss);
                }
            }
        }

    }
}
