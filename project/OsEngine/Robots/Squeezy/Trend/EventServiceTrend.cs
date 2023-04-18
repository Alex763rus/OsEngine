using Kraken.WebSockets;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Position;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Pricing;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.Squeezy.Service;
using OsEngine.Robots.Squeezy.Service.statistic.drawdown;
using OsEngine.Robots.Squeezy.Service.ZigZag;
using OsEngine.Robots.Squeezy.Tester;
using OsEngine.Robots.Squeezy.Trading;
using OsEngine.Robots.SqueezyBot;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using Position = OsEngine.Entity.Position;
using Side = OsEngine.Entity.Side;

namespace OsEngine.Robots.Squeezy.Trend
{
    public class EventServiceTrend : EventService
    {
        private BotTabSimple tab;
        private GeneralParametersTrend generalParameters;
        private MovingAverageService movingAverageService;
        private DealService dealService;
        private LogService logService;
        private PaintService paintService;
        private VolumeSumService volumeSumService;

        private DirectionType directionTypeCurrent; //Направление текущего бара по МА. 
        private decimal priceForPaintGroup;         //Цена чтобы рисовать тренд. Заполняется единажды.
        DateTime timeStartPaintGroup;               //Время начала тренда
        private bool isStart;                       //Признак начала работы
        public EventServiceTrend(BotTabSimple tab, GeneralParametersTrend generalParameters, LogService logService)
        {
            this.tab = tab;
            this.generalParameters = generalParameters;
            this.logService = logService;
            init();
        }

        private void init()
        {
            isStart = true;
            movingAverageService = new MovingAverageService(tab, generalParameters);
            dealService = new DealService(tab, generalParameters, logService);
            paintService = new PaintService(tab);
            directionTypeCurrent = DirectionType.None;
        }
        public void candleFinishedEventLogic(List<Candle> candles)
        {
            //Для тестовой среды: Если мало баров или нет медленной, ничего не делаем:
            if (candles.Count <= 2 || movingAverageService.getMaLastValueSlow() == 0)
            {
                return;
            }
            if (isStart)
            {
                volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);
                movingAverageService.updateMaLen();
                paintService.deleteAllChartElement();
                priceForPaintGroup = MathService.getValueAddPercent(candles[candles.Count - 1].Low, generalParameters.getPaintGroup());
                timeStartPaintGroup = candles[candles.Count - 1].TimeStart;
                isStart = false;
            }
            DirectionType directionTypeTmp = getDirectionType();
            if(directionTypeCurrent == directionTypeTmp)
            {
                return;
            }
            if (directionTypeCurrent == DirectionType.None)
            {
                directionTypeCurrent = directionTypeTmp;
                return;
            }
            if (generalParameters.getPaintGroupEnabled()) {
                paintService.paintGroup(timeStartPaintGroup, candles[candles.Count - 1].TimeStart, priceForPaintGroup, directionTypeCurrent);
                timeStartPaintGroup = candles[candles.Count - 1].TimeStart;
            }
            //сменился тренд:
            string comment = "Новый тренд:" + directionTypeCurrent + " -> " + directionTypeTmp;
            directionTypeCurrent = directionTypeTmp;
            dealService.closeAllDeals(comment);
            if (directionTypeCurrent == DirectionType.Flat)
            {
                return;
            }
            string message = comment
                    + ". предпоследний бар:" + LogService.getCandleInfo(candles[candles.Count - 2])
                    + " последний бар:" + LogService.getCandleInfo(candles[candles.Count - 1]);
            logService.sendLogSystem(message);
            Side side = directionTypeCurrent == DirectionType.Up ? Side.Buy : Side.Sell;
            dealService.openDeal(side, directionTypeCurrent.ToString(), comment, volumeSumService.getVolumeSum(Side.Sell));
            printEndBarInfo();
        }
     
        private void printEndBarInfo()
        {
            int countPosition = 0;
            if (dealService.hasOpendeal(Side.Sell))
            {
                ++countPosition;
            }
            if (dealService.hasOpendeal(Side.Buy))
            {
                ++countPosition;
            }
            StringBuilder currentInfo = new StringBuilder();
            currentInfo.Append("******> Закрыт бар. Группа:").Append(directionTypeCurrent)
                .Append(" Позиций:").Append(countPosition);
            logService.sendLogSystem(currentInfo.ToString());
        }

        public void positionClosingSuccesEventLogic(Position position)
        {
            bool isProfit = position.ProfitPortfolioPunkt > 0;
            paintService.paintClosedPosition(position, isProfit);
            volumeSumService.updateLevel(position.Direction, isProfit);
        }

        public void positionOpeningSuccesEventLogic(Position position)
        {
        }

        private DirectionType getDirectionType()
        {
            DirectionType directionType;
            if (movingAverageService.getMaLastValueSlow() == 0)
            {
                directionType = DirectionType.Test;
            }
            else if ((     movingAverageService.getMaLastValueSlow() < movingAverageService.getMaLastValueFast() 
                        && movingAverageService.getMaLastValueFast() < MathService.getValueAddPercent(movingAverageService.getMaLastValueSlow(), generalParameters.getMaStrength()))
                    || (   movingAverageService.getMaLastValueFast() < movingAverageService.getMaLastValueSlow() 
                        && movingAverageService.getMaLastValueSlow() < MathService.getValueAddPercent(movingAverageService.getMaLastValueFast(), generalParameters.getMaStrength())))
            {
                directionType = DirectionType.Flat;
            }
            else if (movingAverageService.getMaLastValueFast() > movingAverageService.getMaLastValueSlow())
            {
                directionType = DirectionType.Up;
            }
            else
            {
                directionType = DirectionType.Down;
            }
            return directionType;
        }

        public void parametrsChangeByUserLogic()
        {
            movingAverageService.updateMaLen();
            paintService.deleteAllChartElement();
            volumeSumService = new VolumeSumService(generalParameters.getVolumeSum(), generalParameters.getCoeffMonkey(), logService);
            logService.setup(generalParameters.getLogEnabled(), generalParameters.getCountBufferLogLine());
        }

        public void bestBidAskChangeEventLogic(decimal bestBid, decimal bestAsk)
        {
        }

        public void positionOpeningFailEventLogic(Position position)
        {
        }
        public void testingStartEventLogic()
        {
            init();
        }

        public void testingEndEventLogic()
        {
            //throw new NotImplementedException();
        }
    }
}
