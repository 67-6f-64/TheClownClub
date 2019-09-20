using System.Collections.Generic;

public abstract class Bot
    {
        private readonly BotType _botType;

        private readonly Queue<BotTask> _tasks;

    public Bot(BotType botType)
    {
        this._botType = botType;
    }

    protected void execute()
    {
        foreach(BotTask task in _tasks) {
            if (task.condition())
            {
                task.execute();
                System.Console.WriteLine(task.getBotTaskType());
            }
        }
    }

    public BotType getBotType()
        {
            return this._botType;
        }
    }