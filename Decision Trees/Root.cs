using StardewValley.ItemTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    internal class Root : DecisionTreeNode
    {
        private DecisionTreeNode nextNode;
        private string qualifiedItemId;
        private int desiredCount;

        public Root(DecisionTreeNode nextNode, string qualifiedItemId, int desiredCount) 
        {
            this.nextNode = nextNode;
            this.qualifiedItemId = qualifiedItemId;
            this.desiredCount = desiredCount;
        }

        public override DecisionTreeNode MakeDecision()
        {
            TaskManager.Instance.UpdateRequiredItemsDictionary(qualifiedItemId, desiredCount);
            return nextNode.MakeDecision();
        }
    }
}
