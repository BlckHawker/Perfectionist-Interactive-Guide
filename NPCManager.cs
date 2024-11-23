using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
