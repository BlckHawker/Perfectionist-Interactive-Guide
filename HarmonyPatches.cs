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
        internal static void Harvest_Postfix(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null, bool isForcedScytheHarvest = false)
        {
            try
            {
                Log($"A crop has been harvested"); // your patch logic here
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
