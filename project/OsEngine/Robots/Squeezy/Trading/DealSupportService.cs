using OsEngine.Charts.CandleChart.Elements;
using OsEngine.Entity;
using OsEngine.Market.Servers.GateIo.Futures.Response;
using OsEngine.Robots.Squeezy.Trading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service
{
    public class DealSupportService
    {
        private DealSupport dealSupportBuy;
        private DealSupport dealSupportSell;

        public DealSupportService()
        {
            dealSupportBuy = new DealSupport();
            dealSupportSell = new DealSupport();
        }
        public void addChartElementSell(IChartElement[] elements)
        {
            dealSupportSell.addChartElement(elements);
        }
        public void addChartElementBuy(IChartElement[] elements)
        {
            dealSupportBuy.addChartElement(elements);
        }
        public void addChartElementSell(IChartElement element)
        {
            dealSupportSell.addChartElement(element);
        }
        public void addChartElementBuy(IChartElement element)
        {
            dealSupportBuy.addChartElement(element);
        }
        public IChartElement[] getChartElementsSell()
        {
            return dealSupportSell.getChartElements();
        }
        public int getChartElementsBuyCount()
        {
            IChartElement[] elements = getChartElementsBuy();
            return getChartElementCount(elements);
        }
        public int getChartElementsSellCount()
        {
            IChartElement[] elements = getChartElementsSell();
            return getChartElementCount(elements);
        }
        private int getChartElementCount(IChartElement[] elements)
        {
            int count = 0;
            for (int i = 0; i < elements.Length; ++i)
            {
                if (elements[i] != null)
                {
                    ++count;
                }
                else
                {
                    return count;
                }
            }
            return 0;
        }
        public IChartElement[] getChartElementsBuy()
        {
            return dealSupportBuy.getChartElements();
        }

        public Position getSellPosition()
        {
            return dealSupportSell.getPosition();
        }
        public Position getBuyPosition()
        {
            return dealSupportBuy.getPosition();
        }
        public DealSupport getDealSupportBuy()
        {
            return dealSupportBuy;
        }

        public DealSupport getDealSupportSell()
        {
            return dealSupportSell;
        }

        public ProcessState getProcessState(Side side)
        {
            if (side == Side.Sell)
            {
                return dealSupportSell.getProcessState();
            }
            return dealSupportBuy.getProcessState();
        }
        public void setProcessStateSell(ProcessState processState)
        {
            dealSupportSell.setProcessState(processState);
        }
        public void setProcessStateBuy(ProcessState processState)
        {
            dealSupportBuy.setProcessState(processState);
        }
        public void saveNewLimitPosition(Side side, ProcessState processState, GroupParametersTrading groupParametersTrading, Position position)
        {
            if (side == Side.Sell)
            {
                dealSupportSell.dealSupportUpdate(groupParametersTrading, processState, position);
            }
            else if(side == Side.Buy)
            {
                dealSupportBuy.dealSupportUpdate(groupParametersTrading, processState, position);
            }
        }
        public void dealSupportResetSell()
        {
            dealSupportSell.dealSupportUpdate(null, ProcessState.WAIT_TRIGGER_START, null);
        }
        public void dealSupportResetBuy()
        {
            dealSupportBuy.dealSupportUpdate(null, ProcessState.WAIT_TRIGGER_START, null);
        }

        public string getGroupTypeSell()
        {
            return dealSupportSell.getGroupType();
        }

        public string getGroupTypeBuy()
        {
            return dealSupportBuy.getGroupType();
        }
    }
}
