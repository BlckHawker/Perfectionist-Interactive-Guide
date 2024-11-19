using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    internal abstract class DecisionTreeNode
    {
        //Recursively walk through the tree.
        public abstract DecisionTreeNode MakeDecision();
    }
}
