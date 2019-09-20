using System;

using System.Threading.Tasks;

public abstract class BotTask
{
    private readonly Bot _bot;

    public abstract bool condition();

    public abstract void execute();

    public Bot getBot()
    {
        return this._bot;
    }

    public String getBotTaskType()
    {
        return "Checkout Task";
    }
}
