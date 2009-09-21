// <copyright file="AsyncTimeoutTimer.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Timers;

    /// <summary>
    /// Provides asynchronous timeout notifications.
    /// </summary>
    /// <remarks>
    /// <para>This is not a general-purpose timer class; it should only be used to detect timeout situations.</para>
    /// </remarks>
    [Obsolete("Nito.Async.AsyncTimeoutTimer has been replaced by Nito.Async.Timer: Timeout -> Elapsed, Set -> SetSingleShot, Reset -> Restart")]
    public sealed class AsyncTimeoutTimer : IDisposable
    {
        /// <summary>
        /// The actual underlying timer. We use <see cref="System.Timers.Timer"/> because it has a ready-to-use SynchronizingObject property,
        /// so it handles the event synchronization for us.
        /// </summary>
        private System.Timers.Timer timer;

        /// <summary>
        /// The current context of timer callbacks. This is necessary because it is possible for the user to <see cref="Reset"/> a timer that
        /// has already gone off and has queued a <see cref="TimerElapsed"/> callback.
        /// </summary>
        private CallbackContext context;

        /// <summary>
        /// The callback for the timer, bound to the <see cref="context"/> that was current when the callback was set. This delegate is saved so
        /// that the callbacks may be removed and replaced when the context changes.
        /// </summary>
        private ElapsedEventHandler timerElapsedHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTimeoutTimer"/> class.
        /// </summary>
        public AsyncTimeoutTimer()
        {
            this.timer = new System.Timers.Timer();
            this.timer.AutoReset = false;
            this.timer.SynchronizingObject = new GenericSynchronizingObject();
            this.context = new CallbackContext();
        }

        /// <summary>
        /// Notifies that a timeout has occurred.
        /// </summary>
        /// <remarks>
        /// <para>The timeout has been stopped by the time this is invoked.</para>
        /// </remarks>
        public event Action Timeout;

        /// <summary>
        /// Frees all system resources for the timer. This method will <see cref="Cancel"/> the timer if it is active.
        /// </summary>
        public void Dispose()
        {
            // Make sure no one gets a surprise notification. :)
            this.Cancel();
            this.context.Dispose();
            this.timer.Dispose();
        }

        /// <summary>
        /// Starts a timeout.
        /// </summary>
        /// <remarks>
        /// <para>The timeout will stop either when <see cref="Reset"/> is called, or when the timeout occurs and <see cref="Timeout"/> is invoked.</para>
        /// </remarks>
        /// <param name="when">The timeout value.</param>
        public void Set(TimeSpan when)
        {
            // Cancel any pending notifications.
            this.Cancel();

            // Set the timer for the requested interval.
            this.timer.Interval = when.TotalMilliseconds;

            // Add the callback to the timer with the current context.
            this.timerElapsedHandler = (sender, e) => this.context.Bind(this.TimerElapsed);
            this.timer.Elapsed += this.timerElapsedHandler;

            // Start the timer.
            this.timer.Start();
        }

        /// <summary>
        /// Re-starts the current timeout. See <see cref="Set"/>.
        /// </summary>
        public void Reset()
        {
            this.Set(TimeSpan.FromMilliseconds(this.timer.Interval));
        }

        /// <summary>
        /// Cancels a timeout.
        /// </summary>
        /// <remarks>
        /// <para>Has no effect if the timeout isn't active.</para>
        /// <para>Once this method is called, <see cref="Timeout"/> will not be invoked until <see cref="Set"/> is called to reactivate the timer.</para>
        /// </remarks>
        public void Cancel()
        {
            // Do nothing if the timer isn't running. Note that "Timer.Enabled" cannot be used to check this because it is possible that the
            //  timer callback has been queued and Timer.Enabled has been set to false.
            if (this.context.Invalidated)
            {
                return;
            }

            // Stop the underlying timer.
            this.timer.Stop();

            // Remove the callback, which hasn't run yet. It may be queued, but TimerElapsed handles that situation.
            this.timer.Elapsed -= this.timerElapsedHandler;

            // Reset the context object; this enables TimerElapsed to detect it shouldn't run.
            this.context.Reset();
        }

        /// <summary>
        /// Handler for the Elapsed event of the underlying Timer.
        /// </summary>
        private void TimerElapsed()
        {
            // Remove this callback from the timer, so that if the user sets the timeout, we won't end up with a duplicate entry.
            this.timer.Elapsed -= this.timerElapsedHandler;

            // Invoke the user-supplied callback, if any.
            if (this.Timeout != null)
            {
                this.Timeout();
            }
        }
    }
}
