using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using System.Diagnostics;
using Stardew_100_Percent_Mod.Decision_Trees;
using StardewValley.Locations;
using System.Linq;
using StardewValley.Buildings;
using static Stardew_100_Percent_Mod.TaskManager;
using System.ComponentModel;
using HarmonyLib;
using StardewValley.GameData.Crops;
using StardewValley.TerrainFeatures;
using StardewValley.BellsAndWhistles;
using xTile.Dimensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace Stardew_100_Percent_Mod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private readonly Stopwatch renderWatch = new();
        private string framerate = "--";
        private int frames = 0;
        private IModHelper helper;

        //holds storage of what all the stores are currently selling items 
        private static Dictionary<string, List<ItemStockInformation>> shopItemsCache = null;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Display.Rendered += Rendered;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            Menu.SetMonitor(Monitor);

            HarmonyPatches.Initialize(Monitor);

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
               postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Harvest_Postfix))
            );
        }

        private void SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            //initialize task manager
            TaskManager.InitalizeInstance(Log);
        }

        /*********
        ** Private methods
        *********/

        private void OnUpdateTicked(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;


            if (e.IsOneSecond)
            {
                framerate = $"Framerate: {frames} FPS.";
                frames = 0;
            }
            TaskManager.PreFix();
            //Go through the decsion tree and check what the desired action is
            List<Action> actions = TaskManager.roots.Select(root => (Action)root.MakeDecision() ).ToList();
            actions = TaskManager.CombineActions(actions);
            actions.Insert(0, new Action(framerate));


            //check the selling price for parsnips
            ObjectData parsnipObjectData;

            Game1.objectData.TryGetValue(ItemIds[ItemName.Parsnip].Replace("(O)", ""), out parsnipObjectData);

            if (parsnipObjectData == null)
            {
                actions.Add(new Action("parsnipData is null"));

            }

            else
            {
                actions.Add(new Action($"parsnipData price: {parsnipObjectData.Price}"));
            }

            //check the crop the seed grows into

            CropData parsnipCropData;
            Game1.cropData.TryGetValue(ItemIds[ItemName.ParsnipSeeds].Replace("(O)", ""), out parsnipCropData);

            if (parsnipCropData == null)
            {
                actions.Add(new Action("parsnipCropData is null"));

            }

            else
            {
                actions.Add(new Action($"parsnipCropData unqualified harvest id: {parsnipCropData.HarvestItemId}"));
            }


            //figure out if a shop sells the parnsip seeds certain item and for how much

            List<string> shopApplicableShopNames = new List<string>();

            foreach (KeyValuePair<string, List<ItemStockInformation>> kv in shopItemsCache)
            {
                if (kv.Value.Any(item => item.SyncedKey == TaskManager.ItemIds[ItemName.ParsnipSeeds]))
                {
                    shopApplicableShopNames.Add(kv.Key);
                }

                //only show shops who are open at least for 10 minutes on the current day
            }

            if (shopApplicableShopNames.Count > 0)
            {
                actions.Add(new Action(string.Join(", ", shopApplicableShopNames.ToArray())));
            }

            else
            { 
                actions.Add(new Action("No shop currently selling the desired item"));
            }

            Game1.locations.

            Menu.SetTasks(actions);

            //check the selling price for green beans

            TaskManager.PostFix();
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e) 
        {
            Menu.DrawMenuBackground(e.SpriteBatch);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            //only update the shop's stock cache if it's the first (or it's currently null)
            if (shopItemsCache == null || Game1.dayOfMonth == 1)
            {
                GetShopItems();
            }
            
        }

        /// <summary>
        /// Get all the items currently in all shops
        /// </summary>
        private void GetShopItems()
        {
            shopItemsCache = new Dictionary<string, List<ItemStockInformation>>();

            Dictionary<string, ShopData> shops = DataLoader.Shops(Game1.content);
            foreach (KeyValuePair<string, ShopData> kv in shops)
            {
                shopItemsCache.Add(kv.Key, ShopBuilder.GetShopStock(kv.Key).Values.ToList());
            }

        }

        /// <summary>
        /// Helper method to log statements to the console
        /// </summary>
        private void Log(string message, LogLevel logLevel = LogLevel.Debug)
        {
            this.Monitor.Log(message, logLevel);
        }

        private void Rendered(object? sender, RenderedEventArgs e)
        {
            this.frames += 1;
        }

    }
}
