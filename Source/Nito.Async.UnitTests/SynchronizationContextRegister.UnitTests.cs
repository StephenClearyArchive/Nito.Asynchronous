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
        public void TestThreadPoolSynchronizationContextProperties()
        {
            using (var x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost);
            }

            using (var x = new ScopedSynchronizationContext(null))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost);
            }
        }

        [TestMethod]
        public void TestWindowsFormsSynchronizationContextProperties()
        {
            using (var x = new ScopedSynchronizationContext(new WindowsFormsSynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.Standard);
            }
        }

        [TestMethod]
        public void TestDispatcherSynchronizationContextProperties()
        {
            using (var x = new ScopedSynchronizationContext(new DispatcherSynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.Standard);
            }
        }

        [TestMethod]
        public void TestRegisteredSynchronizationContextProperties()
        {
            SynchronizationContextRegister.Register(typeof(MySynchronizationContext), SynchronizationContextProperties.NonReentrantPost);

            using (var x = new ScopedSynchronizationContext(new MySynchronizationContext()))
            {
                SynchronizationContextRegister.Verify(SynchronizationContextProperties.NonReentrantPost);
            }
        }

        private sealed class MySynchronizationContext : SynchronizationContext
        {
        }
    }
}
