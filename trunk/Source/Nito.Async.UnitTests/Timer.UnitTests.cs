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
        public void Constructor_SyncContextNotSupportingSynchronized_ThrowsInvalidOperationException()
        {
            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(null))
            {
                using (Timer timer = new Timer())
                {
                }
            }
        }

        [TestMethod]
        public void Timer_AfterConstruction_IsDisabled()
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

        [TestMethod]
        public void Timer_AfterSetSingleShot_IsEnabled()
        {
            bool enabled = true;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetSingleShot(TimeSpan.FromMilliseconds(50));
                        enabled = timer.Enabled;
                    }
                });
            }

            Assert.IsTrue(enabled, "Timer.Enabled should be true");
        }

        [TestMethod]
        public void TimerType_AfterSetSingleShot_IsSingleShot()
        {
            bool autoReset = true;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetSingleShot(TimeSpan.FromMilliseconds(50));
                        autoReset = timer.AutoReset;
                    }
                });
            }

            Assert.IsFalse(autoReset, "Timer should be single-shot");
        }

        [TestMethod]
        public void TimerInterval_AfterSetSingleShot_IsSetToArgument()
        {
            TimeSpan interval = default(TimeSpan);

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetSingleShot(TimeSpan.FromMilliseconds(50));
                        interval = timer.Interval;
                    }
                });
            }

            Assert.AreEqual(TimeSpan.FromMilliseconds(50), interval, "Timer should have its Interval set");
        }
    }
}
