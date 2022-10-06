using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    public class GeneralParameters
    {
        private StrategyParameterInt maLenSlow;
        private StrategyParameterDecimal maCorridorHighSlow;
        private StrategyParameterInt maLenFast;
        private StrategyParameterDecimal volumePercent;
        private StrategyParameterInt countBarForClose;

        public GeneralParameters(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterInt countBarForClose)
        {
            this.maLenSlow = maLenSlow;
            this.maCorridorHighSlow = maCorridorHighSlow;
            this.maLenFast = maLenFast;
            this.volumePercent = volumePercent;
            this.countBarForClose = countBarForClose;
        }

        public int getMaLenSlow()
        {
            return maLenSlow.ValueInt;
        }

        public decimal getMaCorridorHighSlow()
        {
            return maCorridorHighSlow.ValueDecimal;
        }
        public int getMaLenFast()
        {
            return maLenFast.ValueInt;
        }

        public decimal getVolumePercent()
        {
            return volumePercent.ValueDecimal;
        }

        public int getCountBarForClose()
        {
            return countBarForClose.ValueInt;
        }
    }
}
