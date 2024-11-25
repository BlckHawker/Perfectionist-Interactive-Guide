using Microsoft.Xna.Framework.Content;
using Stardew_100_Percent_Mod.Decision_Trees;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
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
using xTile;
using xTile.Dimensions;
using static Stardew_100_Percent_Mod.NPCManager;

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

        Dictionary<string, int> requiredItemsDictionary;

        //keeps track of what items the player has in their inventory. Mainly used to reserve items for quests
        Dictionary<string, int> inventoryItemReserveDictonary;

        //the amount of items we currently have (reserving them for quests)
        Dictionary<string, int> reservedInventory;
        
        public TaskManager()
        {

        }

        public static void InitalizeInstance(LogMethod logMethod)
        {
            Instance = new TaskManager();

            Instance.logMethod = logMethod;

            Instance.requiredItemsDictionary = new Dictionary<string, int>();

            Instance.completeAction = new Action("");

            string parsnipId = "472";

            DecisionTreeNode parsnipSeedsTree = GetProducableItemTree(parsnipId, 15);

            DecisionTreeNode becomeFriendsWithSebastian = BecomeFriendsWithNPC("Sebastian");

            Instance.roots = new List<DecisionTreeNode>(new[] { parsnipSeedsTree, becomeFriendsWithSebastian });
        }

        /// <summary>
        /// Helper method that will get all of the nodes that will check if the player
        /// has the desired item throughout the entire world
        /// </summary>
        /// <returns>The root node of the tree that will check for parsnips</returns>
        private static DecisionTreeNode GetProducableItemTree(string itemId, int desiredAmount)
        { 

            Item item = ItemLocator.GetItem(itemId);

            #region Delegate Methods

            #region Delegate Actions

            string GetItemFromStore()
            {
                int playerInventoryCount = ItemLocator.PlayerItemCount(itemId);

                return $"Buy {Instance.requiredItemsDictionary[itemId] - playerInventoryCount} {item.Name}(s) from store";
            }

            //Return the amount of items he player should get from that location
            string GetItemCountFromLocation(string uniqueLocationName)
            {
                GameLocation location = Game1.locations.First(l => l.NameOrUniqueName == uniqueLocationName);

                Chest chest = ItemLocator.GetChestsWithItem(location, itemId).First();

                int locationItemCount = ItemLocator.GetChestItemCount(chest, itemId);

                int playerInventoryCount = ItemLocator.PlayerItemCount(itemId);

                int desiredCount = Math.Min(locationItemCount, Instance.requiredItemsDictionary[itemId] - playerInventoryCount);

                //did i just change this?
                return $"Get {desiredCount} {item.Name} from chest in {uniqueLocationName} at {chest.TileLocation}";
            }

            //Check how many {item} are in the FarmHouse and tells the plyaer to get them
            string GetParsnipSeedCountFromFarmHouse()
            {
                return GetItemCountFromLocation("FarmHouse");
            }

            //Check how many {item} are in the Farm and tells the plyaer to get them
            string GetParsnipSeedCountFromFarm()
            {
                return GetItemCountFromLocation("Farm");
            }
            #endregion

            #region Delegate Checks

            //A certain location has the desired item
            bool LocationHasItem(string uniqueLocationName)
            {
                GameLocation location = Game1.locations.First(l => l.NameOrUniqueName == uniqueLocationName);
                return ItemLocator.LocationHasItem(location, itemId);
            }

            //The farm has the desired item
            bool FarmHasParsnipSeeds()
            {
                return LocationHasItem("Farm");
            }

            //The farm house has the desired item
            bool FarmHouseHasParsnipSeeds()
            {
                return LocationHasItem("FarmHouse");
            }

            //The player has the desired amount of {item}
            bool PlayerHasDesieredAmountOfItem()
            {
                //check if found in the player's ivnentory
                Instance.inventoryItemReserveDictonary.TryGetValue(itemId, out int itemCount);
                Instance.UpdateReservedItemDicionary(itemId, -desiredAmount);
                bool conditon = itemCount >= desiredAmount;
                return conditon;
            }
            #endregion

            #endregion

            #region Tree
            //there is at least one loction where the player has parsnip seeds to in the farm
            Decision playerHasItemOnFarm = new Decision(
                new GetItemAction(itemId, GetParsnipSeedCountFromFarm),
                new GetItemAction(itemId, GetItemFromStore),
                new Decision.DecisionDelegate(FarmHasParsnipSeeds));

            //there is at least one loction where the player has parsnip seeds to in the farm house
            Decision playerHasItemOnFarmHouse = new Decision(
                new GetItemAction(itemId, GetParsnipSeedCountFromFarmHouse),
                playerHasItemOnFarm,
                new Decision.DecisionDelegate(FarmHouseHasParsnipSeeds));

            //the player has {desiredAmount} {item} on them
            Decision playerHasItemInInventory = new Decision(
                Instance.completeAction,
                playerHasItemOnFarmHouse,
                new Decision.DecisionDelegate(PlayerHasDesieredAmountOfItem));

            return new Root(playerHasItemInInventory, itemId, desiredAmount);
            #endregion
        }

        /// <summary>
        /// Get the branch to become friends with Sebestian
        /// </summary>
        /// <returns></returns>
        private static DecisionTreeNode BecomeFriendsWithNPC(string npcName)
        {
            const int fullHeartAmount = 250;
            #region Delegate Actions

            #endregion

            #region Delegate Checks

            bool PlayerKnowsSebastian()
            {
                return Game1.player.friendshipData.ContainsKey(npcName);
            }

            bool PlayerBestFriendsWithSebastion()
            {
                Friendship friendship = GetFriendshipData(npcName);
                return friendship.Points >= fullHeartAmount * (IsDatable(npcName) ? 8 : 10);
            }

            bool CanGiveNPCGift()
            {
                Friendship friendship = GetFriendshipData(npcName);
                
                NPC npc = GetNPC(npcName);
                
                return friendship.GiftsThisWeek > 2 
                    && friendship.GiftsToday == 0
                    && npc.CanReceiveGifts();
            }

            #endregion

            //max friendship
            DecisionTreeNode maxFriendship = new Decision(
                Instance.completeAction,
                new Action($"Do not have max friendship with {npcName}"),
                new Decision.DecisionDelegate(PlayerBestFriendsWithSebastion), true);

            //player knows sebastian
            DecisionTreeNode knowSebastian = new Decision(
                maxFriendship,
                new Action($"Meet {npcName}"),
                new Decision.DecisionDelegate(PlayerKnowsSebastian), true);
                
            return knowSebastian;
        }

        /// <summary>
        /// Combine Actions that tell the player to get the same item, but different amounts
        /// </summary>
        /// <param name="actions">The actions</param>
        public List<Action> CombineItemActions(List<Action> actions)
        { 
            List<string> itemIds = new List<string>();

            //get all the ids of the GetItem actions
            foreach(Action action in actions)
            {
                if (action is GetItemAction)
                {
                    string itemId = ((GetItemAction)action).ItemId;

                    if (!itemIds.Contains(itemId))
                    {
                        itemIds.Add(itemId);
                    }
                }
            }

            //combine all of the GetItemActions
            foreach (string id in itemIds)
            {
                actions = CombineItemAction(id, actions);
            }

            return actions;
        }

        /// <summary>
        /// Combines Actions that have a specifc item
        /// </summary>
        /// <param name="itemId">the id of the desired item</param>
        /// <param name="actions">the original list of actions</param>
        /// <returns>a new list of actions with combined action that say to get the same item</returns>
        private List<Action> CombineItemAction(string itemId, List<Action> actions)
        {
            List<Action> newList = new List<Action>();
            bool foundItemAction = false;
            for (int i = 0; i < actions.Count; i++)
            {
                Action action = actions[i];
                if (action is GetItemAction)
                {
                    GetItemAction getItemAction = (GetItemAction)action;

                    if (getItemAction.ItemId == itemId && !foundItemAction)
                    {
                        foundItemAction = true;
                        newList.Add(getItemAction);
                    }
                }

                else
                {
                    newList.Add(action);
                }
            }

            return newList;
        }

        public void UpdateRequiredItemsDictionary(string itemId, int count)
        {
            if (!requiredItemsDictionary.ContainsKey(itemId))
            {
                requiredItemsDictionary[itemId] = count;
            }

            else
            { 
                requiredItemsDictionary[itemId] += count;
            }
        }

        public void UpdateReservedItemDicionary(string itemId, int count)
        {
            if (inventoryItemReserveDictonary.ContainsKey(itemId))
            {
                inventoryItemReserveDictonary[itemId] += count;

                //clamp to zero
                inventoryItemReserveDictonary[itemId] = Math.Clamp(inventoryItemReserveDictonary[itemId], 0, int.MaxValue);
            }
            
        }

        public void ResetItemDictionarys()
        {
            requiredItemsDictionary.Clear();

            inventoryItemReserveDictonary = new Dictionary<string, int>();

            Utility.ForEachItem(delegate (Item item)
            {
                inventoryItemReserveDictonary[item.ItemId] = ItemLocator.PlayerItemCount(item.ItemId);
                return true;
            });

            //check the cursor's held item (unsure why that is not caught in the for each mehod)
            if (Game1.player.CursorSlotItem != null)
            {
                Item item = Game1.player.CursorSlotItem;
                if (inventoryItemReserveDictonary.ContainsKey(item.ItemId))
                {
                    inventoryItemReserveDictonary[item.ItemId] += ItemLocator.PlayerItemCount(item.ItemId);
                }

                else
                { 
                    inventoryItemReserveDictonary[item.ItemId] = ItemLocator.PlayerItemCount(item.ItemId);
                }
            }
            
            return;
        }
    }
}
