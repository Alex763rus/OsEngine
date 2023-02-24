using OsEngine.Charts.CandleChart.Elements;
using OsEngine.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class DealSupport
    {
        private Side side;
        private GroupParametersTrading groupParametersTrading;
        private ProcessState processState;
        private Position position;
        private IChartElement[] chartElements;
        private int chartCounter;
        private int counterBar;

        public DealSupport(Side side)
        {
            this.side = side;
            reset();
        }

        public void dealSupportUpdate(GroupParametersTrading groupParametersTrading, ProcessState processState, Position position)
        {
            this.groupParametersTrading = groupParametersTrading;
            this.processState = processState;
            this.position = position;
            chartElements = new IChartElement[100];
            chartCounter = 0;
            counterBar = 0;
        }

        public void addCounterBar()
        {
            counterBar = counterBar + 1;
        }
        public int getCounterBar()
        {
            return counterBar;
        }
        public Side getSide()
        {
            return side;
        }
        public void reset()
        {
            processState = ProcessState.FREE;
            chartElements = new IChartElement[100];
            chartCounter = 0;
            counterBar = 0;
            position = null;
        }
        public string getGroupType()
        {
            if(groupParametersTrading == null)
            {
                return "";
            }
            return groupParametersTrading.getGroupType().ToString();
        }

        public IChartElement[] getChartElements()
        {
            return chartElements;
        }

        public void addChartElement(IChartElement[] elements)
        {
            for(int i = 0; i < elements.Length; i = i + 1)
            {
                chartElements[chartCounter] = elements[i];
                ++chartCounter;
            }
        }

        public void addChartElement(IChartElement element)
        {
            chartElements[chartCounter] = element;
            ++chartCounter;
        }
        public void setProcessState(ProcessState processState)
        {
            this.processState = processState;
        }
        public ProcessState getProcessState()
        {
            return processState;
        }
        public GroupParametersTrading getGroupParametersTrading()
        {
            return groupParametersTrading;
        }
        public void setGroupParametersTrading(GroupParametersTrading groupParametersTrading)
        {
            this.groupParametersTrading = groupParametersTrading;
        }

        public bool hasPosition()
        {
            return position != null;
        }

        public Position getPosition()
        {
            return position;
        }

        public int getPositionNumber()
        {
            if (position != null)
            {
                return position.Number;
            }
            return 0;
        }
        public void setPosition(Position position)
        {
            this.position = position;
        }

        public int getChartElementCount()
        {
            int count = 0;
            for (int i = 0; i < chartElements.Length; ++i)
            {
                if (chartElements[i] != null)
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

        public int getCountBarForClose()
        {
            return groupParametersTrading.getCountBarForClose();
        }
    }
}
