﻿using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.Internal;
using StardewValley.Menus;
using Netcode;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.GameData.Characters;
using static Stardew_100_Percent_Mod.NPCManager;
using StardewValley.GameData;
using System.Diagnostics;
using StardewValley.Locations;

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
            //List<Action> actions = TaskManager.Instance.roots.Select(root => (Action)root.MakeDecision() ).ToList();

            List<Action> actions = new List<Action>();
            FarmHouse farmhouse = (FarmHouse)Game1.locations.First(l => l.NameOrUniqueName == "FarmHouse");
            Point fridgePoint = farmhouse.fridgePosition;
            
            actions = TaskManager.Instance.CombineItemActions(actions);
            actions.Insert(0, new Action(framerate));
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
