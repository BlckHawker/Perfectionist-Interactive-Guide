using StardewModdingAPI.Events;
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

namespace Stardew_100_Percent_Mod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicked;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;

            Menu.SetMonitor(this.Monitor);

            //initialize task manager
            TaskManager.InitalizeInstance();
        }

        /*********
        ** Private methods
        *********/

        private void OnUpdateTicked(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            //Go through the decsion tree and check what the desired action is
            Action action = (Action)TaskManager.Instance.root.MakeDecision();
            //new way with decision tree
            Menu.SetTasks(new[] { action }.ToList() ) ;


            //try and get the item that the player is holding in their hand
            Log(Game1.player.CursorSlotItem?.ItemId);
            return;
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e) 
        {
            Menu.DrawMenuBackground(e.SpriteBatch);
        }

        /// <summary>
        /// Debug method to test how much space a character text while drawing. 
        /// Will draw 2 menu: one for a single character, 
        /// and one for a string of 5 of the same character
        /// </summary>
        /// <param name="c">the character that will be drawn</param>
        /// <param name="spriteBatch">used to start drawing</param>
        /// <param name="xOfsset">Where the menus will be drawn on the x axis</param>
        private void RenderLetterMenu(char c, SpriteBatch spriteBatch, int xOfsset)
        {
            string s = "";
            //                                 1    2   3   4    5   6   7    8   9
            int[] charSizes = new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 };

            for (int i = 0; i < 5; i++)
            {
                s += c;
            }

            for (int i = 0; i < charSizes.Length; i++)
            {
                int height = i * 100;
                int charSize = charSizes[i];
                Menu.DebugDrawMenuBackground(spriteBatch, "" + c, xOfsset, height, charSize);
                Menu.DebugDrawMenuBackground(spriteBatch, s, xOfsset, height + 50, charSize);
            }
        }

        /// <summary>
        /// Helper method to log statements to the console
        /// </summary>
        private void Log(string message, LogLevel logLevel = LogLevel.Debug)
        {
            this.Monitor.Log(message, logLevel);
        }
    }
}
