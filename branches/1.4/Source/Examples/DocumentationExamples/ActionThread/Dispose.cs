using Nito.Async;

class ActionThreadDispose
{
    static void Main()
    {
        using (ActionThread actionThread = new ActionThread())
        {
            actionThread.Start();

            // Dispose performs an implicit Join at the end of this block,
            //  which waits for the ActionThread to finish its current queue
            //  of actions.
        }
    }
}