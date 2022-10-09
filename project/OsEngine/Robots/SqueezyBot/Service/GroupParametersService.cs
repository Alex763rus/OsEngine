using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    public class GroupParametersService
    {

        private List<GroupParametersRuler> groupParametersList;

        public GroupParametersService()
        {
            groupParametersList = new List<GroupParametersRuler>();
        }

        public void addGroupParameters(GroupParametersRuler groupParameters)
        {
            groupParametersList.Add(groupParameters);
        }

        public GroupParametersRuler getGroupParameters(GroupType groupType)
        {
            foreach (var groupParameters in groupParametersList)
            {
                if(groupParameters.getGroupType() == groupType)
                {
                    return groupParameters;
                }
            }
            return null;
        }

        public GroupParametersRuler getGroupParameters(String groupType)
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
    }
}
