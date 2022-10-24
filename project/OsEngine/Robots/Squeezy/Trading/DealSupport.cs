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
        private GroupParametersTrading groupParametersTrading;
        private ProcessState processState;
        private Position position;
        private IChartElement[] chartElements;
        private int chartCounter;

        public DealSupport()
        {
            processState = ProcessState.WAIT_TRIGGER_START;
            chartElements = new IChartElement[100];
            chartCounter = 0;
        }

        public void dealSupportUpdate(GroupParametersTrading groupParametersTrading, ProcessState processState, Position position)
        {
            this.groupParametersTrading = groupParametersTrading;
            this.processState = processState;
            this.position = position;
            chartElements = new IChartElement[100];
            chartCounter = 0;
        }
        public string getGroupType()
        {
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

        public Position getPosition()
        {
            return position;
        }

        public void setPosition(Position position)
        {
            this.position = position;
        }
    }
}
