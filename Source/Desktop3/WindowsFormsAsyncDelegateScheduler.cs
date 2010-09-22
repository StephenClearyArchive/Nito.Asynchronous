namespace Nito.Communication
{
    using System;
    using System.Windows.Forms;

    public sealed class WindowsFormsAsyncDelegateScheduler : IAsyncDelegateScheduler
    {
        private readonly Control control;

        public WindowsFormsAsyncDelegateScheduler()
        {
            this.control = new Control();
            var junk = this.control.Handle;
        }

        public void Schedule(Action action)
        {
            this.control.BeginInvoke(action);
        }
    }
}
