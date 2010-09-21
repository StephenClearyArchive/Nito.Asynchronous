using System;

namespace Nito.Communication
{
    using System.Threading;

    using Async;

    public sealed class SynchronizationContextAsyncDelegateScheduler : IAsyncDelegateScheduler
    {
        private readonly SynchronizationContext context;

        public SynchronizationContextAsyncDelegateScheduler()
        {
            this.context = SynchronizationContext.Current;
            SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost | SynchronizationContextProperties.Sequential | SynchronizationContextProperties.Synchronized);
        }

        public void Schedule(Action action)
        {
            this.context.Post(_ => action(), null);
        }

        public void Dispose()
        {
        }
    }
}
