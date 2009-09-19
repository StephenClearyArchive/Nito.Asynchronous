using Nito.Async;

class ActionDispatcherQueueActionCurrent
{
    static void Main()
    {
        using (ActionDispatcher actionDispatcher = new ActionDispatcher())
        {
            // At this point in the code, ActionDispatcher.Current is null
            // However, inside an action queued to actionDispatcher, ActionDispatcher.Current
            //  refers to actionDispatcher.
            actionDispatcher.QueueAction(() => ActionDispatcher.Current.QueueExit());
            actionDispatcher.Run();

            // Once Run returns, it is safe to Dispose the ActionDispatcher
        }
    }
}