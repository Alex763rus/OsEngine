using OsEngine.Entity;
using OsEngine.Robots.SqueezyBot.rulerVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot.promVersion
{
    public class GeneralParametersProm : GeneralParametersRuler
    {
        private StrategyParameterDecimal triggerStartPercent;

        public GeneralParametersProm(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterInt countBarForClose, StrategyParameterDecimal triggerStartPercent, StrategyParameterInt countBufferLogLine)
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
                + " triggerStartPercent = " + getTriggerStartPercent();
            return settings;
        }
    }
}
