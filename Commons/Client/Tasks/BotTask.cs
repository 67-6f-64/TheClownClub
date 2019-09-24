using System;
using System.Collections.Generic;

namespace Common {
    public abstract class BotTask {
        /// <summary>
        /// The parent bot for this task
        /// </summary>
        private readonly Bot _bot;

        /// <summary>
        /// List of tasks sorted by ascending priority
        /// </summary>
        private readonly SortedSet<BotTask> _tasks =
            new SortedSet<BotTask>(Comparer<BotTask>.Create((a, b) => a.Priority() - b.Priority()));

        protected BotTask(Bot bot) {
            _bot = bot;
        }

        /// <summary>
        /// The priority which determines the order in which the tasks will be ran
        /// </summary>
        /// <returns>The priority of this task</returns>
        public abstract int Priority();

        /// <summary>
        /// Checks whether the task should be ran under the current conditions
        /// </summary>
        /// <returns>A boolean representing whether or not the task should run</returns>
        public abstract bool Validate();

        /// <summary>
        /// Runs the task
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Used to give a description of the task
        /// </summary>
        /// <returns>Description of task</returns>
        public abstract string Description();

        /// <summary>
        /// Add child tasks to parent task
        /// </summary>
        /// <param name="tasks">All the tasks to be added</param>
        public void Append(params BotTask[] tasks) {
            Array.ForEach(tasks, task => _tasks.Add(task));
        }

        public Bot GetBot() {
            return _bot;
        }
    }
}