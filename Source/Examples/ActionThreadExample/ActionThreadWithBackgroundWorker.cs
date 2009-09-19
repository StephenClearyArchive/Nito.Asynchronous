using Nito.Async;
using System;
using System.Threading;
using System.ComponentModel;

class ActionThreadWithBackgroundWorker
{
    static void Main()
    {
        // Normally, a BackgroundWorker (BGW) used within a Console application
        //  will invoke its ProgressChanged and RunWorkerCompleted events on a
        //  ThreadPool thread. This defeats the purpose of using a BGW.

        // An ActionThread provides a SynchronizationContext that the BGW can use
        //  to marshal its ProgressChanged and RunWorkerCompleted back to the
        //  ActionThread thread instead of on a ThreadPool thread.

        Console.WriteLine("Main console thread ID is " + Thread.CurrentThread.ManagedThreadId +
            " and is " + (Thread.CurrentThread.IsThreadPoolThread ? "" : "not ") + "a threadpool thread");

        // In this example, we kick off an ActionThread and then exit from Main.
        //  Since the ActionThread is a foreground thread, it will continue to
        //  run until it completes processing.
        ActionThread actionThread = new ActionThread();
        actionThread.Start();
        actionThread.Do(FirstAction);
    }

    // This is the first action done by the ActionThread
    static void FirstAction()
    {
        Console.WriteLine("ActionThread thread ID is " + Thread.CurrentThread.ManagedThreadId +
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

    // This is an event raised by the BGW. Since the BGW is owned by the ActionThread,
    //  this event is raised on the ActionThread.
    static void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Console.WriteLine("BGW event thread ID is " + Thread.CurrentThread.ManagedThreadId +
            " and is " + (Thread.CurrentThread.IsThreadPoolThread ? "" : "not ") + "a threadpool thread");

        // When the BGW is done, signal our ActionThread to exit
        ActionDispatcher.Current.QueueExit();
    }
}