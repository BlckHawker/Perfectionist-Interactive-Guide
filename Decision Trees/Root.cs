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
        private string itemId;
        private int desiredCount;

        public Root(DecisionTreeNode nextNode, string itemId, int desiredCount) 
        {
            this.nextNode = nextNode;
            this.itemId = itemId;
            this.desiredCount = desiredCount;
        }

        public override DecisionTreeNode MakeDecision()
        {
            TaskManager.Instance.UpdateRequiredItemsDictionary(itemId, desiredCount);
            return nextNode.MakeDecision();
        }
    }
}
