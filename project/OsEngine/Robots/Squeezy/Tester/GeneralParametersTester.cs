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
        private StrategyParameterInt coeffMonkey;
        private StrategyParameterInt countBufferLogLine;
        private StrategyParameterBool logEnabled;
        private StrategyParameterBool statisticEnabled;
        private StrategyParameterDecimal maStrength;
        private StrategyParameterBool paintGroupEnabled;
        private StrategyParameterDecimal paintGroup;
        private StrategyParameterBool paintSqueezyEnabled;
        private StrategyParameterDecimal paintSqueezy;

        private StrategyParameterBool onlyRuler;            //режим работы как рулетка
        private StrategyParameterInt rulerBarCount;         //количество анализируемых баров в режиме рулетка
        private StrategyParameterDecimal rulerStepSqueezy;  //шаг сквиза формирования эксельного файла статистики
        private StrategyParameterDecimal rulerStepProfit;   //шаг профита формирования эксельного файла статистики
        private StrategyParameterDecimal rulerStepLoss;     //шаг лосса формирования эксельного файла статистики

        private StrategyParameterBool tgAlertEnabled;
        private StrategyParameterString stand;

        public GeneralParametersTester(StrategyParameterInt maLenSlow, StrategyParameterDecimal maCorridorHighSlow, StrategyParameterInt maLenFast
            , StrategyParameterDecimal volumePercent, StrategyParameterDecimal volumeSum, StrategyParameterInt coeffMonkey, StrategyParameterInt countBufferLogLine
            , StrategyParameterBool logEnabled, StrategyParameterBool statisticEnabled, StrategyParameterDecimal maStrength, StrategyParameterBool paintGroupEnabled, StrategyParameterDecimal paintGroup
            , StrategyParameterBool paintSqueezyEnabled, StrategyParameterDecimal paintSqueezy)
        {
            this.maLenSlow = maLenSlow;
            this.maCorridorHighSlow = maCorridorHighSlow;
            this.maLenFast = maLenFast;
            this.volumePercent = volumePercent;
            this.volumeSum = volumeSum;
            this.coeffMonkey = coeffMonkey;
            this.countBufferLogLine = countBufferLogLine;
            this.logEnabled = logEnabled;
            this.statisticEnabled = statisticEnabled;
            this.maStrength = maStrength;
            this.paintGroupEnabled = paintGroupEnabled;
            this.paintGroup = paintGroup;
            this.paintSqueezyEnabled = paintSqueezyEnabled;
            this.paintSqueezy = paintSqueezy;
        }
        public void setStand(StrategyParameterString stand)
        {
            this.stand = stand;
        }

        public string getStand()
        {
            return stand.ValueString;
        }
        public void setTgAlertEnabled(StrategyParameterBool tgAlertEnabled)
        {
            this.tgAlertEnabled = tgAlertEnabled;
        }

        public bool getTgAlertEnabled()
        {
            return tgAlertEnabled.ValueBool;
        }
        public decimal getRulerStepSqueezy()
        {
            return rulerStepSqueezy.ValueDecimal;
        }
        public decimal getRulerStepProfit()
        {
            return rulerStepProfit.ValueDecimal;
        }
        public decimal getRulerStepLoss()
        {
            return rulerStepLoss.ValueDecimal;
        }

        public void setRulerStepSqueezy(StrategyParameterDecimal rulerStepSqueezy)
        {
            this.rulerStepSqueezy = rulerStepSqueezy;
        }
        public void setRulerStepProfit(StrategyParameterDecimal rulerStepProfit)
        {
            this.rulerStepProfit = rulerStepProfit;
        }
        public void setRulerStepLoss(StrategyParameterDecimal rulerStepLoss)
        {
            this.rulerStepLoss = rulerStepLoss;
        }
        public int getRulerBarCount()
        {
            return rulerBarCount.ValueInt;
        }
        public void setRulerBarCount(StrategyParameterInt rulerBarCount)
        {
            this.rulerBarCount = rulerBarCount;
        }
        public bool getOnlyRuler()
        {
            return onlyRuler.ValueBool;
        }
        public void setOnlyRuler(StrategyParameterBool onlyRuler)
        {
            this.onlyRuler = onlyRuler;
        }
        public int getCoeffMonkey()
        {
            return coeffMonkey.ValueInt;
        }
        public decimal getPaintGroup()
        {
            return paintGroup.ValueDecimal;
        }
        public decimal getPaintSqueezy()
        {
            return paintSqueezy.ValueDecimal;
        }
        public bool getPaintSqueezyEnabled()
        {
            return paintSqueezyEnabled.ValueBool;
        }
        public bool getPaintGroupEnabled()
        {
            return paintGroupEnabled.ValueBool;
        }

        public bool getLogEnabled()
        {
            return logEnabled.ValueBool;
        }

        public bool getStatisticEnabled()
        {
            return statisticEnabled.ValueBool;
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
            StringBuilder str = new StringBuilder(" Settings:");
            str.Append(" maLenSlow = ").Append(getMaLenSlow());
            str.Append(", maCorridorHighSlow = ").Append(getMaCorridorHighSlow());
            str.Append(", maLenFast = ").Append(getMaLenFast());
            str.Append(", volumePercent = ").Append(getVolumePercent());
            str.Append(", volumeSum = ").Append(getVolumeSum());
            str.Append(", coeffMonkey = ").Append(getCoeffMonkey());
            str.Append(", countBufferLogLine = ").Append(getCountBufferLogLine());
            str.Append(", logEnabled = ").Append(getLogEnabled());
            str.Append(", maStrength = ").Append(getMaStrength());
            str.Append(", PaintGroupEnabled = ").Append(getPaintGroupEnabled());
            str.Append(", paintGroup = ").Append(getPaintGroup());                           ;
            str.Append(", PaintSqueezyEnabled = ").Append(getPaintSqueezyEnabled());                           ;
            str.Append(", PaintSqueezy = ").Append(getPaintSqueezy());                           ;
            return str.ToString();
        }
    }
}
