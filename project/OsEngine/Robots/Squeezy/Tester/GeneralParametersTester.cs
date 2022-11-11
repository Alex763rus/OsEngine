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
        private StrategyParameterDecimal volumeSum;
        private StrategyParameterInt countBufferLogLine;
        private StrategyParameterBool testSettings;
        private StrategyParameterBool logEnabled;
        private StrategyParameterDecimal maStrength;

        public GeneralParametersTester(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast, StrategyParameterDecimal volumePercent, StrategyParameterDecimal volumeSum, StrategyParameterInt countBufferLogLine, StrategyParameterBool testSettings, StrategyParameterBool logEnabled, StrategyParameterDecimal maStrength)
        {
            this.maLenSlow = maLenSlow;
            this.maCorridorHighSlow = maCorridorHighSlow;
            this.maLenFast = maLenFast;
            this.volumePercent = volumePercent;
            this.volumeSum = volumeSum;
            this.countBufferLogLine = countBufferLogLine;
            this.testSettings = testSettings;
            this.logEnabled = logEnabled;
            this.maStrength = maStrength;
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

        public int getCountBufferLogLine()
        {
            return countBufferLogLine.ValueInt;
        }
        public decimal getMaStrength()
        {
            return maStrength.ValueDecimal;
        }

        public decimal getVolumeSum()
        {
            return volumeSum.ValueDecimal;
        }
        public string getAllSettings()
        {
            string settings = " Settings:"
                            + " maLenSlow = " + getMaLenSlow()
                            + ", maCorridorHighSlow = " + getMaCorridorHighSlow()
                            + ", maLenFast = " + getMaLenFast()
                            + ", volumePercent = " + getVolumePercent()
                            + ", volumeSum = " + getVolumeSum()
                            + ", countBufferLogLine = " + getCountBufferLogLine()
                            + ", testSettings = " + getTestSettings()
                            + ", logEnabled = " + getLogEnabled()
                            + ", maStrength = " + getMaStrength()
                            ;
            return settings;
        }
    }
}
