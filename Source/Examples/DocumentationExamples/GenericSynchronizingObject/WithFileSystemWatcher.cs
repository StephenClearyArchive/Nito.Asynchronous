using Nito.Async;
using System;
using System.Threading;
using System.ComponentModel;
using System.IO;

class GenericSynchronizingObjectWithFileSystemWatcher
{
    static void Main()
    {
        // GenericSynchronizingObject depends on a SynchronizationContext, which
        //  isn't normally provided on Console applications. We use an ActionThread
        //  to provide the SynchronizationContext.

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
        Console.WriteLine("ActionThread thread ID is " + Thread.CurrentThread.ManagedThreadId);

        // Start a FileSystemWatcher
        FileSystemWatcher watcher = new FileSystemWatcher(".") { Filter = "test.txt" };
        watcher.SynchronizingObject = new GenericSynchronizingObject();
        watcher.Created += new FileSystemEventHandler(watcher_Created);
        watcher.EnableRaisingEvents = true;

        // After a short pause, have a random thread create the file we're watching
        ThreadPool.QueueUserWorkItem(_ => { Thread.Sleep(200); File.Create("test.txt").Close(); });
    }

    // Normally, FileSystemWatcher.Created is called back on a ThreadPool thread; however, its
    //  GenericSynchronizingObject redirects this event to the ActionDispatcherSynchronizationContext
    //  from the ActionThread.
    static void watcher_Created(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("FileSystemWriter.Created thread ID is " + Thread.CurrentThread.ManagedThreadId);

        ActionDispatcher.Current.QueueExit();
    }
}