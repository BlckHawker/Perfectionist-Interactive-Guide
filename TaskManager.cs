/*
 * todo
 Check that Grow Harvest works with regrowable crops like green beans and blueberries
 https://gitlab.com/enom/time-before-harvest-enhanced/-/blob/main/ModEntry.cs?ref_type=heads
 */

using Microsoft.Xna.Framework;
using Netcode;
using Stardew_100_Percent_Mod.Decision_Trees;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using static Stardew_100_Percent_Mod.NPCManager;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Manages the Tree of of taks needed in order to 100% the game
    /// </summary>
    internal static class TaskManager
    {
        const int MONTH_DAY_COUNT = 28; //The number of days in a month
        public static List<DecisionTreeNode> roots { get; private set; }

        //keep try of items that don't exist in the world
        private static List<DummyItem> dummyItems;

        public delegate bool TaskComplateDelegate();
        public delegate string UpdateTaskDisplayNameDelegate(Task t);

        public delegate void LogMethod(string message, LogLevel logLevel = LogLevel.Debug);
        public static LogMethod logMethod;

        //means either the player completed the overall task, or they need to go to bed.
        //Either way, there's nothing to display to them, which is why it's an empty string
        private static Action completeAction = new Action("");

        //action that tells the user to get money
        private static GetMoneyAction getMoneyAction;

        //Keeps track of how many of each item the user needs
        //Used so there aren't multiple tasks saying "Buy x items at store"
        //Example, there are two tasks that both rquire the user to get 15 seeds
        //We don't want two tasks saying "Buy 15 seeds at store"
        //It should say "Buy 30 seeds at store" instead

        //make this private. Became public for debugging purposes
        public static Dictionary<string, int> requiredItemsDictionary;

        //keeps track of what items the player has in their inventory. Mainly used to reserve items for quests
        //key is the item id
        //value is the amount of that item that is free to use
        public static Dictionary<string, int> InventoryItemReserveDictonary { get; private set; }

        //Keeps track of the number of crops grown. Key is qualifiedItemId
        //this is public for debugging purposes. It should be private
        public static Dictionary<string, int> cropsGrownDictinary;

        //Keeps track of the desired amount of crops to be grown. Key is qualifiedItemId
        private static Dictionary<string, int> requiredCropsGrownDictinary;

        //how much money the player has to spare
        public static int reservedMoney;
        private static int requiredMoney;
        //A dictionary to easily hold the id of each item
        public static Dictionary<ItemName, string> ItemIds { get; private set; }

        //List of all of the custom crafting recipes (see class defintion to see reason why it exists)
        private static List<DummyCraftingRecipe> dummyCraftingRecipes;
        private static List<DummyCookingRecipe> dummyCookingRecipes;


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
            Parsnip,
            ParsnipSeeds,
            Stone,
            VoidEgg,
            Wood
        }

        //All item of the following item names lists are ordered by easiest to hardest to get

        //all of the eggs that are considered eggs for a cooking recipe
        public static readonly List<ItemName> EggList = new List<ItemName>() { ItemName.Egg, ItemName.BrownEgg, ItemName.LargeEgg, ItemName.BrownLargeEgg,
                                                                ItemName.DuckEgg, ItemName.VoidEgg, ItemName.OstritchEgg, ItemName.GoldenEgg  };
        //all of the milks that are considered milk for a cooking recipe
        public static readonly List<ItemName> MilkList = new List<ItemName>() { ItemName.Milk, ItemName.GoatMilk, ItemName.LargeMilk, ItemName.LargeGoatMilk };

        //tree that tell the player how to upgrade the house to a specific level
        private static DecisionTreeNode[] houseUpgradeTrees = new DecisionTreeNode[3];

        public static void InitalizeInstance(LogMethod logMethod)
        {

            TaskManager.logMethod = logMethod;

            requiredItemsDictionary = new Dictionary<string, int>();
            InventoryItemReserveDictonary = new Dictionary<string, int>();
            requiredCropsGrownDictinary = new Dictionary<string, int>();
            cropsGrownDictinary = new Dictionary<string, int>();

            getMoneyAction = new GetMoneyAction(GetDesiredMoneyAmount);


            ItemIds = new Dictionary<ItemName, string>()
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
                { ItemName.Parsnip, "(O)24" },
                { ItemName.ParsnipSeeds, "(O)472"},
                { ItemName.Stone, "(O)390" },
                { ItemName.Wood, "(O)388"},
                { ItemName.VoidEgg, "(O)305" }
            };

            //check if any ItemName keys are missing in ItemIds
            List<ItemName> missingItemIds = Enumerable.Range(0, Enum.GetNames(typeof(ItemName)).Length)
                .Where(i => !ItemIds.ContainsKey((ItemName)i))
                .Select(i => (ItemName)i).ToList();

            if (missingItemIds.Count > 0)
            {
                throw new Exception($"The following keys are missing in \"ItemIds\": {string.Join(", ", missingItemIds)}");
            }

            dummyCraftingRecipes = DummyCraftingRecipe.GetAllRecipes();
            dummyCookingRecipes = DummyCookingRecipe.GetAllRecipes();
            dummyItems = ItemIds.Select(kv => new DummyItem(kv.Value, kv.Key.ToString())).ToList();

            #region Construction Objects
            Construction shed = new Construction(
                "Shed",
                Game1.buildingData.First(d => d.Key == "Shed").Value, 
                false,
                completeFunction: () => GetLocation("Farm").buildings.Any(b => b.buildingType.Value == "Shed" && b.daysOfConstructionLeft.Value < 1),
                underConstructionFunction: () => GetLocation("Farm").buildings.Any(b => b.buildingType.Value == "Shed" && b.daysOfConstructionLeft.Value > 0));

            Construction bigShed = new Construction(
                "Big Shed",
                Game1.buildingData.First(d => d.Key == "Big Shed").Value,
                false,
                completeFunction: () => GetLocation("Farm").buildings.Any(b => b.buildingType.Value == "Big Shed" && b.daysOfConstructionLeft.Value < 1),
                underConstructionFunction: () => GetLocation("Farm").buildings.Any(b => b.buildingType.Value == "Shed" && b.daysUntilUpgrade.Value > 0),
                shed);

            Construction houseUpgrade1 = new Construction(
                "House Upgrade 1",
                true,
                completeFunction: HasDesiredLevelHouse(1),
                underConstructionFunction: HasRequestedHouseUpgrade(1),
                10000,
                materialNeeded: new Dictionary<ItemName, int>() { { ItemName.Wood, 450 } });

            Construction houseUpgrade2 = new Construction(
                "House Upgrade 2",
                true,
                completeFunction: HasDesiredLevelHouse(2),
                underConstructionFunction: HasRequestedHouseUpgrade(2),
                65000,
                materialNeeded: new Dictionary<ItemName, int>() { { ItemName.Hardwood, 100 } },
                houseUpgrade1);

            Construction houseUpgrade3 = new Construction(
                "House Upgrade 3",
                true,
                completeFunction: HasDesiredLevelHouse(3),
                underConstructionFunction: HasRequestedHouseUpgrade(3),
                100000,
                materialNeeded: new Dictionary<ItemName, int>(),
                houseUpgrade2);

            #endregion

            DecisionTreeNode buildShed = ConstructionTask(shed, false);
            DecisionTreeNode buildBigShed = ConstructionTask(bigShed, false);
            houseUpgradeTrees[0] = ConstructionTask(houseUpgrade1, false);
            houseUpgradeTrees[1] = ConstructionTask(houseUpgrade2, false);
            houseUpgradeTrees[2] = ConstructionTask(houseUpgrade3, false);


            DecisionTreeNode parsnipSeedsTree = GetProducableItemTree(ItemName.ParsnipSeeds, 15);

            DecisionTreeNode becomeFriendsWithJas = BecomeFriendsWithNPC("Jas");

            DecisionTreeNode craftChest = CraftItem("Chest", false);

            DecisionTreeNode cookOmelet = CraftItem("Omelet", true);

            DecisionTreeNode growFiveParsnips = GrowCrop(ItemName.Parsnip, 5);

            roots = new List<DecisionTreeNode>() { becomeFriendsWithJas, craftChest, cookOmelet, growFiveParsnips, buildBigShed };
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
                    List<Decision> milkTree = CreateIngrediantTree(MilkList, desiredMilkCount);
                    #endregion

                    #region Get Egg
                    int desiredEggCount = 1;
                    List<Decision> eggTree = CreateIngrediantTree(EggList, desiredEggCount);
                    eggTree.ForEach(t => t.SetTrueNode(milkTree[0]));
                    #endregion

                    root = eggTree[0];
                    break;

                default:
                    throw new Exception($"A recipe with a name {recipeName} has not be implemented yet");
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
            List<Decision.DecisionDelegate> checks = GetDecisionChecks(itemNames, desiredAmount);
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
        private static List<Decision.DecisionDelegate> GetDecisionChecks(List<ItemName> itemNames, int desiredAmount)
        {
            //this method is currently only being used in CreateIngrediantTree. If that stays, then this can be combined into said method
            return itemNames.Select(e =>
            {
                return (Decision.DecisionDelegate)(() => PlayerHasDesieredAmountOfItem(ItemIds[e], desiredAmount));
            }).ToList();
        }

        private static DecisionTreeNode GrowCrop(ItemName itemName, int desiredAmount)
        {
            //todo
            //Make it so the location is set automatically based on the seed
            //Make a dictionary that will contain which seeds grow which crops

            Farm farm = (Farm)GetLocation("Farm");
            string seedId = "472"; //parnip seed

            return GrowCrop(farm, ItemIds[itemName], seedId, desiredAmount);
        }

        /// <summary>
        /// Task to grow a number of a specific crop
        /// </summary>
        /// <param name="location">The location the crop is growing</param>
        /// <param name="qualifiedItemId">the qualified id of the desired crop</param>
        /// <param name="unqualifiedSeedId">the unqualified id of the seed that grows into the desired crop</param>
        /// <param name="desiredAmount">the amount of the crop that wants to be grown</param>
        /// <returns></returns>
        private static DecisionTreeNode GrowCrop(GameLocation location, string qualifiedItemId, string unqualifiedSeedId, int desiredAmount)
        {
            Item item = ItemLocator.GetItem(qualifiedItemId);
            DummyItem dummyItem = dummyItems.First(i => i.QualifiedItemId == qualifiedItemId);
            string qualifiedSeedId = $"(O){unqualifiedSeedId}";

            IEnumerable<Crop> GetDesiredCrops()
            {
                //Under the assumption that all qualified ids start with (O) followed by the unqualified id
                string desiredUnqualifiedId = qualifiedItemId.Replace("(O)", "");
                List<Crop> plantedCrops = location.terrainFeatures.Pairs.Where(pair => pair.Value is HoeDirt hoeDirt && hoeDirt.crop != null).Select(pair => ((HoeDirt)pair.Value).crop).ToList();
                List<Crop> desiredCrops = plantedCrops.Where(crop => crop.indexOfHarvest.Value == desiredUnqualifiedId).ToList();
                return desiredCrops;
            }

            IEnumerable<Crop> GetDesiredFullyGrownCrops()
            {
                List<Crop> desiredCrops = GetDesiredCrops().ToList();
                List<Crop> fullyGrownCrops = desiredCrops
                .Where(crop =>
                {
                    bool cropReady1 = crop.currentPhase.Value >= crop.phaseDays.Count - 1
                        && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0);

                    int totalDays = crop.phaseDays.Take(crop.phaseDays.Count() - 1).Sum();
                    int growProgress = crop.phaseDays.Take(crop.currentPhase.Value).Sum() + crop.dayOfCurrentPhase.Value;

                    bool cropReady2 = growProgress >= totalDays;

                    return cropReady1 || cropReady2;
                }).ToList();

                return fullyGrownCrops;
            }

            //order by how fast the crop is going to grow
            IEnumerable<Crop> GetDesiredNotFullyGrownCrops()
            {
                return GetDesiredCrops().Except(GetDesiredFullyGrownCrops()).OrderBy(crop =>
                {
                    int totalDays = crop.phaseDays.Take(crop.phaseDays.Count() - 1).Sum();
                    int growProgress = crop.phaseDays.Take(crop.currentPhase.Value).Sum() + crop.dayOfCurrentPhase.Value;
                    return totalDays - growProgress;
                });
            }

            IEnumerable<Crop> GetUnWateredDesiredCrops()
            {
                List<Crop> desiredNotGrownCrops = GetDesiredNotFullyGrownCrops().ToList();
                List<Crop> unwanteredCrops = desiredNotGrownCrops.Where(crop => !crop.Dirt.isWatered() && crop.Dirt.needsWatering()).ToList();
                return unwanteredCrops;
            }

            //the number of seeds the player needs to plant
            //bool total - if the method should check all tasks requiring this seed, or just this specific one
            int GetRequiredSeedsCount(bool total)
            {
                int requiredAmount;
                if (total)
                {
                    requiredCropsGrownDictinary.TryGetValue(qualifiedItemId, out requiredAmount);
                }

                else
                {
                    requiredAmount = desiredAmount;
                }
                int cropsGrown;
                int cropsInGroundCount = GetDesiredCrops().Count();
                cropsGrownDictinary.TryGetValue(qualifiedItemId, out cropsGrown);
                //the number of crops to plant should be the desired amount to grow - the # crops of crops already grown
                //- the crops in the ground
                return requiredAmount - cropsGrown - cropsInGroundCount;
            }

            string HarvestCropAction()
            {
                Crop crop = GetDesiredFullyGrownCrops().First();
                Vector2 position = crop.tilePosition;
                string location = crop.currentLocation.ToString().Replace("StardewValley.", "");
                return $"Harvest {ItemIds.First(kv => kv.Value == qualifiedItemId).Key} at location ({position.X},{position.Y}) on the {location}";
            }

            string GrowCropAction()
            {
                int currentAmountGrown = cropsGrownDictinary.ContainsKey(qualifiedItemId) ? cropsGrownDictinary[qualifiedItemId] : 0;
                return $"Grow {requiredCropsGrownDictinary[qualifiedItemId] - currentAmountGrown} {item?.Name ?? dummyItem.DisplayName}";
            }

            string WaterCropAction()
            {
                Crop crop = GetUnWateredDesiredCrops().First();
                Vector2 position = crop.tilePosition;
                string location = crop.currentLocation.ToString().Replace("StardewValley.", "");
                return $"Water {ItemIds.First(kv => kv.Value == qualifiedItemId).Key} at location ({position.X},{position.Y}) on the {location}";
            }

            string PlantCrops()
            {
                int totalRequiredSeedCount = GetRequiredSeedsCount(true);
                int taskRequiredSeedCount = GetRequiredSeedsCount(false);
                int currentSeedCount = ItemLocator.PlayerItemCount(qualifiedSeedId);

                
                int seedsToPlantCount;

                //refactor this to be a ternerary

                //check if the player can plant the total required seed amount
                if (currentSeedCount >= totalRequiredSeedCount)
                {
                    seedsToPlantCount = totalRequiredSeedCount;
                }
                //if not then tell them to plant the task amount
                else
                { 
                    seedsToPlantCount = taskRequiredSeedCount;
                }

                string seedName = ItemIds.First(kv => kv.Value == qualifiedSeedId).Key.ToString();
                return $"Plant {seedsToPlantCount} {seedName}";
            }

            

            //Checks if the player has enough
            bool GrownEnoughCrops()
            {
                return PlayerHasGrownDesiredAmountOfCrop(qualifiedItemId);
            }

            bool HasCropPlanted()
            {
                List<Crop> desiredCrops = GetDesiredCrops().ToList();


                //the number of crops the player should have planted is required amount - amount planted and harvested
                //this may break when having multiple of the same task with the same crop
                requiredCropsGrownDictinary.TryGetValue(qualifiedItemId, out int requiredAmout);
                cropsGrownDictinary.TryGetValue(qualifiedItemId, out int cropGrownCount);


                //logMethod($"Requires: {requiredAmout}");
                //logMethod($"Has planted: {desiredCrops.Count}");

                return (requiredAmout - desiredCrops.Count - cropGrownCount) <= 0;
            }

            //Can this crop grown this season?
            //If yes, are there enough days to grow the crop?
            //If not, can the crop grown next season?
            bool CropCanGrow(Crop? crop = null)
            {
                CropData data;
                Crop.TryGetData(unqualifiedSeedId, out data);

                #region Can Crop Grown This Season
                if (!Crop.IsInSeason(location, unqualifiedSeedId))
                {
                    return false;
                }

                int currentPhase;
                int dayOfCurrentPhase;

                NetIntList phaseDays = new NetIntList();
                if (crop == null)
                {
                    phaseDays.AddRange(data.DaysInPhase);
                    phaseDays.Add(99999);
                    currentPhase = 0;
                    dayOfCurrentPhase = 0;
                }

                else
                {
                    phaseDays = crop.phaseDays;
                    currentPhase = crop.currentPhase.Value;
                    dayOfCurrentPhase = crop.dayOfCurrentPhase.Value;
                }

                //is there enough days within this seaon for the crop to grow
                int totalDays = phaseDays.Take(phaseDays.Count() - 1).Sum();
                int growProgress = phaseDays.Take(currentPhase).Sum() + dayOfCurrentPhase;
                int daysLeftForCropToGrow = totalDays - growProgress;
                int daysLeftInSeason = MONTH_DAY_COUNT - Game1.dayOfMonth;

                if (daysLeftInSeason >= daysLeftForCropToGrow)
                {
                    return true;
                }
                #endregion

                #region Can Crop Grow Next Season
                Season nextSeason = (Season)((int)(location.GetSeason() + 1) % Enum.GetNames(typeof(Season)).Length);

                //if the seed ignores seasons in a specific location, then it can grow next seasn
                if (location.SeedsIgnoreSeasonsHere())
                {
                    return true;
                }

                return data.Seasons?.Contains(nextSeason) ?? false;
                #endregion

            }

            bool CropReadyForHarvest()
            {
                return GetDesiredFullyGrownCrops().Any();
            }


            //If a crop that is currenlty growing can be harvested before the season is over
            bool SalvageableCrop()
            {
                Crop crop = GetDesiredNotFullyGrownCrops().FirstOrDefault();
                return CropCanGrow(crop);
            }

            //There is at least one crop in the ground that is not watered
            bool UnwateredCrop()
            {
                return GetUnWateredDesiredCrops().Any();
            }

            bool PlayerHasEnoughSeeds()
            {
                int requiredSeedCount = GetRequiredSeedsCount(false);
                InventoryItemReserveDictonary.TryGetValue(qualifiedSeedId, out int currentSeedCount);

                if (requiredSeedCount <= currentSeedCount)
                { 
                    UpdateReservedItemDicionary(qualifiedItemId, -requiredSeedCount);
                    return true;
                }

                return false;
            }

            //Player has enough seeds to complete the task
            Decision hasEnoughSeeds = new Decision(PlayerHasEnoughSeeds);
            hasEnoughSeeds.SetTrueNode(new Action(PlantCrops));
            hasEnoughSeeds.SetFalseNode(GetProducableItemTree(qualifiedSeedId, desiredAmount));

            //Can crop grow this seaason without crop in ground
            Decision cropGrowsThisSeason = new Decision(() => CropCanGrow());
            cropGrowsThisSeason.SetTrueNode(hasEnoughSeeds);
            cropGrowsThisSeason.SetFalseNode(new Action(""));

            //There is a crop in the ground that is not watered
            Decision unwateredCrop = new Decision(UnwateredCrop);
            unwateredCrop.SetTrueNode(new Action(WaterCropAction));
            unwateredCrop.SetFalseNode(new Action(""));

            //There is at least one crop that can be fully grown if the player continues to water it before it dies
            Decision savalableCrop = new Decision(SalvageableCrop);
            savalableCrop.SetTrueNode(unwateredCrop);
            //Wait til it's the right season again
            savalableCrop.SetFalseNode(new Action(""));

            //There is at least a desired crop ready for harvest
            Decision cropReadyForHarvest = new Decision(CropReadyForHarvest);
            cropReadyForHarvest.SetTrueNode(new Action(HarvestCropAction));
            cropReadyForHarvest.SetFalseNode(savalableCrop);

            //Does the player have at least the desired amount of the crop planted
            Decision cropPlanted = new Decision(HasCropPlanted);
            cropPlanted.SetTrueNode(cropReadyForHarvest);
            cropPlanted.SetFalseNode(cropGrowsThisSeason);

            //Has the player harvested the desired amount of crops?
            Decision grownEnoughCrops = new Decision(GrownEnoughCrops, true);
            grownEnoughCrops.SetTrueNode(completeAction);
            grownEnoughCrops.SetFalseNode(cropPlanted);
            return new GrowCropNode(grownEnoughCrops, qualifiedItemId, desiredAmount);
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
            DummyItem dummyItem = dummyItems.First(i => i.QualifiedItemId == qualifiedItemId);

            #region Delegate Methods

            #region Delegate Actions

            string GetItemFromStore()
            {
                //get the amount of the item the player currently has including iventory, iventory cursor, and shop cursor
                int playerInventoryCount = ItemLocator.PlayerItemCount(qualifiedItemId) + ItemLocator.ShopCursorCount(qualifiedItemId);

                //check if the player is buying something from the store and their cursor has the desried item
                

                if (item != null)
                {
                    return $"Buy {requiredItemsDictionary[qualifiedItemId] - playerInventoryCount} {item.Name}(s) from store";
                }

                return $"Buy {requiredItemsDictionary[qualifiedItemId] - playerInventoryCount} {dummyItem.DisplayName}(s) from store";
            }

            //Return the amount of items he player should get from that location
            string GetItemCountFromLocation(string uniqueLocationName)
            {
                GameLocation location = GetLocation(uniqueLocationName);

                Chest chest = ItemLocator.GetChestsWithItem(location, qualifiedItemId).First();

                int locationItemCount = ItemLocator.GetChestItemCount(chest, qualifiedItemId);

                int playerInventoryCount = ItemLocator.PlayerItemCount(qualifiedItemId);

                int desiredCount = Math.Min(locationItemCount, requiredItemsDictionary[qualifiedItemId] - playerInventoryCount);

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
                GameLocation location = GetLocation(uniqueLocationName);
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
                new Decision.DecisionDelegate(() => TaskManager.PlayerHasDesieredAmountOfItem(qualifiedItemId, desiredAmount, decreaseCount)));

            return new GetItemNode(playerHasItemInInventory, qualifiedItemId, desiredAmount);
            #endregion
        }

        /// <summary>
        /// Gets a location that matches the given the uniqueLocationName
        /// </summary>
        /// <param name="uniqueLocationName"></param>
        /// <returns></returns>
        public static GameLocation GetLocation(string uniqueLocationName)
        {
            //this method public for debugging purposes, but should private
            return Game1.locations.First(l => l.NameOrUniqueName == uniqueLocationName);
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
            return GetProducableItemTree(ItemIds[itemName], desiredAmount, actionAfterward, decreaseCount);
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
                dummyRecipe = dummyCookingRecipes.First(r => r.Name == name);
            }
            else
            { 
                dummyRecipe = dummyCraftingRecipes.First(r => r.Name == name);
            }

            #region Tree
            //Does player have the required items to create the item in their inventory it's different depending on if
            //it's a cooking recipe because cooking recipies can accept multiple types of ingrediants

            DecisionTreeNode hasItemsCooking = cooking ? GetMissingRecipeIngrediants(name) : null;

            DecisionTreeNode hasItemsCrafting = new Decision(
                new Action($"{(cooking ? "Cook" : "Craft")} {name}"),
                cooking ? hasItemsCooking : new CraftItemAction(recipe, cooking),
                () => TaskManager.PlayerHasRequiredItems(dummyRecipe));


            //Has the player crafted the item at least once?
            DecisionTreeNode craftedItem = new Decision(
                completeAction,
                hasItemsCrafting,
                () => TaskManager.PlayerHasCraftedRecipie(dummyRecipe, cooking),
                true);

            //does the player know the recipe
            DecisionTreeNode knowRecipe = new Decision(
                craftedItem,
                new Action($"Learn {name} recipe"),
                () => TaskManager.PlayerKnowsRecipe(name),
                true);

            //does the player have a kitchen
            Decision playerHasKitchen = new Decision(knowRecipe,
                                        houseUpgradeTrees[0],
                                        HasDesiredLevelHouse(1),
                                        true);
            #endregion

            return cooking ? playerHasKitchen : knowRecipe;
        }

        /// <summary>
        /// Create a task for the player to either build or upgrade a building from Robin
        /// </summary>
        /// <param name="permament">if the task is permanent</param>
        /// <param name="constructionObject">The building/upgrade to be complete</param>
        /// <param name="getBigMoneyTask">If the player should get money using the big "get money" task. Otherwise will get money from small "get money task"</param>
        /// <returns></returns>
        private static DecisionTreeNode ConstructionTask(Construction construction, bool getBigMoneyTask)
        {
            //Robin is aviable to upgrade / build something
            Decision robinAvaiable = new Decision(RobinAviableToBuild());
            robinAvaiable.SetTrueNode(new Action($"Build {construction.Name}"));
            robinAvaiable.SetFalseNode(new Action(""));

            DecisionTreeNode GetItemTree(int index)
            {
                if (index == construction.MaterialsNeeded.Count)
                    return robinAvaiable;
                KeyValuePair<string, int> kv = construction.MaterialsNeeded.ElementAt(index);
                return GetProducableItemTree(kv.Key, kv.Value, GetItemTree(index + 1));
            }

            //Player has the necessary materials for the construction (make a dynamic tree based on the materials required
            DecisionTreeNode getMaterialTree = GetItemTree(0);

            //Player has enough money for construction
            Decision playerHasEnoughMoney = new Decision(() => HasDesiredMoney(construction.BuildCost));
            playerHasEnoughMoney.SetTrueNode(getMaterialTree);
            playerHasEnoughMoney.SetFalseNode(getBigMoneyTask ? GetBigMoney() : GetSmallMoney());

            //Player has prerequisite building / upgrade
            Decision playerHasPrereq = new Decision(() => construction.PrequisiteConstruction == null || construction.PrequisiteConstruction.Complete);
            playerHasPrereq.SetTrueNode(playerHasEnoughMoney);
            playerHasPrereq.SetFalseNode(construction.PrequisiteConstruction == null ? new Action("") : ConstructionTask(construction.PrequisiteConstruction, getBigMoneyTask));

            //player has requested desired building
            Decision requestedBuilding = new Decision(construction.UnderConstructionFunction);
            requestedBuilding.SetTrueNode(new Action(""));
            requestedBuilding.SetFalseNode(playerHasPrereq);

            //Player has the building constructed / upgraded the building on the farm
            Decision hasConstructedBuilding = new Decision(() => construction.Complete, construction.Permanent);
            hasConstructedBuilding.SetTrueNode(new Action(""));
            hasConstructedBuilding.SetFalseNode(requestedBuilding);

            return hasConstructedBuilding;
        }

        private static DecisionTreeNode GetSmallMoney()
        {
            return new Action(() => $"Get {requiredMoney} gold (small)");
        }

        private static DecisionTreeNode GetBigMoney()
        {
            return new Action(() => $"Get {requiredMoney} gold (big)");
        }

        public static Decision.DecisionDelegate HasRequestedHouseUpgrade(int level)
        {
            //this is true if the player's house level is (level - 1) and the player has requsted a house upgrade
            return () => level - 1 == ((FarmHouse)Game1.locations.First(l => l.NameOrUniqueName == "FarmHouse")).upgradeLevel
                && IsUpgradingHouse();
        }

        /// <summary>
        /// Get a list of all of the items the player is missing to craft the desired recipe
        /// </summary>
        /// <param name="recipe">the deisred recipie to craft</param>
        /// <returns>A list of the qualified item ids of the missing items. Empty if the user has enough of all the items</returns>
        public static List<string> GetRecipeMissingItems(DummyRecipe recipe)
        {
            List<string> missingItemsIds = new List<string>();

            foreach (Dictionary<string, int> list in recipe.RecipeLists)
            {
                //for each list in recipe list, check if the player has the desired amount for at least one item in the list
                bool metCondition = false;

                foreach (KeyValuePair<string, int> kv in list)
                {
                    InventoryItemReserveDictonary.TryGetValue(kv.Key, out int inventoryCount);
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
        private static Decision.DecisionDelegate HasDesiredLevelHouse(int level)
        {
            return () => ((FarmHouse)Game1.locations.First(l => l.NameOrUniqueName == "FarmHouse")).upgradeLevel >= level;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A delegate that tells if Robin is avaiable to build something for the player</returns>
        private static Decision.DecisionDelegate RobinAviableToBuild()
        {
            Farm farm = (Farm)Game1.locations.First(l => l.NameOrUniqueName == "Farm");
            IEnumerable<Building> buildings = farm.buildings;
            return () => !IsUpgradingHouse() && buildings.All(s => s.daysOfConstructionLeft.Value <= 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the house is being upgraded</returns>
        private static bool IsUpgradingHouse()
        {
            return Game1.player.daysUntilHouseUpgrade.Value != -1;
        }


        private static bool HasDesiredMoney(int desiredAmount)
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
        private static bool PlayerHasDesieredAmountOfItem(string qualifiedItemId, int desiredAmount, bool decreaseCount = true)
        {
            InventoryItemReserveDictonary.TryGetValue(qualifiedItemId, out int itemCount);
            if (decreaseCount)
            { 
                UpdateReservedItemDicionary(qualifiedItemId, -desiredAmount);
            }
            return itemCount >= desiredAmount;
        }

        /// <summary>
        /// Checks if the player has grown the desired amount of crops
        /// </summary>
        /// <param name="qualifiedItemId"></param>
        /// <returns>true if </returns>
        private static bool PlayerHasGrownDesiredAmountOfCrop(string qualifiedItemId)
        { 
            cropsGrownDictinary.TryGetValue(qualifiedItemId, out int grownAmount);
            requiredCropsGrownDictinary.TryGetValue(qualifiedItemId, out int requiredGrownAmount);
            return requiredGrownAmount > 0 && grownAmount >= requiredGrownAmount;
        }
        #endregion

        #region Delegate Actions

        /// <summary>
        /// Actions that says how much money the player should get
        /// </summary>
        /// <returns></returns>
        private static string GetDesiredMoneyAmount()
        {
            return $"Get {requiredMoney - Game1.player.Money} gold";
        }



        #endregion


        private static bool PlayerKnowsRecipe(string name)
        {
            return Game1.player.knowsRecipe(name);
        }

        private static bool PlayerHasCraftedRecipie(DummyRecipe recipe, bool cooking)
        {
            if (cooking)
            {
                Game1.player.recipesCooked.TryGetValue(((DummyCookingRecipe)recipe).UnqualifiedItemId, out int timesCrafted);
                return timesCrafted > 0;
            }

            return new CraftingRecipe(recipe.Name).timesCrafted > 0;
        }

        private static bool PlayerHasRequiredItems(DummyRecipe dummyRecipe)
        {
            return GetRecipeMissingItems(dummyRecipe).Count == 0;
        }

        /// <summary>
        /// Combine Actions that tell the player to get the same item, but different amounts
        /// </summary>
        /// <param name="actions">The actions</param>
        private static List<Action> CombineItemActions(List<Action> actions)
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
        public static List<Action> CombineActions(List<Action> actions)
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

        public static void UpdateRequiredCropsGrownDictionary(string qualifiedItemId, int count)
        {
            //increment the required number of crops to grow
            if (requiredCropsGrownDictinary.ContainsKey(qualifiedItemId))
            {
                requiredCropsGrownDictinary[qualifiedItemId] += count;
            }

            else
            {
                requiredCropsGrownDictinary[qualifiedItemId] = count;
            }
        }


        public static void UpdateRequiredItemsDictionary(string qualifiedItemId, int count)
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

        public static void UpdateReservedItemDicionary(string qualifiedItemId, int count)
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
        private static void UpdateReservedMoney(int count)
        {
            //clamp to zero
            requiredMoney = Math.Clamp(requiredMoney - count, 0, int.MaxValue);

            //clamp to zero
            reservedMoney = Math.Clamp(reservedMoney + count, 0, int.MaxValue);
        }

        /// <summary>
        /// Things to do right before evaluating each task
        /// </summary>
        public static void PreFix()
        {
            requiredMoney = 0;
            reservedMoney = Game1.player.Money;
            requiredItemsDictionary.Clear();
            InventoryItemReserveDictonary.Clear();
            requiredCropsGrownDictinary.Clear();

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
            Item? shopItem = ItemLocator.ShopCursorItem();
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
        /// Things to do after evaluating each task
        /// </summary>
        public static void PostFix()
        {
            //foreach id in cropsGrownDictinary, if the id can't be found or the value is 0 in requiredCropsGrownDictinary, set the value to 0 in cropsGrownDictinary

            IEnumerable<string> targetedKeys = cropsGrownDictinary.Keys.Where(key => cropsGrownDictinary[key] > 0).ToList();

            foreach (string key in targetedKeys)
            { 
                if (!requiredCropsGrownDictinary.ContainsKey(key) || requiredCropsGrownDictinary[key] == 0)
                {
                    cropsGrownDictinary[key] = 0;
                }
            }
        }

        public static void AddToGrowCropCount(string qualifiedItemId)
        {

            if (!cropsGrownDictinary.ContainsKey(qualifiedItemId))
            {
                cropsGrownDictinary[qualifiedItemId] = 1;
            }

            else
            {
                cropsGrownDictinary[qualifiedItemId]++;
            }
        }

        /// <summary>
        /// Helper method that orders InventoryItemReserveDictonary based on the value in descending
        /// </summary>
        private static void OrderInventoryItemReserveDictonary()
        {
            InventoryItemReserveDictonary = InventoryItemReserveDictonary.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        }


        /// <summary>
        /// Combine get money actions
        /// </summary>
        /// <param name="actions">the original list of actions</param>
        /// <returns>A new list with the combined </returns>
        private static List<Action> CombineMoneyActions(List<Action> actions)
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
                if (action is GetMoneyAction getMoneyAction)
                {
                    moneyRequired += getMoneyAction.MoneyRequired;
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
        private static List<Action> CombineItemAction(string qualifiedItemId, List<Action> actions)
        {
            List<Action> newList = new List<Action>();
            bool foundItemAction = false;

            for (int i = 0; i < actions.Count; i++)
            {
                Action action = actions[i];
                if (action is GetItemAction getItemAction)
                {
                    //add the GetItemAction if the desired QualifiedItemId hasn't been found yet or
                    //if the QualifiedItemId is not the one that's desired
                    if (getItemAction.QualifiedItemId != qualifiedItemId)
                    {
                        newList.Add(getItemAction);
                    }

                    else if (getItemAction.QualifiedItemId == qualifiedItemId && !foundItemAction)
                    { 
                        newList.Add(getItemAction);
                        foundItemAction = true;
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
