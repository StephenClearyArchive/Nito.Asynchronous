using System;

namespace Nito.Communication
{
    public interface IAsyncDelegateScheduler
    {
        void Schedule(Action action);
    }
}
