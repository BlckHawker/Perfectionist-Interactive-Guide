using Microsoft.Xna.Framework;
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
            Tasks = new List<string>();

            foreach (Action action in actions)
            {
                if (action.changeDisplayNameMethod != null)
                {
                    Tasks.Add(action.changeDisplayNameMethod());
                }

                else if(action.DisplayName != "")
                {
                    Tasks.Add(action.DisplayName);
                }
            }
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

            //to get the width/height of the background
            int backgroundWidth = SpriteText.getWidthOfString(longestString);
            float backgroundHeight = heightDifference * Tasks.Count + heightOffset;

            //draw the background
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(menuXPostion, menuYPosition, backgroundWidth, (int)Math.Ceiling(backgroundHeight)), Color.Black * 0.5f);

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
        /// Debug metho to draw a certain string on the screen with a black background
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="str">the string that will be drawn</param>
        /// <param name="menuXPostion">the x position of the top left of the menu</param>
        /// <param name="menuYPosition">the y position of the top left of the menu</param>
        /// <param name="charLength">The width of one char. Will calcuate the length of the menu by being multiplied by the length of the string</param>
        public static void DebugDrawMenuBackground(SpriteBatch spriteBatch, string str, int menuXPostion, int menuYPosition, int charLength)
        {
            //todo to get the width of the background, get what the width should be for a specifc character and sum the values
            float backgroundWidth = charLength * str.Length;
            float backgroundHeight = 50 + heightOffset;

            Rectangle rectangle = new Rectangle(menuXPostion, menuYPosition, (int)Math.Ceiling(backgroundWidth), (int)Math.Ceiling(backgroundHeight));

            //draw the background
            spriteBatch.Draw(Game1.staminaRect, rectangle, Color.Black * 0.5f);


            //draw the text
            Vector2 startingPosition = new Vector2(widthOffset + menuXPostion, heightOffset + menuYPosition);
            Vector2 newPos = startingPosition;
            spriteBatch.DrawString(Game1.dialogueFont, str, newPos, Color.White);
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
