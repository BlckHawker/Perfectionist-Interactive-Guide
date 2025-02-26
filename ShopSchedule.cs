using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.HomeRenovations;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

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
        /// <summary>
        /// Says if a shop is open
        /// </summary>
        /// <param name="shopId">the id of the shop</param>
        /// <returns></returns>
        public static bool ShopOpen(Dictionary<string, ShopData> shopData, string shopId, List<Action> actionList)
        {
            switch (shopId)
            {
                case "SeedShop":

                    return false;

                default:
                    return false;
            }
          
        }


        /// <summary>
        /// Returns the owner of the shop
        /// </summary>
        /// <param name="shopId">the id of the shop</param>
        /// <returns></returns>
        private static string GetShopOwner(string shopId)
        {
            switch (shopId)
            {
                case "SeedShop":
                    return "Pierre";
                default:
                    return null;
            }
        }
    }
}
