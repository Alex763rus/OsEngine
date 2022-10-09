using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    public class EventServiceProm
    {
        private GeneralParametersRuler generalParameters;
        private GroupParametersService groupParametersService;

        private MovingAverageService movingAverageService;
        private DealService dealService;
        private CountBarService countBarService;
        public EventServiceProm(BotTabSimple tab, GeneralParametersRuler generalParameters, GroupParametersService groupParametersService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;

            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters);
            countBarService = new CountBarService();
        }

        public void finishedEventLogic(List<Candle> candles)
        {
            //throw new NotImplementedException();
        }

        public void newTickEventLogic(Trade trade)
        {
            //throw new NotImplementedException();
        }

        public void positionClosingSuccesEventLogic(Position positiobotn)
        {
           // throw new NotImplementedException();
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
            //throw new NotImplementedException();
        }

        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            //throw new NotImplementedException();
        }
    }
}
