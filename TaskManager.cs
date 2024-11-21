using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Manages the Tree of of taks needed in order to 100% the game
    /// </summary>
    internal class TaskManager
    {
        public static TaskManager Instance;
        //todo implement a selector and sequence ckasses
        private List<Task> tree;
        public DecisionTreeNode root { get; private set; }

        public List<Task> avaibleTasks;

        public delegate bool TaskComplateDelegate();
        public delegate string UpdateTaskDisplayNameDelegate(Task t);

        public TaskManager()
        {
            
        }

        public static void InitalizeInstance()
        {
            Instance = new TaskManager();
            
            Action taskCompleteAction = new Action("Task Complete");

            Decision have15Parsnips = new Decision(taskCompleteAction,
                                      new Action("Do not have 15 parsnips"),
                                      new Decision.DecisionDelegate(Instance.PlayerHas15Parsnips));
            Instance.root = have15Parsnips;
        }

        private int ParsnipInventoryCount()
        {
            //the count of the item in the player's iventory + the count currently selceted

            const string id = "472";

            Farmer player = Game1.player;

            Item heldItem = player.CursorSlotItem;

            int count = player.Items.CountId("472");

            if (heldItem?.ItemId == id)
            {
                count += heldItem.Stack;
            }
            return count;
        }

        private bool PlayerHas15Parsnips()
        {


            return ParsnipInventoryCount() >= 15;
        }
    }
}
