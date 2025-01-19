using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    internal class Construction
    {
        private bool complete;

        public string Name { get; private set; }
        public bool Permanent { get; private set; }
        private Decision.DecisionDelegate completeFunction { get; set; }
        public Decision.DecisionDelegate UnderConstructionFunction { get; private set; }
        public bool Complete 
        {
            get
            {
                if ((Permanent && complete) || completeFunction())
                {
                    complete = true;
                    return true;
                }

                return false;
            }
        }
        public Dictionary<string, int> MaterialsNeeded { get; private set; }
        public Construction? PrequisiteConstruction { get; private set; }
        public int BuildCost { get; private set; }


        public Construction(string name, BuildingData buildingData, bool permament, Decision.DecisionDelegate completeFunction, Decision.DecisionDelegate underConstructionFunction, Construction? prequisiteConstruction = null) :
            this(name, permament, completeFunction, underConstructionFunction, buildingData.BuildCost, 
                buildingData.BuildMaterials.ToDictionary(material => material.ItemId, material => material.Amount), 
                prequisiteConstruction)
        { }

        public Construction(string name, bool permament, Decision.DecisionDelegate completeFunction, Decision.DecisionDelegate underConstructionFunction, int buildCost, Dictionary<TaskManager.ItemName, int> materialNeeded, Construction? prequisiteConstruction = null)
            : this(name, permament, completeFunction, underConstructionFunction, buildCost,
               materialNeeded.ToDictionary(kv => TaskManager.ItemIds[kv.Key], kv => kv.Value),
               prequisiteConstruction) 
        { }

        /// <summary>
        /// An object that tells the player what they are trying to build / upgrade
        /// </summary>
        /// <param name="name">The name of the building construction</param>
        /// <param name="permament">If this construction is permanent</param>
        /// <param name="completeFunction">function that tells if the construction is complete</param>
        /// <param name="underConstructionFunction">function that tells if construction is under construction</param>
        /// <param name="buildCost">the amount of gold required in order to start the construction</param>
        /// <param name="materialNeeded">The materials needed in order to make the construction. Key being the unqualified id of the material. Value is the number of said </param>
        /// <param name="prequisiteConstruction">The task that needs to be done in order to do this one</param>
        /// 
        public Construction(string name, bool permament, Decision.DecisionDelegate completeFunction, Decision.DecisionDelegate underConstructionFunction, int buildCost, Dictionary<string, int> materialNeeded, Construction? prequisiteConstruction = null) 
        {
            Name = name;
            this.completeFunction = completeFunction;
            BuildCost = buildCost;
            UnderConstructionFunction = underConstructionFunction;
            Permanent = permament;
            MaterialsNeeded = materialNeeded;
            PrequisiteConstruction = prequisiteConstruction;
        }
    }
}
