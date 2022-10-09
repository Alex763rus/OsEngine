﻿
using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System.Collections.Generic;
using System.Windows;

namespace OsEngine.Robots.SqueezyBot
{
    public class Squeezy : BotPanel
    {
        public static string SQUEEZY_BOT = "SqueezyBot";
        public static string SEPARATE_PARAMETR_LINE = "=====================================================";

        public static int separateCounter = 0;    
        private EventServiceRuler eventServiceRuler;
        private GeneralParametersRuler generalParameters;
        private GroupParametersService groupParametersService;
        private BotTabSimple tab;
        
        public Squeezy(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];

            generalParameters = new GeneralParametersRuler(
                          CreateParameter("MovingAverage длина slow", 20, 0, 50, 5)
                        , CreateParameter("%MovingAverage высота коридора slow", 0.1m, 0.0m, 1.0m, 0.1m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 50, 5)
                        , CreateParameter("% депозита для сделки", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Количество баров до выхода", 10, 0, 50, 1)
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
            groupParametersService = new GroupParametersService();
            groupParametersService.addGroupParameters(upLong);
            groupParametersService.addGroupParameters(upShort);
            groupParametersService.addGroupParameters(dnLong);
            groupParametersService.addGroupParameters(dnShort);
            
            eventServiceRuler = new EventServiceRuler(tab, generalParameters, groupParametersService);

            tab.CandleFinishedEvent += finishedEventLogic;
            tab.PositionClosingSuccesEvent += positionClosingSuccesEventLogic;
            tab.PositionOpeningSuccesEvent += positionOpeningSuccesEventLogic;
        }

        private void addSeparateParameter()
        {
            CreateParameter(SEPARATE_PARAMETR_LINE + separateCounter, SEPARATE_PARAMETR_LINE);
            ++separateCounter;
        }
        public override string GetNameStrategyType()
        {
            return SQUEEZY_BOT;
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

    }
}
