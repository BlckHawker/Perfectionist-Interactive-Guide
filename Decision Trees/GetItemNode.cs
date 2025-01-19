using StardewValley.ItemTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    /// <summary>
    /// Used as a starting place when the player needs to get an item. Updates the RequiredItemsDictionary
    /// </summary>
    internal class GetItemNode : DecisionTreeNode
    {
        private readonly DecisionTreeNode nextNode;
        private readonly string qualifiedItemId;
        private readonly int desiredCount;

        public GetItemNode(DecisionTreeNode nextNode, string qualifiedItemId, int desiredCount) 
        {
            this.nextNode = nextNode;
            this.qualifiedItemId = qualifiedItemId;
            this.desiredCount = desiredCount;
        }

        public override DecisionTreeNode MakeDecision()
        {
            TaskManager.UpdateRequiredItemsDictionary(qualifiedItemId, desiredCount);
            return nextNode.MakeDecision();
        }
    }
}
