using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Trading
{
    public interface EventService
    {
        public void parametrsChangeByUserLogic();

        public void candleFinishedEventLogic(List<Candle> candles);
        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk);

        public void positionOpeningSuccesEventLogic(Position position);
        public void positionClosingSuccesEventLogic(Position position);
        public void positionOpeningFailEventLogic(Position position);

        public void testingEndEventLogic();
        public void testingStartEventLogic();
    }
}
