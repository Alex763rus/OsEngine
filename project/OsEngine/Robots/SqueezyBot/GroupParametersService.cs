using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.SqueezyBot
{
    public class GroupParametersService
    {

        private List<GroupParameters> groupParametersList;

        public GroupParametersService()
        {
            groupParametersList = new List<GroupParameters>();
        }

        public void addGroupParameters(GroupParameters groupParameters)
        {
            groupParametersList.Add(groupParameters);
        }

        public GroupParameters getGroupParameters(GroupType groupType)
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

        public GroupParameters getGroupParameters(String groupType)
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
