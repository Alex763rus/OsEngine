﻿using OsEngine.Entity;
using OsEngine.Robots.Squeezy.Tester;
using System;

namespace OsEngine.Robots.Squeezy.Trend
{
    public class GroupParametersTrend : GroupParametersTester
    {

        public GroupParametersTrend(GroupType groupType, StrategyParameterBool groupOn, StrategyParameterDecimal triggerCandleDiff, StrategyParameterDecimal takeProfit, StrategyParameterDecimal stopLoss, StrategyParameterInt countBarForClose)
            :base(groupType, groupOn, triggerCandleDiff, takeProfit, stopLoss, countBarForClose)
         { 
        }
        public new string getAllGroupParameters()
        {
            string settings = base.getAllGroupParameters()
                            ;
            return settings;
        }


    }
}
