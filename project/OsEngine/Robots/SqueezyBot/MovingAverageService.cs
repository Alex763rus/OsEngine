﻿using OsEngine.Indicators;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OsEngine.Robots.SqueezyBot
{

    public class MovingAverageService
    {
        private GeneralParameters generalParameters;

        private Aindicator smaSlow;
        private Aindicator smaFast;

        public MovingAverageService(BotTabSimple tab, GeneralParameters generalParameters)
        {
            this.generalParameters = generalParameters;

            smaSlow = IndicatorsFactory.CreateIndicatorByName("Sma", "smaSlow Bollinger", false);
            smaSlow = (Aindicator)tab.CreateCandleIndicator(smaSlow, "Prime");
            smaSlow.ParametersDigit[0].Value = generalParameters.getMaLenSlow();
            smaSlow.Save();

            smaFast = IndicatorsFactory.CreateIndicatorByName("Sma", "smaFast Bollinger", false);
            smaFast = (Aindicator)tab.CreateCandleIndicator(smaFast, "Prime");
            smaFast.ParametersDigit[0].Value = generalParameters.getMaLenFast();
            smaFast.Save();
            
        }

        public decimal getMaLastValueSlow()
        {
            return smaSlow.DataSeries[0].Last;
        }

        public decimal getMaLastValueFast()
        {
            return smaFast.DataSeries[0].Last;
        }

    }
}
