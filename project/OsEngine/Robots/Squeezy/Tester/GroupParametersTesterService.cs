using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Tester
{
    public class GroupParametersTesterService
    {

        private List<GroupParametersTester> groupParametersList;

        public GroupParametersTesterService()
        {
            groupParametersList = new List<GroupParametersTester>();
        }

        public void addGroupParameters(GroupParametersTester groupParameters)
        {
            groupParametersList.Add(groupParameters);
        }

        public GroupParametersTester getGroupParameters(GroupType groupType)
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

        public GroupParametersTester getGroupParameters(String groupType)
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

        public List<GroupParametersTester>  getGroupsParameters()
        {
            return groupParametersList;
        }
    }
}
