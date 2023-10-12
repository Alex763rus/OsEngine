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
        private const string PARAMS_NOT_USED_NAME = "not used";

        public int separateCounter = 0;
        private EventService eventService;

        private GeneralParametersTrend generalParameters;
        private BotTabSimple tab;
        private LogService logService;
        public SqueezyTrend(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];
            
            generalParameters = new GeneralParametersTrend(
                          CreateParameter("MovingAverage длина slow", 130, 0, 130, 5)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 10, 1)
                        , CreateParameter("Сумма для открытия", 100.0m, 0.0m, 100.0m, 5.0m)
                        , CreateParameter("Коэфф мартышки:", 1, 1, 1, 1)
                        , CreateParameter("%Триггер старта", 0.5m, 0.5m, 0.5m, 0.5m, PARAMS_NOT_USED_NAME)
                        , CreateParameter("Количество строк лога в буфере", 1, 0, 1, 1, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Логгирование", true, TAB_SERVICE_CONTROL_NAME)
                        , CreateParameter("Статистика", false, PARAMS_NOT_USED_NAME)
                        , CreateParameter("%MA ширина канала", 4m, 4m, 4m, 4m)
                        , CreateParameter("Графика Линия группы:", true)
                        , CreateParameter("% отрисовки линии группы", 1.0m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("Графика Линия сквиза:", true, PARAMS_NOT_USED_NAME)
                        , CreateParameter("% отрисовки линии сквиза", 1.2m, 1.2m, 1.2m, 1.2m, PARAMS_NOT_USED_NAME)
                        );
            generalParameters.setClearJournal(CreateParameter("Очистка журнала:", true, PARAMS_NOT_USED_NAME));
            generalParameters.setDevelopMode(CreateParameter("Режим разработчика:", false, PARAMS_NOT_USED_NAME));
            generalParameters.setTgAlertEnabled(CreateParameter("Телеграмм оповещения :", false, PARAMS_NOT_USED_NAME));
            generalParameters.setTgPingEnabled(CreateParameter("Пинг ТГ Бот :", false, PARAMS_NOT_USED_NAME));
            generalParameters.setStand(CreateParameter("Контур :", "ИФТ", TAB_SERVICE_CONTROL_NAME));
            addSeparateParameter();
            addSeparateParameter();
            
            string uniqPart = BOT_NAME + "_" + NameStrategyUniq + ".txt";
            string logFileName = "C:\\1_LOGS\\log_" + uniqPart;
            string stDrawdownFilePath = "C:\\1_LOGS\\stDrawdown\\statistic_" + uniqPart;

            logService = new LogService(logFileName, generalParameters.getLogEnabled(), generalParameters.getCountBufferLogLine(), tab);
            eventService = new EventServiceTrend(tab, generalParameters, logService);

            tab.CandleFinishedEvent += candleFinishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;
            tab.PositionOpeningFailEvent += positionOpeningFailEventLogic;
            ParametrsChangeByUser += parametrsChangeByUserLogic;
            ParametrsChangeByUser += testingStartEventLogic;

            if (startProgram == StartProgram.IsTester)
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
