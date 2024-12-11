using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using System.Diagnostics;
using Stardew_100_Percent_Mod.Decision_Trees;
using StardewValley.Locations;
using System.Linq;
using StardewValley.Buildings;

namespace Stardew_100_Percent_Mod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private readonly Stopwatch renderWatch = new();
        private string RenderTime = "--";
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

            TaskManager.Instance.ResetItemDictionarys();

            //Go through the decsion tree and check what the desired action is
            List<Action> actions = TaskManager.Instance.roots.Select(root => (Action)root.MakeDecision() ).ToList();
            actions.Insert(0, new Action(framerate));
            actions = TaskManager.Instance.CombineActions(actions);

            //check if the player has a shed on the farm
            Farm farmHouse = (Farm)Game1.locations.First(l => l.NameOrUniqueName == "Farm");
            actions.Add(new Action($"Shed construction in progress: unknown"));
            actions.Add(new Action($"Shed count: {farmHouse.buildings.Count(b => b.GetType() == typeof(Shed))}"));
            actions.Add(new Action($"Big Shed construction in progress: unknown"));
            actions.Add(new Action($"Big Shed count: unknown"));



            Menu.SetTasks(actions);
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
            this.renderWatch.Stop();
            double ms = this.renderWatch.Elapsed.TotalMilliseconds;
            if (Game1.ticks % 5 == 0 || ms > 5)
            {
                this.RenderTime = $"Render time: {ms:00.00} ms.";
            }

            this.frames += 1;
        }

    }
}
