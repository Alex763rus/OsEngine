
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy;
using OsEngine.Robots.Squeezy.Ruler;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.Squeezy.Ruler
{
    public class SqueezyRuler : BotPanel, Loggable
    {
        public static string BOT_NAME = "SqueezyRulerBot";
        private const string VERSION = "0.0.1";
        public static string SEPARATE_PARAMETR_LINE = "=====================================================";

        public static int separateCounter = 0;    
        private EventServiceRuler eventServiceRuler;
        private GeneralParametersRuler generalParameters;
        private GroupParametersRulerService groupParametersRulerService;
        private BotTabSimple tab;
        private LogService logService;

        public SqueezyRuler(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];

            generalParameters = new GeneralParametersRuler(
                          CreateParameter("MovingAverage длина slow", 20, 0, 50, 5)
                        , CreateParameter("%MovingAverage высота коридора slow", 0.1m, 0.0m, 1.0m, 0.1m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 50, 5)
                        , CreateParameter("% депозита для сделки", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Количество баров до выхода", 10, 0, 50, 1)
                        , CreateParameter("Количество строк лога в буфере", 50, 0, 50, 1)
                        );
            addSeparateParameter();
            addSeparateParameter();

            GroupParametersRuler upLong = new GroupParametersRuler(
                          GroupType.UpLong
                        , CreateParameter("Включить UpLong торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%TakeProfit UpLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%StopLoss UpLong", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersRuler upShort = new GroupParametersRuler(
                          GroupType.UpShort
                        , CreateParameter("Включить UpShort торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpShort", 1.5m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%TakeProfit UpShort", 1.5m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%StopLoss UpShort", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersRuler dnLong = new GroupParametersRuler(
                          GroupType.DownLong
                        , CreateParameter("Включить DownLong торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownLong", 1.5m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%TakeProfit DownLong", 1.5m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%StopLoss DownLong", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersRuler dnShort = new GroupParametersRuler(
                          GroupType.DownShort
                        , CreateParameter("Включить DownShort торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownShort", 1.5m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%TakeProfit DownShort", 1.5m, 0.0m, 0.5m, 3.0m)
                        , CreateParameter("%StopLoss DownShort", 3m, 0.0m, 1.0m, 10.0m)
                        );
            groupParametersRulerService = new GroupParametersRulerService();
            groupParametersRulerService.addGroupParameters(upLong);
            groupParametersRulerService.addGroupParameters(upShort);
            groupParametersRulerService.addGroupParameters(dnLong);
            groupParametersRulerService.addGroupParameters(dnShort);

            logService = new LogService(this);

            eventServiceRuler = new EventServiceRuler(tab, generalParameters, groupParametersRulerService, logService);

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
        }

        private void addSeparateParameter()
        {
            CreateParameter(SEPARATE_PARAMETR_LINE + separateCounter, SEPARATE_PARAMETR_LINE);
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
            eventServiceRuler.finishedEventLogic(candles);
        }
        private void positionClosingSuccesEventLogic(Position position)
        {
            eventServiceRuler.positionClosingSuccesEventLogic(position);
        }

        private void positionOpeningSuccesEventLogic(Position position)
        {
            eventServiceRuler.positionOpeningSuccesEventLogic(position);
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
    }
}
