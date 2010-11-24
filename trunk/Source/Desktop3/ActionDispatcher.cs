// <copyright file="ActionDispatcher.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;

    /// <summary>
    /// A thread-safe queue of actions. Provides an event-based message loop when <see cref="Run"/>, along with a synchronization context for the executed actions.
    /// </summary>
    /// <remarks>
    /// <para>Actions are executed in the order they are queued.</para>
    /// <para>Each action executes within an <see cref="ActionDispatcherSynchronizationContext"/>.</para>
    /// </remarks>
    /// <threadsafety>This class is used for thread synchronization, so see the notes on each member for thread safety information.</threadsafety>
    /// <example>The following code sample demonstrates how to use an ActionDispatcher to convert a Console application's main thread into an event-driven thread:
    /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\WithTimer.cs"/>
    /// The code example above produces this output:
    /// <code lang="None" title="Output">
    /// In main thread (thread ID 1)
    /// Elapsed running in thread pool thread (thread ID 4)
    /// Hello from main thread (thread ID 1)
    /// Elapsed running in thread pool thread (thread ID 4)
    /// </code>
    /// The following code sample demonstrates how the event-based loop provided by ActionDispatcher is sufficient to own event-based asynchronous pattern types like the BackgroundWorker:
    /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\WithBackgroundWorker.cs"/>
    /// The code example above produces this output:
    /// <code lang="None" title="Output">
    /// Main console thread ID is 1 and is not a threadpool thread
    /// ActionDispatcher thread ID is 1 and is not a threadpool thread
    /// BackgroundWorker thread ID is 3 and is a threadpool thread
    /// BGW event thread ID is 1 and is not a threadpool thread
    /// </code>
    /// </example>
    public sealed class ActionDispatcher : IDisposable
    {
        /// <summary>
        /// An event that is signalled when the action queue has at least one action to run.
        /// </summary>
        private readonly ManualResetEvent actionQueueNotEmptyEvent;

        /// <summary>
        /// The queue holding the actions to run.
        /// </summary>
        private readonly Queue<Action> actionQueue;

        /// <summary>
        /// The action to queue that causes <see cref="Run"/> to exit.
        /// </summary>
        private static readonly Action ExitAction;

        static ActionDispatcher()
        {
            ExitAction = () => { throw ExitException.Instance; };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDispatcher"/> class with an empty action queue.
        /// </summary>
        /// <example>The following code sample demonstrates how to create an ActionDispatcher, queue an exit action, and run it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\ConstructQueueExitRun.cs"/>
        /// </example>
        public ActionDispatcher()
        {
            this.actionQueueNotEmptyEvent = new ManualResetEvent(false);
            this.actionQueue = new Queue<Action>();
        }

        /// <summary>
        /// Gets the currently active action queue. For executing actions, this is their own action queue; for other threads, this is null.
        /// </summary>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        /// <example>The following code sample demonstrates how to queue an action to an ActionDispatcher and access the Current property:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\QueueActionCurrent.cs"/>
        /// </example>
        public static ActionDispatcher Current
        {
            get
            {
                var context = SynchronizationContext.Current as ActionDispatcherSynchronizationContext;
                if (context == null)
                {
                    return null;
                }

                return context.ActionDispatcher;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.actionQueue != null);
            Contract.Invariant(this.actionQueueNotEmptyEvent != null);
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        /// <threadsafety>
        /// <note class="warning">This method should not be called while a thread is executing <see cref="Run"/>.</note>
        /// <para>If there is a thread executing <see cref="Run"/>, call <see cref="QueueExit"/> and wait for the thread to exit before calling this method.</para>
        /// </threadsafety>
        /// <example>The following code sample demonstrates how to create an ActionDispatcher, queue an exit action, and run it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\ConstructQueueExitRun.cs"/>
        /// </example>
        public void Dispose()
        {
            this.actionQueueNotEmptyEvent.Close();
        }

        /// <summary>
        /// Executes the action queue.
        /// </summary>
        /// <remarks>
        /// <para>This method only returns after <see cref="QueueExit"/> is called. When the action queue is empty, the thread waits for additional actions to be queued via <see cref="QueueAction"/> or <see cref="QueueExit"/>.</para>
        /// <para>Executing actions may access their own action queue via the <see cref="Current"/> property, and may queue other actions and/or an exit action.</para>
        /// <para>This method should not be called from a thread pool thread in most cases.</para>
        /// </remarks>
        /// <threadsafety>
        /// <para>This method may only be called by one thread at a time.</para>
        /// <para>If event-based asynchronous components are owned by this ActionDispatcher (or if any actions access <see cref="SynchronizationContext.Current"/>), then this method may only be called by one thread.</para>
        /// </threadsafety>
        /// <example>The following code sample demonstrates how to create an ActionDispatcher, queue an exit action, and run it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\ConstructQueueExitRun.cs"/>
        /// </example>
        public void Run()
        {
            try
            {
                // Set the synchronization context
                SynchronizationContext.SetSynchronizationContext(new ActionDispatcherSynchronizationContext(this));
                while (true)
                {
                    // Dequeue and run an action
                    this.DequeueAction()();
                }
            }
            catch (ExitException)
            {
            }
        }

        /// <summary>
        /// Queues an action to an action dispatcher.
        /// </summary>
        /// <param name="action">The action to execute. May not be <c>null</c>.</param>
        /// <remarks>
        /// <para>Actions are executed in the order they are queued.</para>
        /// <para>Actions may queue other actions and/or an exit action by using the <see cref="Current"/> action dispatcher.</para>
        /// </remarks>
        /// <threadsafety>This method may be called by any thread at any time.</threadsafety>
        /// <example>The following code sample demonstrates how to queue an action to an ActionDispatcher and access the Current property:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\QueueActionCurrent.cs"/>
        /// </example>
        public void QueueAction(Action action)
        {
            Contract.Requires(action != null);

            lock (this.actionQueue)
            {
                // Add the action to the action queue
                this.actionQueue.Enqueue(action);

                // Set the signal if necessary
                if (this.actionQueue.Count == 1)
                {
                    this.actionQueueNotEmptyEvent.Set();
                }
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
        /// <example>The following code sample demonstrates how to create an ActionDispatcher, queue an exit action, and run it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\ConstructQueueExitRun.cs"/>
        /// </example>
        public void QueueExit()
        {
            this.QueueAction(() => { throw new ExitException(); });
        }

        /// <summary>
        /// Waits for the action queue to be non-empty, removes a single action, and returns it.
        /// </summary>
        /// <returns>The next action from the action queue.</returns>
        private Action DequeueAction()
        {
            Contract.Ensures(Contract.Result<Action>() != null);

            // Wait for an action to arrive
            this.actionQueueNotEmptyEvent.WaitOne();

            Action ret;

            lock (this.actionQueue)
            {
                // Remove an action from the action queue
                ret = this.actionQueue.Dequeue();
                Contract.Assume(ret != null);

                // Reset the signal if necessary
                if (this.actionQueue.Count == 0)
                {
                    this.actionQueueNotEmptyEvent.Reset();
                }
            }

            return ret;
        }

        /// <summary>
        /// A special exception type; when thrown, this indicates the thread should exit <see cref="Run"/>.
        /// </summary>
        private sealed class ExitException : Exception
        {
            public static readonly ExitException Instance;

            static ExitException()
            {
                Instance = new ExitException();
            }
        }
    }
}
