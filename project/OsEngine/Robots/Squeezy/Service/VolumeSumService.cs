
using OsEngine.Entity;
using OsEngine.Robots.SqueezyBot.Service;
using System;
using System.Text;

namespace OsEngine.Robots.Squeezy.Service
{
    public class VolumeSumService
    {
        private LogService logService;
        private int levelBuy;
        private int levelSell;
        private decimal [] calculatedSum;
        private int [] coeffForCalculateSum;

        public VolumeSumService(decimal volumeSum, int coeffMonkey, LogService logService)
        {
            this.logService = logService;
            calculateAndSetVolumeSum(volumeSum, coeffMonkey);
        }

        private void calculateAndSetVolumeSum(decimal volumeSum, int coeffMonkey)
        {
            coeffForCalculateSum = new int[coeffMonkey];
            int coeffSum = 0;
            for (int i = 0; i < coeffForCalculateSum.Length; ++i)
            {
                if (i == 0)
                {
                    coeffForCalculateSum[i] = 1;
                }
                else if (i == 1)
                {
                    coeffForCalculateSum[i] = 2;
                }
                else
                {
                    coeffForCalculateSum[i] = coeffForCalculateSum[i - 2] + coeffForCalculateSum[i - 1];
                }
                coeffSum = coeffSum + coeffForCalculateSum[i];
            }
            levelBuy = 0;
            levelSell = 0;
            calculatedSum = new decimal[coeffForCalculateSum.Length];
            for (int i = 0; i < calculatedSum.Length; ++i)
            {
                calculatedSum[i] = volumeSum * coeffForCalculateSum[i]/coeffSum;
            }

            StringBuilder sb = new StringBuilder("Настройки размера ставок:");
            for (int i = 0; i < coeffForCalculateSum.Length; ++i)
            {
                sb.Append(i).Append(") ").Append(coeffForCalculateSum[i]).Append(" - ").Append(calculatedSum[i]).Append(", ");
            }
            sb.Append("всего уровней: ").Append(coeffForCalculateSum.Length);
            logService.sendLogSystem(sb.ToString());
        }
        public decimal getVolumeSum(Side side)
        {
            decimal volumeSum = 0;
            if (side == Side.Buy)
            {
                volumeSum = calculatedSum[levelBuy];
            }
            else
            {
                volumeSum = calculatedSum[levelSell];
            }

            logService.sendLogSystem("VolumeSumService: levelBuy = " + levelBuy + ", levelSell = " + levelSell + ", side = " + side  + ", volumeSum = " + volumeSum);
            return volumeSum;
        }

        public void updateLevel(Side side, bool isProfit)
        {
            if (isProfit)
            {
                resetLevel(side);
            } else {
                upLevel(side);
            }
        }
        public void resetLevel(Side side)
        {
            if (side == Side.Buy)
            {
                levelBuy = 0;
            }
            if (side == Side.Sell)
            {
                levelSell = 0;
            }
            logService.sendLogSystem("Последняя сделка по " + side + " закрылась в плюс."
                + "Новый уровень Buy:" + levelBuy + ", % суммы :" + coeffForCalculateSum[levelBuy] + ", сумма Buy:" + calculatedSum[levelBuy]
                + " Новый уровень Sell:" + levelSell + ", % суммы:" + coeffForCalculateSum[levelSell] + ", сумма Sell:" + calculatedSum[levelSell]);
        }

        public void upLevel(Side side)
        {
            if (side == Side.Buy && levelBuy < (calculatedSum.Length - 1))
            {
                ++levelBuy;
            }
            if (side == Side.Sell && levelSell < (calculatedSum.Length - 1))
            {
                ++levelSell;
            }
            logService.sendLogSystem("Последняя сделка по " + side + " закрылась в минус."
                + "Новый уровень Buy:" + levelBuy + ", % суммы :" + coeffForCalculateSum[levelBuy] + ", сумма Buy:" + calculatedSum[levelBuy]
                + " Новый уровень Sell:" + levelSell + ", % суммы:" + coeffForCalculateSum[levelSell] + ", сумма Sell:" + calculatedSum[levelSell]
                );
        }
    }
}
