using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Class that is similar to "CraftingRecipe", but "recipeList" keys will use "QualifiedItemId" isntead of "recipeList"
    /// </summary>
    internal class DummyCraftingRecipe
    {
        //the name of the item that will be crafted
        public string Name { get; private set; }

        //the items needed in order to create the recipie
        //key is the "QualifiedItemId" of the item while the value is the amount needed
        public Dictionary<string, int> RecipeList { get; private set; }

        DummyCraftingRecipe(string name, Dictionary<string, int> recipeList)
        { 
            Name = name;
            RecipeList = recipeList;
        }

        /// <summary>
        /// Get all of the crafting recipes
        /// </summary>
        /// <returns></returns>
        public static List<DummyCraftingRecipe> GetAllRecipes()
        {
            return new List<DummyCraftingRecipe>()
            {
                //120 wood, 2 copper bars
                { new DummyCraftingRecipe("Big Chest", new Dictionary<string, int>() { { "(O)388", 120 }, {"(O)344", 2 }  }) },

                //50 wood
                { new DummyCraftingRecipe("Chest", new Dictionary<string, int>() { { "(O)388", 50 }  }) }
            };

        }
    }
}
