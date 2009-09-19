// <copyright file="ActionDispatcher.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;
    using System.Threading;

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
        /// An event that is signalled when the action queue has at least one action to run.
        /// </summary>
        private ManualResetEvent actionQueueNotEmptyEvent;

        /// <summary>
        /// The queue holding the actions to run.
        /// </summary>
        private Queue<Action> actionQueue;

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
                ActionDispatcherSynchronizationContext context = SynchronizationContext.Current as ActionDispatcherSynchronizationContext;
                if (context == null)
                {
                    return null;
                }

                return context.ActionDispatcher;
            }
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
        /// </remarks>
        /// <threadsafety>
        /// <para>This method may only be called by one thread at a time.</para>
        /// </threadsafety>
        /// <example>The following code sample demonstrates how to create an ActionDispatcher, queue an exit action, and run it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionDispatcher\ConstructQueueExitRun.cs"/>
        /// </example>
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
        /// <param name="action">The action to execute.</param>
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
            // Wait for an action to arrive
            this.actionQueueNotEmptyEvent.WaitOne();

            Action ret;

            lock (this.actionQueue)
            {
                // Remove an action from the action queue
                ret = this.actionQueue.Dequeue();

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
        }
    }
}
