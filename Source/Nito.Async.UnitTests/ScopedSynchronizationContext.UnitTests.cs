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
        public void CurrentSyncContext_WithinScopedSyncContext_IsReplaced()
        {
            var newSyncContext = new SynchronizationContext();

            using (var x = new ScopedSynchronizationContext(newSyncContext))
            {
                Assert.AreSame(newSyncContext, SynchronizationContext.Current, "ScopedSynchronizationContext did not replace SynchronizationContext.Current");
            }
        }

        [TestMethod]
        public void PreviousSyncContext_WithinScopedSyncContext_IsPreserved()
        {
            var oldSyncContext = SynchronizationContext.Current;

            using (var x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                Assert.AreSame(oldSyncContext, x.PreviousContext, "ScopedSynchronizationContext did not save the old SynchronizationContext");
            }
        }

        [TestMethod]
        public void CurrentSyncContext_AfterScopedSyncContext_IsRestored()
        {
            var oldSyncContext = SynchronizationContext.Current;

            using (var x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
            }

            Assert.AreSame(oldSyncContext, SynchronizationContext.Current, "ScopedSynchronizationContext did not restore the old SynchronizationContext.Current");
        }

        [TestMethod]
        public void CurrentSyncContext_WithinNullScopedSyncContext_IsNull()
        {
            using (var x = new ScopedSynchronizationContext(null))
            {
                Assert.IsNull(SynchronizationContext.Current, "ScopedSynchronizationContext did not replace SynchronizationContext.Current");
            }
        }

        [TestMethod]
        public void PreviousNullSyncContext_WithinScopedSyncContext_IsPreserved()
        {
            using (var x = new ScopedSynchronizationContext(null))
            {
                using (var y = new ScopedSynchronizationContext(new SynchronizationContext()))
                {
                    Assert.IsNull(y.PreviousContext, "ScopedSynchronizationContext did not save the old null SynchronizationContext.Current");
                }
            }
        }
    }
}
