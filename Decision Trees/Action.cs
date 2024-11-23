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

        public delegate string ChangeDisplayNameDelegate();

        //this should only be used if the string paramter is a GameLocation's NameOrUniqueName
        public delegate string ChangeDisplayNameDelegateWithLocationParamter(string location);

        public ChangeDisplayNameDelegate changeDisplayNameMethod { get; private set; }

        public Action(string displayName)
        {
            DisplayName = displayName;
        }

        public Action(ChangeDisplayNameDelegate changeDisplayNameMethod)
        { 
            this.changeDisplayNameMethod = changeDisplayNameMethod;
        }

        public override DecisionTreeNode MakeDecision()
        {
            return this;
        }

    }
}
