using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Stardew_100_Percent_Mod.TaskManager;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Class that is similar to "CraftingRecipe", but "recipeList" keys will use "QualifiedItemId" isntead of "recipeList"
    /// </summary>
    internal class DummyCookingRecipe : DummyRecipe
    {
        public string UnqualifiedItemId { get; private set; }
        DummyCookingRecipe(string name, Dictionary<string, int> recipeList) : base(name, recipeList) 
        {
            UnqualifiedItemId = new CraftingRecipe(name).getIndexOfMenuView();
        }

        /// <summary>
        /// Get all of the crafting recipes
        /// </summary>
        /// <returns></returns>
        public static List<DummyCookingRecipe> GetAllRecipes()
        {
            TaskManager tm = TaskManager.Instance;
            return new List<DummyCookingRecipe>()
            {
                //1 egg, 1 milk
                { new DummyCookingRecipe("Omelet", new Dictionary<string, int>() { { tm.ItemIds[ItemName.Egg], 1 }, { tm.ItemIds[ItemName.Milk], 1 }  }) },
            };

        }
    }
}
