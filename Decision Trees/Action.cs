using Stardew_100_Percent_Mod.Decision_Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    internal class Action : DecisionTreeNode
    {
        public string DisplayName
        {
            get
            {
                if (changeDisplayNameMethod != null)
                {
                    displayName = changeDisplayNameMethod();
                }

                return displayName;
            }
        }

        private string? displayName;

        public delegate string ChangeDisplayNameDelegate();

        protected ChangeDisplayNameDelegate changeDisplayNameMethod { get;  set; }


        public Action(string displayName)
        {
            this.displayName = displayName;
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
