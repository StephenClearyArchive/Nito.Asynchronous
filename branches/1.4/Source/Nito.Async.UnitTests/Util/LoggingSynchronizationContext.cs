using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UnitTests.Util
{
    public sealed class LoggingSynchronizationContext : SynchronizationContext
    {
        private SynchronizationContext synchronizationContext;

        public LoggingSynchronizationContext(SynchronizationContext synchronizationContext)
        {
            this.synchronizationContext = synchronizationContext;
        }

        public Action OnOperationCompleted { get; set; }
        public override void OperationCompleted()
        {
            if (this.OnOperationCompleted != null)
                this.OnOperationCompleted();
            this.synchronizationContext.OperationCompleted();
        }

        public Action OnOperationStarted { get; set; }
        public override void OperationStarted()
        {
            if (this.OnOperationStarted != null)
                this.OnOperationStarted();
            this.synchronizationContext.OperationStarted();
        }

        public Action OnPost { get; set; }
        public override void Post(SendOrPostCallback d, object state)
        {
            if (this.OnPost != null)
                this.OnPost();
            this.synchronizationContext.Post(d, state);
        }

        public Action OnSend { get; set; }
        public override void Send(SendOrPostCallback d, object state)
        {
            if (this.OnSend != null)
                this.OnSend();
            this.synchronizationContext.Send(d, state);
        }
    }
}
