using Nito.Async;
using System;
using System.Threading;

class ActionThreadDoGet
{
    static void Main()
    {
        using (ActionThread actionThread = new ActionThread())
        {
            actionThread.Start();

            Console.WriteLine("Console thread ID: " + Thread.CurrentThread.ManagedThreadId);

            // Any delegates passed to ActionThread.DoGet are run by that ActionThread (synchronously)
            int actionThreadID = actionThread.DoGet(() => Thread.CurrentThread.ManagedThreadId);
            
            Console.WriteLine("ActionThread thread ID: " + actionThreadID);
        }
    }
}