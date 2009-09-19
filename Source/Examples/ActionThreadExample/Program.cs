using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        try
        {
            // Create an ActionThread
            Console.WriteLine("Creating ActionThread...");
            Nito.Async.ActionThread actionThread = new Nito.Async.ActionThread();

            // Display properties
            Console.WriteLine("ActionThread properties:");
            Console.WriteLine("  IsAlive: " + actionThread.IsAlive);
            Console.WriteLine("  IsBackground: " + actionThread.IsBackground);
            Console.WriteLine("  ManagedThreadId: " + actionThread.ManagedThreadId);
            Console.WriteLine("  Name: " + actionThread.Name);
            Console.WriteLine("  Priority: " + actionThread.Priority);

            // The Name property can be set, but only before the thread has started
            // If the thread is started without setting Name, then Name will be set to a reasonable default value, e.g., "Nito.Async.ActionThread"
            actionThread.Name = "Bob";

            Console.WriteLine("Starting ActionThread...");
            actionThread.Start();

            Console.WriteLine("ActionThread properties:");
            Console.WriteLine("  IsAlive: " + actionThread.IsAlive);
            Console.WriteLine("  IsBackground: " + actionThread.IsBackground);
            Console.WriteLine("  ManagedThreadId: " + actionThread.ManagedThreadId);
            Console.WriteLine("  Name: " + actionThread.Name);
            Console.WriteLine("  Priority: " + actionThread.Priority);

            // An ActionThread may be a background thread if desired
            actionThread.IsBackground = true;

            // The priority of an ActionThread may be adjusted; like regular thread priorities, setting this property is not recommended
            actionThread.Priority = System.Threading.ThreadPriority.AboveNormal;

            Console.WriteLine("Joining ActionThread...");
            actionThread.Join();

            Console.WriteLine("ActionThread properties:");
            Console.WriteLine("  IsAlive: " + actionThread.IsAlive);
            // IsBackground may not be accessed after a Join
            Console.WriteLine("  ManagedThreadId: " + actionThread.ManagedThreadId);
            Console.WriteLine("  Name: " + actionThread.Name);
            // Priority may not be accessed after a Join

            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected exception [" + ex.GetType().Name + "] " + ex.Message);
            Console.ReadLine();
        }
    }
}