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
            Menu.SetTasks(actions);
            TaskManager.PostFix();
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
