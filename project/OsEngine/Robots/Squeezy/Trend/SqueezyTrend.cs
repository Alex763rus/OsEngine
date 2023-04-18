using OsEngine.Entity;
using OsEngine.Market.Servers.Tester;
using OsEngine.Market;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Service.statistic.drawdown;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.Squeezy.Trend;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.Squeezy.Trend
{
    public class SqueezyTrend : BotPanel
    {
        public static string BOT_NAME = "SqueezyTrendBot";
        public static string VERSION = "0.0.1";
        private const string TAB_SERVICE_CONTROL_NAME = "Service";

        public int separateCounter = 0;
        private EventService eventService;

        private GeneralParametersTrend generalParameters;
        private GroupParametersTrendService groupParametersService;
        private BotTabSimple tab;
        private LogService logService;
        public SqueezyTrend(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];
            
            generalParameters = new GeneralParametersTrend(
                          CreateParameter("MovingAverage длина slow", 130, 0, 130, 5)
                        , CreateParameter("ОТКЛЮЧЕН %MovingAverage высота коридора slow ", 0.4m, 0.4m, 0.4m, 0.4m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 10, 1)
                        , CreateParameter("ОТКЛЮЧЕН % депозита для сделки", 10.0m, 10.0m, 50.0m, 5.0m)
                        , CreateParameter("Сумма для открытия", 100.0m, 0.0m, 100.0m, 5.0m)
                        , CreateParameter("Коэфф мартышки:", 1, 1, 1, 1)
                        , CreateParameter("%Триггер старта", 0.5m, 0.5m, 0.5m, 0.5m)
                        , CreateParameter("Количество строк лога в буфере", 1, 0, 1, 1, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Логгирование", true, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Статистика", false, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("%MA ширина канала", 4m, 4m, 4m, 4m)
                        , CreateParameter("Графика Линия группы:", true)
                        , CreateParameter("% отрисовки линии группы", 1.0m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("Графика Линия сквиза:", true)
                        , CreateParameter("% отрисовки линии сквиза", 1.2m, 1.2m, 1.2m, 1.2m)
                        );
            generalParameters.setClearJournal(CreateParameter("Очистка журнала:", true));
            generalParameters.setDevelopMode(CreateParameter("Режим разработчика:", false, TAB_SERVICE_CONTROL_NAME));
            generalParameters.setTgAlertEnabled(CreateParameter("Телеграмм оповещения :", false, TAB_SERVICE_CONTROL_NAME));
            generalParameters.setTgPingEnabled(CreateParameter("Пинг ТГ Бот :", false, TAB_SERVICE_CONTROL_NAME));
            generalParameters.setStand(CreateParameter("Контур :", "ИФТ", TAB_SERVICE_CONTROL_NAME));
            addSeparateParameter();
            addSeparateParameter();

            GroupParametersTrend upBuy = new GroupParametersTrend(
                          GroupType.UpBuy
                        , CreateParameter("Включить UpBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpBuy", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit UpBuy", 3.0m, 0.0m, 3.0m, 1.0m)
                        , CreateParameter("%StopLoss UpBuy", 5.0m, 0.0m, 5.0m, 5.0m)
                        , CreateParameter("Количество баров до выхода UpBuy", 2, 0, 2, 1)
                        );
            addSeparateParameter();
            GroupParametersTrend upSell = new GroupParametersTrend(
                          GroupType.UpSell
                        , CreateParameter("Включить UpSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpSell", 1.0m, 0.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit UpSell", 8.5m, 0.0m, 8.5m, 8.0m)
                        , CreateParameter("%StopLoss UpSell", 4.0m, 0.0m, 4.0m, 4.0m)
                        , CreateParameter("Количество баров до выхода UpSell", 5, 0, 5, 5)
                        );
            addSeparateParameter();
            GroupParametersTrend dnBuy = new GroupParametersTrend(
                          GroupType.DownBuy
                        , CreateParameter("Включить DownBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownBuy", 1.0m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit DownBuy", 5.0m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss DownBuy", 6m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownBuy", 18, 0, 30, 1)
                        );
            addSeparateParameter();
            GroupParametersTrend dnSell = new GroupParametersTrend(
                          GroupType.DownSell
                        , CreateParameter("Включить DownSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownSell", 1.0m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit DownSell", 5.0m, 0.0m, 5.0m, 5.0m)
                        , CreateParameter("%StopLoss DownSell", 10.0m, 0.0m, 6.0m, 10.0m)
                        , CreateParameter("Количество баров до выхода DownSell", 30, 0, 30, 30)
                        );
            addSeparateParameter();
            GroupParametersTrend flatBuy = new GroupParametersTrend(
                          GroupType.FlatBuy
                        , CreateParameter("Включить FlatBuy торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера FlatBuy", 1.0m, 0.0m, 0.5m, 1.0m)
                        , CreateParameter("%TakeProfit FlatBuy", 6.0m, 0.0m, 0.5m, 6.0m)
                        , CreateParameter("%StopLoss FlatBuy", 2m, 0.0m, 1.0m, 2.0m)
                        , CreateParameter("Количество баров до выхода FlatBuy", 5, 0, 30, 5)
                        );
            addSeparateParameter();
            GroupParametersTrend flatSell = new GroupParametersTrend(
                          GroupType.FlatSell
                        , CreateParameter("Включить FlatSell торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера FlatSell", 1.0m, 0.0m, 0.5m, 1.0m)
                        , CreateParameter("%TakeProfit FlatSell", 3.0m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%StopLoss FlatSell", 8m, 0.0m, 1.0m, 8.0m)
                        , CreateParameter("Количество баров до выхода FlatSell", 18, 0, 30, 18)
                        );

            groupParametersService = new GroupParametersTrendService();
            groupParametersService.addGroupParameters(upBuy);
            groupParametersService.addGroupParameters(upSell);
            groupParametersService.addGroupParameters(dnBuy);
            groupParametersService.addGroupParameters(dnSell);
            groupParametersService.addGroupParameters(flatBuy);
            groupParametersService.addGroupParameters(flatSell);

            string uniqPart = BOT_NAME + "_" + NameStrategyUniq + ".txt";
            string logFileName = "C:\\1_LOGS\\log_" + uniqPart;
            string stDrawdownFilePath = "C:\\1_LOGS\\stDrawdown\\statistic_" + uniqPart;

            logService = new LogService(logFileName, generalParameters.getLogEnabled(), generalParameters.getCountBufferLogLine(), tab);
            TgService tgService = new TgService(generalParameters.getTgAlertEnabled(), generalParameters.getStand(), NameStrategyUniq);
            tgService.sendSqueezyStart(VERSION);
            PingService.getInstance(generalParameters.getTgPingEnabled(), tgService);

            eventService = new EventServiceTrend(tab, generalParameters, groupParametersService, logService, tgService);

            tab.CandleFinishedEvent += candleFinishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;
            tab.PositionOpeningFailEvent += positionOpeningFailEventLogic;
            ParametrsChangeByUser += parametrsChangeByUserLogic;
            ParametrsChangeByUser += testingStartEventLogic;

            if (startProgram == StartProgram.IsTester || startProgram == StartProgram.IsOsOptimizer)
            {
                TesterServer testerServer = (TesterServer)ServerMaster.GetServers()[0];
                testerServer.TestingStartEvent += testingStartEventLogic;
            }
        }

        private void positionOpeningFailEventLogic(Position position)
        {
            logService.sendLogUser("подписка:positionOpeningFailEventLogic:" + NameStrategyUniq+ " - " + LogService.getPositionInfo(position));
            eventService.positionOpeningFailEventLogic(position);
        }

        private void positionClosingSuccesEventLogic(Position position)
        {
            logService.sendLogUser("подписка:positionClosingSuccesEventLogic:" + NameStrategyUniq + " - " + LogService.getPositionInfo(position));
            //eventService.positionClosingSuccesEventLogic(position);
        }

        private void positionOpeningSuccesEventLogic(Position position)
        {
            logService.sendLogUser("подписка:positionOpeningSuccesEventLogic:" + NameStrategyUniq + " - " + LogService.getPositionInfo(position));
            eventService.positionOpeningSuccesEventLogic(position);
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
            eventService.candleFinishedEventLogic(candles);
        }

        private void parametrsChangeByUserLogic()
        {
            eventService.parametrsChangeByUserLogic();
        }

        private void testingStartEventLogic()
        {
            eventService.testingStartEventLogic();
        }

    }
}
