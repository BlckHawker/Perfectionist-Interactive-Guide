using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley.Locations;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// All methods that are harmony patches
    /// </summary>
    internal class HarmonyPatches
    {
        private static IMonitor Monitor;

        internal static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        // patches need to be static!
        internal static void Harvest_Postfix(Crop __instance, ref bool __result)
        {
            try
            {
                //Make it so this only gets called when the crop is fully grown
                if (__result)
                { 
                    Log($"A crop with an unqualified id of {__instance.indexOfHarvest.Value} has been harvested");
                    TaskManager.AddToGrowCropCount($"(O){__instance.indexOfHarvest.Value}");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed in {nameof(Harvest_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        internal static void TryOpenShopMenu_Postfix(ref bool __result, string shopId, GameLocation location, Rectangle? ownerArea, int? maxOwnerY, bool forceOpen, bool playOpenSound, Action<string> showClosedMessage)
        {
            //todo verify that the method is being called properly
            Log("TryOpenShopMenu_Postfix called");

            //if you can call the method, then when the result is true, check what the other paramters for that specific shop id
            if (__result)
            {
                Log("shop was opened");
                Log($"ShodId: {shopId}");
                Log($"location: {location}");
                Log($"ownerArea: {ownerArea}");
                Log($"maxOwnerY: {maxOwnerY}");

                GameLocation? seedShop = Game1.locations.FirstOrDefault(l => l is SeedShop);
                if (seedShop != null) 
                {
                    Log((seedShop.DisplayName == location.DisplayName).ToString());
                }

                
            }
        }

        private static void Log(string message, LogLevel logLevel = LogLevel.Debug)
        {
            Monitor.Log(message, logLevel);
        }
    }
}
