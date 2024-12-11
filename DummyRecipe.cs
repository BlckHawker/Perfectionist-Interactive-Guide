using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    internal abstract class DummyRecipe
    {
        //the name of the item that will be crafted
        public string Name { get; private set; }

        //the items needed in order to create the recipie
        //key is the "QualifiedItemId" of the item while the value is the amount needed
        public Dictionary<string, int> RecipeList { get; private set; }

        protected DummyRecipe(string name, Dictionary<string, int> recipeList)
        {
            Name = name;
            RecipeList = recipeList;
        }
    }
}
