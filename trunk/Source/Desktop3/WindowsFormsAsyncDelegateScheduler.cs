namespace Nito.Communication
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// A delegate scheduler that queues the delegate to a Windows Forms message loop.
    /// </summary>
    public sealed class WindowsFormsAsyncDelegateScheduler : IAsyncDelegateScheduler
    {
        /// <summary>
        /// The control used to schedule the delegate
        /// </summary>
        private readonly Control control;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsFormsAsyncDelegateScheduler"/> class. The calling thread must be an STA thread which does message pumping (e.g., a Windows Forms GUI thread).
        /// </summary>
        public WindowsFormsAsyncDelegateScheduler()
        {
            // Ensure that there is a Windows Forms context by creating a Win32 control handle.
            this.control = new Control();
            var junk = this.control.Handle;
        }

        /// <summary>
        /// Schedules the specified delegate to execute in a Windows Forms message loop.
        /// </summary>
        /// <param name="action">The delegate to schedule.</param>
        public void Schedule(Action action)
        {
            this.control.BeginInvoke(action);
        }
    }
}
