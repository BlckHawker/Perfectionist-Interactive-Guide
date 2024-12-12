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
        private bool permanent;
        private bool complete;

        //the method that will decide if we go with the true or false nodex
        public DecisionDelegate checkTask;




        /// <summary>
        /// 
        /// </summary>
        /// <param name="trueNode">The the node that will be travers</param>
        /// <param name="falseNode"></param>
        /// <param name="checkTask"></param>
        /// <param name="permanent"></param>
        public Decision(DecisionTreeNode trueNode, DecisionTreeNode falseNode, DecisionDelegate checkTask, bool permanent = false)
        {
            this.trueNode = trueNode;
            this.falseNode = falseNode;
            this.checkTask = checkTask;
            this.permanent = permanent;
        }

        public Decision(DecisionDelegate checkTask, bool permanent = false) : this(null, null, checkTask, permanent) { }

        //Perform the test
        private DecisionTreeNode GetBranch()
        {
            if ((permanent && complete) || checkTask())
            {
                complete = true;
                return trueNode;
            }

            return falseNode;
        }

        // Recursively walk through the tree
        public override DecisionTreeNode MakeDecision()
        {
            //Make the decision and recurse based on the result.
            DecisionTreeNode branch = GetBranch();
            return branch.MakeDecision();
        }

        public void SetTrueNode(DecisionTreeNode trueNode)
        {
            this.trueNode = trueNode;
        }

        public void SetFalseNode(DecisionTreeNode falseNode)
        {
            this.falseNode = falseNode;
        }
    }
}
