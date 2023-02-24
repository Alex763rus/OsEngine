using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class GeneralParametersTrading : GeneralParametersTester
    {
        private StrategyParameterDecimal triggerStartPercent;
        private StrategyParameterBool clearJournal;

        public GeneralParametersTrading(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, 
            StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterDecimal volumeSum, StrategyParameterInt coeffMonkey,
            StrategyParameterDecimal triggerStartPercent, StrategyParameterInt countBufferLogLine, StrategyParameterBool testSettings,
            StrategyParameterBool logEnabled, StrategyParameterBool statisticEnabled, StrategyParameterDecimal maStrength, StrategyParameterBool paintGroupEnabled, StrategyParameterDecimal paintGroup,
            StrategyParameterBool paintSqueezyEnabled, StrategyParameterDecimal paintSqueezy)
        : base(maLenSlow, maCorridorHighSlow, maLenFast, volumePercent, volumeSum, coeffMonkey, countBufferLogLine, testSettings, logEnabled, statisticEnabled, maStrength, paintGroupEnabled, paintGroup, paintSqueezyEnabled, paintSqueezy)
        {
            this.triggerStartPercent = triggerStartPercent;
        }
        
        public void setClearJournal(StrategyParameterBool clearJournal)
        {
            this.clearJournal = clearJournal;
        }

        public bool getClearJournal()
        {
            return clearJournal.ValueBool;
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
