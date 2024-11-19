using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    internal class Decision : DecisionTreeNode
    {
        public delegate bool DecisionDelegate();

        private DecisionTreeNode trueNode;
        private DecisionTreeNode falseNode;

        //the method that will decide if we go with the true or false nodex
        public DecisionDelegate checkTask;



        public Decision(DecisionTreeNode trueNode, DecisionTreeNode falseNode, DecisionDelegate checkTask)
        {
            this.trueNode = trueNode;
            this.falseNode = falseNode;
            this.checkTask = checkTask;
        }

        //Perform the test
        private DecisionTreeNode GetBranch()
        {
            return checkTask() ? trueNode : falseNode;
        }

        // Recursively walk through the tree
        public override DecisionTreeNode MakeDecision()
        {
            //Make the decision and recurse based on the result.
            DecisionTreeNode branch = GetBranch();
            return branch.MakeDecision();
        }
    }
}
