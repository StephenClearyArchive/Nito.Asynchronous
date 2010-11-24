// <copyright file="ActionDispatcherSynchronizationContext.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System.Diagnostics.Contracts;
    using System.Threading;

    /// <summary>
    /// Provides a synchronization context for a thread running an <see cref="Nito.Async.ActionDispatcher"/>.
    /// </summary>
    /// <remarks>
    /// <para>Note that most users will not use this class directly. Instances of this class are provided by <see cref="Nito.Async.ActionDispatcher"/> and <see cref="Nito.Async.ActionThread"/>. This class is consumed by the .NET standard types <see cref="System.ComponentModel.AsyncOperation"/> and <see cref="System.ComponentModel.AsyncOperationManager"/>.</para>
    /// <para>This type registers itself with <see cref="SynchronizationContextRegister"/> as supporting all of the <see cref="SynchronizationContextProperties.Standard"/> properties. Technically, this is only true if only one thread ever calls <see cref="Nito.Async.ActionDispatcher.Run"/> on its <see cref="Nito.Async.ActionDispatcher"/>.</para>
    /// </remarks>
    public sealed class ActionDispatcherSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        /// The action queue for the thread to synchronize with.
        /// </summary>
        private readonly ActionDispatcher actionDispatcher;

        /// <summary>
        /// Initializes static members of the <see cref="ActionDispatcherSynchronizationContext"/> class by registering with <see cref="SynchronizationContextRegister"/>.
        /// </summary>
        static ActionDispatcherSynchronizationContext()
        {
            SynchronizationContextRegister.Register(typeof(ActionDispatcherSynchronizationContext), SynchronizationContextProperties.Standard);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDispatcherSynchronizationContext"/> class by using the specified <see cref="Nito.Async.ActionDispatcher"/>.
        /// </summary>
        /// <param name="actionDispatcher">The action queue to associate with this <see cref="ActionDispatcherSynchronizationContext"/>. May not be <c>null</c>.</param>
        public ActionDispatcherSynchronizationContext(ActionDispatcher actionDispatcher)
        {
            Contract.Requires(actionDispatcher != null);
            this.actionDispatcher = actionDispatcher;
        }

        /// <summary>
        /// Gets the action queue for the thread to synchronize with.
        /// </summary>
        internal ActionDispatcher ActionDispatcher
        {
            get { return this.actionDispatcher; }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.actionDispatcher != null);
        }

        /// <summary>
        /// Creates a copy of this <see cref="ActionDispatcherSynchronizationContext"/>.
        /// </summary>
        /// <returns>The copy of this synchronization context.</returns>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        public override SynchronizationContext CreateCopy()
        {
            return new ActionDispatcherSynchronizationContext(this.actionDispatcher);
        }

        /// <summary>
        /// Invokes the callback in the synchronization context asynchronously. The callback is placed in the action queue.
        /// </summary>
        /// <param name="d">The delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        public override void Post(SendOrPostCallback d, object state)
        {
            this.actionDispatcher.QueueAction(() => d(state));
        }

        /// <summary>
        /// Invokes the callback in the synchronization context synchronously. The callback is placed in the action queue.
        /// </summary>
        /// <param name="d">The delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        /// <remarks>
        /// <para>This method cannot be called from the thread running the action queue associated with this synchronization context.</para>
        /// </remarks>
        public override void Send(SendOrPostCallback d, object state)
        {
            using (var evt = new ManualResetEvent(false))
            {
                this.actionDispatcher.QueueAction(() => { d(state); evt.Set(); });
                evt.WaitOne();
            }
        }
    }
}
