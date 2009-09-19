using Nito.Async;
using System;

class ActionThreadIsAlive
{
    static void Main()
    {
        using (ActionThread actionThread = new ActionThread())
        {
            Console.WriteLine("ActionThread.IsAlive before Start: " + actionThread.IsAlive);

            actionThread.Start();

            Console.WriteLine("ActionThread.IsAlive after Start, before Join: " + actionThread.IsAlive);

            actionThread.Join();

            Console.WriteLine("ActionThread.IsAlive after Join: " + actionThread.IsAlive);
        }
    }
}