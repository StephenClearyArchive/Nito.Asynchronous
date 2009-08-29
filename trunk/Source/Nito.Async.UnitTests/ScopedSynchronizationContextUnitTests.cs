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
            }

            Assert.AreSame(oldSyncContext, SynchronizationContext.Current, "ScopedSynchronizationContext did not restore the old SynchronizationContext.Current");
        }
    }
}
