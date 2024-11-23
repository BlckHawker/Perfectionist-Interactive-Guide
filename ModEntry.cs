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
using Netcode;
using StardewValley.Network;
using StardewValley.Objects;

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
            TaskManager.InitalizeInstance(Log);
        }

        /*********
        ** Private methods
        *********/

        private void OnUpdateTicked(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            //Go through the decsion tree and check what the desired action is

            List<Action> actions = TaskManager.Instance.roots.Select(root => (Action)root.MakeDecision() ).ToList();

            //get character relationship
            string npcNames = string.Join(", ", NPCManager.GetSocialCharacters().Select(npc => npc.Name));



            NPC sebastian = NPCManager.GetSocialCharacters().Where(n => n.Name == "Sebastian").FirstOrDefault();

            var data = Game1.player.friendshipData;

            //Friendship friendship = Game1.player.friendshipData[sebastian.Name];

            //Evelyn, George, Alex, Emily, Haley, Jodi, Sam, Vincent, Clint, Lewis, Abigail, Caroline, Pierre, Gus,
            //Pam, Penny, Harvey, Elliott, Demetrius, Maru, Robin, Sebastian, Linus, Wizard, Jas, Marnie, Shane, Leah, Dwarf, Krobus, Willy
            if (true)
            { 
                Menu.SetTasks(actions);
            }

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

    }
}
