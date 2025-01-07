using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    TaskManager.Instance.AddToGrowCropCount($"(O){__instance.indexOfHarvest.Value}");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed in {nameof(Harvest_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static void Log(string message, LogLevel logLevel = LogLevel.Debug)
        {
            Monitor.Log(message, logLevel);
        }
    }
}
