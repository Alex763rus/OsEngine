using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class GeneralParametersTrading : GeneralParametersTester
    {
        private StrategyParameterDecimal triggerStartPercent;
        private StrategyParameterDecimal volumeSum;
        private StrategyParameterDecimal maStrength;
        
        public GeneralParametersTrading(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterDecimal volumeSum, StrategyParameterDecimal triggerStartPercent, StrategyParameterInt countBufferLogLine, StrategyParameterBool testSettings, StrategyParameterBool logEnabled, StrategyParameterDecimal maStrength)
        : base(maLenSlow, maCorridorHighSlow, maLenFast, volumePercent, countBufferLogLine, testSettings, logEnabled)
        {
            this.triggerStartPercent = triggerStartPercent;
            this.volumeSum = volumeSum;
            this.maStrength = maStrength;
        }

        public decimal getTriggerStartPercent()
        {
            return triggerStartPercent.ValueDecimal;
        }
        public decimal getVolumeSum()
        {
            return volumeSum.ValueDecimal;
        }
        public decimal getMaStrength()
        {
            return maStrength.ValueDecimal;
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
