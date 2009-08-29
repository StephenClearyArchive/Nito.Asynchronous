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
        /// Gets or sets the name of this <see cref="ActionThread"/>.
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
        public bool Join(TimeSpan timeout)
        {
            this.dispatcher.QueueExit();
            return this.thread.Join(timeout);
        }

        /// <summary>
        /// Requests this <see cref="ActionThread"/> to exit and then blocks the calling thread until this <see cref="ActionThread"/> exits.
        /// </summary>
        public void Join()
        {
            this.dispatcher.QueueExit();
            this.thread.Join();
        }

        /// <summary>
        /// Starts this <see cref="ActionThread"/> running.
        /// </summary>
        public void Start()
        {
            this.thread.Start();
        }

        /// <summary>
        /// Queues work for the <see cref="ActionThread"/> to do.
        /// </summary>
        /// <param name="action">The work to do.</param>
        public void Do(Action action)
        {
            this.dispatcher.QueueAction(action);
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
