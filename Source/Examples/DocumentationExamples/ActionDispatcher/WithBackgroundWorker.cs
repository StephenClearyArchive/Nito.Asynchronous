using System;
using System.ComponentModel;
using System.Threading;
using Nito.Async;

// Normally, a BackgroundWorker (BGW) used within a Console application
//  will invoke its ProgressChanged and RunWorkerCompleted events on a
//  ThreadPool thread. This defeats the purpose of using a BGW.

// An ActionDispatcher provides a SynchronizationContext that the BGW can use
//  to marshal its ProgressChanged and RunWorkerCompleted back to the
//  thread running the ActionDispatcher instead of on a ThreadPool thread.

class ActionDispatcherWithBackgroundWorker
{
    static ActionDispatcher actionDispatcher;
    
    static void Main()
    {
        Console.WriteLine("Main console thread ID is " + Thread.CurrentThread.ManagedThreadId +
            " and is " + (Thread.CurrentThread.IsThreadPoolThread ? "" : "not ") + "a threadpool thread");

        // The event-driven main loop provided by ActionDispatcher is
        //  sufficient to own objects with managed thread affinity such
        //  as those using the event-based asynchronous pattern
        //  (e.g., BackgroundWorker).
        using (actionDispatcher = new ActionDispatcher())
        {
            actionDispatcher.QueueAction(FirstAction);
            actionDispatcher.Run();
        }
    }

    // This is the first action done by the main thread when it runs the ActionDispatcher
    static void FirstAction()
    {
        Console.WriteLine("ActionDispatcher thread ID is " + Thread.CurrentThread.ManagedThreadId +
            " and is " + (Thread.CurrentThread.IsThreadPoolThread ? "" : "not ") + "a threadpool thread");

        // Start a BGW
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork += BackgroundWorkerWork;
        backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
        backgroundWorker.RunWorkerAsync();
    }

    // This is the BackgroundWorker's work that it has to do
    static void BackgroundWorkerWork(object sender, DoWorkEventArgs e)
    {
        Console.WriteLine("BackgroundWorker thread ID is " + Thread.CurrentThread.ManagedThreadId +
            " and is " + (Thread.CurrentThread.IsThreadPoolThread ? "" : "not ") + "a threadpool thread");

        // Sleep is very important work; don't let anyone tell you otherwise
        Thread.Sleep(TimeSpan.FromSeconds(1));
    }

    // This is an event raised by the BGW. Since the BGW is owned by the main thread,
    //  this event is raised on the main thread.
    static void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Console.WriteLine("BGW event thread ID is " + Thread.CurrentThread.ManagedThreadId +
            " and is " + (Thread.CurrentThread.IsThreadPoolThread ? "" : "not ") + "a threadpool thread");

        // When the BGW is done, signal our ActionThread to exit
        ActionDispatcher.Current.QueueExit();
    }
}