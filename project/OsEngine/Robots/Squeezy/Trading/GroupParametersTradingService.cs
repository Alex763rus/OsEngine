using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OsEngine.Robots.Squeezy.Trading
{
    public class GroupParametersTradingService
    {
        private List<GroupParametersTrading> groupParametersList;

        public GroupParametersTradingService()
        {
            groupParametersList = new List<GroupParametersTrading>();
        }

        public void addGroupParameters(GroupParametersTrading groupParameters)
        {
            groupParametersList.Add(groupParameters);
        }

        public GroupParametersTrading getGroupParameters(GroupType groupType)
        {
            foreach (var groupParameters in groupParametersList)
            {
                if (groupParameters.getGroupType() == groupType)
                {
                    return groupParameters;
                }
            }
            return null;
        }

        public GroupParametersTrading getGroupParameters(String groupType)
        {
            foreach (var groupParameters in groupParametersList)
            {
                if (groupParameters.getGroupType().ToString().Equals(groupType))
                {
                    return groupParameters;
                }
            }
            return null;
        }

        public List<GroupParametersTrading> getGroupsParameters()
        {
            return groupParametersList;
        }

    }
}
