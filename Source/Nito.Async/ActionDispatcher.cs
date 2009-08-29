using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Security.Permissions;

// Copyright 2009 by Nito Programs.

namespace Nito.Async
{
    /// <summary>
    /// A thread-safe queue of actions.
    /// </summary>
    /// <remarks>
    /// <para>Actions are executed in the order they are queued.</para>
    /// </remarks>
    /// <threadsafety>This class is used for thread synchronization, so see the notes on each member for thread safety information.</threadsafety>
    public sealed class ActionDispatcher : IDisposable
    {
        /// <summary>
        /// A special type; when thrown, this indicates the thread should exit <see cref="Run"/>.
        /// </summary>
        private sealed class ExitException : Exception { }

        /// <summary>
        /// An event that is signalled when the action queue has at least one action to run.
        /// </summary>
        private ManualResetEvent ActionQueueNotEmpty_;

        /// <summary>
        /// The queue holding the actions to run.
        /// </summary>
        private Queue<Action> ActionQueue_;

        /// <summary>
        /// Initializes a new, empty action queue.
        /// </summary>
        public ActionDispatcher()
        {
            ActionQueueNotEmpty_ = new ManualResetEvent(false);
            ActionQueue_ = new Queue<Action>();
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        /// <threadsafety>
        /// <note class="warning">This method should not be called while a thread is executing <see cref="Run"/>.</note>
        /// <para>If there is a thread executing <see cref="Run"/>, call <see cref="QueueExit"/> and wait for the thread to exit before calling this method.</para>
        /// </threadsafety>
        public void Dispose()
        {
            ActionQueueNotEmpty_.Close();
        }

        /// <summary>
        /// Waits for the action queue to be non-empty, removes a single action, and returns it.
        /// </summary>
        private Action DequeueAction()
        {
            // Wait for an action to arrive
            ActionQueueNotEmpty_.WaitOne();

            Action ret;

            lock (ActionQueue_)
            {
                // Remove an action from the action queue
                ret = ActionQueue_.Dequeue();

                // Reset the signal if necessary
                if (ActionQueue_.Count == 0)
                    ActionQueueNotEmpty_.Reset();
            }

            return ret;
        }

        /// <summary>
        /// Executes the action queue.
        /// </summary>
        /// <remarks>
        /// <para>This method only returns after <see cref="QueueExit"/> is called. When the action queue is empty, the thread waits for additional actions to be queued via <see cref="QueueAction"/> or <see cref="QueueExit"/>.</para>
        /// <para>Executing actions may access their own action queue via the <see cref="Current"/> property, and may queue other actions and/or an exit action.</para>
        /// </remarks>
        /// <threadsafety>
        /// <para>This method may only be called by one thread at a time.</para>
        /// </threadsafety>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy)]
        public void Run()
        {
            try
            {
                // Set the synchronization context
                SynchronizationContext.SetSynchronizationContext(new ActionDispatcherSynchronizationContext(this));
                while (true)
                {
                    // Dequeue and run an action
                    DequeueAction()();
                }
            }
            catch (ExitException)
            {
            }
        }

        /// <summary>
        /// Queues an action to an action dispatcher.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <remarks>
        /// <para>Actions are executed in the order they are queued.</para>
        /// <para>Actions may queue other actions and/or an exit action by using the <see cref="Current"/> action dispatcher.</para>
        /// </remarks>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        public void QueueAction(Action action)
        {
            lock (ActionQueue_)
            {
                // Add the action to the action queue
                ActionQueue_.Enqueue(action);

                // Set the signal if necessary
                if (ActionQueue_.Count == 1)
                    ActionQueueNotEmpty_.Set();
            }
        }

        /// <summary>
        /// Queues an exit action, causing <see cref="Run"/> to return.
        /// </summary>
        /// <remarks>
        /// <para>An exit action may be queued by an action from within <see cref="Run"/>; alternatively, another thread may queue the exit action.</para>
        /// <para><see cref="Run"/> may not return immediately; the exit action is queued like any other action and must wait its turn.</para>
        /// </remarks>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        public void QueueExit()
        {
            QueueAction(() => { throw new ExitException(); });
        }

        /// <summary>
        /// Returns the currently active action queue. For executing actions, this is their own action queue; for other threads, this is null.
        /// </summary>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        public static ActionDispatcher Current
        {
            get
            {
                ActionDispatcherSynchronizationContext context = SynchronizationContext.Current as ActionDispatcherSynchronizationContext;
                if (context == null)
                    return null;
                return context.ActionDispatcher_;
            }
        }
    }

    /// <summary>
    /// Provides a synchronization context for a thread running an <see cref="ActionDispatcher"/>.
    /// </summary>
    public sealed class ActionDispatcherSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        /// The action queue for the thread to synchronize with.
        /// </summary>
        internal ActionDispatcher ActionDispatcher_;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDispatcherSynchronizationContext"/> class by using the specified <see cref="ActionDispatcher"/>.
        /// </summary>
        /// <param name="actionDispatcher">The action queue to associate with this <see cref="ActionDispatcherSynchronizationContext"/>.</param>
        public ActionDispatcherSynchronizationContext(ActionDispatcher actionDispatcher)
        {
            ActionDispatcher_ = actionDispatcher;
        }

        /// <summary>
        /// Creates a copy of this <see cref="ActionDispatcherSynchronizationContext"/>.
        /// </summary>
        /// <returns>The copy of this synchronization context.</returns>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        public override SynchronizationContext CreateCopy()
        {
            return new ActionDispatcherSynchronizationContext(ActionDispatcher_);
        }

        /// <summary>
        /// Invokes the callback in the synchronization context asynchronously. The callback is placed in the action queue.
        /// </summary>
        /// <param name="d">The delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        public override void Post(SendOrPostCallback d, object state)
        {
            ActionDispatcher_.QueueAction(() => d(state));
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
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                ActionDispatcher_.QueueAction(() => { d(state); evt.Set(); });
                evt.WaitOne();
            }
        }
    }
}
