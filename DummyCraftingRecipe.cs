using StardewValley.Buildings;
using System.Collections.Generic;
using static Stardew_100_Percent_Mod.TaskManager;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Class that is similar to "CraftingRecipe", but "recipeList" keys will use "QualifiedItemId" isntead of "recipeList"
    /// </summary>
    internal class DummyCraftingRecipe: DummyRecipe
    {
        DummyCraftingRecipe(string name, List<Dictionary<string, int>> recipeList) : base(name, recipeList) { }

        /// <summary>
        /// Get all of the crafting recipes
        /// </summary>
        /// <returns></returns>
        public static List<DummyCraftingRecipe> GetAllRecipes()
        {
            TaskManager tm = Instance;

            return new List<DummyCraftingRecipe>()
            {
                { 
                    new DummyCraftingRecipe("Big Chest", 
                    //Recipe
                    new List<Dictionary<string, int>>()
                    {
                        //120 wood
                        new Dictionary<string, int>() { { tm.ItemIds[ItemName.Wood], 120 } },
                        //2 copper bars
                        new Dictionary<string, int>() { { tm.ItemIds[ItemName.CopperBar], 2 }}
                    }) 
                },

                { 
                    new DummyCraftingRecipe("Chest", 
                    //Recipe
                    new List<Dictionary<string, int>>()
                    {
                        //50 wood
                        new Dictionary<string, int>() { { tm.ItemIds[ItemName.Wood], 50 } },
                    }) 
                }
            };

        }
    }
}
