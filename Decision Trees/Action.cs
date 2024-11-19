using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    internal class Action : DecisionTreeNode
    {
        public string DisplayName { get; private set; }

        public Action(string displayName)
        {
            DisplayName = displayName;
        }

        public override DecisionTreeNode MakeDecision()
        {
            return this;
        }

    }
}
