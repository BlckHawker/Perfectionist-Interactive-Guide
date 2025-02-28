using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.HomeRenovations;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Tiles;

namespace Stardew_100_Percent_Mod
{
    //todo the scheduels are currently hardcoded, it would be wise to find a more dynmaic way
    //todo instead of relying on the wiki schedule
    /// <summary>
    /// Says which shops are currently open
    /// </summary>
    internal static class ShopSchedule
    {
        public delegate void LogMethod(string message, LogLevel logLevel = LogLevel.Debug);
        public delegate bool BuildingOpenCondition();
        /// <summary>
        /// Says if a shop is open
        /// </summary>
        /// <param name="shopId">the id of the shop</param>
        /// <returns></returns>
        public static bool ShopOpen(string shopId)
        {
            //if (shopId != "Festival_NightMarket_MagicBoat_Day1" && false)
            //{ 
            //    return false;
            //}

            bool ownerIsAtStand = OwnerIsAtStand(shopId);
            bool buildingOpen = BuildingOpen(shopId);
            return ownerIsAtStand && buildingOpen;
        }

        /// <summary>
        /// Tells if the store location is open
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool BuildingOpen(string shopId)
        {
            BuildingOpenInfo buildingOpenInfo = GetBuildingOpenInfo(shopId);

            if (buildingOpenInfo == null)
            {
                return false;
            }

            Point tile = buildingOpenInfo.tile;
            GameLocation currentLocation = buildingOpenInfo.currentLocation;
            string locationName = buildingOpenInfo.desiredLocationName;
            int openTime = buildingOpenInfo.openTime;
            int closeTime = buildingOpenInfo.closeTime;
            string npcName = buildingOpenInfo.npcName;
            int minFriendship = buildingOpenInfo.minFriendship;
            BuildingOpenCondition? condition = buildingOpenInfo.condition;

            if (condition != null)
            {
                return condition();
            }

            

        //the code below is take straight from GameLocation.lockedDoorWarp

        bool town_key_applies = Game1.player.HasTownKey;
            if (GameLocation.AreStoresClosedForFestival() && currentLocation.InValleyContext())
            {
                return false;
            }
            if (locationName == "SeedShop" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent("191393") && !town_key_applies)
            {
                return false;
            }
            if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
            {
                openTime = 800;
            }
            if (town_key_applies)
            {
                if (town_key_applies && !currentLocation.InValleyContext())
                {
                    town_key_applies = false;
                }
                if (town_key_applies && currentLocation is BeachNightMarket && locationName != "FishShop")
                {
                    town_key_applies = false;
                }
            }
            Friendship friendship;
            bool canOpenDoor = (town_key_applies || (Game1.timeOfDay >= openTime && Game1.timeOfDay < closeTime)) && (minFriendship <= 0 || currentLocation.IsWinterHere() || (Game1.player.friendshipData.TryGetValue(npcName, out friendship) && friendship.Points >= minFriendship));
            if (currentLocation.IsGreenRainingHere() && Game1.year == 1 && !(currentLocation is Beach) && !(currentLocation is Forest) && !locationName.Equals("AdventureGuild"))
            {
                canOpenDoor = true;
            }
            if (canOpenDoor)
            {
                return true;
            }
            else if (minFriendship <= 0)
            {
                return false;
            }
            else if (Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tells if the shop owner (if there is one) is at their stand
        /// </summary>
        /// <param name="shopId">the id of the shop</param>
        /// <returns></returns>
        public static bool OwnerIsAtStand(string shopId)
        {
            //get the neccessary information needed to check if the shop is open
            OwnerAtStandInfo? scheduleInfo = GetShopScheduleInfo(shopId);

            if (scheduleInfo == null)
            {
                return false;
            }

            //the code below is copied directly from TryOpenShopMenu. I am uncertain exactly how it works
            //though I do understand this is the code that is responsible for opening a specific shop menu.
            //If those conditions are true, then that means the shop is open


            if (scheduleInfo.moreParamters)
            {
                GameLocation location = scheduleInfo.location;
                Rectangle? ownerArea = scheduleInfo.ownerArea;
                int? maxOwnerY = scheduleInfo.maxOwnerY;
                bool forceOpen = scheduleInfo.forceOpen;

                if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var shop))
                {
                    return false;
                }
                IList<NPC> characters = location.currentEvent?.actors;
                if (characters == null)
                {
                    characters = location.characters;
                }
                NPC owner = null;
                ShopOwnerData ownerData = null;
                ShopOwnerData[] currentOwners = ShopBuilder.GetCurrentOwners(shop).ToArray();
                ShopOwnerData[] array = currentOwners;
                foreach (ShopOwnerData curOwner in array)
                {
                    if (forceOpen && curOwner.ClosedMessage != null)
                    {
                        continue;
                    }
                    foreach (NPC npc in characters)
                    {
                        if (curOwner.IsValid(npc.Name))
                        {
                            Point tile = npc.TilePoint;
                            if ((!ownerArea.HasValue || ownerArea.Value.Contains(tile)) && (!maxOwnerY.HasValue || tile.Y <= maxOwnerY))
                            {
                                owner = npc;
                                ownerData = curOwner;
                                break;
                            }
                        }
                    }
                    if (ownerData != null)
                    {
                        break;
                    }
                }
                if (ownerData == null)
                {
                    ownerData = currentOwners.FirstOrDefault((ShopOwnerData p) => (p.Type == ShopOwnerType.AnyOrNone || p.Type == ShopOwnerType.None) && (!forceOpen || p.ClosedMessage == null));
                }
                if (forceOpen && ownerData == null)
                {
                    array = currentOwners;
                    foreach (ShopOwnerData entry in array)
                    {
                        if (entry.Type == ShopOwnerType.Any)
                        {
                            ownerData = entry;
                            owner = characters.FirstOrDefault((NPC p) => p.IsVillager);
                            if (owner == null)
                            {
                                Utility.ForEachVillager(delegate (NPC npc)
                                {
                                    owner = npc;
                                    return false;
                                });
                            }
                        }
                        else
                        {
                            owner = Game1.getCharacterFromName(entry.Name);
                            if (owner != null)
                            {
                                ownerData = entry;
                            }
                        }
                        if (ownerData != null)
                        {
                            break;
                        }
                    }
                }
                if (ownerData != null && ownerData.ClosedMessage != null)
                {
                    return false;
                }
                if (ownerData != null || forceOpen)
                {
                    return true;
                }

                return false;
            }

            else
            {
                if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var shop))
                {
                    return false;
                }

