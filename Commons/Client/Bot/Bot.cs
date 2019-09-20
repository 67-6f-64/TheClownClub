using System.Collections.Generic;

public abstract class Bot
    {
        private readonly BotType _botType;

        private string _status;

    private readonly SortedSet<BotTask> _tasks =
         new SortedSet<BotTask>(Comparer<BotTask>.Create((a, b) => a.Priority() - b.Priority()));

    public Bot(BotType botType)
    {
        this._botType = botType;
    }

    protected void Execute()
    {
        foreach(BotTask task in _tasks) {
            if (task.Validate())
            {

                task.Execute();

                this._status = task.Description();
                System.Console.WriteLine(this._status);
            }
        }
    }

    public BotType getBotType()
        {
            return this._botType;
        }
    }