﻿using StardewValley;
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
        private DummyRecipe recipe;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipe">The recipe we are trying to make</param>
        /// <param name="cooking">if the recipe is a cooking item</param>
        public CraftItemAction(CraftingRecipe recipe, bool cooking)
        {
            if (cooking)
            {
                this.recipe = DummyCookingRecipe.GetAllRecipes().First(r => r.Name == recipe.DisplayName);
            }
            else
            { 
                this.recipe = DummyCraftingRecipe.GetAllRecipes().First(r => r.Name == recipe.DisplayName);
            }
        }



        /// <summary>
        ///Tells the user to get an item they're missing for the recipe
        /// </summary>
        /// <returns>A tree of getting the missing item</returns>
        public override DecisionTreeNode MakeDecision()
        {
            //it's garunteed that the list is not empty. Otherwise this Action would have not been created
            string id = TaskManager.GetRecipeMissingItems(recipe)[0];

            //get the first dicttionary that has the item id
            var dictoionary = recipe.RecipeLists.First(d => d.ContainsKey(id));

            return TaskManager.GetProducableItemTree(id, dictoionary[id]).MakeDecision();
        }
    }
}
