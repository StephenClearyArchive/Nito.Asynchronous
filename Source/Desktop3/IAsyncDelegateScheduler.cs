using System;

namespace Nito.Communication
{
    public interface IAsyncDelegateScheduler : IDisposable
    {
        void Schedule(Action action);
    }
}
