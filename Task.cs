using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Stardew_100_Percent_Mod.TaskManager;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// A task needed to complete in game in order to 100% the game
    /// </summary>
    internal class Task
    {
        //the overall name of the task (does not change)
        //could be used as an id
        private readonly string name;

        //the name of the task that will appear in the menu
        public string displayName { get; private set; }

        //the tasks that need to be completed in order for this task to appear (be unlocked)
        public List<Task> prerequisites { get; private set; }

        //if this task has been completed
        private bool complete;

        //the method to check if the task is complete
        public readonly TaskComplateDelegate completeTaskDelegate;

        //the deleagate that will chagnge the displayName
        public readonly UpdateTaskDisplayNameDelegate updateTaskDisplayNameDelegate;
        //if the task can only be complated once
        //once the task is completed, it can't be uncompleted
        public bool permanant { get; }

        public Task(string name, List<Task> prerequisites, TaskComplateDelegate completeDelegate, UpdateTaskDisplayNameDelegate updateTaskDisplayNameDelegate,  bool permanant)
        {
            complete = false;
            this.name = name;
            this.prerequisites = prerequisites;
            this.permanant = permanant;
            this.completeTaskDelegate = completeDelegate;
            this.updateTaskDisplayNameDelegate = updateTaskDisplayNameDelegate;
            UpdateTaskDisplayName();
        }


        public bool TaskComplete()
        {
            return complete;
        }

        public void UpdateTaskComplete()
        {
            if (permanant && complete)
            {
                return;
            }
            complete = completeTaskDelegate();
        }

        public void UpdateTaskDisplayName()
        {
            displayName = updateTaskDisplayNameDelegate(this);
        }

    }
}
