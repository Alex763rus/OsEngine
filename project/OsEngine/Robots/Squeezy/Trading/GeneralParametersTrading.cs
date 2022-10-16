using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Ruler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class GeneralParametersTrading : GeneralParametersRuler
    {
        private StrategyParameterDecimal triggerStartPercent;

        public GeneralParametersTrading(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterInt countBarForClose, StrategyParameterDecimal triggerStartPercent, StrategyParameterInt countBufferLogLine)
        : base(maLenSlow, maCorridorHighSlow, maLenFast, volumePercent, countBarForClose, countBufferLogLine)
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
