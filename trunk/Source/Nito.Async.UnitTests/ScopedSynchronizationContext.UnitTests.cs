using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace UnitTests
{
    [TestClass]
    public class ScopedSynchronizationContextUnitTests
    {
        [TestMethod]
        public void TestBasicFunctionality()
        {
            var oldSyncContext = SynchronizationContext.Current;
            var newSyncContext = new SynchronizationContext();

            using (var x = new ScopedSynchronizationContext(newSyncContext))
            {
                Assert.AreSame(newSyncContext, SynchronizationContext.Current, "ScopedSynchronizationContext did not replace SynchronizationContext.Current");
                Assert.AreSame(oldSyncContext, x.PreviousContext, "ScopedSynchronizationContext did not save the old SynchronizationContext");
            }

            Assert.AreSame(oldSyncContext, SynchronizationContext.Current, "ScopedSynchronizationContext did not restore the old SynchronizationContext.Current");
        }

        [TestMethod]
        public void TestWithNullSynchronizationContexts()
        {
            using (var x = new ScopedSynchronizationContext(null))
            {
                using (var y = new ScopedSynchronizationContext(new SynchronizationContext()))
                {
                    Assert.IsNull(y.PreviousContext);
                }

                Assert.IsNull(SynchronizationContext.Current);
            }
        }

        [TestMethod]
        public void TestNestedFunctionality()
        {
            using (var y = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                var oldSyncContext = SynchronizationContext.Current;
                var newSyncContext = new SynchronizationContext();

                Assert.IsNotNull(oldSyncContext, "ScopedSynchronizationContext replaced SynchronizationContext() with null");

                using (var x = new ScopedSynchronizationContext(newSyncContext))
                {
                    Assert.AreSame(newSyncContext, SynchronizationContext.Current, "ScopedSynchronizationContext did not replace SynchronizationContext.Current");
                    Assert.AreSame(oldSyncContext, x.PreviousContext, "ScopedSynchronizationContext did not save the old SynchronizationContext");
                }

                Assert.AreSame(oldSyncContext, SynchronizationContext.Current, "ScopedSynchronizationContext did not restore the old SynchronizationContext.Current");
            }
        }
    }
}
