using OsEngine.Alerts;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.Robots.Squeezy.Service.statistic;
using OsEngine.Robots.Squeezy.Service.statistic.drawdown;
using OsEngine.Robots.Squeezy.Service.statistic.statistic2;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service
{
    public class StatisticService
    {
        private bool isEnabled;

        private StDrawdownService stDrawdownService;
        private StSlTpService stSlTpService;
        
        private string statisticProfitPath;

        public StatisticService(string filePathStatistic, string statisticProfitPath, bool isEnabled, decimal step)
        {
            this.isEnabled = isEnabled;
            this.statisticProfitPath = statisticProfitPath;
            stDrawdownService = new StDrawdownService(filePathStatistic);
            stSlTpService = new StSlTpService(statisticProfitPath, step);
        }

        public string getStatisticProfitPath()
        {
            return statisticProfitPath;
        }
        public void newSqueezyLogic(GroupType groupType, Side side, decimal lastCandleClose, decimal price, GroupParametersTester groupParameters)
        {
            if (!isEnabled)
            {
                return;
            }
            stDrawdownService.newSqueezyLogic(groupType, side, lastCandleClose, price);
        }
        public void rulerSqueezyLogic(GroupType groupType, Side side, decimal lastCandleClose, decimal price, GeneralParametersTester generalParameters)
        {
            stSlTpService.newSqueezyLogic(groupType, side, lastCandleClose, price, generalParameters.getRulerBarCount());
        }
        public void rulerCandleFinishedEventLogic(Candle lastCandle, decimal stepProfit, decimal stepLoss)
        {
            stSlTpService.candleFinishedEventLogic(lastCandle, stepProfit, stepLoss);
        }

        public void rulerNewTestLogic(string instrument, decimal stepSqueezy, decimal stepProfit, decimal stepLoss)
        {
            StringBuilder sb = new StringBuilder(instrument);
            sb.Append(";").Append(stepSqueezy)
                .Append(";").Append(stepProfit)
                .Append(";").Append(stepLoss);
            stSlTpService.startTestInit(sb.ToString());
        }

        public void candleFinishedEventLogic(GroupType groupTypeBuy, GroupType groupTypeSell, DealService dealService, Candle lastCandle)
        {
            if (!isEnabled)
            {
                return;
            }
            stDrawdownService.candleFinishedEventLogic(groupTypeBuy, groupTypeSell, dealService);
        }
 
        public void setIsEnabled(bool isEnabled)
        {
            this.isEnabled = isEnabled;
        }
    }
}
