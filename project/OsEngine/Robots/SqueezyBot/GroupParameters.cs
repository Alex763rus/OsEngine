using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    public class GroupParameters
    {
        private GroupType groupType;
        private StrategyParameterBool groupOn;
        private StrategyParameterDecimal triggerCandleDiff;
        private StrategyParameterDecimal stopLoss;
        private StrategyParameterDecimal takeProfit;

        public GroupParameters(GroupType groupType, StrategyParameterBool groupOn, StrategyParameterDecimal triggerCandleDiff, StrategyParameterDecimal takeProfit, StrategyParameterDecimal stopLoss)
        {
            this.groupType = groupType;
            this.groupOn = groupOn;
            this.triggerCandleDiff = triggerCandleDiff;
            this.takeProfit = takeProfit;
            this.stopLoss = stopLoss;
        }

        public GroupType getGroupType()
        {
            return groupType;
        }

        public bool getGroupOn()
        {
            return groupOn.ValueBool;
        }

        public decimal getTriggerCandleDiff()
        {
            return triggerCandleDiff.ValueDecimal;
        }

        public decimal getStopLoss()
        {
            return stopLoss.ValueDecimal;
        }

        public decimal getTakeProfit()
        {
            return takeProfit.ValueDecimal;
        }

    }

    public enum GroupType
    {
        UpLong,
        UpShort,
        DownLong,
        DownShort,
    }

    public enum TrendType
    {
        Long,  Short
    }
}
