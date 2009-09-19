using Nito.Async;
using System;
using System.Threading;

class ActionThreadDoSynchronouslyWithTimeout
{
    static void Main()
    {
        ActionThread actionThread = new ActionThread();
        // Set IsBackground so that this thread won't prevent the program from exiting
        actionThread.IsBackground = true;
        actionThread.Start();

        // Wait 100 ms for the ActionThread to sleep for 1 minute
        bool done = actionThread.DoSynchronously(() => Thread.Sleep(TimeSpan.FromMinutes(1)), TimeSpan.FromMilliseconds(100));

        Console.WriteLine("ActionThread completed action synchronously: " + done);
    }
}