                return true;
            }

            
        }

        private static BuildingOpenInfo GetBuildingOpenInfo(string shopId)
        {
            switch (shopId)
            {
                case "Festival_NightMarket_MagicBoat_Day1":
                    //the date is winter 15
                    return new BuildingOpenInfo(new BuildingOpenCondition(() => Game1.season == Season.Winter && Game1.dayOfMonth == 15));
                case "SeedShop":
                    //unsure what the actual "npcname" is for this. I have tried an empty string, but the code says it's not equal
                    return new BuildingOpenInfo(new Point(6, 29), 
                        Game1.locations.First(l => l is Town),
                        shopId, 
                        900, 
                        2100, 
                        "", 
                        0);

                case "Joja":
                    //unsure what the actual "npcname" is for this. I have tried an empty string, but the code says it's not equal
                    return new BuildingOpenInfo(new Point(13, 29),
                        Game1.locations.First(l => l is JojaMart),
                        shopId,
                        900,
                        2300,
                        "",
                        0);
            }

            return null;
        }

        /// <summary>
        /// Returns info needed to check if the store owner is at their stand
        /// </summary>
        /// <param name="shopId">the id of the shop</param>
        /// <returns></returns>
        private static OwnerAtStandInfo GetShopScheduleInfo(string shopId)
        {
            switch (shopId)
            {
                case "Festival_NightMarket_MagicBoat_Day1":
                    return new OwnerAtStandInfo(location: Game1.locations.First(l => l is BeachNightMarket),
                                                forceOpen: false,
                                                moreParamters: true);
                case "Joja":
                    return new OwnerAtStandInfo();
                case "SeedShop":
                    return new OwnerAtStandInfo(location: Game1.locations.First(l => l is SeedShop), 
                                                ownerArea: new Rectangle(4, 17, 1, 1), 
                                                maxOwnerY: 18, 
                                                forceOpen: false, 
                                                moreParamters: true);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Infomration needed to check if a shop is open
        /// </summary>
        private class OwnerAtStandInfo
        {
            //the location the shop is in
            public GameLocation location; 

            //The tile area to search for an NPC who can run the shop (or <c>null</c> to search the entire location).
            //If no NPC within the area matches the shop's <see cref="F:StardewValley.GameData.Shops.ShopData.Owners" />,
            //the shop won't be opened
            public Rectangle? ownerArea;

            //The maximum Y tile position for an owner NPC, or <c>null</c> for no maximum.
            //This is used for shops that only work if the NPC is behind the counter.
            public int? maxOwnerY;

            //Whether to open the menu regardless of whether an owner NPC was found.
            public bool forceOpen;

            //if the appropriate TryOpenShopMenu to call is the one with more paramters. 
            public bool moreParamters;

            public OwnerAtStandInfo(GameLocation location, bool forceOpen, bool moreParamters, int? maxOwnerY = null, Rectangle? ownerArea = null)
            {
                this.location = location;
                this.ownerArea = ownerArea;
                this.maxOwnerY = maxOwnerY;
                this.forceOpen = forceOpen;
                this.moreParamters = moreParamters;
            }

            public OwnerAtStandInfo()
            { 
                moreParamters = false;
            }
        }

        /// <summary>
        /// Information needed to check if a building is open. Information is taken from GameLocation.lockedDoorWarp
        /// </summary>
        private class BuildingOpenInfo 
        {
            public Point tile;
            public GameLocation currentLocation;
            public string desiredLocationName;
            public int openTime;
            public int closeTime;
            public string npcName;
            public int minFriendship;
            public BuildingOpenCondition? condition; //the condition to tell if the building is open for the player to enter

            public BuildingOpenInfo(Point tile, GameLocation currentLocation, string desiredLocationName, int openTime, int closeTime, string npcName, int minFriendship)
            {
                this.tile = tile;
                this.currentLocation = currentLocation;
                this.desiredLocationName = desiredLocationName;
                this.openTime = openTime;
                this.closeTime = closeTime;
                this.npcName = npcName;
                this.minFriendship = minFriendship;
            }

            public BuildingOpenInfo(BuildingOpenCondition condition)
            {
                this.condition = condition;
            }

        }

    }
}
