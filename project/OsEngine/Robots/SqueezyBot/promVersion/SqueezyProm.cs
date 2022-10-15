using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.SqueezyBot.rulerVersion;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.SqueezyBot.promVersion
{
    public class SqueezyProm : BotPanel, Loggable
    {
        public static string BOT_NAME = "SqueezyPromBot";
        private const string VERSION = "0.0.1";
        public static string SEPARATE_PARAMETR_LINE = "=====================================================";

        public static int separateCounter = 0;
        private EventServiceProm eventServiceProm;
        private GeneralParametersProm generalParametersProm;
        private GroupParametersPromService groupParametersPromService;
        private BotTabSimple tab;
        private static LogService logService;

        public SqueezyProm(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];

            generalParametersProm = new GeneralParametersProm(
                          CreateParameter("MovingAverage длина slow", 20, 0, 50, 5)
                        , CreateParameter("%MovingAverage высота коридора slow", 0.1m, 0.0m, 1.0m, 0.1m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 50, 5)
                        , CreateParameter("% депозита для сделки", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Количество баров до выхода", 10, 0, 50, 1)
                        , CreateParameter("%Триггер старта", 1m, 1m, 1m, 1m)
                        , CreateParameter("Количество строк лога в буфере", 50, 0, 50, 1)
                        );
            addSeparateParameter();
            addSeparateParameter();

            GroupParametersProm upLong = new GroupParametersProm(
                          GroupType.UpLong
                        , CreateParameter("Включить UpLong торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp UpLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit UpLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl UpLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss UpLong", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersProm upShort = new GroupParametersProm(
                          GroupType.UpShort
                        , CreateParameter("Включить UpShort торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера UpShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp UpShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit UpShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl UpShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss UpShort", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersProm dnLong = new GroupParametersProm(
                          GroupType.DownLong
                        , CreateParameter("Включить DownLong торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp DownLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit DownLong", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl DownLong", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss DownLong", 3m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParametersProm dnShort = new GroupParametersProm (
                          GroupType.DownShort
                        , CreateParameter("Включить DownShort торговлю", true)
                        , CreateParameter("%Триггер отложенного ордера DownShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта tp DownShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%TakeProfit DownShort", 1.5m, 0.0m, 0.5m, 5.0m)
                        , CreateParameter("%Триггер старта sl DownShort", 1m, 1.0m, 1.0m, 1.0m)
                        , CreateParameter("%StopLoss DownShort", 3m, 0.0m, 1.0m, 10.0m)
                        );
            groupParametersPromService = new GroupParametersPromService();
            groupParametersPromService.addGroupParameters(upLong);
            groupParametersPromService.addGroupParameters(upShort);
            groupParametersPromService.addGroupParameters(dnLong);
            groupParametersPromService.addGroupParameters(dnShort);

            logService = new LogService(this);

            eventServiceProm = new EventServiceProm(tab, generalParametersProm, groupParametersPromService, logService);

            tab.CandleFinishedEvent += finishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;
            tab.NewTickEvent += newTickEventLogic;
            tab.Connector.BestBidAskChangeEvent += bestBidAskChangeEventLogic;

            //Логгирование стартовых настроек:
            logService.sendLogSystem(SEPARATE_PARAMETR_LINE);
            logService.sendLogSystem(BOT_NAME + " init successful, started version bot:" + VERSION);
            logService.sendLogSystem(generalParametersProm.getAllSettings());
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
            eventServiceProm.finishedEventLogic(candles);
        }
        private void positionClosingSuccesEventLogic(Position position)
        {
            eventServiceProm.positionClosingSuccesEventLogic(position);
        }

        private void positionOpeningSuccesEventLogic(Position position)
        {
            eventServiceProm.positionOpeningSuccesEventLogic(position);
        }

        private void newTickEventLogic(Trade trade)
        {
            //eventServiceProm.newTickEventLogic(trade);
        }

        private void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
            eventServiceProm.bestBidAskChangeEventLogic(bestBid, bestAsk);
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
            return generalParametersProm.getCountBufferLogLine();
        }

        public string getFilePath()
        {
            return "C:\\1_LOGS\\" + BOT_NAME + "_log.txt";
        }
    }
}
