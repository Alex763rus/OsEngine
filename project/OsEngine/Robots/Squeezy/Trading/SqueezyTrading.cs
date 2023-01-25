﻿using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class SqueezyTrading : BotPanel, Loggable, Statisticable
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
                          CreateParameter("MovingAverage длина slow", 130, 0, 130, 5)
                        , CreateParameter("ОТКЛЮЧЕН %MovingAverage высота коридора slow ", 0.4m, 0.4m, 0.4m, 0.4m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 10, 1)
                        , CreateParameter("ОТКЛЮЧЕН % депозита для сделки", 10.0m, 10.0m, 50.0m, 5.0m)
                        , CreateParameter("Сумма для открытия", 100.0m, 0.0m, 100.0m, 5.0m)
                        , CreateParameter("Коэфф мартышки:", 1, 1, 1, 1)
                        , CreateParameter("%Триггер старта", 0.5m, 0.5m, 0.5m, 0.5m)
                        , CreateParameter("Количество строк лога в буфере", 1, 0, 1, 1, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Тестовые параметры", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Логгирование", true, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Статистика", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%MA ширина канала", 4m, 4m, 4m, 4m)
                        , CreateParameter("Графика Линия группы:", true)
                        , CreateParameter("% отрисовки линии группы", 1.0m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("Графика Линия сквиза:", true)
                        , CreateParameter("% отрисовки линии сквиза", 1.2m, 1.2m, 1.2m, 1.2m)
                        );
            addSeparateParameter();
            addSeparateParameter();

            GroupParametersTrading upBuy = new GroupParametersTrading(
                          GroupType.UpBuy
                        , CreateParameter("Включить UpBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit UpBuy", 3.0m, 0.0m, 3.0m, 1.0m)
                        , CreateParameter("%StopLoss UpBuy", 5.0m, 0.0m, 5.0m, 5.0m)
                        , CreateParameter("Количество баров до выхода UpBuy", 2, 0, 2, 1)
                        );
            addSeparateParameter();
            GroupParametersTrading upSell = new GroupParametersTrading(
                          GroupType.UpSell
                        , CreateParameter("Включить UpSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpSell", 1.0m, 0.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit UpSell", 8.5m, 0.0m, 8.5m, 8.0m)
                        , CreateParameter("%StopLoss UpSell", 4.0m, 0.0m, 4.0m, 4.0m)
                        , CreateParameter("Количество баров до выхода UpSell", 5, 0, 5, 5)
                        );
            addSeparateParameter();
            GroupParametersTrading dnBuy = new GroupParametersTrading(
                          GroupType.DownBuy
                        , CreateParameter("Включить DownBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownBuy", 1.0m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit DownBuy", 5.0m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss DownBuy", 6m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownBuy", 18, 0, 30, 1)
                        );
            addSeparateParameter();
            GroupParametersTrading dnSell = new GroupParametersTrading (
                          GroupType.DownSell
                        , CreateParameter("Включить DownSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownSell", 1.0m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit DownSell", 5.0m, 0.0m, 5.0m, 5.0m)
                        , CreateParameter("%StopLoss DownSell", 10.0m, 0.0m, 6.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownSell", 30, 0, 30, 30)
                        );
            addSeparateParameter();
            GroupParametersTrading flatBuy = new GroupParametersTrading(
                          GroupType.FlatBuy
                        , CreateParameter("Включить FlatBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера FlatBuy", 1.0m, 0.0m, 0.5m, 1.0m)
                        , CreateParameter("%TakeProfit FlatBuy", 6.0m, 0.0m, 0.5m, 6.0m)
                        , CreateParameter("%StopLoss FlatBuy", 2m, 0.0m, 1.0m, 2.0m)
                        , CreateParameter("Количество баров до выхода FlatBuy", 5, 0, 30, 5)
                        );
            addSeparateParameter();
            GroupParametersTrading flatSell = new GroupParametersTrading(
                          GroupType.FlatSell
                        , CreateParameter("Включить FlatSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера FlatSell", 1.0m, 0.0m, 0.5m, 1.0m)
                        , CreateParameter("%TakeProfit FlatSell", 3.0m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%StopLoss FlatSell", 8m, 0.0m, 1.0m, 8.0m)
                        , CreateParameter("Количество баров до выхода FlatSell", 18, 0, 30, 18)
                        );
            //==Панель с техническими параметрами: ======================================================================================================================
            addSeparateParameter(TAB_SERVICE_CONTROL_NAME);
            GroupParametersTrading testTest = new GroupParametersTrading(
                          GroupType.TestTest
                        , CreateParameter("Включить TestTest торговлю", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер отложенного ордера TestTest", 1.5m, 0.0m, 0.5m, 5.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%TakeProfit TestTest", 1.5m, 0.0m, 0.5m, 5.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%StopLoss TestTest", 3m, 0.0m, 1.0m, 10.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Количество баров до выхода TestTest", 2, 0, 30, 1, TAB_SERVICE_CONTROL_NAME)
                        );
            //===========================================================================================================================================================
            groupParametersTradingService = new GroupParametersTradingService();
            groupParametersTradingService.addGroupParameters(upBuy);
            groupParametersTradingService.addGroupParameters(upSell);
            groupParametersTradingService.addGroupParameters(dnBuy);
            groupParametersTradingService.addGroupParameters(dnSell);
            groupParametersTradingService.addGroupParameters(flatBuy);
            groupParametersTradingService.addGroupParameters(flatSell);
            groupParametersTradingService.addGroupParameters(testTest);

            logService = new LogService(this);
            StatisticService statisticService = new StatisticService(this);

            eventServiceTrading = new EventServiceTrading(tab, generalParametersTrading, groupParametersTradingService, logService, statisticService);

            tab.CandleFinishedEvent += candleFinishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;
            tab.NewTickEvent += newTickEventLogic;
            tab.Connector.BestBidAskChangeEvent += bestBidAskChangeEventLogic;
            tab.PositionSellAtStopActivateEvent += positionSellAtStopActivateEventlogic;
            tab.PositionBuyAtStopActivateEvent += positionBuyAtStopActivateEventLogic;
            tab.PositionOpeningFailEvent += positionOpeningFailEventLogic;
            ParametrsChangeByUser += parametrsChangeByUserLogic;
        }

        private void parametrsChangeByUserLogic()
        {
            eventServiceTrading.parametrsChangeByUserLogic();
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
            CreateParameter(LogService.SEPARATE_PARAMETR_LINE + BOT_NAME +  separateCounter, LogService.SEPARATE_PARAMETR_LINE, tabControlName);
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

        public string getUniqBotName()
        {
            return this.NameStrategyUniq;
        }
        public string getFilePath()
        {
            return "C:\\1_LOGS\\" + BOT_NAME +  "_log.txt";
        }

        public DateTime getTimeServerCurrent()
        {
            return tab.TimeServerCurrent;
        }

        public bool loggingEnabled()
        {
            return generalParametersTrading.getLogEnabled();
        }

        public bool statisticEnabled()
        {
            return generalParametersTrading.getStatisticEnabled();
        }

        public string getFilePathStatistic()
        {
            return "C:\\1_LOGS\\" + BOT_NAME + "_statistic.txt";
        }
    }
}
