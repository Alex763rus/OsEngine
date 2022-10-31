using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;
using System;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class GroupParametersTrading : GroupParametersTester
    {

        private StrategyParameterDecimal takeProfitTriggerStart;
        private StrategyParameterDecimal stopLossTriggerStart;

        public GroupParametersTrading(GroupType groupType, StrategyParameterBool groupOn, StrategyParameterDecimal triggerCandleDiff, StrategyParameterDecimal takeProfitTriggerStart, StrategyParameterDecimal takeProfit, StrategyParameterDecimal stopLossTriggerStart, StrategyParameterDecimal stopLoss, StrategyParameterInt countBarForClose)
            :base(groupType, groupOn, triggerCandleDiff, takeProfit, stopLoss, countBarForClose)
         { 
            this.takeProfitTriggerStart = takeProfitTriggerStart;
            this.stopLossTriggerStart = stopLossTriggerStart;
        }

        public decimal getTakeProfitTriggerStart()
        {
            return takeProfitTriggerStart.ValueDecimal;
        }

        public decimal getStopLossTriggerStart()
        {
            return stopLossTriggerStart.ValueDecimal;
        }

        public new string getAllGroupParameters()
        {
            string settings = base.getAllGroupParameters()
                            + ", takeProfitTriggerStart = " + getTakeProfitTriggerStart()
                            + ", stopLossTriggerStart = " + getStopLossTriggerStart()
                            ;
            return settings;
        }

    }
}
