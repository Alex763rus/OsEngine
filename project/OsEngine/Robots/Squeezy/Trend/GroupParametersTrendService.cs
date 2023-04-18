using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OsEngine.Robots.Squeezy.Trend
{
    public class GroupParametersTrendService
    {
        private List<GroupParametersTrend> groupParametersList;

        public GroupParametersTrendService()
        {
            groupParametersList = new List<GroupParametersTrend>();
        }

        public void addGroupParameters(GroupParametersTrend groupParameters)
        {
            groupParametersList.Add(groupParameters);
        }

        public GroupParametersTrend getGroupParameters(GroupType groupType)
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

        public GroupParametersTrend getGroupParameters(String groupType)
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

        public List<GroupParametersTrend> getGroupsParameters()
        {
            return groupParametersList;
        }

    }
}
