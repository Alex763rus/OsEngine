using OsEngine.Entity;
using OsEngine.Robots.SqueezyBot.rulerVersion;

namespace OsEngine.Robots.SqueezyBot.promVersion
{
    public class GroupParametersProm : GroupParametersRuler
    {

        private StrategyParameterDecimal takeProfitTriggerStart;
        private StrategyParameterDecimal stopLossTriggerStart;
        //не является параметром:
        private GroupStatus groupStatus;
        public GroupParametersProm(GroupType groupType, StrategyParameterBool groupOn, StrategyParameterDecimal triggerCandleDiff, StrategyParameterDecimal takeProfitTriggerStart, StrategyParameterDecimal takeProfit, StrategyParameterDecimal stopLossTriggerStart, StrategyParameterDecimal stopLoss)
            :base(groupType, groupOn, triggerCandleDiff, takeProfit, stopLoss)
         { 
            this.takeProfitTriggerStart = takeProfitTriggerStart;
            this.stopLossTriggerStart = stopLossTriggerStart;
            setGroupStatus(GroupStatus.WAIT_TRIGGER_START);
        }

        public decimal getTakeProfitTriggerStart()
        {
            return takeProfitTriggerStart.ValueDecimal;
        }

        public decimal getStopLossTriggerStart()
        {
            return stopLossTriggerStart.ValueDecimal;
        }

        public void setGroupStatus(GroupStatus groupStatus)
        {
            this.groupStatus = groupStatus;
        }

        public GroupStatus getGroupStatus()
        {
            return groupStatus;
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
