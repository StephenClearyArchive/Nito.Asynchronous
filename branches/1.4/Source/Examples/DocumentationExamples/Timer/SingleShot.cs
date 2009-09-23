using Nito.Async;
using System;

class TimerSingleShot
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
        timer.SetSingleShot(TimeSpan.FromMilliseconds(100));
    }

    // This is called by the main loop when the timer elapses
    static void timer_Elapsed()
    {
        // Exit the main loop
        ActionDispatcher.Current.QueueExit();
    }
}