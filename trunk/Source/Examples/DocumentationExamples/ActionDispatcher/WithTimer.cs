using System;
using System.Threading;
using System.Timers;
using Nito.Async;

// The System.Timers.Timer class raises its Elapsed event on a ThreadPool thread
//  (if SynchronizingObject is null, which is true for this example code).
using Timer = System.Timers.Timer;

class ActionDispatcherWithTimer
{
    static ActionDispatcher actionDispatcher;
    
    static void Main()
    {
        Console.WriteLine("In main thread (thread ID " +
            Thread.CurrentThread.ManagedThreadId + ")");

        // By using an ActionDispatcher, we can give a Console application thread
        //  (or any other thread) an event-driven main loop.
        using (actionDispatcher = new ActionDispatcher())
        using (Timer timer = new Timer())
        {
            timer.AutoReset = false;
            timer.Interval = 100;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            actionDispatcher.Run();
        }
    }

    static int elapsedCount = 0;
    static void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        // (This method executes in a ThreadPool thread)
        Console.WriteLine("Elapsed running in thread pool thread (thread ID " +
            Thread.CurrentThread.ManagedThreadId + ")");

        Timer timer = (Timer)sender;
        if (elapsedCount == 0)
        {
            // The first time the timer goes off, send a message to the main thread
            elapsedCount = 1;
            actionDispatcher.QueueAction(
                () => Console.WriteLine("Hello from main thread (thread ID " +
                    Thread.CurrentThread.ManagedThreadId + ")"));
            timer.Start();
        }
        else
        {
            // The second time the timer goes off, tell the main thread to exit
            actionDispatcher.QueueExit();
        }
    }
}