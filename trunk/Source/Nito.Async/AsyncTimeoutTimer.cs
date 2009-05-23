using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

// Copyright 2009 by Nito Programs.

namespace Nito.Async
{
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
        private System.Timers.Timer Timer;

        /// <summary>
        /// The current context of timer callbacks. This is necessary because it is possible for the user to <see cref="Reset"/> a timer that
        /// has already gone off and has queued a <see cref="TimerElapsed"/> callback.
        /// </summary>
        private CallbackContext Context;

        /// <summary>
        /// The callback for the timer, bound to the <see cref="Context"/> that was current when the callback was set. This delegate is saved so
        /// that the callbacks may be removed and replaced when the context changes.
        /// </summary>
        private ElapsedEventHandler TimerElapsedHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTimeoutTimer"/> class.
        /// </summary>
        public AsyncTimeoutTimer()
        {
            Timer = new System.Timers.Timer();
            Timer.AutoReset = false;
            Timer.SynchronizingObject = new GenericSynchronizingObject();
            Context = new CallbackContext();
        }

        /// <summary>
        /// Frees all system resources for the timer. This method will <see cref="Cancel"/> the timer if it is active.
        /// </summary>
        public void Dispose()
        {
            // Make sure no one gets a surprise notification. :)
            Cancel();
            Context.Dispose();
            Timer.Dispose();
        }

        private void TimerElapsed()
        {
            // Remove this callback from the timer, so that if the user sets the timeout, we won't end up with a duplicate entry.
            Timer.Elapsed -= TimerElapsedHandler;

            // Invoke the user-supplied callback, if any.
            if (Timeout != null)
                Timeout();
        }

        /// <summary>
        /// Notifies that a timeout has occurred.
        /// </summary>
        /// <remarks>
        /// <para>The timeout has been stopped by the time this is invoked.</para>
        /// </remarks>
        public event Action Timeout;

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
            Cancel();

            // Set the timer for the requested interval.
            Timer.Interval = when.TotalMilliseconds;

            // Add the callback to the timer with the current context.
            TimerElapsedHandler = (sender, e) => Context.Bind(TimerElapsed);
            Timer.Elapsed += TimerElapsedHandler;

            // Start the timer.
            Timer.Start();
        }

        /// <summary>
        /// Re-starts the current timeout. See <see cref="Set"/>.
        /// </summary>
        public void Reset()
        {
            Set(TimeSpan.FromMilliseconds(Timer.Interval));
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
            if (Context.Invalidated)
                return;

            // Stop the underlying timer.
            Timer.Stop();

            // Remove the callback, which hasn't run yet. It may be queued, but TimerElapsed handles that situation.
            Timer.Elapsed -= TimerElapsedHandler;

            // Reset the context object; this enables TimerElapsed to detect it shouldn't run.
            Context.Reset();
        }
    }
}
