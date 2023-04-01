
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Service.statistic;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.Squeezy.Tester
{
    public class SqueezyTester : BotPanel
    {
        public static string BOT_NAME = "SqueezyTesterBot";
        private const string VERSION = "0.0.1";
        private const string TAB_SERVICE_CONTROL_NAME = "Service";
        private const string TAB_SERVICE_RULER_NAME = "Ruler";
        public static string SEPARATE_PARAMETR_LINE = "=====================================================";

        public static int separateCounter = 0;    
        private EventServiceTester eventServiceTester;
        private GeneralParametersTester generalParameters;
        private GroupParametersTesterService groupParametersTesterService;
        private BotTabSimple tab;

        public SqueezyTester(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];

            generalParameters = new GeneralParametersTester(
                          CreateParameter("MovingAverage длина slow", 20, 0, 50, 5)
                        , CreateParameter("%ОТКЛЮЧЕН MovingAverage высота коридора slow", 0.1m, 0.0m, 1.0m, 0.1m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 50, 5)
                        , CreateParameter("ОТКЛЮЧЕН % депозита для сделки", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Сумма для открытия", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Коэфф мартышки:", 1, 5, 5, 1)
                        , CreateParameter("Количество строк лога в буфере", 1, 1, 1, 1, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Тестовые параметры", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Логгирование", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Статистика", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%MA ширина канала", 1m, 1m, 1m, 1m)
                        , CreateParameter("Графика Линия группы:", true)
                        , CreateParameter("% отрисовки линии группы", 1.0m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("Графика Линия сквиза:", true)
                        , CreateParameter("% отрисовки линии сквиза", 1.2m, 1.2m, 1.2m, 1.2m)
                        );
            generalParameters.setOnlyRuler(CreateParameter("ТОЛЬКО РУЛЕТКА", false, TAB_SERVICE_RULER_NAME));
            generalParameters.setRulerBarCount(CreateParameter("Рулетка, бары", 10, 10, 10, 10, TAB_SERVICE_RULER_NAME));
            generalParameters.setRulerStepSqueezy(CreateParameter("Рулетка, шаг сквиза", 1m, 1m, 1m, 1m, TAB_SERVICE_RULER_NAME));
            generalParameters.setRulerStepProfit(CreateParameter("Рулетка, шаг профита", 1m, 1m, 1m, 1m, TAB_SERVICE_RULER_NAME));
            generalParameters.setRulerStepLoss(CreateParameter("Рулетка, шаг лосса", 1m, 1m, 1m, 1m, TAB_SERVICE_RULER_NAME));

            addSeparateParameter();
            addSeparateParameter();

            GroupParametersTester upBuy = new GroupParametersTester(
                                      GroupType.UpBuy
                                    , CreateParameter("Включить UpBuy торговлю", true)
                                    , CreateParameter("%Триггер отложенного ордера UpBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                                    , CreateParameter("%TakeProfit UpBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                                    , CreateParameter("%StopLoss UpBuy", 3m, 0.0m, 1.0m, 10.0m)
                                    , CreateParameter("Количество баров до выхода UpBuy", 2, 0, 30, 1)
                                    );
            addSeparateParameter();
            GroupParametersTester upSell = new GroupParametersTester(
                          GroupType.UpSell
                        , CreateParameter("Включить UpSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpSell", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit UpSell", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss UpSell", 3m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода UpSell", 2, 0, 30, 1)
                        );
            addSeparateParameter();
            GroupParametersTester dnBuy = new GroupParametersTester(
                          GroupType.DownBuy
                        , CreateParameter("Включить DownBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit DownBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss DownBuy", 3m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownBuy", 2, 0, 30, 1)
                        );
            addSeparateParameter();
            GroupParametersTester dnSell = new GroupParametersTester(
                          GroupType.DownSell
                        , CreateParameter("Включить DownSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownSell", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit DownSell", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss DownSell", 3m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownSell", 2, 0, 30, 1)
                        );
            addSeparateParameter();
            GroupParametersTester flatBuy = new GroupParametersTester(
                          GroupType.FlatBuy
                        , CreateParameter("Включить FlatBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера FlatBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit FlatBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss FlatBuy", 3m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода FlatBuy", 2, 0, 30, 1)
                        );
            addSeparateParameter();
            GroupParametersTester flatSell = new GroupParametersTester(
                          GroupType.FlatSell
                        , CreateParameter("Включить FlatSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера FlatSell", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit FlatSell", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss FlatSell", 3m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода FlatSell", 2, 0, 30, 1)
                        );
            //==Панель с техническими параметрами: ======================================================================================================================
            addSeparateParameter(TAB_SERVICE_CONTROL_NAME);
            GroupParametersTester testTest = new GroupParametersTester(
                          GroupType.TestTest
                        , CreateParameter("Включить TestTest торговлю", true, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%Триггер отложенного ордера TestTest", 1.5m, 0.0m, 0.5m, 5.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%TakeProfit TestTest", 1.5m, 0.0m, 0.5m, 5.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%StopLoss TestTest", 3m, 0.0m, 1.0m, 10.0m, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Количество баров до выхода TestTest", 2, 0, 30, 1, TAB_SERVICE_CONTROL_NAME)
                        );
            //===========================================================================================================================================================
            groupParametersTesterService = new GroupParametersTesterService();
            groupParametersTesterService.addGroupParameters(upBuy);
            groupParametersTesterService.addGroupParameters(upSell);
            groupParametersTesterService.addGroupParameters(dnBuy);
            groupParametersTesterService.addGroupParameters(dnSell);
            groupParametersTesterService.addGroupParameters(flatBuy);
            groupParametersTesterService.addGroupParameters(flatSell);
            groupParametersTesterService.addGroupParameters(testTest);

            string statisticFileName = "C:\\1_LOGS\\" + BOT_NAME + "_" + NameStrategyUniq;
            string statisticProfitPath = "C:\\1_LOGS\\stat\\statisticProfit.txt"; //+ BOT_NAME + "_" + NameStrategyUniq + ".txt";
            StatisticService statisticService = new StatisticService(statisticFileName + "_statistic.txt", statisticProfitPath, generalParameters.getStatisticEnabled(), generalParameters.getRulerStepSqueezy());

            LogService logService = new LogService(statisticFileName + "_log.txt", generalParameters.getLogEnabled(), generalParameters.getCountBufferLogLine(), tab);
            eventServiceTester = new EventServiceTester(tab, generalParameters, groupParametersTesterService, logService, statisticService);

            tab.CandleFinishedEvent += candleFinishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;
            ParametrsChangeByUser += parametrsChangeByUserLogic;
        }


        private void parametrsChangeByUserLogic()
        {
            eventServiceTester.parametrsChangeByUserLogic();
        }
        private void addSeparateParameter(string tabControlName = null)
        {
            CreateParameter(SEPARATE_PARAMETR_LINE + BOT_NAME + separateCounter, SEPARATE_PARAMETR_LINE, tabControlName);
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
            eventServiceTester.candleFinishedEventLogic(candles);
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

    }
}
