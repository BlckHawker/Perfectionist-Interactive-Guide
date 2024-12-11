using static Stardew_100_Percent_Mod.TaskManager;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Class that is similar to "CraftingRecipe", but "recipeList" keys will use "QualifiedItemId" isntead of "recipeList"
    /// </summary>
    internal class DummyCraftingRecipe: DummyRecipe
    {
        DummyCraftingRecipe(string name, Dictionary<string, int> recipeList) : base(name, recipeList) { }

        /// <summary>
        /// Get all of the crafting recipes
        /// </summary>
        /// <returns></returns>
        public static List<DummyCraftingRecipe> GetAllRecipes()
        {
            TaskManager tm = Instance;

            return new List<DummyCraftingRecipe>()
            {

                //120 wood, 2 copper bars
                { new DummyCraftingRecipe("Big Chest", new Dictionary<string, int>() { { tm.ItemIds[ItemName.Wood], 120 }, { tm.ItemIds[ItemName.CopperBar], 2 }  }) },

                //50 wood
                { new DummyCraftingRecipe("Chest", new Dictionary<string, int>() { { tm.ItemIds[ItemName.Wood], 50 }  }) }
            };

        }
    }
}
