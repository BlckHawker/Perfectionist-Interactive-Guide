using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    /// <summary>
    /// Used to handle crafting a recipe
    /// </summary>
    internal class CraftItemAction : DecisionTreeNode
    {
        private DummyCraftingRecipe recipe;
        public CraftItemAction(CraftingRecipe recipe)
        {
            this.recipe = DummyCraftingRecipe.GetAllRecipes().First(r => r.Name == recipe.DisplayName);
        }



        /// <summary>
        ///Tells the user to get an item they're missing for the recipe
        /// </summary>
        /// <returns>A tree of getting the missing item</returns>
        public override DecisionTreeNode MakeDecision()
        {
            //it's garunteed that the list is not empty. Otherwise this Action would have not been created
            string id = TaskManager.Instance.GetRecipeMissingItems(recipe)[0];

            return TaskManager.GetProducableItemTree(id, recipe.RecipeList[id]).MakeDecision();
        }
    }
}
