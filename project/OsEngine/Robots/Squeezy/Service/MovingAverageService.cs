using OsEngine.Indicators;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Tester;

namespace OsEngine.Robots.SqueezyBot
{

    public class MovingAverageService
    {
        private GeneralParametersTester generalParametersTester;
        private Aindicator smaSlow;
        private Aindicator smaFast;

        public MovingAverageService(BotTabSimple tab, GeneralParametersTester generalParametersTester)
        {
            this.generalParametersTester = generalParametersTester;

            smaSlow = IndicatorsFactory.CreateIndicatorByName("Sma", "smaSlow Bollinger", false);
            smaSlow = (Aindicator)tab.CreateCandleIndicator(smaSlow, "Prime");
            smaSlow.ParametersDigit[0].Value = generalParametersTester.getMaLenSlow();
            smaSlow.Save();

            smaFast = IndicatorsFactory.CreateIndicatorByName("Sma", "smaFast Bollinger", false);
            smaFast = (Aindicator)tab.CreateCandleIndicator(smaFast, "Prime");
            smaFast.ParametersDigit[0].Value = generalParametersTester.getMaLenFast();
            smaFast.Save();
        }

        public void updateMaLen()
        {
            updateMaLen(smaSlow, generalParametersTester.getMaLenSlow());
            updateMaLen(smaFast, generalParametersTester.getMaLenFast());
        }

        public decimal getMaLastValueSlow()
        {
            return smaSlow.DataSeries[0].Last;
        }

        public decimal getMaLastValueFast()
        {
            return smaFast.DataSeries[0].Last;
        }

        private void updateMaLen(Aindicator sma, int maLen)
        {
            sma.ParametersDigit[0].Value = maLen;
            sma.Save();
            sma.Reload();
        }
    }
}
