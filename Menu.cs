using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Stardew_100_Percent_Mod
{
    /// <summary>Shows the list of things the player needs to do</summary>
    internal class Menu
    {
        //offeset so text isn't it very top left corner of the menu
        const int widthOffset = 12;
        const int heightOffset = 12;
        private static void InitializeTexture(IModHelper helper)
        {
            //if anything is null, assign it
        }

        
        /// <summary>
        /// Draws the menu background
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        public static void DrawMenuBackground(IModHelper helper, SpriteBatch spriteBatch)
        {
            InitializeTexture(helper);
            //spriteBatch.Draw(texture, new Vector2(1280, 800), null, Color.White);
        }
    }
}
