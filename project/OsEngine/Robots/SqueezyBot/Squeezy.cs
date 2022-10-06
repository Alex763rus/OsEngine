using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OsEngine.Robots.SqueezyBot
{
    public class Squeezy : BotPanel
    {
        public static string SQUEEZY_BOT = "SqueezyBot";
        public static string SEPARATE_PARAMETR_LINE = "=====================================================";

        public static int separateCounter = 0;
        private EventService eventService;
        private GeneralParameters generalParameters;
        private GroupParametersService groupParametersService;
        private BotTabSimple tab;

        public Squeezy(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];

           
            generalParameters = new GeneralParameters(
                          CreateParameter("MovingAverage длина slow", 20, 0, 50, 1)
                        , CreateParameter("%MovingAverage высота коридора slow", 0.1m, 0.0m, 1.0m, 0.1m)
                        , CreateParameter("MovingAverage длина fast", 10, 0, 50, 1)
                        , CreateParameter("% депозита для сделки", 10.0m, 5.0m, 50.0m, 5.0m)
                        , CreateParameter("Количество баров до выхода", 20, 0, 50, 1)
                        );
 
            addSeparateParameter();
            addSeparateParameter();
            GroupParameters upLong = new GroupParameters(
                          GroupType.UpLong
                        , CreateParameter("Включить UpLong торговлю", true)
                        , CreateParameter("%Триггер начала сделки UpLong", 1.4m, 0.0m, 1.0m, 3.0m)
                        , CreateParameter("%TakeProfit UpLong", 2.4m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("%StopLoss UpLong", 4m, 0.0m, 1.0m, 10.0m)

                        );
            addSeparateParameter();
            GroupParameters upShort = new GroupParameters(
                          GroupType.UpShort
                        , CreateParameter("Включить UpShort торговлю", true)
                        , CreateParameter("%Триггер начала сделки UpShort", 1.4m, 0.0m, 1.0m, 3.0m)
                        , CreateParameter("%TakeProfit UpShort", 2.4m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("%StopLoss UpShort", 4m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParameters dnLong = new GroupParameters(
                          GroupType.DownLong
                        , CreateParameter("Включить DownLong торговлю", true)
                        , CreateParameter("%Триггер начала сделки DownLong", 1.4m, 0.0m, 1.0m, 3.0m)
                        , CreateParameter("%TakeProfit DownLong", 2.4m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("%StopLoss DownLong", 4m, 0.0m, 1.0m, 10.0m)
                        );
            addSeparateParameter();
            GroupParameters dnShort = new GroupParameters(
                          GroupType.DownShort
                        , CreateParameter("Включить DownShort торговлю", true)
                        , CreateParameter("%Триггер начала сделки DownShort", 1.4m, 0.0m, 1.0m, 3.0m)
                        , CreateParameter("%TakeProfit DownShort", 2.4m, 0.0m, 1.0m, 10.0m)
                        , CreateParameter("%StopLoss DownShort", 4m, 0.0m, 1.0m, 10.0m)
                        );
            groupParametersService = new GroupParametersService();
            groupParametersService.addGroupParameters(upLong);
            groupParametersService.addGroupParameters(upShort);
            groupParametersService.addGroupParameters(dnLong);
            groupParametersService.addGroupParameters(dnShort);
           
            eventService = new EventService(tab, generalParameters, groupParametersService);

            tab.CandleFinishedEvent += finishedEventLogic;
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
            eventService.finishedEventLogic(candles);
        }
    }
}
