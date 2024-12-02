using Stardew_100_Percent_Mod.Decision_Trees;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
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

        //keep try of items that don't exist in the world
        private List<DummyItem> dummyItems;

        public delegate bool TaskComplateDelegate();
        public delegate string UpdateTaskDisplayNameDelegate(Task t);

        public delegate void LogMethod(string message, LogLevel logLevel = LogLevel.Debug);
        public LogMethod logMethod;

        private Action completeAction;

        Dictionary<string, int> requiredItemsDictionary;

        //keeps track of what items the player has in their inventory. Mainly used to reserve items for quests
        //key is the item id
        //value is the amount of that item that is free to use
        public Dictionary<string, int> inventoryItemReserveDictonary { get; private set; }
        public TaskManager()
        {

        }

        public static void InitalizeInstance(LogMethod logMethod)
        {
            Instance = new TaskManager();

            Instance.logMethod = logMethod;

            Instance.requiredItemsDictionary = new Dictionary<string, int>();

            Instance.completeAction = new Action("");

            Instance.dummyItems = new List<DummyItem>()
            { new DummyItem("388", "Wood") };

            string parsnipId = "472";
            string woodId = "388";


            DecisionTreeNode woodsTree = GetProducableItemTree(woodId, 50);

            DecisionTreeNode parsnipSeedsTree = GetProducableItemTree(parsnipId, 15);

            DecisionTreeNode becomeFriendsWithSebastian = BecomeFriendsWithNPC("Sebastian");

            DecisionTreeNode becomeFriendsWithJas = BecomeFriendsWithNPC("Jas");

            DecisionTreeNode craftChest = CraftItem("Chest");


            Instance.roots = new List<DecisionTreeNode>(new[] { woodsTree, craftChest, parsnipSeedsTree, becomeFriendsWithJas });
        }

        /// <summary>
        /// Helper method that will get all of the nodes that will check if the player
        /// has the desired item throughout the entire world
        /// </summary>
        /// <returns>The root node of the tree that will check for parsnips</returns>
        private static DecisionTreeNode GetProducableItemTree(string itemId, int desiredAmount)
        { 

            Item item = ItemLocator.GetItem(itemId);
            DummyItem dummyItem = Instance.dummyItems.FirstOrDefault(i => i.itemId == itemId);



            #region Delegate Methods

            #region Delegate Actions

            string GetItemFromStore()
            {
                int playerInventoryCount = ItemLocator.PlayerItemCount(itemId);

                if (item != null)
                {
                    return $"Buy {Instance.requiredItemsDictionary[itemId] - playerInventoryCount} {item.Name}(s) from store";
                }

                return $"Buy {Instance.requiredItemsDictionary[itemId] - playerInventoryCount} {dummyItem.DisplayName}(s) from store";
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
            NPC npc = GetNPC(npcName);
            Friendship friendship = GetFriendshipData(npcName);

            #region Delegate Actions

            #endregion

            #region Delegate Checks

            bool PlayerKnowsNPC()
            {
                return Game1.player.friendshipData.ContainsKey(npcName);
            }

            bool PlayerBestFriendsWithNPC()
            {
                friendship = GetFriendshipData(npcName);
                return friendship.Points >= fullHeartAmount * (IsDatable(npcName) ? 8 : 10);
            }

            bool CanGiveNPCGift()
            {
                return (friendship.GiftsThisWeek < 2 || npc.isBirthday()) && friendship.GiftsToday == 0;
            }

            bool CanTalk()
            {
                return !Game1.player.hasPlayerTalkedToNPC(npc.Name);
            }

            #endregion

            #region Tree
            //the player can talk to npc today
            DecisionTreeNode canTalk = new Decision(
                new Action($"Talk to {npcName}"),
                Instance.completeAction,
                new Decision.DecisionDelegate(CanTalk));



            //The player can give npc a gift today and they haven't given him a gift yet
            DecisionTreeNode canGiveGift = new Decision(
                new Action($"Give {npcName} a gift"),
                canTalk,
                new Decision.DecisionDelegate(CanGiveNPCGift));

            //max friendship
            DecisionTreeNode maxFriendship = new Decision(
                Instance.completeAction,
                canGiveGift,
                new Decision.DecisionDelegate(PlayerBestFriendsWithNPC), true);

            //player knows npc
            DecisionTreeNode knowSebastian = new Decision(
                maxFriendship,
                new Action($"Meet {npcName}"),
                new Decision.DecisionDelegate(PlayerKnowsNPC), true);
                
            return knowSebastian;
            #endregion
        }

        /// <summary>
        /// Get the branch to craft an item
        /// </summary>
        /// <param name="name">the name of the item that the player would like to craft</param>
        /// <returns></returns>
        public static DecisionTreeNode CraftItem(string name)
        {
            ///when the actions is reached, remove items from inventory reserve
            CraftingRecipe recipe = new CraftingRecipe(name);
            #region Delegate Actions

            #endregion
            #region Delegate Checks
            bool PlayerKnowsRecipe()
            {
                return Game1.player.knowsRecipe(name);
            }

            bool PlayerHasCraftedRecipie()
            {
                return new CraftingRecipe(name).timesCrafted > 0;
            }

            bool PlayerHasRequiredItems()
            {
                foreach (KeyValuePair<string, int> kv in recipe.recipeList)
                {
                    Instance.inventoryItemReserveDictonary.TryGetValue(kv.Key, out int inventoryCount);
                    if(inventoryCount < kv.Value)
                    {
                        return false;
                    }
                }

                return true;
            }

            #endregion

            #region Tree

            //Does player have the required items to create the item in their inventory
            DecisionTreeNode hasItems = new Decision(
                new CraftItemAction(recipe),
                new Action("Player does not have required items"),
                PlayerHasRequiredItems);

            //Has the player crafted the item at least once?
            DecisionTreeNode craftedItem = new Decision(
                Instance.completeAction,
                hasItems,
                PlayerHasCraftedRecipie,
                true);

            //does the player know the recipe
            DecisionTreeNode knowRecipe = new Decision(
                craftedItem,
                new Action("Player doesn't knows recipe"),
                PlayerKnowsRecipe,
                true);
            #endregion

            return knowRecipe;
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


    }
}
