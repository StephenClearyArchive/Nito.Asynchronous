namespace Nito.Communication
{
    using System;

    /// <summary>
    /// A type that asynchronously schedules a delegate to execute in a different context.
    /// </summary>
    public interface IAsyncDelegateScheduler
    {
        /// <summary>
        /// Schedules the specified delegate to execute in a different context.
        /// </summary>
        /// <param name="action">The delegate to schedule.</param>
        void Schedule(Action action);
    }
}
