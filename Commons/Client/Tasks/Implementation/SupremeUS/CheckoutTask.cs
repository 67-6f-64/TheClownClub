using System;
using System.Collections.Generic;
using System.Text;

namespace Commons.Client.Bot.Tasks.Implementation.SupremeUS
{
    class CheckoutTask : BotTask
    {
        public override bool condition()
        {
            //Check if the condition to continue to checkout exists.
            //For instance if you can find a checkout button.
            return false;
        }

        public override void execute()
        {
            //Execute the code.
            throw new NotImplementedException();
        }
    }
}
