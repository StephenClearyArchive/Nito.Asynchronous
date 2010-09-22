
namespace Nito.Communication
{
    public static class AsyncDelegateSchedulerFactory
    {
        public static IAsyncDelegateScheduler Create()
        {
            return new SynchronizationContextAsyncDelegateScheduler();
        }
    }
}
