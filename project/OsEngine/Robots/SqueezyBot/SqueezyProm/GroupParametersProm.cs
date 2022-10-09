using OsEngine.Entity;

namespace OsEngine.Robots.SqueezyBot
{
    public class GroupParametersProm : GroupParametersRuler
    {

        private StrategyParameterDecimal takeProfitTriggerStart;
        private StrategyParameterDecimal stopLossTriggerStart;

        public GroupParametersProm(GroupType groupType, StrategyParameterBool groupOn, StrategyParameterDecimal triggerCandleDiff, StrategyParameterDecimal takeProfitTriggerStart, StrategyParameterDecimal takeProfit, StrategyParameterDecimal stopLossTriggerStart, StrategyParameterDecimal stopLoss)
            :base(groupType, groupOn, triggerCandleDiff, takeProfit, stopLoss)
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
    }
}
