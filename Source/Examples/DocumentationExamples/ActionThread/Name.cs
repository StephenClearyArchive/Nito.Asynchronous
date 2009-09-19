using Nito.Async;
using System;

class ActionThreadName
{
    static void Main()
    {
        using (ActionThread actionThread = new ActionThread())
        {
            // The name of an ActionThread may only be set once, before it is started
            actionThread.Name = "Bob";

            // The following line of code would raise an exception
            //// actionThread.Name = "Sue";
        }

        using (ActionThread actionThread = new ActionThread())
        {
            actionThread.Start();

            // The following line of code would raise an exception
            //// actionThread.Name = "Bob";
        }
    }
}
