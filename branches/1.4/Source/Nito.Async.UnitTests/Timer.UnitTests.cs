using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using Timer = Nito.Async.Timer;

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
            bool enabled = false;

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

        [TestMethod]
        public void Timer_AfterSetPeriodic_IsEnabled()
        {
            bool enabled = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetPeriodic(TimeSpan.FromMilliseconds(50));
                        enabled = timer.Enabled;
                    }
                });
            }

            Assert.IsTrue(enabled, "Timer.Enabled should be true");
        }

        [TestMethod]
        public void TimerType_AfterSetPeriodic_IsPeriodic()
        {
            bool autoReset = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetPeriodic(TimeSpan.FromMilliseconds(50));
                        autoReset = timer.AutoReset;
                    }
                });
            }

            Assert.IsTrue(autoReset, "Timer should be periodic");
        }

        [TestMethod]
        public void TimerInterval_AfterSetPeriodic_IsSetToArgument()
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

        [TestMethod]
        public void Timer_AfterCancel_IsDisabled()
        {
            bool enabled = true;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetPeriodic(TimeSpan.FromMilliseconds(50));
                        timer.Cancel();
                        enabled = timer.Enabled;
                    }
                });
            }

            Assert.IsFalse(enabled, "Timer.Enabled should be false");
        }

        [TestMethod]
        public void Timer_AfterRestart_IsEnabled()
        {
            bool enabled = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetPeriodic(TimeSpan.FromMilliseconds(50));
                        timer.Restart();
                        enabled = timer.Enabled;
                    }
                });
            }

            Assert.IsTrue(enabled, "Timer.Enabled should be true");
        }

        [TestMethod]
        public void SingleShotTimer_Elapsed_InvokesElapsedExactlyOnce()
        {
            int actionCount = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () => { ++actionCount; };
                    timer.AutoReset = false;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.AreEqual(1, actionCount, "Timer did not run Elapsed exactly once");
        }

        [TestMethod]
        public void PeriodicTimer_Elapsed_InvokesElapsedMoreThanOnce()
        {
            int actionCount = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () => { ++actionCount; };
                    timer.AutoReset = true;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.IsTrue(actionCount > 1, "Timer did not run Elapsed more than once");
        }

        [TestMethod]
        public void SingleShotTimer_Elapsed_CanRestartTimer()
        {
            int actionCount = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        if (actionCount == 0)
                        {
                            timer.Restart();
                        }
                        ++actionCount;
                    };
                    timer.AutoReset = false;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.AreEqual(2, actionCount, "Timer did not honor Restart when called from Elapsed");
        }

        [TestMethod]
        public void PeriodicTimer_Elapsed_CanCancelTimer()
        {
            int actionCount = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        ++actionCount;
                        timer.Cancel();
                    };
                    timer.AutoReset = true;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.AreEqual(1, actionCount, "Timer did not honor Cancel when called from Elapsed");
        }

        [TestMethod]
        public void SingleShotTimer_Elapsed_IsDisabled()
        {
            bool enabled = true;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        enabled = timer.Enabled;
                    };
                    timer.AutoReset = false;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.IsFalse(enabled, "Single-shot Timer should be disabled when called from Elapsed");
        }

        [TestMethod]
        public void PeriodicTimer_Elapsed_IsEnabled()
        {
            bool enabled = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        enabled = timer.Enabled;
                        timer.Cancel();
                    };
                    timer.AutoReset = true;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.IsTrue(enabled, "Periodic Timer should be enabled when called from Elapsed");
        }

        [TestMethod]
        public void SingleShotTimer_Elapsed_CanChangeInterval()
        {
            TimeSpan interval = default(TimeSpan);

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        timer.Interval = TimeSpan.FromMilliseconds(1);
                        interval = timer.Interval;
                    };
                    timer.AutoReset = false;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.AreEqual(TimeSpan.FromMilliseconds(1), interval, "Interval should be honored when called from Elapsed");
        }

        [TestMethod]
        public void PeriodicTimer_Elapsed_CanChangeInterval()
        {
            TimeSpan interval = default(TimeSpan);

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        timer.Interval = TimeSpan.FromMilliseconds(1);
                        interval = timer.Interval;
                        timer.Cancel();
                    };
                    timer.AutoReset = true;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.AreEqual(TimeSpan.FromMilliseconds(1), interval, "Interval should be honored when called from Elapsed");
        }

        [TestMethod]
        public void SingleShotTimer_Elapsed_CanChangeToPeriodic()
        {
            bool autoReset = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        timer.SetPeriodic(timer.Interval);
                        autoReset = timer.AutoReset;
                        timer.Cancel();
                    };
                    timer.AutoReset = false;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.IsTrue(autoReset, "Single-shot Timer should be able to change to Periodic within Elapsed");
        }

        [TestMethod]
        public void PeriodicTimer_Elapsed_CanChangeToSingleShot()
        {
            bool autoReset = true;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Timer timer = null;
                thread.DoSynchronously(() =>
                {
                    timer = new Timer();
                    timer.Elapsed += () =>
                    {
                        timer.SetSingleShot(timer.Interval);
                        autoReset = timer.AutoReset;
                        timer.Cancel();
                    };
                    timer.AutoReset = true;
                    timer.Interval = TimeSpan.FromMilliseconds(0);
                    timer.Enabled = true;
                });
                Thread.Sleep(10);
                thread.DoSynchronously(() => timer.Dispose());
            }

            Assert.IsFalse(autoReset, "Periodic Timer should be able to change to Single-shot within Elapsed");
        }

        [TestMethod]
        public void Timer_ElapsedAfterDisposed_DoesNotInvokeElapsed()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.Elapsed += () => { sawAction = true; };
                        timer.SetSingleShot(TimeSpan.FromMilliseconds(0));
                        Thread.Sleep(10);
                    }
                });
            }

            Assert.IsFalse(sawAction, "Disposed timer invoked Elapsed");
        }

        [TestMethod]
        public void Timer_Running_CanChangeInterval()
        {
            TimeSpan interval = default(TimeSpan);

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() =>
                {
                    using (Timer timer = new Timer())
                    {
                        timer.SetSingleShot(TimeSpan.FromMilliseconds(0));
                        timer.Interval = TimeSpan.FromMilliseconds(1);
                        interval = timer.Interval;
                    }
                });
            }

            Assert.AreEqual(TimeSpan.FromMilliseconds(1), interval, "Interval should be settable while the timer is running");
        }
    }
}
