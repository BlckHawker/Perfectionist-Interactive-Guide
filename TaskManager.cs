using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using static Stardew_100_Percent_Mod.TaskManager;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Manages the Tree of of taks needed in order to 100% the game
    /// </summary>
    internal class TaskManager
    {
        public static TaskManager Instance;
        //todo implement a selector and sequence ckasses
        private List<Task> tree;
        public List<DecisionTreeNode> roots { get; private set; }

        public List<Task> avaibleTasks;

        public delegate bool TaskComplateDelegate();
        public delegate string UpdateTaskDisplayNameDelegate(Task t);

        public delegate void LogMethod(string message, LogLevel logLevel = LogLevel.Debug);
        public LogMethod logMethod;

        private Action completeAction;



        public TaskManager()
        {

        }

        public static void InitalizeInstance(LogMethod logMethod)
        {
            Instance = new TaskManager();

            Instance.logMethod = logMethod;

            Instance.completeAction = new Action("");

            DecisionTreeNode parsnipSeedsTree = GetParsnipSeedsTree(15);

            DecisionTreeNode becomeFriendsWithSebastian = BecomeFriendsWithSebastian();

            Instance.roots = new List<DecisionTreeNode>(new[]{ parsnipSeedsTree, becomeFriendsWithSebastian });
        }

        /// <summary>
        /// Helper method that will get all of the nodes that will check if the player
        /// has parsnip seeds throughout the entire world
        /// </summary>
        /// <returns>The root node of the tree that will check for parsnips</returns>
        private static DecisionTreeNode GetParsnipSeedsTree(int desiredAmount)
        {
            string parsnipSeedsItemId = "472";

            #region Delegate Methods

            #region Delegate Actions

            string GetParsnipFromStore()
            {
                int playerInventoryCount = ItemLocator.PlayerItemCount(parsnipSeedsItemId);

                return $"Buy {desiredAmount - playerInventoryCount} parsnip seed(s) from store";
            }

            //Return the amount of seeds needed to get from that location
            string GetParsnipSeedCountFromLocation(string uniqueLocationName)
            {
                GameLocation location = Game1.locations.First(l => l.NameOrUniqueName == uniqueLocationName);

                Chest chest = ItemLocator.GetChestsWithItem(location, parsnipSeedsItemId).First();

                int locationItemCount = ItemLocator.GetChestItemCount(chest, parsnipSeedsItemId);

                int playerInventoryCount = ItemLocator.PlayerItemCount(parsnipSeedsItemId);

                int desiredCount = Math.Min(locationItemCount, desiredAmount - playerInventoryCount);

                return $"Get {desiredCount} parsnip seed(s) from chest in {uniqueLocationName} at {chest.TileLocation}";
            }

            //Check how many parsnip seeds are in the FarmHouse and tells the plyaer to get them
            string GetParsnipSeedCountFromFarmHouse()
            {
                //get the amount of parsnips found in farmhouse
                return GetParsnipSeedCountFromLocation("FarmHouse");
            }

            //Check how many parsnip seeds are in the Farm and tells the plyaer to get them
            string GetParsnipSeedCountFromFarm()
            {
                //get the amount of parsnips found in farm
                return GetParsnipSeedCountFromLocation("Farm");
            }
            #endregion

            #region Delegate Checks

            bool LocationHasParsnipSeeds(string uniqueLocationName)
            {
                GameLocation location = Game1.locations.First(l => l.NameOrUniqueName == uniqueLocationName);
                return ItemLocator.LocationHasItem(location, parsnipSeedsItemId);
            }

            bool FarmHasParsnipSeeds()
            { 
                return LocationHasParsnipSeeds("Farm");
            }

            //returns true if the player has at least 1 parsnip in their house
            bool FarmHouseHasParsnipSeeds()
            {
                return LocationHasParsnipSeeds("FarmHouse");
            }

            bool PlayerHasDesieredAmountOfParsnipSeeds()
            {
                return ItemLocator.PlayerHasItem(parsnipSeedsItemId, desiredAmount, true);
            }
            #endregion

            #endregion

            //there is at least one loction where the player has parsnip seeds to in the farm
            Decision playerHasParsnipSeedsOnFarm = new Decision(
                new Action(GetParsnipSeedCountFromFarm),
                new Action(GetParsnipFromStore),
                new Decision.DecisionDelegate(FarmHasParsnipSeeds));

            //there is at least one loction where the player has parsnip seeds to in the farm house
            Decision playerHasParsnipSeedsOnFarmHouse = new Decision(
                new Action(GetParsnipSeedCountFromFarmHouse),
                playerHasParsnipSeedsOnFarm,
                new Decision.DecisionDelegate(FarmHouseHasParsnipSeeds));

            //the player has 15 parsnips seeds on them
            Decision playerHas15ParsnipInInventory = new Decision(
                Instance.completeAction,
                playerHasParsnipSeedsOnFarmHouse,
                new Decision.DecisionDelegate(PlayerHasDesieredAmountOfParsnipSeeds));

            return playerHas15ParsnipInInventory;
        }

        /// <summary>
        /// Get the branch to become friends with Sebestian
        /// </summary>
        /// <returns></returns>
        private static DecisionTreeNode BecomeFriendsWithSebastian()
        { 
            bool PlayerKnowsSebastian()
            {
                return Game1.player.friendshipData.ContainsKey("Sebastian");
            }

            //player knows sebastian
            DecisionTreeNode knowSebastian = new Decision(
                new Action("Player knows Sebastian"),
                new Action("Meet Sebastian"),
                new Decision.DecisionDelegate(PlayerKnowsSebastian));
                
            return knowSebastian;
        }

        private static DecisionTreeNode GetSebastianRelationshipTree()
        {
            return new Action("");
        }
    }
}
