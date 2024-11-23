using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

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

        //What the width of each charcter should be
        private static readonly Dictionary<char, int> widthDictionary = new Dictionary<char, int>
        {
            { 'A', 25 },
            { 'B', 30 },
            { 'C', 30 },
            { 'D', 30 },
            { 'E', 30 },
            { 'F', 25 },
            { 'G', 40 },
            { 'H', 30 },
            { 'I', 10 },
            { 'J', 20 },
            { 'K', 25 },
            { 'L', 25 },
            { 'M', 35 },
            { 'N', 30 },
            { 'O', 35 },
            { 'P', 30 },
            { 'Q', 35 },
            { 'R', 35 },
            { 'S', 30 },
            { 'T', 30 },
            { 'U', 30 },
            { 'V', 30 },
            { 'W', 40 },
            { 'X', 30 },
            { 'Y', 30 },
            { 'Z', 30 },
            { 'a', 25 },
            { 'b', 25 },
            { 'c', 25 },
            { 'd', 25 },
            { 'e', 25 },
            { 'f', 20 },
            { 'g', 25 },
            { 'h', 25 },
            { 'i', 10 },
            { 'j', 20 },
            { 'k', 20 },
            { 'l', 15 },
            { 'm', 35 },
            { 'n', 25 },
            { 'o', 25 },
            { 'p', 25 },
            { 'q', 25 },
            { 'r', 25 },
            { 's', 25 },
            { 't', 20 },
            { 'u', 40 },
            { 'v', 25 },
            { 'w', 30 },
            { 'x', 25 },
            { 'y', 25 },
            { 'z', 25 },
            { '0', 25 },
            { '1', 25 },
            { '2', 25 },
            { '3', 25 },
            { '4', 25 },
            { '5', 25 },
            { '6', 25 },
            { '7', 25 },
            { '8', 25 },
            { '9', 25 },
            { ' ', 25 },
            { '(', 25 },
            { ')', 25 },
            { ':', 25 },
            { ',', 25 },
            { '{', 25 },
            { '}', 25 },


        };

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

            //to get the width of the background, get what the width should be for a specifc character and sum the values
            
            string longestString = Tasks.OrderByDescending(s => s.Length).FirstOrDefault();

            //a custom exception just in case the char key doesn't exist
            foreach (string task in Tasks)
            {
                foreach (char c in longestString)
                {
                    if (!widthDictionary.ContainsKey(c))
                    {
                        throw new Exception($"The char \"{c}\" does not exist in the dictionary \"widthDictionary\"");
                    }
                }
            }

            float backgroundWidth = longestString.Select(c => widthDictionary[c]).Sum() + widthOffset;
            
            float backgroundHeight = heightDifference * Tasks.Count + heightOffset;

            //draw the background
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(menuXPostion, menuYPosition, (int)Math.Ceiling(backgroundWidth), (int)Math.Ceiling(backgroundHeight)), Color.Black * 0.5f);

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
