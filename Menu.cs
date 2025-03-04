﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.BellsAndWhistles;

namespace Stardew_100_Percent_Mod
{
    /// <summary>Shows the list of things the player needs to do</summary>
    internal class Menu
    {
        //used to call the log function
        private static IMonitor monitor;

        //the list of tasks that need to be displayed
        public static List<string> Tasks = new List<string>();

        //offeset so text isn't it very top left corner of the menu
        private const int widthOffset = 12;
        private const int heightOffset = 12;

        public static void SetTasks(List<Action> actions)
        {
            Tasks = actions.Select(a => a.DisplayName).Where(str => str != "").ToList();
        }

        /// <summary>
        /// Draws the menu background
        /// </summary>
        /// <param name="spriteBatch"></param>
        public static void DrawMenuBackground(SpriteBatch spriteBatch)
        {
            DrawMenuBackground(spriteBatch, 0, 0);
        }

        public static void DrawMenuBackground(SpriteBatch spriteBatch, int menuXPostion, int menuYPosition)
        {
            //if there are no tasks, do not draw anything
            if (Tasks.Count == 0)
            {
                return;
            }
            //the amount of vertical space to move down starting from the top left of the current task
            const int heightDifference = 40;

            string longestString = Tasks.OrderByDescending(s => s.Length).FirstOrDefault();

            //get the width/height of the background
            int backgroundWidth = SpriteText.getWidthOfString(longestString) + widthOffset;
            int backgroundHeight = Tasks.Select(t => SpriteText.getHeightOfString(t)).Sum() + heightOffset;

            //draw the background
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(menuXPostion, menuYPosition, backgroundWidth, backgroundHeight), Color.Black * 0.5f);

            //draw the text
            Vector2 startingPosition = new Vector2(widthOffset + menuXPostion, heightOffset + menuYPosition);
            for (int i = 0; i < Tasks.Count; i++)
            {
                Vector2 newPos = startingPosition;
                newPos.Y += heightDifference * i;
                spriteBatch.DrawString(Game1.dialogueFont, Tasks[i], newPos, Color.White);
            }
        }

        /// <summary>
        /// Set the monitor value
        /// </summary>
        /// <param name="monitor">The monitor object needed to access the Log function</param>
        public static void SetMonitor(IMonitor monitor)
        {
            Menu.monitor = monitor;
        }
        private static void Log(string message, LogLevel logLevel = LogLevel.Debug)
        {
            if (monitor == null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }
            monitor.Log(message, logLevel);
        }
    }
}
