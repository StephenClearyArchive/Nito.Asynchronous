using Nito.Async;

class ActionThreadConstructStartJoin
{
    static void Main()
    {
        ActionThread actionThread = new ActionThread();
        actionThread.Start();
        actionThread.Join();
    }
}