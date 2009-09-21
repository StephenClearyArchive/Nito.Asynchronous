using Nito.Async;
using System;

class TimerSingleShotProperties
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

        // Set it as a single-shot timer
        timer.AutoReset = false;

        // Set the timeout interval
        timer.Interval = TimeSpan.FromMilliseconds(100);

        // Start the timer
        timer.Enabled = true;
    }

    // This is called by the main loop when the timer elapses
    static void timer_Elapsed()
    {
        // Exit the main loop
        ActionDispatcher.Current.QueueExit();
    }
}