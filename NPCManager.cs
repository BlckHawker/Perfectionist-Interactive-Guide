using StardewValley;
using StardewValley.Buildings;
using StardewValley.Internal;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Minigames.MineCart;

namespace Stardew_100_Percent_Mod
{
    internal class NPCManager
    {
        /// <summary>
        /// Gets all of the characters the player can have a relationship with
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<NPC> GetSocialCharacters()
        {
            foreach (NPC npc in Utility.getAllCharacters())
            {
                if (npc.CanSocialize || Game1.player.friendshipData.ContainsKey(npc.Name))
                    yield return npc;
            }
        }

        /// <summary>
        /// Get an NPc based on their name
        /// </summary>
        /// <param name="name">the name of the NPC</param>
        /// <returns>An npc object with the desired name</returns>

        public static NPC GetNPC(string name)
        {
            NPC desiredNPC = null;
            Utility.ForEachCharacter(delegate (NPC npc)
            {
                bool npcFound = false;
                if (npc.Name == name)
                { 
                    desiredNPC = npc;
                    npcFound = true;

                }
                return !npcFound;
            });

            return desiredNPC;
        }

        /// <summary>
        /// Checks if an npc is datable
        /// </summary>
        /// <param name="name">the name of the npc</param>
        /// <returns>true if the player can date the npc</returns>
        public static bool IsDatable(string name)
        {
            return new[] { "Alex", "Elliott", "Harvey", "Sam","Sebastian","Shane",
                "Abigail", "Emily", "Haley", "Leah", "Maru", "Penny" }.Contains(name);
        }

        public static Friendship GetFriendshipData(string name)
        { 
            Game1.player.friendshipData.TryGetValue(name, out Friendship? friendship);
            return friendship;
        }
    }
}
