using Nito.Async;

class ActionThreadIsBackground
{
    static void Main()
    {
        ActionThread actionThread = new ActionThread();

        // A background ActionThread has the same semantics as a background Thread
        actionThread.IsBackground = true;

        // The program will exit (not hang) without requiring the ActionThread to Join
        //  because it is a background thread
    }
}
