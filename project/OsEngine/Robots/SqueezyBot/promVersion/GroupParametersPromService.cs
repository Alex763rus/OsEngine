using OsEngine.Robots.SqueezyBot.rulerVersion;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsEngine.Robots.SqueezyBot.promVersion
{
    public class GroupParametersPromService
    {
        private List<GroupParametersProm> groupParametersList;

        public GroupParametersPromService()
        {
            groupParametersList = new List<GroupParametersProm>();
        }

        public void addGroupParameters(GroupParametersProm groupParameters)
        {
            groupParametersList.Add(groupParameters);
        }

        public GroupParametersProm getGroupParameters(GroupType groupType)
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

        public GroupParametersProm getGroupParameters(String groupType)
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

        public List<GroupParametersProm> getGroupsParameters()
        {
            return groupParametersList;
        }

    }
}
