
namespace Nito.Communication
{
    using System;
    using System.Threading;
    using Nito.Async;

    /// <summary>
    /// A delegate scheduler that queues the delegate to a <see cref="SynchronizationContext"/>. The <see cref="SynchronizationContext"/> must be non-reentrant, sequential, and synchronized.
    /// </summary>
    public sealed class SynchronizationContextAsyncDelegateScheduler : IAsyncDelegateScheduler
    {
        /// <summary>
        /// The context used for queueing the delegate.
        /// </summary>
        private readonly SynchronizationContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationContextAsyncDelegateScheduler"/> class, capturing the current <see cref="SynchronizationContext"/>.
        /// </summary>
        public SynchronizationContextAsyncDelegateScheduler()
        {
            this.context = SynchronizationContext.Current;
            SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost | SynchronizationContextProperties.Sequential | SynchronizationContextProperties.Synchronized);
        }

        /// <summary>
        /// Schedules the specified delegate to execute in the captured <see cref="SynchronizationContext"/> context.
        /// </summary>
        /// <param name="action">The delegate to schedule.</param>
        public void Schedule(Action action)
        {
            this.context.Post(_ => action(), null);
        }
    }
}
