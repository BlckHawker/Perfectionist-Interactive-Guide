using Stardew_100_Percent_Mod.Decision_Trees;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using static Stardew_100_Percent_Mod.NPCManager;
using static System.Collections.Specialized.BitVector32;

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

        //means either the player completed the overall task, or they need to go to bed.
        //Either way, there's nothing to display to them, which is why it's an empty string
        private static Action completeAction = new Action("");

        //action that tells the user to get money
        private GetMoneyAction getMoneyAction;



        //Keeps track of how many of each item the user needs
        //Used so there aren't multiple tasks saying "Buy x items at store"
        //Example, there are two tasks that both rquire the user to get 15 seeds
        //We don't want two tasks saying "Buy 15 seeds at store"
        //It should say "Buy 30 seeds at store" instead
        Dictionary<string, int> requiredItemsDictionary;

        //keeps track of what items the player has in their inventory. Mainly used to reserve items for quests
        //key is the item id
        //value is the amount of that item that is free to use
        public Dictionary<string, int> InventoryItemReserveDictonary { get; private set; }

        //how much money the player has to spare
        public int reservedMoney;
        private int requiredMoney;
        //A dictionary to easily hold the id of each item
        public Dictionary<ItemName, string> ItemIds { get; private set; }

        //List of all of the custom crafting recipes (see class defintion to see reason why it exists)
        private List<DummyCraftingRecipe> dummyCraftingRecipes;

        public enum ItemName
        {
            CopperBar,
            ParsnipSeeds,
            Wood
        }

        public TaskManager()
        {

        }

        public static void InitalizeInstance(LogMethod logMethod)
        {
            Instance = new TaskManager();

            Instance.logMethod = logMethod;

            Instance.requiredItemsDictionary = new Dictionary<string, int>();
            Instance.InventoryItemReserveDictonary = new Dictionary<string, int>();

            Instance.getMoneyAction = new GetMoneyAction(Instance.GetDesiredMoneyAmount);

            Instance.ItemIds = new Dictionary<ItemName, string>()
            {
                { ItemName.ParsnipSeeds, "(O)472"},
                { ItemName.Wood, "(O)388"},
                { ItemName.CopperBar, "(O)344"},

            };

            Instance.dummyCraftingRecipes = DummyCraftingRecipe.GetAllRecipes();

            Instance.dummyItems = new List<DummyItem>();

            foreach(ItemName k in Instance.ItemIds.Keys)
            {
                Instance.dummyItems.Add(GetDummyItem(k));
            }

            DecisionTreeNode parsnipSeedsTree = GetProducableItemTree(Instance.ItemIds[ItemName.ParsnipSeeds], 15);

            DecisionTreeNode becomeFriendsWithJas = BecomeFriendsWithNPC("Jas");

            DecisionTreeNode craftChest = CraftItem("Chest");

            DecisionTreeNode cookOmelet = CookItem("Omelet");

            //Instance.roots = new List<DecisionTreeNode>();

            Instance.roots = new List<DecisionTreeNode>(new[] { cookOmelet, cookOmelet });

            //Instance.roots = new List<DecisionTreeNode>(new[] { craftChest, parsnipSeedsTree, becomeFriendsWithJas });
        }

        /// <summary>
        /// Helper method to create a DummyItem out of an ItemName
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        private static DummyItem GetDummyItem(ItemName itemName)
        {
            KeyValuePair<ItemName, string> kv = Instance.ItemIds.First(kv => kv.Key == itemName);
            return new DummyItem(kv.Value, kv.Key.ToString());
        }

        /// <summary>
        /// Helper method that will get all of the nodes that will check if the player
        /// has the desired item throughout the entire world
        /// </summary>
        /// <returns>The root node of the tree that will check for parsnips</returns>
        public static DecisionTreeNode GetProducableItemTree(string qualifiedItemId, int desiredAmount)
        { 

            Item item = ItemLocator.GetItem(qualifiedItemId);
            DummyItem dummyItem = Instance.dummyItems.First(i => i.QualifiedItemId == qualifiedItemId);



            #region Delegate Methods

            #region Delegate Actions

            string GetItemFromStore()
            {
                //get the amount of the item the player currently has including iventory, iventory cursor, and shop cursor
                int playerInventoryCount = ItemLocator.PlayerItemCount(qualifiedItemId) + ItemLocator.ShopCursorCount(qualifiedItemId);

                //check if the player is buying something from the store and their cursor has the desried item
                

                if (item != null)
                {
                    return $"Buy {Instance.requiredItemsDictionary[qualifiedItemId] - playerInventoryCount} {item.Name}(s) from store";
                }

                return $"Buy {Instance.requiredItemsDictionary[qualifiedItemId] - playerInventoryCount} {dummyItem.DisplayName}(s) from store";
            }

            //Return the amount of items he player should get from that location
            string GetItemCountFromLocation(string uniqueLocationName)
            {
                GameLocation location = Game1.locations.First(l => l.NameOrUniqueName == uniqueLocationName);

                Chest chest = ItemLocator.GetChestsWithItem(location, qualifiedItemId).First();

                int locationItemCount = ItemLocator.GetChestItemCount(chest, qualifiedItemId);

                int playerInventoryCount = ItemLocator.PlayerItemCount(qualifiedItemId);

                int desiredCount = Math.Min(locationItemCount, Instance.requiredItemsDictionary[qualifiedItemId] - playerInventoryCount);

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
                return ItemLocator.LocationHasItem(location, qualifiedItemId);
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
                Instance.InventoryItemReserveDictonary.TryGetValue(qualifiedItemId, out int itemCount);
                Instance.UpdateReservedItemDicionary(qualifiedItemId, -desiredAmount);
                bool conditon = itemCount >= desiredAmount;
                return conditon;
            }

            #endregion

            #endregion

            #region Tree
            //there is at least one loction where the player has parsnip seeds to in the farm
            Decision playerHasItemOnFarm = new Decision(
                new GetItemAction(qualifiedItemId, GetParsnipSeedCountFromFarm),
                new GetItemAction(qualifiedItemId, GetItemFromStore),
                new Decision.DecisionDelegate(FarmHasParsnipSeeds));

            //there is at least one loction where the player has parsnip seeds to in the farm house
            Decision playerHasItemOnFarmHouse = new Decision(
                new GetItemAction(qualifiedItemId, GetParsnipSeedCountFromFarmHouse),
                playerHasItemOnFarm,
                new Decision.DecisionDelegate(FarmHouseHasParsnipSeeds));

            //the player has {desiredAmount} {item} on them
            Decision playerHasItemInInventory = new Decision(
                completeAction,
                playerHasItemOnFarmHouse,
                new Decision.DecisionDelegate(PlayerHasDesieredAmountOfItem));

            return new Root(playerHasItemInInventory, qualifiedItemId, desiredAmount);
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
                completeAction,
                new Decision.DecisionDelegate(CanTalk));



            //The player can give npc a gift today and they haven't given him a gift yet
            DecisionTreeNode canGiveGift = new Decision(
                new Action($"Give {npcName} a gift"),
                canTalk,
                new Decision.DecisionDelegate(CanGiveNPCGift));

            //max friendship
            DecisionTreeNode maxFriendship = new Decision(
                completeAction,
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
        private static DecisionTreeNode CraftItem(string name)
        {
            ///when the actions is reached, remove items from inventory reserve
            CraftingRecipe recipe = new CraftingRecipe(name);
            DummyCraftingRecipe dummyRecipe = Instance.dummyCraftingRecipes.First(r => r.Name == name);
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
                return Instance.GetRecipeMissingItems(dummyRecipe).Count == 0;
            }

            #endregion

            #region Tree

            //Does player have the required items to create the item in their inventory
            DecisionTreeNode hasItems = new Decision(
                new Action($"Craft {name}"),
                new CraftItemAction(recipe),
                PlayerHasRequiredItems);

            //Has the player crafted the item at least once?
            DecisionTreeNode craftedItem = new Decision(
                completeAction,
                hasItems,
                PlayerHasCraftedRecipie,
                true);

            //does the player know the recipe
            DecisionTreeNode knowRecipe = new Decision(
                craftedItem,
                new Action($"Learn {name} recipe"),
                PlayerKnowsRecipe,
                true);
            #endregion

            return knowRecipe;
        }

        /// <summary>
        /// Get the branch to cook an item
        /// </summary>
        /// <param name="name">the name of the recipe to be cooked</param>
        /// <returns></returns>
        private static DecisionTreeNode CookItem(string name)
        {

            #region Delegate Action
            #endregion

            #region Delegate Checks
            bool PlayerHasKitchen()
            {
                FarmHouse farmhouse = (FarmHouse)Game1.locations.First(l => l.NameOrUniqueName == "FarmHouse");
                Microsoft.Xna.Framework.Point fridgePoint = farmhouse.fridgePosition;
                return fridgePoint != Microsoft.Xna.Framework.Point.Zero;
            }

            bool HasRequestedHouseUpgrade()
            {
                return Game1.player.daysUntilHouseUpgrade.Value != -1;
            }

            bool HasMoneyForHouseUpgrade()
            {
                return Instance.HasDesiredMoney(10000);
            }
            #endregion


            #region Tree

            //does the player have enough money for the first house upgrade
            Decision enoughMoney = new Decision(new Action("Player has 10k"),
                                   Instance.getMoneyAction,
                                   HasMoneyForHouseUpgrade);

            //has the player requested a house upgrade?
            Decision hasRequesetedHouseUpgrade = new Decision(completeAction,
                                        enoughMoney,
                                        HasRequestedHouseUpgrade);

            //does the player have a kitchen
            Decision playerHasKitchen = new Decision(new Action("Player has a kitchen"),
                                        hasRequesetedHouseUpgrade,
                                        PlayerHasKitchen,
                                        true);
            #endregion

            return playerHasKitchen;
        }

        /// <summary>
        /// Get a list of all of the items the player is missing to craft the desired recipe
        /// </summary>
        /// <param name="recipe">the deisred recipie to craft</param>
        /// <returns>A list of the qualified item ids of the missing items. Empty if the user has enough of all the items</returns>
        public List<string> GetRecipeMissingItems(DummyCraftingRecipe recipe)
        {
            List<string> missingItems = new List<string>();

            foreach (KeyValuePair<string, int> kv in recipe.RecipeList)
            {

                Instance.InventoryItemReserveDictonary.TryGetValue(kv.Key, out int inventoryCount);
                if (inventoryCount < kv.Value)
                {
                    missingItems.Add(kv.Key);
                }
            }

            return missingItems;
        }


        #region Delegate Checks
        
        /// <summary>
        /// Actions that says how much money the player should get
        /// </summary>
        /// <returns></returns>
        private string GetDesiredMoneyAmount()
        { 
            return $"Get {requiredMoney - Game1.player.Money} gold" ;
        }
        #endregion

        #region Delegate Actions
        private bool HasDesiredMoney(int desiredAmount)
        {
            bool condition = reservedMoney >= desiredAmount;
            UpdateReservedMoney(-desiredAmount);
            return condition;
        }

        #endregion



        /// <summary>
        /// Combine Actions that tell the player to get the same item, but different amounts
        /// </summary>
        /// <param name="actions">The actions</param>
        private List<Action> CombineItemActions(List<Action> actions)
        { 
            
            List<string> itemIds = new List<string>();
            //the index the first GetItemAction of the same qualifiedItemId is found

            //get all the ids of the GetItem actions (and their index)
            for (int i = 0; i < actions.Count; i++)
            {
                Action action = actions[i];
                if (action is GetItemAction)
                {
                    string qualifiedItemId = ((GetItemAction)action).QualifiedItemId;

                    if (!itemIds.Contains(qualifiedItemId))
                    {
                        itemIds.Add(qualifiedItemId);
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
        /// Combines a list of duplicate actions into one
        /// </summary>
        /// <param name="actions">the original list of actions</param>
        /// <returns>a new list of actions without duplicates</returns>
        public List<Action> CombineActions(List<Action> actions)
        {
            actions = CombineItemActions(actions);
            actions = CombineMoneyActions(actions);
            return actions;
        }

        public void UpdateRequiredItemsDictionary(string qualifiedItemId, int count)
        {
            if (!requiredItemsDictionary.ContainsKey(qualifiedItemId))
            {
                requiredItemsDictionary[qualifiedItemId] = count;
            }

            else
            { 
                requiredItemsDictionary[qualifiedItemId] += count;
            }
        }

        

        public void UpdateReservedItemDicionary(string qualifiedItemId, int count)
        {
            if (InventoryItemReserveDictonary.ContainsKey(qualifiedItemId))
            {
                InventoryItemReserveDictonary[qualifiedItemId] += count;
            }

            else
            {
                InventoryItemReserveDictonary[qualifiedItemId] = count;
            }

            //clamp to zero
            InventoryItemReserveDictonary[qualifiedItemId] = Math.Clamp(InventoryItemReserveDictonary[qualifiedItemId], 0, int.MaxValue);

        }

        /// <summary>
        /// Updates the money reserved and money required
        /// </summary>
        /// <param name="count">The amount of money that will be added to reserved money</param>
        private void UpdateReservedMoney(int count)
        {
            //clamp to zero
            requiredMoney = Math.Clamp(requiredMoney - count, 0, int.MaxValue);

            //clamp to zero
            reservedMoney = Math.Clamp(reservedMoney + count, 0, int.MaxValue);
        }

        public void ResetItemDictionarys()
        {
            requiredMoney = 0;
            reservedMoney = Game1.player.Money;
            requiredItemsDictionary.Clear();
            InventoryItemReserveDictonary.Clear();

            //foreach item in the world, count the amount of it appears in the player's inventory
            Utility.ForEachItem(delegate (Item item)
            {
                InventoryItemReserveDictonary[item.QualifiedItemId] = ItemLocator.PlayerItemCount(item.QualifiedItemId);
                return true;
            });

            //check the cursor's held item in the player's inventory (unsure why that is not caught in the for each mehod)
            if (Game1.player.CursorSlotItem != null)
            {
                Item item = Game1.player.CursorSlotItem;
                string id = item.QualifiedItemId;
                UpdateReservedItemDicionary(id, ItemLocator.PlayerItemCount(id));
            }

            //check the shop menu's cursor to see if the player has bought the item in said shop
            Item shopItem = ItemLocator.ShopCursorItem();
            if (shopItem != null)
            {
                string id = shopItem.QualifiedItemId;
                UpdateReservedItemDicionary(id, ItemLocator.ShopCursorCount(id));
            }


            //order by count in decending order
            InventoryItemReserveDictonary = InventoryItemReserveDictonary.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            return;
        }


        /// <summary>
        /// Combine get money actions
        /// </summary>
        /// <param name="actions">the original list of actions</param>
        /// <returns>A new list with the combined </returns>
        private List<Action> CombineMoneyActions(List<Action> actions)
        { 
            List<Action> newList = new List<Action>();

            //if the count of GetMoneyAction is 0 or 1, return the original list as the new list will be the same
            if(actions.Count(a => a is GetMoneyAction) < 2)
            {
                return actions;
            }

            //the total amount of money required
            int moneyRequired = 0;

            //the index of the first GetMoneyAction object 
            int index = -1;

            for (int i = 0; i < actions.Count; i++)
            {
                Action action = actions[i];
                if (action is GetMoneyAction)
                {
                    moneyRequired += ((GetMoneyAction)action).MoneyRequired;
                    if (index == -1)
                    {
                        index = i;
                    }
                }

                else
                {
                    newList.Add(action);
                }
            }

            newList.Insert(index, new GetMoneyAction(GetDesiredMoneyAmount));

            return newList;
        }


        /// <summary>
        /// Combines Actions that have a specifc item
        /// </summary>
        /// <param name="qualifiedItemId">the id of the desired item</param>
        /// <param name="actions">the original list of actions</param>
        /// <returns>a new list of actions with combined action that say to get the same item</returns>
        private List<Action> CombineItemAction(string qualifiedItemId, List<Action> actions)
        {
            List<Action> newList = new List<Action>();
            bool foundItemAction = false;
            for (int i = 0; i < actions.Count; i++)
            {
                Action action = actions[i];
                if (action is GetItemAction)
                {
                    GetItemAction getItemAction = (GetItemAction)action;

                    if (getItemAction.QualifiedItemId == qualifiedItemId && !foundItemAction)
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
