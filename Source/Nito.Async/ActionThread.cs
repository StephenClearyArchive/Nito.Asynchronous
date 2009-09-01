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
    /// <para>Each action executed by this thread runs in an <see cref="ActionDispatcherSynchronizationContext"/>. This means that <see cref="ActionThread"/> objects may own asychronous objects.</para>
    /// </remarks>
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
        public ActionThread()
        {
            this.dispatcher = new ActionDispatcher();
            this.thread = new Thread(() => this.dispatcher.Run());
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ActionThread"/> is still alive (has started but not yet exited).
        /// </summary>
        public bool IsAlive
        {
            get { return this.thread.IsAlive; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ActionThread"/> is a background thread.
        /// </summary>
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
        /// Gets or sets the name of this <see cref="ActionThread"/>. This property may only be set once.
        /// </summary>
        public string Name
        {
            get { return this.thread.Name; }
            set { this.thread.Name = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of this <see cref="ActionThread"/>.
        /// </summary>
        public ThreadPriority Priority
        {
            get { return this.thread.Priority; }
            set { this.thread.Priority = value; }
        }

        /// <summary>
        /// Requests this <see cref="ActionThread"/> to exit and then blocks the calling thread until either this <see cref="ActionThread"/> exits or a timeout occurs.
        /// </summary>
        /// <param name="timeout">The length of time to wait for this <see cref="ActionThread"/> to exit.</param>
        /// <returns><c>true</c> if this <see cref="ActionThread"/> exited cleanly; <c>false</c> if the timout occurred.</returns>
        /// <remarks>
        /// <para>This method has no effect if the thread has not started or has already exited.</para>
        /// </remarks>
        public bool Join(TimeSpan timeout)
        {
            if (this.IsAlive)
            {
                this.dispatcher.QueueExit();
                return this.thread.Join(timeout);
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
        public void Start()
        {
            this.thread.Start();
        }

        /// <summary>
        /// Queues work for the <see cref="ActionThread"/> to do.
        /// </summary>
        /// <param name="action">The work to do. This delegate may not throw an exception.</param>
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
        /// <para>Be careful when using short timeout values; the <paramref name="action"/> delegate may not be scheduled for work immediately if the thread already has other work queued.</para>
        /// </remarks>
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
        /// <para>This method may not be called before the thread has started (see <see cref="Start"/>) or after the thread has joined (see <see cref="Join"/>).</para>
        /// </remarks>
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
        public T DoGet<T>(Func<T> action)
        {
            T ret = default(T);
            this.DoSynchronously(() => { ret = action(); }, TimeSpan.FromMilliseconds(-1));
            return ret;
        }

        /// <summary>
        /// Requests this <see cref="ActionThread"/> to exit, blocks the calling thread until this <see cref="ActionThread"/> exits, and then cleans up all resources.
        /// </summary>
        public void Dispose()
        {
            this.Join();
            this.dispatcher.Dispose();
        }
    }
}
