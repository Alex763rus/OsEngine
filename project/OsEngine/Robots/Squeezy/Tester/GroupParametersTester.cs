using OsEngine.Entity;

namespace OsEngine.Robots.Squeezy.Tester
{
    public class GroupParametersTester
    {
        private GroupType groupType;
        private StrategyParameterBool groupOn;
        private StrategyParameterDecimal triggerCandleDiff;
        private StrategyParameterDecimal takeProfit;
        private StrategyParameterDecimal stopLoss;

        public GroupParametersTester(GroupType groupType, StrategyParameterBool groupOn, StrategyParameterDecimal triggerCandleDiff, StrategyParameterDecimal takeProfit, StrategyParameterDecimal stopLoss)
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

        public string getAllGroupParameters()
        {
            string settings = " GroupSettings:"
                            + ", groupType = " + getGroupType()
                            + ", groupOn = " + getGroupOn()
                            + ", triggerCandleDiff = " + getTriggerCandleDiff()
                            + ", takeProfit = " + getTakeProfit()
                            + ", stopLoss = " + getStopLoss()
                            ;
            return settings;
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
