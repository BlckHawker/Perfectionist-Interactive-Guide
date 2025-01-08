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

namespace Stardew_100_Percent_Mod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private readonly Stopwatch renderWatch = new();
        private string framerate = "--";
        private int frames = 0;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Display.Rendered += Rendered;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

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

            Crop a;

            TaskManager instance = TaskManager.Instance;


            if (e.IsOneSecond)
            {
                framerate = $"Framerate: {frames} FPS.";
                frames = 0;
            }


            instance.PreFix();

            //Go through the decsion tree and check what the desired action is
            List<Action> actions = instance.roots.Select(root => (Action)root.MakeDecision() ).ToList();
            actions.Insert(0, new Action(framerate));
            actions = instance.CombineActions(actions);


            Farm farm = (Farm)TaskManager.GetLocation("Farm");
            //check if the player has a shed on the farm
            IEnumerable<Building> buildings = farm.buildings;



            IEnumerable<Building> sheds = farm.buildings.Where(b => b.buildingType.Value == "Shed");
            IEnumerable<Building> bigSheds = farm.buildings.Where(b => b.buildingType.Value == "Big Shed");

            actions.Add(new Action($"Shed under construction: {sheds.Any(s => s.daysOfConstructionLeft.Value > 0)}"));
            actions.Add(new Action($"Built shed count: {sheds.Count(s => s.daysOfConstructionLeft.Value < 1)}"));
            actions.Add(new Action($"Big Shed construction in progress: {bigSheds.Any(s => s.daysOfConstructionLeft.Value > 0)}"));
            actions.Add(new Action($"Big Shed count: {bigSheds.Count(s => s.daysOfConstructionLeft.Value < 1)}"));

            Menu.SetTasks(actions);

            instance.PostFix();
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;
            // print button presses to the console window

            //todo refactor this so it works when the player harvests a crop
            if (e.Button == SButton.Space)
            {
                TaskManager instance = TaskManager.Instance;
                string id = "(O)24"; //Parsnip
                instance.AddToGrowCropCount(id); 
                Log($"The player has \"grown\" {instance.cropsGrownDictinary[id]} parsnips.");
            }
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e) 
        {
            Menu.DrawMenuBackground(e.SpriteBatch);
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
