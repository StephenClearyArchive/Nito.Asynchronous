using Nito.Async;
using System;
using System.Threading;

class ActionThreadDo
{
    static void Main()
    {
        using (ActionThread actionThread = new ActionThread())
        {
            actionThread.Start();

            Console.WriteLine("Console thread ID: " + Thread.CurrentThread.ManagedThreadId);

            // Any delegates passed to an ActionThread are run by that ActionThread (asynchronously)
            actionThread.Do(() => Console.WriteLine("ActionThread thread ID: " + Thread.CurrentThread.ManagedThreadId));
        }
    }
}