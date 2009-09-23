using Nito.Async;
using System;
using System.Threading;

using Timer = Nito.Async.Timer;

class TimerSingleShotDispose
{
    static void Main()
    {
        // Since Timer is an event-based asynchronous pattern object (and this is
        //  a Console application), we have to use an ActionDispatcher in order to
        //  own the Timer. This is not necessary on Windows Forms, WPF, or
        //  Silverlight applications.

        using (ActionDispatcher actionDispatcher = new ActionDispatcher())
        {
            actionDispatcher.QueueAction(FirstAction);
            actionDispatcher.Run();
        }
    }

    // This is the first action done by the main loop
    static void FirstAction()
    {
        // Create the timer
        Timer timer = new Timer();

        // Set the Elapsed handler
        timer.Elapsed += timer_Elapsed;

        // Start the timer
        timer.SetSingleShot(TimeSpan.FromMilliseconds(1));
        Thread.Sleep(10);

        // Dispose the timer
        timer.Dispose();

        // Note that the elapsed method will *not* be called
        //  even though the timer has elapsed, because it was
        //  Disposed before returning to the main loop

        // Exit the main loop
        ActionDispatcher.Current.QueueExit();
    }

    // This is never called
    static void timer_Elapsed()
    {
        throw new Exception("Aaugh! Something is horribly wrong!");
    }
}