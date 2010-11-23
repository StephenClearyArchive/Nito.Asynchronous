// <copyright file="Timer.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Threading;

    /// <summary>
    /// Represents a timer that uses <see cref="SynchronizationContext"/> to synchronize with its creating thread.
    /// </summary>
    /// <remarks>
    /// <para>Timers are initially disabled. They may be enabled by calling <see cref="SetSingleShot"/> or <see cref="SetPeriodic"/>. Alternatively, set <see cref="Interval"/> and <see cref="AutoReset"/>, and then set <see cref="Enabled"/> to true.</para>
    /// <para>Once enabled, a timer may be disabled again by calling <see cref="Cancel"/> or by setting <see cref="Enabled"/> to false.</para>
    /// <para>An enabled timer waits until the time specified by <see cref="Interval"/> has elapsed; at that time, if <see cref="AutoReset"/> is false, the timer becomes disabled. The timer then invokes <see cref="Elapsed"/>. When the <see cref="Elapsed"/> handler returns, if the timer is enabled, it begins waiting again.</para>
    /// <para>Note that periodic timers do not count the time spent in <see cref="Elapsed"/> as part of the wait time.</para>
    /// <para>A timer may be restarted by calling <see cref="Restart"/>, setting <see cref="Interval"/> to its own value, or setting <see cref="Enabled"/> to false and then back to true.</para>
    /// <para>A Timer must be used with a synchronization context that supports <see cref="SynchronizationContextProperties.Synchronized"/>.</para>
    /// </remarks>
    /// <example>The following code sample demonstrates how to construct a periodic Timer, start it, and handle the <see cref="Elapsed"/> event:
    /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\Periodic.cs"/>
    /// The code example above produces this output:
    /// <code lang="None" title="Output">
    /// Timer has fired 1 times.
    /// Timer has fired 2 times.
    /// Timer has fired 3 times.
    /// Timer has fired 4 times.
    /// Timer has fired 5 times.
    /// </code>
    /// The following code sample demonstrates how to construct a single-shot Timer, start it, and handle the <see cref="Elapsed"/> event:
    /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShot.cs"/>
    /// </example>
    public sealed class Timer : IDisposable
    {
        /// <summary>
        /// The captured <see cref="SynchronizationContext"/>.
        /// </summary>
        private SynchronizationContext synchronizationContext;

        /// <summary>
        /// The underlying timer. This is null if the timer is disabled.
        /// </summary>
        private System.Threading.Timer timer;

        /// <summary>
        /// The context for underlying timer callbacks.
        /// </summary>
        private CallbackContext context;

        /// <summary>
        /// Whether or not this timer class is currently executing <see cref="Elapsed"/>.
        /// </summary>
        private bool inElapsed;

        /// <summary>
        /// The backing field for <see cref="Enabled"/> while <see cref="inElapsed"/> is true.
        /// </summary>
        private bool enabledAfterElapsed;

        /// <summary>
        /// The backing field for <see cref="Interval"/> while <see cref="inElapsed"/> is false.
        /// </summary>
        private TimeSpan interval;

        /// <summary>
        /// The backing field for <see cref="Interval"/> while <see cref="inElapsed"/> is true.
        /// </summary>
        private TimeSpan intervalAfterElapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class, binding to <see cref="SynchronizationContext.Current">SynchronizationContext.Current</see>.
        /// </summary>
        /// <example>The following code sample demonstrates how to construct a single-shot Timer, start it, and handle the <see cref="Elapsed"/> event:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShot.cs"/>
        /// </example>
        public Timer()
        {
            // Capture the synchronization context
            this.synchronizationContext = SynchronizationContext.Current;
            if (this.synchronizationContext == null)
            {
                this.synchronizationContext = new SynchronizationContext();
            }

            // Verify that the synchronization context is synchronized
            SynchronizationContextRegister.Verify(this.synchronizationContext.GetType(), SynchronizationContextProperties.Synchronized);

            // Create the context for timer callbacks
            this.context = new CallbackContext();
        }

        /// <summary>
        /// Occurs when the timer's wait time has elapsed.
        /// </summary>
        /// <remarks>
        /// <para>This event is not invoked for disabled timers (see <see cref="Enabled"/>). However, it may be invoked with the <see cref="Enabled"/> property set to false; see below.</para>
        /// <para>If <see cref="AutoReset"/> is true, then <see cref="Enabled"/> remains true when this event is invoked. If <see cref="AutoReset"/> is false, then <see cref="Enabled"/> is set to false immediately before invoking this event.</para>
        /// <para>Handlers for this event may enable/disable the timer or set any properties. These operations will not have an effect until <see cref="Elapsed"/> returns. If <see cref="Elapsed"/> raises an exception, these operations will still apply.</para>
        /// <para>Note that <see cref="AutoReset"/> is not used after <see cref="Elapsed"/> returns. It is only used to determine the value of <see cref="Enabled"/> when <see cref="Elapsed"/> is invoked.</para>
        /// <para>If <see cref="Enabled"/> is true when <see cref="Elapsed"/> returns, then the timer is restarted.</para>
        /// <para><see cref="Elapsed"/> should not raise an exception; if it does, the exception will be passed through to the <see cref="SynchronizationContext"/>. Different <see cref="SynchronizationContext"/> implementations handle this situation differently.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a single-shot Timer, start it, and handle the <see cref="Elapsed"/> event:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShot.cs"/>
        /// </example>
        public event Action Elapsed;

        /// <summary>
        /// Gets or sets a value indicating whether a timer is enabled.
        /// </summary>
        /// <remarks>
        /// <para>Disabled timers do not raise the <see cref="Elapsed"/> event. However, <see cref="Elapsed"/> will be called when <see cref="Enabled"/> is false if <see cref="AutoReset"/> is false (see <see cref="Elapsed"/> for details).</para>
        /// <para>Enabled timers wait approximately the amount of time specified by <see cref="Interval"/>, and then invoke <see cref="Elapsed"/>.</para>
        /// <para>This may be set from within an <see cref="Elapsed"/> handler; however, a timer enabled from the callback will not start waiting until the callback returns. In other words, the time spent processing <see cref="Elapsed"/> is not considered part of <see cref="Interval"/>.</para>
        /// <para>Enabling an already-enabled timer or disabling an already-disabled timer has no effect. Note that these semantics are different than <see cref="Interval"/>.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a single-shot Timer, start it, and handle the <see cref="Elapsed"/> event:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShotProperties.cs"/>
        /// </example>
        public bool Enabled
        {
            get
            {
                if (this.inElapsed)
                {
                    return this.enabledAfterElapsed;
                }
                else
                {
                    return this.timer != null;
                }
            }

            set
            {
                // If we are in the callback, just save the value and return (it will be applied after the callback returns)
                if (this.inElapsed)
                {
                    this.enabledAfterElapsed = value;
                    return;
                }

                // Do nothing if enabling an already-enabled timer, or disabling an already-disabled timer.
                if (this.Enabled == value)
                {
                    return;
                }

                if (value)
                {
                    // Start the timer

                    // Bind the callback to our context and synchronization context
                    Action boundOnTimer = this.context.AsyncBind(this.OnTimer, this.synchronizationContext, false);

                    // The underlying timer delegate (raised on a ThreadPool thread) will first synchronize with the original thread
                    //  using the captured SynchronizationContext. Then it will determine if its binding is still valid and call OnTimer
                    //  if it's OK. OnTimer only handles the user callback logic.
                    this.timer = new System.Threading.Timer((state) => boundOnTimer(), null, this.interval, TimeSpan.FromMilliseconds(-1));

                    // Inform the synchronization context that there is an active asynchronous operation
                    this.synchronizationContext.OperationStarted();
                }
                else
                {
                    // Stop the underlying timer
                    this.context.Reset();
                    this.timer.Dispose();
                    this.timer = null;

                    // Inform the synchronization context that the asynchronous operation has completed
                    this.synchronizationContext.OperationCompleted();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the timer should become enabled again by default when <see cref="Elapsed"/> returns.
        /// </summary>
        /// <remarks>
        /// <para>See <see cref="Elapsed"/> for details of how this property is used by the timer.</para>
        /// <para>Setting this property does not modify the <see cref="Enabled"/> property; to set <see cref="Interval"/>, <see cref="AutoReset"/>, and <see cref="Enabled"/> simultaneously, call <see cref="SetSingleShot"/> or <see cref="SetPeriodic"/>.</para>
        /// <para>This may be set from within an <see cref="Elapsed"/> handler, but will not have an effect until the next period has elapsed. To re-enable a timer from within an <see cref="Elapsed"/> handler, set <see cref="Enabled"/> to true.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a single-shot Timer, start it, and handle the <see cref="Elapsed"/> event:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShotProperties.cs"/>
        /// </example>
        public bool AutoReset { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the approximate time the timer will wait before invoking <see cref="Elapsed"/>. The interval must be equal to a positive number of milliseconds in the range [0, <see cref="Int32.MaxValue" qualifyHint="true"/>].
        /// </summary>
        /// <remarks>
        /// <para>The interval may be 0; in this case, the timer will immediately queue <see cref="Elapsed"/> to be run.</para>
        /// <para>Setting this property will cancel any pending timeouts and restart the timer with the new interval. This is true even if the new value is the same as the old value.</para>
        /// <para>Setting this property does not modify the <see cref="Enabled"/> property; to set <see cref="Interval"/>, <see cref="AutoReset"/>, and <see cref="Enabled"/> simultaneously, call <see cref="SetSingleShot"/> or <see cref="SetPeriodic"/>.</para>
        /// <para>This may be set from within an <see cref="Elapsed"/> handler.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">When setting this property, the new interval value is either negative or greater than <see cref="Int32.MaxValue" qualifyHint="true"/> milliseconds.</exception>
        /// <example>The following code sample demonstrates how to construct a single-shot Timer, start it, and handle the <see cref="Elapsed"/> event:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShotProperties.cs"/>
        /// </example>
        public TimeSpan Interval
        {
            get
            {
                if (this.inElapsed)
                {
                    return this.intervalAfterElapsed;
                }
                else
                {
                    return this.interval;
                }
            }

            set
            {
                // Ensure the interval is in the correct range
                if (value < TimeSpan.Zero || value > TimeSpan.FromMilliseconds(int.MaxValue))
                {
                    throw new ArgumentOutOfRangeException("Interval", "Interval must be equal to a number of milliseconds in the range [0, Int32.MaxValue].");
                }

                // If we are in the callback, just save the value and return (it will be applied after the callback returns)
                if (this.inElapsed)
                {
                    this.intervalAfterElapsed = value;
                    return;
                }

                // If the timer is already running, then stop it, set the time, and restart it.
                if (this.Enabled)
                {
                    this.Enabled = false;
                    this.interval = value;
                    this.Enabled = true;
                    return;
                }

                // The timer is not already running, so we can just directly set the value.
                this.interval = value;
            }
        }

        /// <summary>
        /// Sets the timer to wait for an interval.
        /// </summary>
        /// <param name="interval">The interval to wait.</param>
        /// <remarks>
        /// <para>After this method returns, <see cref="AutoReset"/> is false, <see cref="Interval"/> is <paramref name="interval"/>, and <see cref="Enabled"/> is true.</para>
        /// <para>When the wait completes, <see cref="Elapsed"/> is invoked.</para>
        /// <para>This function cancels any previous pending timeouts.</para>
        /// <para>Calling this method from <see cref="Elapsed"/> is allowed.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a single-shot Timer, start it, and handle the <see cref="Elapsed"/> event:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShot.cs"/>
        /// </example>
        public void SetSingleShot(TimeSpan interval)
        {
            // Only public properties are used here to allow this function to be called from Elapsed
            this.Enabled = false;
            this.AutoReset = false;
            this.Interval = interval;
            this.Enabled = true;
        }

        /// <summary>
        /// Sets the timer to periodically wait for an interval.
        /// </summary>
        /// <param name="period">The period to wait.</param>
        /// <remarks>
        /// <para>After this method returns, <see cref="AutoReset"/> is true, <see cref="Interval"/> is <paramref name="period"/>, and <see cref="Enabled"/> is true.</para>
        /// <para>Note that the "period" does not include the time spent processing <see cref="Elapsed"/>.</para>
        /// <para>When the wait completes, <see cref="Elapsed"/> is invoked. When <see cref="Elapsed"/> returns (assuming <see cref="AutoReset"/> and <see cref="Enabled"/> are still true), the timer will begin another wait of <see cref="Interval"/> length.</para>
        /// <para>This function cancels any previous pending timeouts.</para>
        /// <para>Calling this method from <see cref="Elapsed"/> is allowed.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a periodic Timer, start it, and handle the <see cref="Elapsed"/> event:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\Periodic.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// Timer has fired 1 times.
        /// Timer has fired 2 times.
        /// Timer has fired 3 times.
        /// Timer has fired 4 times.
        /// Timer has fired 5 times.
        /// </code>
        /// </example>
        public void SetPeriodic(TimeSpan period)
        {
            // Only public properties are used here to allow this function to be called from Elapsed
            this.Enabled = false;
            this.AutoReset = true;
            this.Interval = period;
            this.Enabled = true;
        }

        /// <summary>
        /// Cancels any pending timeouts.
        /// </summary>
        /// <remarks>
        /// <para>After this method returns, <see cref="Enabled"/> is false.</para>
        /// <para>It is not necessary to call this function before calling <see cref="Dispose"/>.</para>
        /// <para>Calling this method from <see cref="Elapsed"/> is allowed.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a periodic Timer, start it, restart it, and cancel it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShotRestartCancel.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// Timer has fired 1 times.
        /// Timer has fired 2 times.
        /// Timer has fired 3 times.
        /// Timer has fired 4 times.
        /// Timer has fired 5 times.
        /// </code>
        /// </example>
        public void Cancel()
        {
            // Only public properties are used here to allow this function to be called from Elapsed
            this.Enabled = false;
        }

        /// <summary>
        /// Frees all resources used by this timer.
        /// </summary>
        /// <remarks>
        /// <para>Calling this method from <see cref="Elapsed"/> is allowed.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a single-shot Timer, start it, and dispose it before it elapses:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShotDispose.cs"/>
        /// </example>
        public void Dispose()
        {
            this.Enabled = false;
            this.context.Dispose();
        }

        /// <summary>
        /// Disables and then enables the timer, restarting the wait time.
        /// </summary>
        /// <remarks>
        /// <para>Calling this method from <see cref="Elapsed"/> is allowed; in this case, it has the same effect as setting <see cref="Enabled"/> to true.</para>
        /// </remarks>
        /// <example>The following code sample demonstrates how to construct a periodic Timer, start it, restart it, and cancel it:
        /// <code source="..\..\Source\Examples\DocumentationExamples\Timer\SingleShotRestartCancel.cs"/>
        /// The code example above produces this output:
        /// <code lang="None" title="Output">
        /// Timer has fired 1 times.
        /// Timer has fired 2 times.
        /// Timer has fired 3 times.
        /// Timer has fired 4 times.
        /// Timer has fired 5 times.
        /// </code>
        /// </example>
        public void Restart()
        {
            this.Enabled = false;
            this.Enabled = true;
        }

        /// <summary>
        /// Handles a timer event from the underlying timer.
        /// </summary>
        private void OnTimer()
        {
            // When this is called, we have already been synchronized with the original thread and our context is valid (see Enabled.set() for details).

            // Copy properties for use from within the callback; these are the "default values"
            this.enabledAfterElapsed = this.AutoReset;
            this.intervalAfterElapsed = this.interval;

            // Set the flag indicating we're in the callback
            this.inElapsed = true;

            // Call the callback
            try
            {
                if (this.Elapsed != null)
                {
                    this.Elapsed();
                }
            }
            finally
            {
                // Reset "in callback" flag
                this.inElapsed = false;

                // Apply all variables that may have been set in the callback
                this.interval = this.intervalAfterElapsed;
                if (!this.enabledAfterElapsed)
                {
                    // Destroy the underlying timer
                    this.timer.Dispose();
                    this.timer = null;
                }
                else
                {
                    // Since the timer is enabled (either single-shot or periodic, we don't care), and since it has already elapsed, we can just
                    //  re-use the underlying timer object and context instead of re-creating them at this point.
                    this.timer.Change(this.interval, TimeSpan.FromMilliseconds(-1));
                }
            }
        }
    }
}
