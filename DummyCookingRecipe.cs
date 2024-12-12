using StardewValley;
using StardewValley.Tools;
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
        DummyCookingRecipe(string name, List<Dictionary<string, int>> recipeLists) : base(name, recipeLists) 
        {
            UnqualifiedItemId = new CraftingRecipe(name).getIndexOfMenuView();
        }

        /// <summary>
        /// Get all of the crafting recipes
        /// </summary>
        /// <returns></returns>
        public static List<DummyCookingRecipe> GetAllRecipes()
        {
            TaskManager tm = Instance;

            Dictionary<string, int> eggsDictionary = tm.EggList.ToDictionary(e => tm.ItemIds[e], e => 1);
            Dictionary<string, int> milkDictionary = tm.MilkList.ToDictionary(e => tm.ItemIds[e], e => 1);


            List<Dictionary<string, int>> omeletList = new List<Dictionary<string, int>>
            {
                //1 egg (any)
                eggsDictionary,
                //1 milk(any)
                milkDictionary
            };

            return new List<DummyCookingRecipe>()
            {
                { new DummyCookingRecipe("Omelet", omeletList )}
            };

        }
    }
}
