using Kraken.WebSockets;
using OsEngine.Alerts;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.Market.Servers.GateIo.Futures.Response;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using ru.micexrts.cgate.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Position = OsEngine.Entity.Position;
using Side = OsEngine.Entity.Side;


namespace OsEngine.Robots.Squeezy.Trading
{
    public class EventServiceDevelop:EventService
    {
        private GeneralParametersTrading generalParameters;
        private GroupParametersTradingService groupParametersService;

        private DealService dealService;
        private LogService logService;
        private VolumeSumService volumeSumService;
        private PaintService paintService;

        private Candle lastCandle;
        private bool isStart;                       //Признак начала работы
        private PositionStateType lastState;

        private decimal lastBestAsc;          //последняя известная лучшая цена продажи
        private decimal lastBestBid;          //последняя известная лучшая цена покупки

        private BotTabSimple tab;
        private int counter = 0;

        public EventServiceDevelop(BotTabSimple tab, GeneralParametersTrading generalParameters, GroupParametersTradingService groupParametersService, LogService logService)
        {
            this.generalParameters = generalParameters;
            this.groupParametersService = groupParametersService;
            this.logService = logService;
            dealService = new DealService(tab, generalParameters, logService);
            isStart = true;
            paintService = new PaintService(tab);
            this.tab = tab;
        }
                
        public void candleFinishedEventLogic(List<Candle> candles)
        {
            lastCandle = candles[candles.Count - 1];
            printEndBarInfo();
        }


        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            lastBestBid = bestBid;
            lastBestAsc = bestAsk;

            if (isStart)
            {
                volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);
                paintService.deleteAllChartElement();
                counter = 0;
                isStart = false;
            }
            if(lastCandle == null)
            {
                return;
            }
            ++counter;
            if (counter == 10000)
            {
                sendLogSystemLocal("!!!!!!!!!!!!!!!!*Идем все закрывать:");
                tab.CloseAllAtMarket();
            }
            if (counter != 100)
            {
                return;
            }
            decimal percentRedLine = 0.8m;
            //decimal percentLimit = 2.0m;
            decimal priceRedLine = Math.Round(lastCandle.Close - (lastCandle.Close * percentRedLine / 100), 4);
            decimal priceLimit = priceRedLine;
            
            paintService.paintSqueezy(lastCandle.TimeStart, dealService.getTimeFrame(), priceLimit, SqueezyType.Buy);
            paintService.paintSqueezy(lastCandle.TimeStart, dealService.getTimeFrame(), priceRedLine, SqueezyType.Sell);

            sendLogSystemLocal("-> BuyAtLimit. lastCandle.TimeStart = " + lastCandle.TimeStart 
                + ", lastCandle.Close " + lastCandle.Close 
                + ", priceRedLine:" + priceRedLine
                + ", priceLimit:" + priceLimit
                );
            tab.BuyAtStop(10, priceLimit, priceRedLine, StopActivateType.LowerOrEqyal, 20);

            positionOpenerToStop = tab.PositionOpenerToStopsAll[0];
            int k = 0;


        }
        private PositionOpenerToStopLimit positionOpenerToStop;
        public void parametrsChangeByUserLogic()
        {
            counter = 0;
            paintService.deleteAllChartElement();
            logService.setup(generalParameters.getLogEnabled(), generalParameters.getCountBufferLogLine());
        }

        private void printEndBarInfo()
        {
            StringBuilder currentInfo = new StringBuilder();
            currentInfo.Append("******> Закрыт бар.:")
                .Append(" Позиций:").Append(dealService.getAllPosition().Count)
                ;
            sendLogSystemLocal(currentInfo.ToString());
        }

        private void sendLogSystemLocal(string text, Position position = null, DealSupport dealSupport = null, int level = 0)
        {
            StringBuilder sb = new StringBuilder();
            string positionInfo = "";
            string groupType = "";
            ProcessState processState = ProcessState.FREE;
            if (position != null)
            {
                sb.Append(LogService.getPositionNumber(position)).Append(" ");
                positionInfo = LogService.getPositionInfo(position);
            }
            if (dealSupport != null)
            {
                groupType = dealSupport.getGroupType();
            }
            if (level == 0)
            {
                level = (int)processState;
            }
            sb.Append(processState).Append(" ");
            sb.Append(groupType).Append(" ");
            sb.Append("DEBUG! ");
            sb.Append(text);
            sb.Append(positionInfo);
            sb.Append(" lastBestBid = ").Append(lastBestBid);
            sb.Append(" lastBestAsc = ").Append(lastBestAsc);
            logService.sendLogSystem(sb.ToString(), level);
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
            //throw new NotImplementedException();
        }

        public void positionClosingSuccesEventLogic(Position position)
        {
            //throw new NotImplementedException();
        }

        public void positionOpeningFailEventLogic(Position position)
        {
            //throw new NotImplementedException();
        }

        public void testingEndEventLogic()
        {
            //throw new NotImplementedException();
        }

        public void testingStartEventLogic()
        {
            //throw new NotImplementedException();
        }
    }
}
