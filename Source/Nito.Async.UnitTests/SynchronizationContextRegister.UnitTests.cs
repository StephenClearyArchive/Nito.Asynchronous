using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace UnitTests
{
    [TestClass]
    public class SynchronizationContextRegisterUnitTests
    {
        [TestMethod]
        public void ThreadPoolSyncContext_HasNonreentrantPost()
        {
            using (var x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost);
            }
        }

        [TestMethod]
        public void NullSynchContext_HasNonreentrantPost()
        {
            using (var x = new ScopedSynchronizationContext(null))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost);
            }
        }

        [TestMethod]
        public void WindowsFormsSyncContext_HasStandard()
        {
            using (var x = new ScopedSynchronizationContext(new WindowsFormsSynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.Standard);
            }
        }

        [TestMethod]
        public void DispatcherSyncContext_HasStandard()
        {
            using (var x = new ScopedSynchronizationContext(new DispatcherSynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.Standard);
            }
        }

        [TestMethod]
        public void RegisteredSyncContext_HasRegisteredProperty()
        {
            SynchronizationContextRegister.Register(typeof(MySynchronizationContext), SynchronizationContextProperties.NonReentrantPost);

            using (var x = new ScopedSynchronizationContext(new MySynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisteredSyncContext_ReregisteringProperties_DoesNotMergeWithOldValue()
        {
            SynchronizationContextRegister.Register(typeof(MyOtherSynchronizationContext), SynchronizationContextProperties.NonReentrantPost);
            SynchronizationContextRegister.Register(typeof(MyOtherSynchronizationContext), SynchronizationContextProperties.NonReentrantSend);

            using (var x = new ScopedSynchronizationContext(new MyOtherSynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost);
            }
        }

        [TestMethod]
        public void RegisteredSyncContext_ReregisteringProperties_ReplacesOldValue()
        {
            SynchronizationContextRegister.Register(typeof(MyThirdSynchronizationContext), SynchronizationContextProperties.NonReentrantPost);
            SynchronizationContextRegister.Register(typeof(MyThirdSynchronizationContext), SynchronizationContextProperties.NonReentrantSend);

            using (var x = new ScopedSynchronizationContext(new MyThirdSynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantSend);
            }
        }

        private sealed class MySynchronizationContext : SynchronizationContext
        {
        }

        private sealed class MyOtherSynchronizationContext : SynchronizationContext
        {
        }

        private sealed class MyThirdSynchronizationContext : SynchronizationContext
        {
        }
    }
}
