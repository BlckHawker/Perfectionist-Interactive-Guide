using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    //posibly remove this class since it's pretty redundant compared to a regular Action object
    internal class CraftItemAction : DecisionTreeNode
    {
        private CraftingRecipe recipe;
        public CraftItemAction(CraftingRecipe recipe)
        { 
            this.recipe = recipe;
        }

        public override DecisionTreeNode MakeDecision()
        {
            return new Action($"Craft {recipe.DisplayName}");
        }
    }
}
