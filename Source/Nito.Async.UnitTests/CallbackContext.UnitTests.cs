using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.ComponentModel;

namespace UnitTests
{
    [TestClass]
    public class CallbackContextUnitTests
    {
        [TestMethod]
        public void TestActionBindAndRun()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true; });
                Assert.IsFalse(sawAction, "Bind should not execute action");

                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
                action();
                Assert.IsTrue(sawAction, "Bound action did not execute");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestActionBindAndReset()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true; });
                Assert.IsFalse(sawAction, "Bind should not execute action");

                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
                Assert.IsFalse(sawAction, "Reset should not execute action");

                action();
                Assert.IsFalse(sawAction, "Invalid action did execute");
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void TestActionBindAndDispose()
        {
            bool sawAction = false;
            Action action = null;

            using (CallbackContext context = new CallbackContext())
            {
                action = context.Bind(() => { sawAction = true; });
                Assert.IsFalse(sawAction, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }

            Assert.IsFalse(sawAction, "Dispose should not execute action");

            action();
            Assert.IsFalse(sawAction, "Invalid action did execute");
        }

        [TestMethod]
        public void TestMultipleActionsWithReset()
        {
            bool sawAction1 = false;
            bool sawAction2 = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action1 = context.Bind(() => { sawAction1 = true; });
                var action2 = context.Bind(() => { sawAction2 = true; });
                Assert.IsFalse(sawAction1, "Bind should not execute action");
                Assert.IsFalse(sawAction2, "Bind should not execute action");

                Assert.IsFalse(context.Invalidated, "Bound actions should be valid");
                context.Reset();
                Assert.IsFalse(sawAction1, "Reset should not execute action");
                Assert.IsFalse(sawAction2, "Reset should not execute action");
                Assert.IsTrue(context.Invalidated, "Bound actions should be invalid");

                action1();
                action2();
                Assert.IsFalse(sawAction1, "Invalid action did execute");
                Assert.IsFalse(sawAction2, "Invalid action did execute");
                Assert.IsTrue(context.Invalidated, "Bound actions should be invalid");
            }
        }

        [TestMethod]
        public void TestActionSyncContextBindAndRun()
        {
            SynchronizationContext actionThreadSyncContext = null;
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { actionThreadSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, actionThreadSyncContext);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                action();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestActionSyncContextUsage()
        {
            SynchronizationContext actionThreadSyncContext = null;
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;
            bool sawOperationCompleted = false;
            bool sawOperationStarted = false;
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { actionThreadSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationCompleted = () => { sawOperationCompleted = true; },
                    OnOperationStarted = () => { sawOperationStarted = true; },
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, syncContext, false);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(sawOperationStarted, "Bind should not start operation");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                action();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(sawOperationStarted, "Context incremented operation count");
                Assert.IsFalse(sawOperationCompleted, "Context decremented operation count");
                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestActionSyncContextWithReset()
        {
            SynchronizationContext actionThreadSyncContext = null;
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;
            bool sawOperationCompleted = false;
            bool sawOperationStarted = false;
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { actionThreadSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationCompleted = () => { sawOperationCompleted = true; },
                    OnOperationStarted = () => { sawOperationStarted = true; },
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, syncContext, false);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(sawOperationStarted, "Bind should not start operation");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
                action();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action should not run");
                Assert.IsFalse(sawOperationStarted, "Context incremented operation count");
                Assert.IsFalse(sawOperationCompleted, "Context decremented operation count");
                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestActionDefaultSyncContextBindAndRun()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); }, new SynchronizationContext());
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                action();
                Assert.IsTrue(evt.WaitOne(100), "Action did not run on ThreadPool");
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestActionDefaultSyncContextSkippingTestBindAndRun()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); }, new SynchronizationContext(), false);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                action();
                Assert.IsTrue(evt.WaitOne(100), "Action did not run on ThreadPool");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestActionSyncObjectBindAndRun()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var syncObject = new FakeActionSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); }, syncObject);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                action();
                Assert.IsTrue(evt.WaitOne(100), "Action did not run on ThreadPool");
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestActionCurrentSyncObjectBindAndRun()
        {
            int sawActionThread = ~Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var syncObject = new FakeActionSynchronizingObject(false);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); }, syncObject);
                Assert.AreEqual(~Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                action();
                Assert.IsTrue(evt.WaitOne(0), "Action did not run synchronously");
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(syncObject.sawInvoke, "Bound action did run through synchronizing object");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestActionSyncObjectWithReset()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var syncObject = new FakeActionSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); }, syncObject);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
                action();
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was executed");
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void TestFuncBindAndRun()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true; return 13; });
                Assert.IsFalse(sawAction, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
                Assert.IsTrue(sawAction, "Bound action did not execute");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestFuncBindAndReset()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true; return 13; });
                Assert.IsFalse(sawAction, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                context.Reset();
                Assert.IsFalse(sawAction, "Reset should not execute action");
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");

                int result = action();
                Assert.AreEqual(0, result, "Func result seen");
                Assert.IsFalse(sawAction, "Invalid action did execute");
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void TestFuncBindAndDispose()
        {
            bool sawAction = false;
            Func<int> action = null;

            using (CallbackContext context = new CallbackContext())
            {
                action = context.Bind(() => { sawAction = true; return 13; });
                Assert.IsFalse(sawAction, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }

            Assert.IsFalse(sawAction, "Dispose should not execute action");

            int result = action();
            Assert.AreEqual(0, result, "Func result seen");
            Assert.IsFalse(sawAction, "Invalid action did execute");
        }

        [TestMethod]
        public void TestMultipleFuncsWithReset()
        {
            bool sawAction1 = false;
            bool sawAction2 = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action1 = context.Bind(() => { sawAction1 = true; return 13; });
                var action2 = context.Bind(() => { sawAction2 = true; return 17; });
                Assert.IsFalse(sawAction1, "Bind should not execute action");
                Assert.IsFalse(sawAction2, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound actions should be valid");

                context.Reset();
                Assert.IsFalse(sawAction1, "Reset should not execute action");
                Assert.IsFalse(sawAction2, "Reset should not execute action");
                Assert.IsTrue(context.Invalidated, "Bound actions should be invalid");

                int result1 = action1();
                int result2 = action2();
                Assert.AreEqual(0, result1, "Func result returned");
                Assert.AreEqual(0, result2, "Func result returned");
                Assert.IsFalse(sawAction1, "Invalid action did execute");
                Assert.IsFalse(sawAction2, "Invalid action did execute");
                Assert.IsTrue(context.Invalidated, "Bound actions should be invalid");
            }
        }

        [TestMethod]
        public void TestFuncSyncContextBindAndRun()
        {
            SynchronizationContext actionThreadSyncContext = null;
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { actionThreadSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, actionThreadSyncContext);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestFuncSyncContextUsage()
        {
            SynchronizationContext actionThreadSyncContext = null;
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;
            bool sawOperationCompleted = false;
            bool sawOperationStarted = false;
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { actionThreadSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationCompleted = () => { sawOperationCompleted = true; },
                    OnOperationStarted = () => { sawOperationStarted = true; },
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, syncContext, false);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(sawOperationStarted, "Bind should not start operation");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(sawOperationStarted, "Context incremented operation count");
                Assert.IsFalse(sawOperationCompleted, "Context decremented operation count");
                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestFuncSyncContextWithReset()
        {
            SynchronizationContext actionThreadSyncContext = null;
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;
            bool sawOperationCompleted = false;
            bool sawOperationStarted = false;
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { actionThreadSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationCompleted = () => { sawOperationCompleted = true; },
                    OnOperationStarted = () => { sawOperationStarted = true; },
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, syncContext, false);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(sawOperationStarted, "Bind should not start operation");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");

                int result = action();
                Assert.AreEqual(0, result, "Func result returned");
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action should not run");
                Assert.IsFalse(sawOperationStarted, "Context incremented operation count");
                Assert.IsFalse(sawOperationCompleted, "Context decremented operation count");
                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestFuncDefaultSyncContextBindAndRun()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); return 13; }, new SynchronizationContext());
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
                Assert.IsTrue(evt.WaitOne(100), "Action did not run on ThreadPool");
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestFuncDefaultSyncContextSkippingTestBindAndRun()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); return 13; }, new SynchronizationContext(), false);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
                Assert.IsTrue(evt.WaitOne(100), "Action did not run on ThreadPool");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestFuncSyncObjectBindAndRun()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); return 13; }, syncObject);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
                Assert.IsTrue(evt.WaitOne(100), "Action did not run on ThreadPool");
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestFuncCurrentSyncObjectBindAndRun()
        {
            int sawActionThread = ~Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var syncObject = new FakeFuncSynchronizingObject(false);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); return 13; }, syncObject);
                Assert.AreEqual(~Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
                Assert.IsTrue(evt.WaitOne(0), "Action did not run synchronized");
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
                Assert.IsFalse(syncObject.sawInvoke, "Bound action did run through synchronizing object");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void TestFuncSyncObjectWithReset()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; evt.Set(); return 13; }, syncObject);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bind should not execute action");
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");

                int result = action();
                Assert.AreEqual(0, result, "Func result returned");
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was executed");
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void TestInvalidatedProperty()
        {
            using (CallbackContext context = new CallbackContext())
            {
                context.Bind(() => { });
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");

                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");

                context.Bind(() => { });
                context.Bind(() => { });
                Assert.IsFalse(context.Invalidated, "Bound actions should be valid");

                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound actions should be invalid");
            }
        }

        private sealed class FakeActionSynchronizingObject : ISynchronizeInvoke
        {
            private Action action;

            public bool sawInvoke;

            private bool invokeRequired;

            public FakeActionSynchronizingObject(bool invokeRequired)
            {
                this.invokeRequired = invokeRequired;
            }

            public IAsyncResult BeginInvoke(Delegate method, object[] args)
            {
                this.sawInvoke = true;
                this.action = () => method.DynamicInvoke(args);
                return this.action.BeginInvoke(null, null);
            }

            public object EndInvoke(IAsyncResult result)
            {
                this.action.EndInvoke(result);
                return null;
            }

            public object Invoke(Delegate method, object[] args)
            {
                this.sawInvoke = true;
                this.action = () => method.DynamicInvoke(args);
                this.action.EndInvoke(this.action.BeginInvoke(null, null));
                return null;
            }

            public bool InvokeRequired
            {
                get { return this.invokeRequired; }
            }
        }

        private sealed class FakeFuncSynchronizingObject : ISynchronizeInvoke
        {
            private Func<object> action;

            public bool sawInvoke;

            private bool invokeRequired;

            public FakeFuncSynchronizingObject(bool invokeRequired)
            {
                this.invokeRequired = invokeRequired;
            }

            public IAsyncResult BeginInvoke(Delegate method, object[] args)
            {
                this.sawInvoke = true;
                this.action = () => method.DynamicInvoke(args);
                return this.action.BeginInvoke(null, null);
            }

            public object EndInvoke(IAsyncResult result)
            {
                return this.action.EndInvoke(result);
            }

            public object Invoke(Delegate method, object[] args)
            {
                this.sawInvoke = true;
                this.action = () => method.DynamicInvoke(args);
                return this.action.EndInvoke(this.action.BeginInvoke(null, null));
            }

            public bool InvokeRequired
            {
                get { return this.invokeRequired; }
            }
        }
    }
}
