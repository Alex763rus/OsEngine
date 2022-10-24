using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Tester
{
    public class GeneralParametersTester
    {
        private StrategyParameterInt maLenSlow;
        private StrategyParameterDecimal maCorridorHighSlow;
        private StrategyParameterInt maLenFast;
        private StrategyParameterDecimal volumePercent;
        private StrategyParameterInt countBarForClose;
        private StrategyParameterInt countBufferLogLine;
        private StrategyParameterBool testSettings;
        private StrategyParameterBool logEnabled;

        public GeneralParametersTester(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterInt countBarForClose, StrategyParameterInt countBufferLogLine, StrategyParameterBool testSettings, StrategyParameterBool logEnabled)
        {
            this.maLenSlow = maLenSlow;
            this.maCorridorHighSlow = maCorridorHighSlow;
            this.maLenFast = maLenFast;
            this.volumePercent = volumePercent;
            this.countBarForClose = countBarForClose;
            this.countBufferLogLine = countBufferLogLine;
            this.testSettings = testSettings;
            this.logEnabled = logEnabled;
        }

        public bool getTestSettings()
        {
            return testSettings.ValueBool;
        }

        public bool getLogEnabled()
        {
            return logEnabled.ValueBool;
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

        public int getCountBufferLogLine()
        {
            return countBufferLogLine.ValueInt;
        }
        
        public string getAllSettings()
        {
            string settings = " Settings:"
                            + " maLenSlow = " + getMaLenSlow()
                            + ", maCorridorHighSlow = " + getMaCorridorHighSlow()
                            + ", maLenFast = " + getMaLenFast()
                            + ", volumePercent = " + getVolumePercent()
                            + ", countBarForClose = " + getCountBarForClose()
                            + ", countBufferLogLine = " + getCountBufferLogLine()
                            + ", getTestSettings = " + getTestSettings()
                            + ", getLogEnabled = " + getLogEnabled()
                            ;
            return settings;
        }
    }
}
