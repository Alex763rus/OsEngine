using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class GeneralParametersTrading : GeneralParametersTester
    {
        private StrategyParameterDecimal triggerStartPercent;
        private StrategyParameterDecimal volumeSum;
        
        public GeneralParametersTrading(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterDecimal volumeSum, StrategyParameterInt countBarForClose, StrategyParameterDecimal triggerStartPercent, StrategyParameterInt countBufferLogLine, StrategyParameterBool testSettings, StrategyParameterBool logEnabled)
        : base(maLenSlow, maCorridorHighSlow, maLenFast, volumePercent, countBarForClose, countBufferLogLine, testSettings, logEnabled)
        {
            this.triggerStartPercent = triggerStartPercent;
            this.volumeSum = volumeSum;
        }

        public decimal getTriggerStartPercent()
        {
            return triggerStartPercent.ValueDecimal;
        }
        public decimal getVolumeSum()
        {
            return volumeSum.ValueDecimal;
        }
        public new string getAllSettings()
        {
            string settings = base.getAllSettings()
                + ", triggerStartPercent = " + getTriggerStartPercent()
                + ", volumeSum = " + getVolumeSum();
            return settings;
        }
    }
}
