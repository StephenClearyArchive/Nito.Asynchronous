using Nito.Async;
using System.Threading;
using System;

class ActionThreadJoinWithTimeout
{
    static void Main()
    {
        ActionThread actionThread = new ActionThread();
        // Set IsBackground so that this thread won't prevent the program from exiting
        actionThread.IsBackground = true;
        actionThread.Start();

        // Tell the ActionThread to sleep for a minute
        actionThread.Do(() => Thread.Sleep(TimeSpan.FromMinutes(1)));

        // Attempt to Join for 100 ms; this attempt should fail
        bool threadJoined = actionThread.Join(TimeSpan.FromMilliseconds(100));

        Console.WriteLine("Thread joined: " + threadJoined);
    }
}