
namespace Nito.Communication
{
    /// <summary>
    /// A factory class for <see cref="IAsyncDelegateScheduler"/> types.
    /// </summary>
    public static class AsyncDelegateSchedulerFactory
    {
        /// <summary>
        /// Creates the default <see cref="IAsyncDelegateScheduler"/> for the current platform. This method should be called from a user interface thread or <see cref="Async.ActionThread"/>.
        /// </summary>
        /// <returns>The created <see cref="IAsyncDelegateScheduler"/>.</returns>
        public static IAsyncDelegateScheduler Create()
        {
            return new SynchronizationContextAsyncDelegateScheduler();
        }
    }
}
