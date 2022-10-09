using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    public class GeneralParametersProm : GeneralParametersRuler
    {
        private StrategyParameterDecimal triggerStartPercent;

        public GeneralParametersProm(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterInt countBarForClose, StrategyParameterDecimal triggerStartPercent)
        : base(maLenSlow, maCorridorHighSlow, maLenFast, volumePercent, countBarForClose)
        {
            this.triggerStartPercent = triggerStartPercent;
        }

        public decimal getTriggerStartPercent()
        {
            return triggerStartPercent.ValueDecimal;
        }
    }
}
