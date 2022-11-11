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
        private StrategyParameterInt countBarForClose;
        public GroupParametersTester(GroupType groupType, StrategyParameterBool groupOn, StrategyParameterDecimal triggerCandleDiff, StrategyParameterDecimal takeProfit, StrategyParameterDecimal stopLoss, StrategyParameterInt countBarForClose)
        {
            this.groupType = groupType;
            this.groupOn = groupOn;
            this.triggerCandleDiff = triggerCandleDiff;
            this.takeProfit = takeProfit;
            this.stopLoss = stopLoss;
            this.countBarForClose = countBarForClose;
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

        public int getCountBarForClose()
        {
            return countBarForClose.ValueInt;
        }

        public string getAllGroupParameters()
        {
            string settings = " GroupSettings:"
                            + ", groupType = " + getGroupType()
                            + ", groupOn = " + getGroupOn()
                            + ", triggerCandleDiff = " + getTriggerCandleDiff()
                            + ", takeProfit = " + getTakeProfit()
                            + ", stopLoss = " + getStopLoss()
                            + ", countBarForClose = " + getCountBarForClose()
                            ;
            return settings;
        }

        public override bool Equals(object obj)
        {
            return obj is GroupParametersTester tester &&
                   groupType.ToString().Equals(tester.groupType.ToString());
        }
    }

    public enum GroupType
    {
        UpBuy,
        UpSell,
        DownBuy,
        DownSell,
        FlatBuy,
        FlatSell,
        TestTest
    }

    public enum DirectionType
    {
        Up,
        Down,
        Flat,
        None,
        Test
    }

    public enum TrendType
    {
        Long,  Short
    }
}
