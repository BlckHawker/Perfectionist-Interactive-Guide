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

        public List<Task> avaibleTasks;

        public delegate bool TaskComplateDelegate();
        public delegate string UpdateTaskDisplayNameDelegate(Task t);



        public TaskManager()
        {

        }

        private void UpdateAvaibleTasks()
        {
            //todo if any of the tasks have any prerqes that are not done, don't assign this task
            //todo don't assign tasks that are completed

            Instance.avaibleTasks = new List<Task>();
            foreach (Task t in tree)
            {
                if (!t.TaskComplete() && (t.prerequisites == null || t.prerequisites.All(task => task.TaskComplete())))
                {
                    t.UpdateTaskComplete();
                    avaibleTasks.Add(t);
                }
            }

        }

        /// <summary>
        /// Update if tasks are complete
        /// </summary>
        public void UpdateTaskCompletion()
        {
            tree.ForEach(t => t.UpdateTaskComplete());
        }

        /// <summary>
        /// Gets all the tasks the player needs and can currenlty do
        /// </summary>
        /// <returns></returns>
        public List<Task> GetAvaiableTasks()
        {
            UpdateAvaibleTasks();
            return Instance.avaibleTasks;
        }

        public static void InitalizeInstance()
        {
            Instance = new TaskManager();
            Task t1 = new Task("Plant 15 Parsnip seeds", null, new TaskComplateDelegate(Instance.OutOfFifteenParsnips), new UpdateTaskDisplayNameDelegate(Instance.ParsnipStringInventory), true);
            Task t2 = new Task("Plant 15 Parsnip seeds", null, new TaskComplateDelegate(Instance.OutOfFifteenParsnips), new UpdateTaskDisplayNameDelegate(Instance.ParsnipStringInventory), false);
            Instance.tree = new List<Task>(new []{ t1, t2 });
        }



        private bool OutOfFifteenParsnips()
        {
            return Game1.player.Items.CountId("472") <= 0;
        }

        private string ParsnipStringInventory(Task t)
        {
            return $"Plant 15 Parsnip seeds ({(t.permanant ? "permanant" : "not permanent")})";
        }
    }
}
