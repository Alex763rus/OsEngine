using OsEngine.Charts.CandleChart.Elements;
using OsEngine.Entity;
using OsEngine.Market.Servers.Bitfinex.BitfitnexEntity;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service.ZigZag
{
    public class ZigZagService
    {
        private int depth;                          //Интервал в свечах, на котором индикатор построит новый экстремум если выполнится условие по Deviation
        private decimal deviation;                  //Величина изменения цены в процентах, необходимая, чтобы индикатор отметил вершину на графике.
        private int backstep;                       //Интервал в свечах между максимумами или минимумами
        private PaintService paintService;

        private Candle lastExtremum;                //Последняя зафиксированная свеча экстремум
        private Candle tmpExtremum;                 //Незафиксированный экстремум
        private TrendDirection tmpTrendDirection;   //Направление тренда незафиксированного экстремума
        private IChartElement tmpLine1;             //Предпоследняя перерисовываемая прямая
        private IChartElement tmpLine2;             //Последняя перерисовываемая прямая
        private int tmpDepthCounter;                //Счетчик для вычисления depth
        private int tmpBackstepCounter;             //Счетчик для вычисления backstep

        public ZigZagService(int depth, decimal deviation, int backstep, PaintService paintService)
        {
            this.depth = depth;
            this.deviation = deviation;
            this.backstep = backstep;
            this.paintService = paintService;

            tmpDepthCounter = 0;
            tmpBackstepCounter = 0;
        }

        public void calcNewCandle(Candle candle)
        {
            //++tmpBackstepCounter;
            if (lastExtremum == null)
            {
                lastExtremum = candle;
                return;
            }
            paintService.deleteChartElements(tmpLine2);
            tmpLine2 = null;
            if (tmpExtremum == null)
            {
                findFirstExtremum(candle); 
                return;
            }

            //если темповая точка вверх, и текущая свеча еще выше, пересохраняем темповую
            if (tmpTrendDirection == TrendDirection.UP && candle.High > tmpExtremum.High) {
                ++tmpDepthCounter;
                tmpExtremum = candle;
                paintService.deleteChartElements(tmpLine1);
                tmpLine1 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.Low, tmpExtremum.TimeStart, tmpExtremum.High, "", Color.Yellow, 3);
            }
            //если темповая точка вниз, и текущая свеча еще ниже, пересохраняем темповую
            else if (tmpTrendDirection == TrendDirection.DOWN && candle.Low < tmpExtremum.Low)
            {
                ++tmpDepthCounter;
                tmpExtremum = candle;
                paintService.deleteChartElements(tmpLine1);
                tmpLine1 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.High, tmpExtremum.TimeStart, tmpExtremum.Low, "", Color.Yellow, 3);
            }
            //если темповая точка вверх, а текущая свеча ниже на процент, то фиксируем основную и новую темповую
            else if (tmpTrendDirection == TrendDirection.UP && candle.Low < MathService.getValueSubtractPercent(tmpExtremum.Low, deviation)) {
                lastExtremum = tmpExtremum;
                tmpExtremum = candle;
                tmpLine1 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.High, tmpExtremum.TimeStart, tmpExtremum.Low, "", Color.Yellow, 3);
                tmpTrendDirection = TrendDirection.DOWN;
            }
            //если темповая точка вниз, а текущая свеча выше на процент, то фиксируем основную и новую темповую
            else if (tmpTrendDirection == TrendDirection.DOWN && candle.Low > MathService.getValueSubtractPercent(tmpExtremum.High, deviation))
            {
                lastExtremum = tmpExtremum;
                tmpExtremum = candle;
                tmpLine1 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.Low, tmpExtremum.TimeStart, tmpExtremum.High, "", Color.Yellow, 3);
                tmpTrendDirection = TrendDirection.UP;
            }
            else 
            {
                //если темповая точка вверх, а текущая вниз, но несильно, рисуем вторую темповую черту
                if (tmpTrendDirection == TrendDirection.UP)
                {
                    tmpLine2 = paintService.paintLine(tmpExtremum.TimeStart, tmpExtremum.High, candle.TimeStart, candle.Low, "", Color.Aqua, 3);
                }
                //если темповая точка вниз, а текущая вверх, но несильно, рисуем вторую темповую черту
                else if (tmpTrendDirection == TrendDirection.DOWN)
                {
                    tmpLine2 = paintService.paintLine(tmpExtremum.TimeStart, tmpExtremum.Low, candle.TimeStart, candle.High, "", Color.Aqua, 3);
                }
            }
        }


        private void findFirstExtremum(Candle candle)
        {
            ++tmpDepthCounter;
            if (candle.High > MathService.getValueAddPercent(lastExtremum.High, deviation))
            {
                tmpTrendDirection = TrendDirection.UP;
                tmpExtremum = candle;
                tmpLine2 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.Low, tmpExtremum.TimeStart, tmpExtremum.High, "", Color.Yellow, 3);
            }
            else if (candle.Low < MathService.getValueSubtractPercent(lastExtremum.Low, deviation))
            {
                tmpTrendDirection = TrendDirection.DOWN;
                tmpExtremum = candle;
                tmpLine2 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.High, tmpExtremum.TimeStart, tmpExtremum.Low, "", Color.Yellow, 3);
            }
            else
            {
                if(depth > tmpDepthCounter)
                {
                    return;
                }
                if (candle.High > lastExtremum.High)
                {
                    tmpDepthCounter = 0;
                    tmpLine2 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.Low, candle.TimeStart, candle.High, "", Color.Yellow, 3);
                }
                else if (candle.Low < lastExtremum.Low)
                {
                    tmpDepthCounter = 0;
                    tmpLine2 = paintService.paintLine(lastExtremum.TimeStart, lastExtremum.High, candle.TimeStart, candle.Low, "", Color.Yellow, 3);
                }
            }
        }
    }

    enum TrendDirection
    {
        UP,
        DOWN
    }
}
