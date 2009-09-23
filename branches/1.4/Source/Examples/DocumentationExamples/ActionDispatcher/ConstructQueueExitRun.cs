using Nito.Async;

class ActionDispatcherConstructQueueExitRun
{
    static void Main()
    {
        using (ActionDispatcher actionDispatcher = new ActionDispatcher())
        {
            actionDispatcher.QueueExit();
            actionDispatcher.Run();

            // Once Run returns, it is safe to Dispose the ActionDispatcher
        }
    }
}