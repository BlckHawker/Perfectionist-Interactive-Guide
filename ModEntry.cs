using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using System.Diagnostics;
using StardewValley.Locations;
using static Stardew_100_Percent_Mod.TaskManager;
using HarmonyLib;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

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
        private Dictionary<string, ShopData>? shopData = null;
        public Dictionary<string, ShopData> ShopData => shopData ??= DataLoader.Shops(Game1.content);
        private Dictionary<string, List<ItemStockInformation>>? shopItemsCache = null;
        public Dictionary<string, List<ItemStockInformation>> ShopItemsCache => shopItemsCache ??= ShopData.Keys.ToDictionary(k => k, k => ShopBuilder.GetShopStock(k).Values.ToList());
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

            Menu.SetMonitor(Monitor);

            HarmonyPatches.Initialize(Monitor);


            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
               postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Harvest_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.lockedDoorWarp)),
               postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.lockedDoorWarp_Postfix))
            );


            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.TryOpenShopMenu), new Type[] { typeof(string), typeof(GameLocation), typeof(Rectangle), typeof(int), typeof(bool), typeof(bool), typeof(Action<string>) }),
               postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.TryOpenShopMenu_Postfix_1))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.TryOpenShopMenu), new Type[] { typeof(string), typeof(string), typeof(bool) }),
               postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.TryOpenShopMenu_Postfix_2))
            );


        }
        



        /*********
        ** Private methods
        *********/

        private void SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            //initialize task manager
            TaskManager.InitalizeInstance(Log);
        }

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

            ItemName desiredItem = ItemName.ParsnipSeeds;

            foreach (KeyValuePair<string, List<ItemStockInformation>> kv in ShopItemsCache)
            {
                if (kv.Value.Any(item => item.SyncedKey == TaskManager.ItemIds[desiredItem]))
                {
                    shopApplicableShopNames.Add(kv.Key);
                }

                
            }

            actions.Add(new Action($"Shop that sell the desired seed: {Join(shopApplicableShopNames)}"));

            //todo only show shops who are open at that current time

            //List<string> openStores = shopApplicableShopNames.Where(s => ShopSchedule.ShopOpen(s)).ToList();

            List<string> openStores = shopApplicableShopNames.Where(s => true).ToList();


            actions.Add(new Action($"Shops that are open and sell the desired seed: {Join(openStores)}"));

            //show the open store that is selling the item for the cheapest
            List<string> shopsByPrice = openStores.OrderBy(shopId => ShopItemsCache[shopId].First(item => item.SyncedKey == TaskManager.ItemIds[desiredItem]).Price ).ToList();
            List<int> prices = shopsByPrice.Select(shopId => ShopItemsCache[shopId].First(item => item.SyncedKey == TaskManager.ItemIds[desiredItem]).Price).ToList();


            string s = "Shops by price:";
            for (int i = 0; i < shopsByPrice.Count; i++)
            {
                s += $" {shopsByPrice[i]} ({prices[i]})";
            }

            actions.Add(new Action($"Shops by price: {s}"));



            Menu.SetTasks(actions);

            TaskManager.PostFix();
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e) 
        {
            Menu.DrawMenuBackground(e.SpriteBatch);
        }

        private void Rendered(object? sender, RenderedEventArgs e)
        {
            this.frames += 1;
        }

        /// <summary>
        /// when the the shop data is no longer accurate, wipe the cache
        /// </summary>
        private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(static assetName => assetName.IsEquivalentTo("Data/Shops")))
            {
                shopItemsCache = null;
            }
        }

        /// <summary>
        /// Helper method to log statements to the console
        /// </summary>
        private void Log(string message, LogLevel logLevel = LogLevel.Debug)
        {
            this.Monitor.Log(message, logLevel);
        }

        private string Join<T>(IEnumerable<T> collection)
        {
            return string.Join(", ", collection);
        }

        

    }
}
