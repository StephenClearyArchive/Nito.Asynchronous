// <copyright file="ActionThread.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Threading;

    /// <summary>
    /// A thread that executes actions when commanded and provides its own <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>An ActionThread executes its actions one at a time. This provides some built-in concurrency guarantees.</para>
    /// <para>Each action executed by this thread runs in an <see cref="ActionDispatcherSynchronizationContext"/>. This means that <see cref="ActionThread"/> objects may own objects with managed thread affinity, including objects developed with the event-based asynchronous pattern.</para>
    /// <para>Each ActionThread in a program is logically a message queue, with a dedicated thread executing a message-processing loop. An ActionThread may be used to give a Console or Windows Service application a synchronized, event-based main loop.</para>
    /// <para>When used in other types of applications (Windows Forms, WPF, etc.), ActionThreads provide a secondary message-processing loop that complements the application's main loop.</para>
    /// <para>ActionThreads may also be used in ASP.NET applications if events need to be synchronized.</para>
    /// </remarks>
    /// <example>
    /// The following code example demonstrates how ActionThreads may be used to make an event-driven Console application:
    /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\WithBackgroundWorker.cs"/>
    /// The code example above produces this output:
    /// <code lang="None" title="Output">
    /// Main console thread ID is 1 and is not a threadpool thread
    /// ActionThread thread ID is 3 and is not a threadpool thread
    /// BackgroundWorker thread ID is 4 and is a threadpool thread
    /// BGW event thread ID is 3 and is not a threadpool thread
    /// </code>
    /// </example>
    public sealed class ActionThread : IDisposable
    {
        /// <summary>
        /// The child thread.
        /// </summary>
        private Thread thread;

        /// <summary>
        /// The queue of actions to perform.
        /// </summary>
        private ActionDispatcher dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionThread"/> class, creating a child thread waiting for commands.
        /// </summary>
        /// <example>The following code sample demonstrates how to create an ActionThread, start it, and then join with it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\ConstructStartJoin.cs"/>
        /// </example>
        public ActionThread()
        {
            this.dispatcher = new ActionDispatcher();
            this.thread = new Thread(() => this.dispatcher.Run());
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ActionThread"/> is still alive (has started but not yet exited).
        /// </summary>
        /// <example>The following code sample demonstrates how to read the IsAlive property:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\IsAlive.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// ActionThread.IsAlive before Start: False
        /// ActionThread.IsAlive after Start, before Join: True
        /// ActionThread.IsAlive after Join: False
        /// </code>
        /// </example>
        public bool IsAlive
        {
            get { return this.thread.IsAlive; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ActionThread"/> is a background thread. This property may not be accessed after a <see cref="O:Nito.Async.ActionThread.Join"/>.
        /// </summary>
        /// <example>The following code sample demonstrates how to use the IsBackground property:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\IsBackground.cs"/>
        /// </example>
        public bool IsBackground
        {
            get { return this.thread.IsBackground; }
            set { this.thread.IsBackground = value; }
        }

        /// <summary>
        /// Gets a unique identifier for this <see cref="ActionThread"/>.
        /// </summary>
        public int ManagedThreadId
        {
            get { return this.thread.ManagedThreadId; }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="ActionThread"/>. This property may only be set once, before the thread is started.
        /// </summary>
        /// <remarks>
        /// <para>Starting the thread will set this to a reasonable default value if it has not already been set.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to use the Name property:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\Name.cs"/>
        /// </example>
        public string Name
        {
            get { return this.thread.Name; }

            set { this.thread.Name = value; }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of this <see cref="ActionThread"/>. This property may not be accessed after a <see cref="O:Nito.Async.ActionThread.Join"/>.
        /// </summary>
        /// <remarks>
        /// <para>Like normal <see cref="Thread"/> objects, the priority should not generally be set.</para>
        /// </remarks>
        public ThreadPriority Priority
        {
            get { return this.thread.Priority; }
            set { this.thread.Priority = value; }
        }
#endif

        /// <summary>
        /// Requests this <see cref="ActionThread"/> to exit and then blocks the calling thread until either this <see cref="ActionThread"/> exits or a timeout occurs.
        /// </summary>
        /// <param name="timeout">The length of time to wait for this <see cref="ActionThread"/> to exit.</param>
        /// <returns><c>true</c> if this <see cref="ActionThread"/> exited cleanly; <c>false</c> if the timout occurred.</returns>
        /// <remarks>
        /// <para>This method has no effect if the thread has not started or has already exited.</para>
        /// <para>Be careful when using short timeout values; the thread may already have other work queued.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to join with an ActionThread with a timeout:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\JoinWithTimeout.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// Thread joined: False
        /// </code>
        /// </example>
        public bool Join(TimeSpan timeout)
        {
            if (this.IsAlive)
            {
                this.dispatcher.QueueExit();
                return this.thread.Join((int)timeout.TotalMilliseconds);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Requests this <see cref="ActionThread"/> to exit and then blocks the calling thread until this <see cref="ActionThread"/> exits.
        /// </summary>
        /// <remarks>
        /// <para>This method has no effect if the thread has not started or has already exited.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to create an ActionThread, start it, and then join with it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\ConstructStartJoin.cs"/>
        /// </example>
        public void Join()
        {
            if (this.IsAlive)
            {
                this.dispatcher.QueueExit();
                this.thread.Join();
            }
        }

        /// <summary>
        /// Starts this <see cref="ActionThread"/> running. A thread may not be started more than once.
        /// </summary>
        /// <remarks>
        /// <para>Work may be queued to the thread before it starts running.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to create an ActionThread, start it, and then join with it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\ConstructStartJoin.cs"/>
        /// </example>
        public void Start()
        {
            if (this.Name == null)
            {
                this.Name = "Nito.Async.ActionThread";
            }

            this.thread.Start();
        }

        /// <summary>
        /// Queues work for the <see cref="ActionThread"/> to do.
        /// </summary>
        /// <param name="action">The work to do. This delegate may not throw an exception.</param>
        /// <example>The following code sample demonstrates how to queue work to an ActionThread:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\Do.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// Console thread ID: 1
        /// ActionThread thread ID: 3
        /// </code>
        /// </example>
        public void Do(Action action)
        {
            this.dispatcher.QueueAction(action);
        }

        /// <summary>
        /// Queues work for the <see cref="ActionThread"/> to do, and blocks the calling thread until it is complete or until the specified time has elapsed.
        /// </summary>
        /// <param name="action">The work to do. This delegate may not throw an exception.</param>
        /// <param name="timeout">The time to wait for <paramref name="action"/> to execute.</param>
        /// <returns><c>true</c> if <paramref name="action"/> executed completely; <c>false</c> if there was a timeout.</returns>
        /// <remarks>
        /// <para>If this method returns <c>false</c>, then the action is not aborted; it continues running asynchronously.</para>
        /// <para>Be careful when using short timeout values; the <paramref name="action"/> delegate may not be scheduled for work immediately if the thread already has other work queued.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to queue work synchronously to an ActionThread with a timeout:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\DoSynchronouslyWithTimeout.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// ActionThread completed action synchronously: False
        /// </code>
        /// </example>
        public bool DoSynchronously(Action action, TimeSpan timeout)
        {
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                this.dispatcher.QueueAction(() => { action(); evt.Set(); });
                return evt.WaitOne(timeout);
            }
        }

        /// <summary>
        /// Queues work for the <see cref="ActionThread"/> to do, and blocks the calling thread until it is complete.
        /// </summary>
        /// <param name="action">The work to do. This delegate may not throw an exception.</param>
        /// <remarks>
        /// <para>This method may not be called before the thread has started (see <see cref="Start"/>) or after the thread has joined (see <see cref="O:Nito.Async.ActionThread.Join"/>).</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to queue work synchronously to an ActionThread:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\DoSynchronously.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// Console thread ID: 1
        /// ActionThread thread ID: 3
        /// </code>
        /// </example>
        public void DoSynchronously(Action action)
        {
            // This test actually has a race condition; it's not possible to fully detect all conditions
            if (this.thread.ThreadState == ThreadState.Unstarted || this.thread.ThreadState == ThreadState.Stopped)
            {
                // If we went ahead and queued the work, it probably would result in a deadlock
                throw new ThreadStateException("ActionThread.DoSynchronously can only queue work to a running thread.");
            }

            this.DoSynchronously(action, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Queues work for the <see cref="ActionThread"/> to do, and blocks the calling thread until it is complete.
        /// </summary>
        /// <typeparam name="T">The type of object retrieved by the delegate.</typeparam>
        /// <param name="action">The work to do. This delegate may not throw an exception.</param>
        /// <returns>The return value of the delegate.</returns>
        /// <remarks>
        /// <para>This method may only be called after the thread has been started.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to queue work synchronously to an ActionThread:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\DoGet.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// Console thread ID: 1
        /// ActionThread thread ID: 3
        /// </code>
        /// </example>
        public T DoGet<T>(Func<T> action)
        {
            T ret = default(T);
            this.DoSynchronously(() => { ret = action(); }, TimeSpan.FromMilliseconds(-1));
            return ret;
        }

        /// <summary>
        /// Requests this <see cref="ActionThread"/> to exit, blocks the calling thread until this <see cref="ActionThread"/> exits, and then cleans up all resources.
        /// </summary>
        /// <example>The following code sample demonstrates how to dispose an ActionThread:
        /// <code source="..\..\Source\Examples\DocumentationExamples\ActionThread\Dispose.cs"/>
        /// </example>
        public void Dispose()
        {
            this.Join();
            this.dispatcher.Dispose();
        }
    }
}
