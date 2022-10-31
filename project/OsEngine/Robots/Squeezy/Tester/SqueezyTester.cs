
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.Squeezy.Tester
{
    public class SqueezyTester : BotPanel, Loggable
    {
        public static string BOT_NAME = "SqueezyTesterBot";
        private const string VERSION = "0.0.1";
        private const string TAB_SERVICE_CONTROL_NAME = "Service";
        public static string SEPARATE_PARAMETR_LINE = "=====================================================";

        public static int separateCounter = 0;    
        private EventServiceTester eventServiceTester;
        private GeneralParametersTester generalParameters;
        private GroupParametersTesterService groupParametersTesterService;
        private BotTabSimple tab;
        private LogService logService;

        public SqueezyTester(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];

            generalParameters = new GeneralParametersTester(
                          CreateParameter("MovingAverage длина slow", 20, 0, 50, 5)
                        , CreateParameter("%MovingAverage высота коридора slow", 0.1m, 0.0m, 1.0m, 0.1m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 50, 5)
                        , CreateParameter("% депозита для сделки", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Количество строк лога в буфере", 5000, 5000, 5000, 5000, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Тестовые параметры", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Логгирование", false, TAB_SERVICE_CONTROL_NAME)
                        );
            addSeparateParameter();
            addSeparateParameter();

            GroupParametersTester upLong = new GroupParametersTester(
                          GroupType.UpBuy
                        , CreateParameter("Включить UpBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpBuy", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%TakeProfit UpBuy", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%StopLoss UpBuy", 2m, 0.0m, 4.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода UpBuy", 10, 0, 50, 1)
                        );
            addSeparateParameter();
            GroupParametersTester upShort = new GroupParametersTester(
                          GroupType.UpSell
                        , CreateParameter("Включить UpSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpSell", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%TakeProfit UpSell", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%StopLoss UpSell", 2m, 0.0m, 4.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода UpSell", 10, 0, 50, 1)
                        );
            addSeparateParameter();
            GroupParametersTester dnLong = new GroupParametersTester(
                          GroupType.DownBuy
                        , CreateParameter("Включить DownBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownBuy", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%TakeProfit DownBuy", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%StopLoss DownBuy", 2m, 0.0m, 4.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownBuy", 10, 0, 50, 1)
                        );
            addSeparateParameter();
            GroupParametersTester dnShort = new GroupParametersTester(
                          GroupType.DownSell
                        , CreateParameter("Включить DownSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownSell", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%TakeProfit DownSell", 1.5m, 0.0m, 1.5m, 7.5m)
                        , CreateParameter("%StopLoss DownSell", 2m, 0.0m, 4.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownSell", 10, 0, 50, 1)
                        );
            //==Панель с техническими параметрами: ======================================================================================================================
            addSeparateParameter(TAB_SERVICE_CONTROL_NAME);
            GroupParametersTrading testTest = new GroupParametersTrading(
                          GroupType.TestTest
                        , CreateParameter("Включить TestTest торговлю", true, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер отложенного ордера TestTest", 1.5m, 0.0m, 1.5m, 7.5m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер старта tp TestTest", 1m, 1.0m, 1.0m, 1.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%TakeProfit TestTest", 1.5m, 0.0m, 1.5m, 7.5m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер старта sl TestTest", 1m, 1.0m, 1.0m, 1.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%StopLoss TestTest", 2m, 0.0m, 4.0m, 10.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Количество баров до выхода TestTest", 10, 0, 50, 1)
                        );
            //===========================================================================================================================================================
            groupParametersTesterService = new GroupParametersTesterService();
            groupParametersTesterService.addGroupParameters(upLong);
            groupParametersTesterService.addGroupParameters(upShort);
            groupParametersTesterService.addGroupParameters(dnLong);
            groupParametersTesterService.addGroupParameters(dnShort);
            groupParametersTesterService.addGroupParameters(testTest);

            logService = new LogService(this);

            eventServiceTester = new EventServiceTester(tab, generalParameters, groupParametersTesterService, logService);

            tab.CandleFinishedEvent += finishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;

            //Логгирование стартовых настроек:
            logService.sendLogSystem(SEPARATE_PARAMETR_LINE);
            logService.sendLogSystem(BOT_NAME + " init successful, started version bot:" + VERSION);
            logService.sendLogSystem(generalParameters.getAllSettings());
            logService.sendLogSystem(upLong.getAllGroupParameters());
            logService.sendLogSystem(upShort.getAllGroupParameters());
            logService.sendLogSystem(dnLong.getAllGroupParameters());
            logService.sendLogSystem(dnShort.getAllGroupParameters());
            logService.sendLogSystem(testTest.getAllGroupParameters());
        }

        private void addSeparateParameter(string tabControlName = null)
        {
            CreateParameter(SEPARATE_PARAMETR_LINE + separateCounter, SEPARATE_PARAMETR_LINE, tabControlName);
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

        private void finishedEventLogic(List<Candle> candles)
        {
            eventServiceTester.finishedEventLogic(candles);
        }
        private void positionClosingSuccesEventLogic(Position position)
        {
            eventServiceTester.positionClosingSuccesEventLogic(position);
        }

        private void positionOpeningSuccesEventLogic(Position position)
        {
            eventServiceTester.positionOpeningSuccesEventLogic(position);
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
            return generalParameters.getCountBufferLogLine();
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
            return generalParameters.getLogEnabled();
        }
    }
}
