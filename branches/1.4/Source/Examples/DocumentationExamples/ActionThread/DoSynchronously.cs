using Nito.Async;
using System;
using System.Threading;

class ActionThreadDoSynchronously
{
    static void Main()
    {
        using (ActionThread actionThread = new ActionThread())
        {
            actionThread.Start();

            Console.WriteLine("Console thread ID: " + Thread.CurrentThread.ManagedThreadId);

            // Any delegates passed to ActionThread.DoSynchronously are run by that ActionThread (synchronously)
            int actionThreadID = 0;
            actionThread.DoSynchronously(() => actionThreadID = Thread.CurrentThread.ManagedThreadId);
            
            Console.WriteLine("ActionThread thread ID: " + actionThreadID);
        }
    }
}