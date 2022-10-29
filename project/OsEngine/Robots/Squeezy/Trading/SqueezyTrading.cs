using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class SqueezyTrading : BotPanel, Loggable
    {
        public static string BOT_NAME = "SqueezyTradingBot";
        public static string VERSION = "0.0.1";
        private const string TAB_SERVICE_CONTROL_NAME = "Service";

        public static int separateCounter = 0;
        private EventServiceTrading eventServiceTrading;
        private GeneralParametersTrading generalParametersTrading;
        private GroupParametersTradingService groupParametersTradingService;
        private BotTabSimple tab;
        private static LogService logService;

        public SqueezyTrading(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];

            generalParametersTrading = new GeneralParametersTrading(
                          CreateParameter("MovingAverage длина slow", 150, 0, 150, 5)
                        , CreateParameter("%MovingAverage высота коридора slow", 0.4m, 0.0m, 0.8m, 0.4m)
                        , CreateParameter("MovingAverage длина fast", 50, 0, 50, 25)
                        , CreateParameter("% депозита для сделки ОТКЛЮЧЕН", 10.0m, 10.0m, 50.0m, 5.0m)
                        , CreateParameter("Сумма для открытия", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Количество баров до выхода", 2, 0, 30, 1)
                        , CreateParameter("%Триггер старта", 1m, 1m, 1m, 1m)
                        , CreateParameter("Количество строк лога в буфере", 50, 0, 50, 1, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Тестовые параметры", true, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Логгирование", true, TAB_SERVICE_CONTROL_NAME)
                        );
            addSeparateParameter();
            addSeparateParameter();

            GroupParametersTrading upLong = new GroupParametersTrading(
                          GroupType.UpLong
                        , CreateParameter("Включить UpLong торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp UpLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit UpLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl UpLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss UpLong", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersTrading upShort = new GroupParametersTrading(
                          GroupType.UpShort
                        , CreateParameter("Включить UpShort торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp UpShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit UpShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl UpShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss UpShort", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersTrading dnLong = new GroupParametersTrading(
                          GroupType.DownLong
                        , CreateParameter("Включить DownLong торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp DownLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit DownLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl DownLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss DownLong", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersTrading dnShort = new GroupParametersTrading (
                          GroupType.DownShort
                        , CreateParameter("Включить DownShort торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp DownShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit DownShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl DownShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss DownShort", 3m, 0.0m, 1.0m, 10.0m)
                        );

            //==Панель с техническими параметрами: ======================================================================================================================
            addSeparateParameter(TAB_SERVICE_CONTROL_NAME);
            GroupParametersTrading testTest = new GroupParametersTrading(
                          GroupType.TestTest
                        , CreateParameter("Включить TestTest торговлю", true, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер отложенного ордера TestTest", 1.5m, 0.0m, 0.5m, 5.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер старта tp TestTest", 1m, 1.0m, 1.0m, 1.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%TakeProfit TestTest", 1.5m, 0.0m, 0.5m, 5.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер старта sl TestTest", 1m, 1.0m, 1.0m, 1.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%StopLoss TestTest", 3m, 0.0m, 1.0m, 10.0m, TAB_SERVICE_CONTROL_NAME)
                        );
            //===========================================================================================================================================================
            groupParametersTradingService = new GroupParametersTradingService();
            groupParametersTradingService.addGroupParameters(upLong);
            groupParametersTradingService.addGroupParameters(upShort);
            groupParametersTradingService.addGroupParameters(dnLong);
            groupParametersTradingService.addGroupParameters(dnShort);
            groupParametersTradingService.addGroupParameters(testTest);

            logService = new LogService(this);

            eventServiceTrading = new EventServiceTrading(tab, generalParametersTrading, groupParametersTradingService, logService);

            tab.CandleFinishedEvent += candleFinishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;
            tab.NewTickEvent += newTickEventLogic;
            tab.Connector.BestBidAskChangeEvent += bestBidAskChangeEventLogic;
            tab.PositionSellAtStopActivateEvent += positionSellAtStopActivateEventlogic;
            tab.PositionBuyAtStopActivateEvent += positionBuyAtStopActivateEventLogic;
            tab.PositionOpeningFailEvent += positionOpeningFailEventLogic;

        }

        private void positionSellAtStopActivateEventlogic(Position position)
        {
            int test = 0;
        }
        private void positionBuyAtStopActivateEventLogic(Position position)
        {
            int test = 0;
        }
        private void positionOpeningFailEventLogic(Position position)
        {
            eventServiceTrading.positionOpeningFailEventLogic(position);
        }

        private void positionClosingSuccesEventLogic(Position position)
        {
            eventServiceTrading.positionClosingSuccesEventLogic(position);
        }

        private void positionOpeningSuccesEventLogic(Position position)
        {
            eventServiceTrading.positionOpeningSuccesEventLogic(position);
        }

        private void addSeparateParameter(string tabControlName = null)
        {
            CreateParameter(LogService.SEPARATE_PARAMETR_LINE + separateCounter, LogService.SEPARATE_PARAMETR_LINE, tabControlName);
            ++separateCounter;
        }
        public override string GetNameStrategyType()
        {
            return BOT_NAME;
        }

        public override void ShowIndividualSettingsDialog()
        {
            MessageBox.Show("Нет настроек");
        }

        private void candleFinishedEventLogic(List<Candle> candles)
        {
            eventServiceTrading.candleFinishedEventLogic(candles);
        }

        private void newTickEventLogic(Trade trade)
        {
            //eventServiceTrading.newTickEventLogic(trade);
        }

        private void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            eventServiceTrading.bestBidAskChangeEventLogic(bestBid, bestAsk);
        }

        public void sendLog(string message, LogMessageType logMessageType)
        {
            SendNewLogMessage(message, logMessageType);
        }
        public new void SendNewLogMessage(string message, LogMessageType system)
        {
            base.SendNewLogMessage(message, system);
        }

        public int getCountBufferLogLine()
        {
            return generalParametersTrading.getCountBufferLogLine();
        }

        public string getFilePath()
        {
            return "C:\\1_LOGS\\" + BOT_NAME + "_log.txt";
        }

        public DateTime getTimeServerCurrent()
        {
            return tab.TimeServerCurrent;
        }

        public bool loggingEnabled()
        {
            return generalParametersTrading.getLogEnabled();
        }
    }
}
