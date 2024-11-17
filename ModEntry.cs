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

namespace Stardew_100_Percent_Mod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        private SpriteBatch spriteBatch;
        IModHelper helper;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            spriteBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
            spriteBatch.Begin();
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicked;
            //helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;

        }

        public new void Dispose()
        {
            // Custom logic specific to ModEntry before calling base.Dispose
            Console.WriteLine("ModEntry-specific cleanup before calling base Dispose.");
            spriteBatch.End();


            // Call base.Dispose() explicitly
            base.Dispose();

            // Additional cleanup for ModEntry after calling base.Dispose (if necessary)
            Console.WriteLine("ModEntry-specific cleanup after calling base Dispose.");

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
            //tp
            List<string> text = new List<string>();


            const int widthCount = 2;
            const int heightCount = 2;

            for (int i = 0; i < heightCount; i++)
            {
                string s1 = "";
                char c1 = (char)87;

                for (int j = 0; j < widthCount; j++)
                {
                    s1 += c1;
                }

                text.Add(s1);
            }

            const int widthOffset = 12;
            const int heightOffset = 12;
            //todo scale the width and height of the square based on the text (no wrapping)
            //float backgroundWidth = Game1.viewport.Width * .02f * text.OrderByDescending(s => s.Length).First().Length + widthOffset;
            float backgroundWidth = 50 * text.OrderByDescending(s => s.Length).First().Length + widthOffset;

            //float backgroundHeight = Game1.viewport.Height * .03f * heightCount + heightOffset;
            float backgroundHeight = 50 * heightCount + heightOffset;

            e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, (int)Math.Ceiling(backgroundWidth), (int)Math.Ceiling(backgroundHeight)), Color.Black * 0.5f);

            //the offset of the text
            Vector2 startingPosition = new Vector2(widthOffset, heightOffset);
            const int heightDifference = 40;
            for (int i = 0; i < text.Count; i++)
            {
                Vector2 newPos = startingPosition;
                newPos.Y += heightDifference * i;
                e.SpriteBatch.DrawString(Game1.dialogueFont, text[i], newPos, Color.White);

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
