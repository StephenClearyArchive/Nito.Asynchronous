using Nito.Async;
using System;

class TimerSingleShotRestartCancel
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
    static Timer timer = null;
    static void FirstAction()
    {
        timer = new Timer();
        timer.Elapsed += timer_Elapsed;
        timer.SetSingleShot(TimeSpan.FromMilliseconds(10));
    }

    // This is called by the main loop each time the timer fires
    static int elapsedCount = 0;
    static void timer_Elapsed()
    {
        ++elapsedCount;
        Console.WriteLine("Timer has fired " + elapsedCount + " times.");

        // Restart the timer so that it will call this method again after the next timeout
        timer.Restart();

        // Exit the main loop after the timer has fired 5 times
        if (elapsedCount == 5)
        {
            // Explicitly cancelling the timer ensures that this method won't
            //  be called again
            timer.Cancel();

            ActionDispatcher.Current.QueueExit();
        }
    }
}