using Nito.Async;

class ActionThreadConstructStartJoin
{
    static void Main()
    {
        using (ActionThread actionThread = new ActionThread())
        {
            actionThread.Start();

            // This call to Join is not strictly necessary, since Dispose
            //  will perform an implicit Join.
            actionThread.Join();
        }
    }
}