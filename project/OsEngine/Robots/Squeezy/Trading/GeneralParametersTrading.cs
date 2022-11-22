using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class GeneralParametersTrading : GeneralParametersTester
    {
        private StrategyParameterDecimal triggerStartPercent;

        public GeneralParametersTrading(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, 
            StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterDecimal volumeSum, 
            StrategyParameterDecimal triggerStartPercent, StrategyParameterInt countBufferLogLine, StrategyParameterBool testSettings,
            StrategyParameterBool logEnabled, StrategyParameterDecimal maStrength, StrategyParameterBool paintGroupEnabled, StrategyParameterDecimal paintGroup,
            StrategyParameterBool paintSqueezyEnabled, StrategyParameterDecimal paintSqueezy)
        : base(maLenSlow, maCorridorHighSlow, maLenFast, volumePercent, volumeSum, countBufferLogLine, testSettings, logEnabled, maStrength, paintGroupEnabled, paintGroup, paintSqueezyEnabled, paintSqueezy)
        {
            this.triggerStartPercent = triggerStartPercent;
        }

        public decimal getTriggerStartPercent()
        {
            return triggerStartPercent.ValueDecimal;
        }

        public new string getAllSettings()
        {
            string settings = base.getAllSettings()
                + ", triggerStartPercent = " + getTriggerStartPercent();
            return settings;
        }
    }
}
