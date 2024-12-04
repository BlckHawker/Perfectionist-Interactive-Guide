using StardewValley;
using StardewValley.Internal;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    internal class ItemLocator
    {
        /// <summary>
        /// Gets the count of a certain item being found throughout the entire world
        /// </summary>
        /// <param name="qualifiedItemId">the id of the item</param>
        /// <returns>A dictionary of the count sorted by most to fewest found</returns>
        public static Dictionary<GameLocation, int> GetItemCount(string qualifiedItemId)
        { 
            Dictionary<GameLocation, int> dict = new Dictionary<GameLocation, int>();
            foreach (GameLocation location in Game1.locations)
            {
                dict[location] = GetItemCountInLocation(location, qualifiedItemId);
            }

            return dict.OrderByDescending(kv => kv.Value)
                       .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Gets a reference of an item
        /// </summary>
        /// <param name="qualifiedItemId">The id of the desired item</param>
        /// <returns>The item object with the desired QualifiedItemId</returns>
        public static Item GetItem(string qualifiedItemId)
        {
            Item desiredItem = null;
            Utility.ForEachItem(delegate (Item item)
            {
                if (item.QualifiedItemId == qualifiedItemId)
                {
                    desiredItem = item;
                }
                return desiredItem == null;
            });

            return desiredItem;
        }

        /// <summary>
        /// Get a referrence from an item
        /// </summary>
        /// <param name="name">The name of the desired item</param>
        /// <returns>The item object with the desired name</returns>
        public static Item GetItemName(string name)
        {
            Item desiredItem = null;
            Utility.ForEachItem(delegate (Item item)
            {
                if (item.DisplayName == name)
                {
                    desiredItem = item;
                }
                return desiredItem == null;
            });

            return desiredItem;
        }

        /// <summary>
        /// Gets the count of a certain item being found in a certain location
        /// </summary>
        /// <param name="location">the place the item will be searched for</param>
        /// <param name="qualifiedItemId">the id of the item</param>
        /// <returns>the # of times that item was found</returns>
        public static int GetItemCountInLocation(GameLocation location, string qualifiedItemId)
        {
            int count = 0;

            List<Chest> chests = GetChestsWithItem(location, qualifiedItemId);

            foreach (Chest chest in chests)
            {
                count += chest.Items.Where(item => item.QualifiedItemId == qualifiedItemId).Sum(item => item.Stack);
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location">The location being targeted</param>
        /// <param name="qualifiedItemId">the id of the desire item</param>
        /// <returns>true if the location has at least one of the desired items</returns>
        public static bool LocationHasItem(GameLocation location, string qualifiedItemId)
        {
            return GetItemCountInLocation(location, qualifiedItemId) > 0;
        }

        /// <summary>
        /// Checks if the player has a certain number of an item
        /// </summary>
        /// <param name="id">the id of the item</param>
        /// <param name="count">the count of the item</param>
        /// <param name="atLeast">if the method should return true if the player has more than the count</param>
        /// <returns></returns>
        public static bool PlayerHasItem(string id, int count, bool atLeast)
        {
            return PlayerHasItem(id, count, atLeast ? int.MaxValue : count);
        }

        /// <summary>
        /// Check if a player has a range count of a certain item inclsuvely in their inventory
        /// </summary>
        /// <param name="id">the id of the item</param>
        /// <param name="min">the desired minimum amount the item</param>
        /// <param name="max">the desired maximum amount the item</param>
        /// <returns>true if the player meets the desired range</returns>
        public static bool PlayerHasItem(string id, int min, int max)
        {
            int count = PlayerItemCount(id);
            return min <= count && count <= max;
        }

        /// <summary>
        /// Checks the count of a desired item in the player's inventory
        /// </summary>
        /// <param name="qualifiedItemId">the id of the desired item</param>
        /// <returns>the count of a desired item in the player's inventory</returns>
        public static int PlayerItemCount(string qualifiedItemId)
        {
            Farmer player = Game1.player;

            Item heldItem = player.CursorSlotItem;

            int count = player.Items.CountId(qualifiedItemId);

            if (heldItem?.QualifiedItemId == qualifiedItemId)
            {
                count += heldItem.Stack;
            }

            return count;
        }

        /// <summary>
        /// Checks if there are any chests with the a specifc item
        /// </summary>
        /// <param name="location">the location to check</param>
        /// <param name="qualifiedItemId">the id of the desired itm</param>
        /// <returns>A list of chests that has the desired item</returns>
        public static List<Chest> GetChestsWithItem(GameLocation location, string qualifiedItemId)
        {
            List<Chest> chests = new List<Chest>();
            Utility.ForEachItemContextIn(location, delegate(in ForEachItemContext context)
            {
                bool itemFound = false;
                if (context.Item.QualifiedItemId == qualifiedItemId)
                {
                    itemFound = true;
                    List<Chest> chestPaths = context.GetPath().Where(c => c is Chest).Select(c => (Chest)c).ToList();

                    if (chestPaths.Count > 0)
                    { 
                        chests.AddRange(chestPaths);
                    }
                }
                return !itemFound;
            });

            return chests;
        }

        /// <summary>
        /// Checks if a chest has a certain item in it
        /// </summary>
        /// <param name="chest">the chest that will be checked</param>
        /// <param name="qualifiedItemId">the id of the desired item</param>
        /// <returns>the number of times that item was found in the chest</returns>
        public static int GetChestItemCount(Chest chest, string qualifiedItemId)
        {
            return chest.Items.Where(item => item.QualifiedItemId == qualifiedItemId).Sum(item => item.Stack);
        }

    }
}
