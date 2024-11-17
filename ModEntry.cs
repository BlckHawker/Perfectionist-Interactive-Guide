using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.Internal;

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
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicked;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;

            Menu.SetMonitor(this.Monitor);
            Menu.AddDebugTasks();

        }

        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            Log($"{Game1.player.Name} pressed {e.Button}.");
        }


        private void OnUpdateTicked(object? sender, UpdateTickingEventArgs e)
        {
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e) 
        {
            SpriteBatch spriteBatch = e.SpriteBatch;
            Menu.DrawMenuBackground(spriteBatch);
            Rectangle r = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
            return;
            spriteBatch.Draw(Game1.staminaRect, r, Color.Green);

            const char startingChar = 'y';

            const int count = 8;

            for (int i = 0; i < count; i++)
            { 
                RenderLetterMenu((char)(startingChar + i), spriteBatch, i * 300);
            }
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
