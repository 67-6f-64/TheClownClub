using System;

using System.Threading.Tasks;

using System.Collections.Generic;

public abstract class BotTask
{
    //Any type of bot. Anything that extends bot.
    private readonly Bot _bot;

    //Sort modules by priority.
    private readonly SortedSet<BotTask> _tasks =
        new SortedSet<BotTask>(Comparer<BotTask>.Create((a, b) => a.Priority() - b.Priority()));

    public BotTask(Bot bot) : base(){
        this._bot = bot;
    }

    public abstract int Priority();

    public abstract bool Validate();

    public abstract void Execute();

    public abstract String Description();

    /**
     * Add tasks to our current BotTask.
     */
    public void append(params BotTask[] tasks)
    {
        Array.ForEach(tasks, task => _tasks.Add(task));
    }

    public Bot getBot()
    {
        return this._bot;
    }
}
