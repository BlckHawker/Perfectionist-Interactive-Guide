using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    internal class CraftItemAction : DecisionTreeNode
    {
        private CraftingRecipe recipe;
        public CraftItemAction(CraftingRecipe recipe)
        { 
            this.recipe = recipe;
        }

        public override DecisionTreeNode MakeDecision()
        {
            //reserve the ingrediants needed to craft this recipe
            //reserve the item that is needed to craft
            foreach (KeyValuePair<string, int> kv in TaskManager.Instance.inventoryItemReserveDictonary)
            {
                TaskManager.Instance.inventoryItemReserveDictonary[kv.Key] -= kv.Value;
            }

            return new Action($"Craft {recipe.DisplayName}");
        }
    }
}
