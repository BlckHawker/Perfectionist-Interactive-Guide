﻿using Stardew_100_Percent_Mod.Decision_Trees;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using System;
using System.Linq;
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

        //make this private. Became public for debugging purposes
        public Dictionary<string, int> requiredItemsDictionary;

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
        private List<DummyCookingRecipe> dummyCookingRecipes;


        public enum ItemName
        {
            BrownEgg,
            BrownLargeEgg,
            DuckEgg,
            CopperBar,
            Egg,
            GoatMilk,
            GoldenEgg,
            Hardwood,
            LargeEgg,
            LargeGoatMilk,
            LargeMilk,
            Milk,
            OstritchEgg,
            ParsnipSeeds,
            VoidEgg,
            Wood
        }

        //All item of the following item names lists are ordered by easiest to hardest to get

        //all of the eggs that are considered eggs for a cooking recipe
        public readonly List<ItemName> EggList = new List<ItemName>() { ItemName.Egg, ItemName.BrownEgg, ItemName.LargeEgg, ItemName.BrownLargeEgg, 
                                                                ItemName.DuckEgg, ItemName.VoidEgg, ItemName.OstritchEgg, ItemName.GoldenEgg  };
        //all of the milks that are considered milk for a cooking recipe
        public readonly List<ItemName> MilkList = new List<ItemName>() { ItemName.Milk, ItemName.GoatMilk, ItemName.LargeMilk, ItemName.LargeGoatMilk };

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
                { ItemName.BrownEgg, "(O)180"},
                { ItemName.BrownLargeEgg, "(O)182"},
                { ItemName.CopperBar, "(O)344"},
                { ItemName.DuckEgg, "(O)442"},
                { ItemName.Egg, "(O)176"},
                { ItemName.GoatMilk, "(O)436"},
                { ItemName.GoldenEgg, "(O)928"},
                { ItemName.Hardwood, "(O)709"},
                { ItemName.LargeEgg, "(O)174"},
                { ItemName.LargeGoatMilk, "(O)438" },
                { ItemName.LargeMilk, "(O)186"},
                { ItemName.Milk, "(O)184"},
                { ItemName.OstritchEgg, "(O)289"},
                { ItemName.ParsnipSeeds, "(O)472"},
                { ItemName.Wood, "(O)388"},
                { ItemName.VoidEgg, "(O)305" }
            };

            //check if any ItemName keys are missing in ItemIds
            List<ItemName> missingItemIds = Enumerable.Range(0, Enum.GetNames(typeof(ItemName)).Length)
                .Where(i => !Instance.ItemIds.ContainsKey((ItemName)i))
                .Select(i => (ItemName)i).ToList();

            if(missingItemIds.Count > 0)
            {
                throw new Exception($"The following keys are missing in \"ItemIds\": {string.Join(", ", missingItemIds)}");
            }

            Instance.dummyCraftingRecipes = DummyCraftingRecipe.GetAllRecipes();
            Instance.dummyCookingRecipes = DummyCookingRecipe.GetAllRecipes();

            Instance.dummyItems = new List<DummyItem>();

            foreach(ItemName k in Instance.ItemIds.Keys)
            {
                Instance.dummyItems.Add(GetDummyItem(k));
            }

            DecisionTreeNode parsnipSeedsTree = GetProducableItemTree(ItemName.ParsnipSeeds, 15);

            DecisionTreeNode becomeFriendsWithJas = BecomeFriendsWithNPC("Jas");

            DecisionTreeNode craftChest = CraftItem("Chest", false);

            DecisionTreeNode cookOmelet = CraftItem("Omelet", true);

            DecisionTreeNode getLevel3House = GetHouseUpgrade(3);


            Instance.roots  = new List<DecisionTreeNode>() { cookOmelet };

            //Instance.roots = new List<DecisionTreeNode>(new[] { cookOmelet, craftChest, parsnipSeedsTree, becomeFriendsWithJas });
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
        /// Get a tree branch of how to get the missing ingrediants for a recipe
        /// </summary>
        /// <param name="recipeName">the name of a recipe</param>
        /// <returns></returns>
        private static DecisionTreeNode GetMissingRecipeIngrediants(string recipeName)
        {
            DecisionTreeNode root = null;
            //don't need to set true nodes for last brach in trees since they will theoretically never be reached
            //(assuming this is called to check if the player has all the required ingrediants to make the recipe)

            switch (recipeName)
            {
                case "Omelet":
                    #region Get Milk
                    int desiredMilkCount = 1;
                    List<Decision> milkTree = CreateIngrediantTree(Instance.MilkList, desiredMilkCount);
                    #endregion

                    #region Get Egg
                    int desiredEggCount = 1;
                    List<Decision> eggTree = CreateIngrediantTree(Instance.EggList, desiredEggCount);
                    eggTree.ForEach(t => t.SetTrueNode(milkTree[0]));
                    #endregion

                    root = eggTree[0];
                    break;
            }

            return root;

        }

        /// <summary>
        /// Helper method to create a tree that will check if the player has a certain amount of an item
        /// within a pool
        /// </summary>
        /// <param name="itemNames">the list of items</param>
        /// <param name="desiredAmount">the amount the player needs to have of at least one of the items</param>
        /// <returns></returns>
        private static List<Decision> CreateIngrediantTree(List<ItemName> itemNames, int desiredAmount)
        { 
            List<Decision.DecisionDelegate> checks = Instance.GetDecisionChecks(itemNames, desiredAmount);
            List<Decision> tree = itemNames.Select((egg, index) => new Decision(checks[index])).ToList();

            for (int i = 0; i < tree.Count; i++)
            {
                Decision decision = tree[i];
                decision.SetFalseNode(i == checks.Count - 1 ? GetProducableItemTree(itemNames[0], desiredAmount, null, false) : tree[i + 1]);
            }
            return tree;
        }

        /// <summary>
        /// Gives a lsit of deicsion check of if the player has a certain amount of each item
        /// </summary>
        /// <param name="itemNames">the items that are being checked for</param>
        /// <param name="desiredAmount">the desiredAmount of items that are being checked for</param>
        /// <returns></returns>
        private List<Decision.DecisionDelegate> GetDecisionChecks(List<ItemName> itemNames, int desiredAmount)
        {
            //this method is currently only being used in CreateIngrediantTree. If that stays, then this can be combined into said method
            return itemNames.Select(e =>
            {
                return (Decision.DecisionDelegate)(() => Instance.PlayerHasDesieredAmountOfItem(Instance.ItemIds[e], desiredAmount));
            }).ToList();
        }

        /// <summary>
        /// Tree branch that will tell the user how to get a specific item in the game
        /// </summary>
        /// <param name="qualifiedItemId">the id of the item that needs to be get</param>
        /// <param name="desiredAmount">the amount of the item that would like to be aquired</param>
        /// <param name="actionAfterward">if something should be done after the amount of the item is found</param>
        /// <param name="decreaseCount">if InventoryItemReserveDictonary count should decrease</param>
        /// <returns></returns>
        public static DecisionTreeNode GetProducableItemTree(string qualifiedItemId, int desiredAmount, DecisionTreeNode? actionAfterward = null, bool decreaseCount = true)
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
            string GetItemCountFromFarmHouse()
            {
                return GetItemCountFromLocation("FarmHouse");
            }

            //Check how many {item} are in the Farm and tells the plyaer to get them
            string GetItemCountFromFarm()
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
            bool FarmHasItem()
            {
                return LocationHasItem("Farm");
            }

            //The farm house has the desired item
            bool FarmHouseHasItem()
            {
                return LocationHasItem("FarmHouse");
            }

            //The player has the desired amount of {item}
            bool PlayerHasDesieredAmountOfItem()
            {
                //check if found in the player's ivnentory
                return Instance.PlayerHasDesieredAmountOfItem(qualifiedItemId, desiredAmount, decreaseCount);
            }

            #endregion

            #endregion

            #region Tree
            //there is at least one loction where the player has item to in the farm
            Decision playerHasItemOnFarm = new Decision(
                new GetItemAction(qualifiedItemId, GetItemCountFromFarm),
                new GetItemAction(qualifiedItemId, GetItemFromStore),
                new Decision.DecisionDelegate(FarmHasItem));

            //there is at least one loction where the player has item to in the farm house
            Decision playerHasItemOnFarmHouse = new Decision(
                new GetItemAction(qualifiedItemId, GetItemCountFromFarmHouse),
                playerHasItemOnFarm,
                new Decision.DecisionDelegate(FarmHouseHasItem));

            //the player has {desiredAmount} {item} on them
            Decision playerHasItemInInventory = new Decision(
                actionAfterward == null ? completeAction : actionAfterward,
                playerHasItemOnFarmHouse,
                new Decision.DecisionDelegate(PlayerHasDesieredAmountOfItem));

            return new GetItemNode(playerHasItemInInventory, qualifiedItemId, desiredAmount);
            #endregion
        }

        /// <summary>
        /// Tree branch that will tell the user how to get a specific item in the game
        /// </summary>
        /// <param name="itemName">the name of the item</param>
        /// <param name="desiredAmount">the amount of the item that would like to be aquired</param>
        /// <param name="actionAfterward">if something should be done after the amount of the item is found</param>
        /// <param name="decreaseCount">if InventoryItemReserveDictonary count should decrease</param>
        public static DecisionTreeNode GetProducableItemTree(ItemName itemName, int desiredAmount, DecisionTreeNode? actionAfterward = null, bool decreaseCount = true)
        {
            return GetProducableItemTree(Instance.ItemIds[itemName], desiredAmount, actionAfterward, decreaseCount);
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
        /// <param name="cooking">If the recipie is a cooking recipe. Otherwise it's a crafting recipe</param>
        /// <returns></returns>
        private static DecisionTreeNode CraftItem(string name, bool cooking)
        {
            CraftingRecipe recipe = new CraftingRecipe(name);
            DummyRecipe dummyRecipe;
            
            if (cooking)
            {
                dummyRecipe = Instance.dummyCookingRecipes.First(r => r.Name == name);
            }
            else
            { 
                dummyRecipe = Instance.dummyCraftingRecipes.First(r => r.Name == name);
            }

            #region Delegate Actions

            #endregion
            
            #region Delegate Checks
            bool PlayerKnowsRecipe()
            {
                return Instance.PlayerKnowsRecipe(name);
            }

            bool PlayerHasCraftedRecipie()
            {
                return Instance.PlayerHasCraftedRecipie(dummyRecipe, cooking);
            }

            bool PlayerHasRequiredItems()
            {
                return Instance.PlayerHasRequiredItems(dummyRecipe);
            }

            #endregion

            #region Tree
            //Does player have the required items to create the item in their inventory it's different depending on if
            //it's a cooking recipe because cooking recipies can accept multiple types of ingrediants

            DecisionTreeNode hasItemsCooking = GetMissingRecipeIngrediants(name);

            DecisionTreeNode hasItemsCrafting = new Decision(
                new Action($"{(cooking ? "Cook" : "Craft")} {name}"),
                cooking ? hasItemsCooking : new CraftItemAction(recipe, cooking),
                PlayerHasRequiredItems);


            //Has the player crafted the item at least once?
            DecisionTreeNode craftedItem = new Decision(
                completeAction,
                hasItemsCrafting,
                PlayerHasCraftedRecipie,
                true);

            //does the player know the recipe
            DecisionTreeNode knowRecipe = new Decision(
                craftedItem,
                new Action($"Learn {name} recipe"),
                PlayerKnowsRecipe,
                true);

            //does the player have a kitchen
            Decision playerHasKitchen = new Decision(knowRecipe,
                                        GetHouseUpgrade(1),
                                        Instance.HasDesiredLevelHouse(1),
                                        true);
            #endregion

            return cooking ? playerHasKitchen : knowRecipe;
        }

        /// <summary>
        /// Get a branch to get the farmhouse to a certain upgrade level
        /// </summary>
        /// <param name="desiredLevel">the desired upgrade level of the house</param>
        /// <param name="actionAfterward">if something should be done after the farm house is upgraded to the desired level</param>
        /// <returns></returns>
        private static DecisionTreeNode GetHouseUpgrade(int desiredLevel, DecisionTreeNode? actionAfterward = null)
        {
            Action upgradeHouseAction = new Action("Upgrade house from Robin");
            //the amount of money that is needed for each upgradeLevel
            int[] moneyArr = new int[] { 10000, 65000, 100000 };

            #region Delegate Checks
            Decision.DecisionDelegate HasRequestedHouseUpgrade(int level)
            {
                //this is true if the player's house level is (level - 1) and the player has requsted a house upgrade
                return () => level - 1 == ((FarmHouse)Game1.locations.First(l => l.NameOrUniqueName == "FarmHouse")).upgradeLevel 
                    && Instance.IsUpgradingHouse();
            }

            //Check if the player has the required of money for a specific house upgrade level
            Decision.DecisionDelegate HasRequiredMoneyForUpgrade(int upgradeLevel)
            {
                return () => Instance.HasDesiredMoney(moneyArr[upgradeLevel - 1]);
            }


            #endregion

            //robin is avaiable for requesting construction
            DecisionTreeNode notUpgrading = new Decision(upgradeHouseAction, completeAction, Instance.RobinAviableToBuild());


            //player has enough wood for the level 1 upgrade
            DecisionTreeNode playerHasWood = GetProducableItemTree(ItemName.Wood, 450, notUpgrading);

            //the player has enough hardwood for the level 2 upgrade
            DecisionTreeNode hasHardWood = GetProducableItemTree(ItemName.Hardwood, 100, notUpgrading);
            //the player has the desired amount of gold for level 2 house upgrade
            DecisionTreeNode hasLevel2Money = new Decision(hasHardWood, new Action($"Get {moneyArr[1]} gold"), HasRequiredMoneyForUpgrade(2));


            #region Level 1
            //the player has the desired amount of gold for level 1 house upgrade
            DecisionTreeNode hasLevel1Money = new Decision(playerHasWood, new Action($"Get {moneyArr[0]} gold"), HasRequiredMoneyForUpgrade(1));

            //the player has requested house upgrade level 1
            DecisionTreeNode requestedHouseLevel1 = new Decision(completeAction, hasLevel1Money, HasRequestedHouseUpgrade(1), true);

            //Checks if the player needs to upgrade to a higher level house or if the task is complete
            DecisionTreeNode nextTaskLevel1 = desiredLevel == 1 ? completeAction : hasLevel2Money;

            //the player has a level 1 house
            DecisionTreeNode hasLevel1House = new Decision(nextTaskLevel1, requestedHouseLevel1, Instance.HasDesiredLevelHouse(1), true);
            #endregion

            #region Level 2

            //the player has the desired amount of gold for level 3 house upgrade
            DecisionTreeNode hasLevel3Money = new Decision(notUpgrading, new Action($"Get {moneyArr[2]} gold"), HasRequiredMoneyForUpgrade(3));

            //the player has requested house upgrade level 2
            DecisionTreeNode requestedHouseLevel2 = new Decision(completeAction, hasLevel1House, HasRequestedHouseUpgrade(2), true);

            //Checks if the player needs to upgrade to a higher level house or if the task is complete
            DecisionTreeNode nextTaskLevel2 = desiredLevel == 2 ? completeAction : hasLevel3Money;

            //the player has a level 2 house
            DecisionTreeNode hasLevel2House = new Decision(nextTaskLevel2, requestedHouseLevel2, Instance.HasDesiredLevelHouse(2), true);
            #endregion

            # region Level3
            //player has requested house upgrade level 3
            DecisionTreeNode requestedHouseLevel3 = new Decision(completeAction, hasLevel3Money, HasRequestedHouseUpgrade(3), true);

            //the player has a level 3 house
            DecisionTreeNode hasLevel3House = new Decision(completeAction, requestedHouseLevel3, Instance.HasDesiredLevelHouse(3), true);

            #endregion

            switch (desiredLevel)
            {
                case 1:
                    return hasLevel1House;
                case 2:
                    return hasLevel2House;
                case 3:
                    return hasLevel3House;
                default:
                    throw(new Exception($"Can't upgrade the house to level {desiredLevel}. Valid levels are 1-3 inclusivley"));
            }


        }

        /// <summary>
        /// Get a list of all of the items the player is missing to craft the desired recipe
        /// </summary>
        /// <param name="recipe">the deisred recipie to craft</param>
        /// <returns>A list of the qualified item ids of the missing items. Empty if the user has enough of all the items</returns>
        public List<string> GetRecipeMissingItems(DummyRecipe recipe)
        {
            List<string> missingItemsIds = new List<string>();

            foreach (Dictionary<string, int> list in recipe.RecipeLists)
            {
                //for each list in recipe list, check if the player has the desired amount for at least one item in the list
                bool metCondition = false;

                foreach (KeyValuePair<string, int> kv in list)
                {
                    Instance.InventoryItemReserveDictonary.TryGetValue(kv.Key, out int inventoryCount);
                    if (inventoryCount >= kv.Value)
                    {
                        metCondition = true;
                        break;
                    }
                }

                //If this is false, add the first item in the list to missing items
                if (!metCondition)
                {
                    missingItemsIds.Add(list.ElementAt(0).Key);
                    continue;
                }

            }

            return missingItemsIds;
        }


        #region Delegate Checks

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        ///
        /// <returns>a method that tells if the farmhouse is at least a a certain level</returns>
        Decision.DecisionDelegate HasDesiredLevelHouse(int level)
        {
            return () => ((FarmHouse)Game1.locations.First(l => l.NameOrUniqueName == "FarmHouse")).upgradeLevel >= level;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A delegate that tells if Robin is avaiable to build something for the player</returns>
        Decision.DecisionDelegate RobinAviableToBuild()
        {
            Farm farm = (Farm)Game1.locations.First(l => l.NameOrUniqueName == "Farm");
            IEnumerable<Building> buildings = farm.buildings;
            return () => !IsUpgradingHouse() && buildings.All(s => s.daysOfConstructionLeft.Value <= 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the house is being upgraded</returns>
        bool IsUpgradingHouse()
        {
            return Game1.player.daysUntilHouseUpgrade.Value != -1;
        }


        private bool HasDesiredMoney(int desiredAmount)
        {
            bool condition = reservedMoney >= desiredAmount;
            UpdateReservedMoney(-desiredAmount);
            return condition;
        }

        /// <summary>
        /// Tells if the player has at least the desired amount of an item
        /// </summary>
        /// <param name="qualifiedItemId">the id the item</param>
        /// <param name="desiredAmount">the amount that is wanted</param>
        /// <param name="decreaseCount">if InventoryItemReserveDictonary count should decrease</param>
        /// <returns></returns>
        private bool PlayerHasDesieredAmountOfItem(string qualifiedItemId, int desiredAmount, bool decreaseCount = true)
        {
            Instance.InventoryItemReserveDictonary.TryGetValue(qualifiedItemId, out int itemCount);
            if (decreaseCount)
            { 
                Instance.UpdateReservedItemDicionary(qualifiedItemId, -desiredAmount);
            }
            return itemCount >= desiredAmount;
        }
        #endregion

        #region Delegate Actions

        /// <summary>
        /// Actions that says how much money the player should get
        /// </summary>
        /// <returns></returns>
        private string GetDesiredMoneyAmount()
        {
            return $"Get {requiredMoney - Game1.player.Money} gold";
        }



        #endregion


        bool PlayerKnowsRecipe(string name)
        {
            return Game1.player.knowsRecipe(name);
        }

        bool PlayerHasCraftedRecipie(DummyRecipe recipe, bool cooking)
        {
            if (cooking)
            {
                Game1.player.recipesCooked.TryGetValue(((DummyCookingRecipe)recipe).UnqualifiedItemId, out int timesCrafted);
                return timesCrafted > 0;
            }

            return new CraftingRecipe(recipe.Name).timesCrafted > 0;
        }

        bool PlayerHasRequiredItems(DummyRecipe dummyRecipe)
        {
            return Instance.GetRecipeMissingItems(dummyRecipe).Count == 0;
        }

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
            //combine special actions
            actions = CombineItemActions(actions);
            actions = CombineMoneyActions(actions);

            //combine the regular actions

            //get all of the regular actions strings that are not duplciates
            List<Action> newList = new List<Action>();

            //make a new list of actions gettting rid of duplicate regular actions
            foreach(Action action in actions)
            {
                //if it's a specical action, add it and move on
                if(action.GetType() != typeof(Action))
                {
                    newList.Add(action);
                    continue;
                }

                //if the action is special, only add it if the list doesn't have action display name
                if (newList.All(a => a.DisplayName != action.DisplayName))
                {
                    newList.Add(action);
                }
            }

            return newList;
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

            //uncomment this. This was commented out for debugging purposes

            //clamp to zero
            //InventoryItemReserveDictonary[qualifiedItemId] = Math.Clamp(InventoryItemReserveDictonary[qualifiedItemId], 0, int.MaxValue);

            OrderInventoryItemReserveDictonary();
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
            OrderInventoryItemReserveDictonary();
            return;
        }

        /// <summary>
        /// Helper method that orders InventoryItemReserveDictonary based on the value in descending
        /// </summary>
        private void OrderInventoryItemReserveDictonary()
        {
            InventoryItemReserveDictonary = InventoryItemReserveDictonary.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
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
