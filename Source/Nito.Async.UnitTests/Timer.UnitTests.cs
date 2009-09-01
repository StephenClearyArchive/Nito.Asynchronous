using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class TimerUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_SynchronizationContextNotSupportingSynchronized_ThrowsInvalidOperationException()
        {
            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(null))
            {
                using (Timer timer = new Timer())
                {
                }
            }
        }

        [TestMethod]
        public void Enabled_AfterConstruction_IsFalse()
        {
            bool enabled = true;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                    {
                        using (Timer timer = new Timer())
                        {
                            enabled = timer.Enabled;
                        }
                    });
            }

            Assert.IsFalse(enabled, "Timer.Enabled should initially be false");
        }
    }
}
