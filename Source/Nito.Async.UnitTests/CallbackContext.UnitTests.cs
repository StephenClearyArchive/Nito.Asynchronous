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
    /// <summary>
    /// A "Synced" action is one that has been bound with a SynchronizationContext.
    /// An "Objectsynced" action is one that has been bound with an ISynchronizeInvoke object whose InvokeRequired property is true.
    /// A "Fakesynced" action is one that has been bound with an ISynchronizeInvoke object whose InvokeRequired property is false.
    /// </summary>
    [TestClass]
    public class CallbackContextUnitTests
    {
        [TestMethod]
        public void CallbackContext_WithBoundAction_IsNotInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { });
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void ValidAction_Invoked_Executes()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true; });
                action();
                Assert.IsTrue(sawAction, "Bound action did not execute");
            }
        }

        [TestMethod]
        public void CallbackContext_WithInvokedAction_IsNotInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { });
                action();
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void CallbackContext_ResetAfterBindingAction_IsInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { });
                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void InvalidAction_Invoked_DoesNotExecute()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true; });
                context.Reset();
                action();
                Assert.IsFalse(sawAction, "Invalid action did execute");
            }
        }

        [TestMethod]
        public void CallbackContext_AfterInvokingInvalidAction_IsInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { });
                context.Reset();
                action();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void Action_InvokedAfterContextDispose_DoesNotExecute()
        {
            bool sawAction = false;
            Action action = null;

            using (CallbackContext context = new CallbackContext())
            {
                action = context.Bind(() => { sawAction = true; });
            }

            action();
            Assert.IsFalse(sawAction, "Invalid action did execute");
        }

        [TestMethod]
        public void CallbackContext_Reset_InvalidatesAllActions()
        {
            bool sawAction1 = false;
            bool sawAction2 = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action1 = context.Bind(() => { sawAction1 = true; });
                var action2 = context.Bind(() => { sawAction2 = true; });

                context.Reset();
                action1();
                action2();
                Assert.IsFalse(sawAction1, "Invalid action did execute");
                Assert.IsFalse(sawAction2, "Invalid action did execute");
            }
        }

        [TestMethod]
        public void BoundSyncedAction_Invoked_ExecutesSynchronized()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, actionThreadSyncContext);
                action();

                Assert.AreEqual(thread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
            }
        }

        [TestMethod]
        public void BoundSyncedAction_Invoked_UsesSyncContext()
        {
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { }, syncContext, false);
                action();

                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
            }
        }

        [TestMethod]
        public void BoundSyncedAction_Invoked_IncrementsSyncContextOperationCount()
        {
            bool sawOperationStarted = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationStarted = () => { sawOperationStarted = true; }
                };

                var action = context.Bind(() => { }, syncContext, false);
                action();

                Assert.IsFalse(sawOperationStarted, "Context incremented operation count");
            }
        }

        [TestMethod]
        public void BoundSyncedAction_Invoked_DecrementsSyncContextOperationCount()
        {
            bool sawOperationCompleted = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationCompleted = () => { sawOperationCompleted = true; }
                };

                var action = context.Bind(() => { }, syncContext, false);
                action();

                Assert.IsFalse(sawOperationCompleted, "Context decremented operation count");
            }
        }

        [TestMethod]
        public void InvalidBoundSyncedAction_Invoked_DoesNotExecute()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, actionThreadSyncContext, false);
                context.Reset();
                action();

                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Invalid action should not run");
            }
        }

        [TestMethod]
        public void InvalidBoundSyncedAction_Invoked_DoesSync()
        {
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { }, syncContext, false);
                context.Reset();
                action();

                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
            }
        }

        [TestMethod]
        public void BoundAsyncAction_Invoked_ExecutesSynchronized()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var action = context.AsyncBind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, actionThreadSyncContext);
                action();
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
            }
        }

        [TestMethod]
        public void BoundAsyncAction_Invoked_UsesSyncContext()
        {
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.AsyncBind(() => { }, syncContext, false);
                action();
                thread.Join();

                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
            }
        }

        [TestMethod]
        public void BoundAsyncAction_Invoked_IncrementsSyncContextOperationCount()
        {
            bool sawOperationStarted = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationStarted = () => { sawOperationStarted = true; }
                };

                var action = context.AsyncBind(() => { }, syncContext, false);
                action();
                thread.Join();

                Assert.IsFalse(sawOperationStarted, "Context incremented operation count");
            }
        }

        [TestMethod]
        public void BoundAsyncAction_Invoked_DecrementsSyncContextOperationCount()
        {
            bool sawOperationCompleted = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationCompleted = () => { sawOperationCompleted = true; }
                };

                var action = context.AsyncBind(() => { }, syncContext, false);
                action();
                thread.Join();

                Assert.IsFalse(sawOperationCompleted, "Context decremented operation count");
            }
        }

        [TestMethod]
        public void InvalidBoundAsyncAction_Invoked_DoesNotExecute()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var action = context.AsyncBind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, actionThreadSyncContext, false);
                context.Reset();
                action();
                thread.Join();

                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Invalid action should not run");
            }
        }

        [TestMethod]
        public void InvalidBoundAsyncAction_Invoked_DoesSync()
        {
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.AsyncBind(() => { }, syncContext, false);
                context.Reset();
                action();
                thread.Join();

                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CallbackContext_BindingActionWithDefaultSyncContext_ThrowsInvalidOperationException()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { }, new SynchronizationContext());
            }
        }

        /// <summary>
        /// Note: this is not a supported scenario! The "skip verification" overload is intended for efficiency reasons only.
        /// </summary>
        [TestMethod]
        public void ActionBoundToDefaultSyncContext_Invoked_Executes()
        {
            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var action = context.Bind(() => { evt.Set(); }, new SynchronizationContext(), false);
                action();
                bool signalled = evt.WaitOne(100);
                Assert.IsTrue(signalled, "Action did not run");
            }
        }

        [TestMethod]
        public void BoundObjectsyncedAction_Invoked_Executes()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(true);
                var action = context.Bind(() => { sawAction = true; }, syncObject);
                action();
                Assert.IsTrue(sawAction, "Bound action did not run");
            }
        }

        [TestMethod]
        public void BoundObjectsyncedAction_Invoked_UsesSyncObject()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(true);
                var action = context.Bind(() => { }, syncObject);
                action();
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
            }
        }

        [TestMethod]
        public void BoundObjectsyncedAction_Invoked_SynchronizesWithSyncObject()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, syncObject);
                action();
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
            }
        }

        [TestMethod]
        public void BoundFakesyncedAction_Invoked_Executes()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(false);
                var action = context.Bind(() => { sawAction = true; }, syncObject);
                action();
                Assert.IsTrue(sawAction, "Bound action did not run");
            }
        }

        [TestMethod]
        public void BoundFakesyncedAction_Invoked_DoesNotUseSyncObject()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(false);
                var action = context.Bind(() => { }, syncObject);
                action();
                Assert.IsFalse(syncObject.sawInvoke, "Bound action did run through synchronizing object");
            }
        }

        [TestMethod]
        public void BoundFakesyncedAction_Invoked_DoesNotSynchronizeWithSyncObject()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(false);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, syncObject);
                action();
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not run inline");
            }
        }

        [TestMethod]
        public void InvalidBoundObjectsyncedAction_Invoked_SynchronizesWithSyncObject()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(true);
                var action = context.Bind(() => { }, syncObject);
                context.Reset();
                action();
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
            }
        }

        [TestMethod]
        public void InvalidBoundObjectsyncedAction_Invoked_DoesNotExecute()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeActionSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; }, syncObject);
                context.Reset();
                action();
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was executed");
            }
        }

        [TestMethod]
        public void CallbackContext_WithBoundFunc_IsNotInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { return 13; });
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void ValidFunc_Invoked_Executes()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true; return 13; });
                int result = action();
                Assert.IsTrue(sawAction, "Bound action did not execute");
            }
        }

        [TestMethod]
        public void ValidFunc_Invoked_ReturnsValue()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { return 13; });
                int result = action();
                Assert.AreEqual(13, result, "Func result not returned");
            }
        }

        [TestMethod]
        public void CallbackContext_WithInvokedFunc_IsNotInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { return 13; });
                int result = action();
                Assert.IsFalse(context.Invalidated, "Bound action should be valid");
            }
        }

        [TestMethod]
        public void CallbackContext_ResetAfterBindingFunc_IsInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { return 13; });
                context.Reset();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void InvalidFunc_Invoked_DoesNotExecute()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { sawAction = true;  return 13; });
                context.Reset();
                int result = action();
                Assert.IsFalse(sawAction, "Invalid action did execute");
            }
        }

        [TestMethod]
        public void InvalidFunc_Invoked_ReturnsDefault()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { return 13; });
                context.Reset();
                int result = action();
                Assert.AreEqual(default(int), result, "Invalid func had a non-default return value");
            }
        }

        [TestMethod]
        public void CallbackContext_AfterInvokingInvalidFunc_IsInvalid()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { return 13; });
                context.Reset();
                int result = action();
                Assert.IsTrue(context.Invalidated, "Bound action should be invalid");
            }
        }

        [TestMethod]
        public void Func_InvokedAfterContextDispose_DoesNotExecute()
        {
            bool sawAction = false;
            Func<int> action = null;

            using (CallbackContext context = new CallbackContext())
            {
                action = context.Bind(() => { sawAction = true; return 13; });
            }

            int result = action();
            Assert.IsFalse(sawAction, "Invalid action did execute");
        }

        [TestMethod]
        public void Func_InvokedAfterContextDispose_ReturnsDefault()
        {
            Func<int> action = null;

            using (CallbackContext context = new CallbackContext())
            {
                action = context.Bind(() => { return 13; });
            }

            int result = action();
            Assert.AreEqual(0, result, "Invalid func had a non-default return value");
        }

        [TestMethod]
        public void CallbackContext_Reset_InvalidatesAllFuncs()
        {
            bool sawAction1 = false;
            bool sawAction2 = false;

            using (CallbackContext context = new CallbackContext())
            {
                var action1 = context.Bind(() => { sawAction1 = true; return 13; });
                var action2 = context.Bind(() => { sawAction2 = true; return 17; });

                context.Reset();
                int result1 = action1();
                int result2 = action2();

                Assert.IsFalse(sawAction1, "Invalid action did execute");
                Assert.IsFalse(sawAction2, "Invalid action did execute");
            }
        }

        [TestMethod]
        public void BoundSyncedFunc_Invoked_ExecutesSynchronized()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, actionThreadSyncContext);
                int result = action();

                Assert.AreEqual(thread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
            }
        }

        [TestMethod]
        public void BoundSyncedFunc_Invoked_UsesSyncContext()
        {
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { return 13;  }, syncContext, false);
                int result = action();

                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
            }
        }

        [TestMethod]
        public void BoundSyncedFunc_Invoked_IncrementsSyncContextOperationCount()
        {
            bool sawOperationStarted = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationStarted = () => { sawOperationStarted = true; }
                };

                var action = context.Bind(() => { return 13; }, syncContext, false);
                int result = action();

                Assert.IsFalse(sawOperationStarted, "Context incremented operation count");
            }
        }

        [TestMethod]
        public void BoundSyncedFunc_Invoked_DecrementsSyncContextOperationCount()
        {
            bool sawOperationCompleted = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnOperationCompleted = () => { sawOperationCompleted = true; }
                };

                var action = context.Bind(() => { return 13; }, syncContext, false);
                int result = action();

                Assert.IsFalse(sawOperationCompleted, "Context decremented operation count");
            }
        }

        [TestMethod]
        public void InvalidBoundSyncedFunc_Invoked_DoesNotExecute()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, actionThreadSyncContext, false);
                context.Reset();
                int result = action();

                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Invalid action should not run");
            }
        }

        [TestMethod]
        public void InvalidBoundSyncedFunc_Invoked_DoesSync()
        {
            bool sawSync = false;

            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var syncContext = new Util.LoggingSynchronizationContext(actionThreadSyncContext)
                {
                    OnPost = () => { sawSync = true; },
                    OnSend = () => { sawSync = true; }
                };

                var action = context.Bind(() => { return 13; }, syncContext, false);
                context.Reset();
                int result = action();

                Assert.IsTrue(sawSync, "Context did not use SyncContext for sync");
            }
        }

        [TestMethod]
        public void InvalidBoundSyncedFunc_Invoked_ReturnsDefault()
        {
            using (CallbackContext context = new CallbackContext())
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                var action = context.Bind(() => { return 13; }, actionThreadSyncContext, false);
                context.Reset();
                int result = action();

                Assert.AreEqual(0, result, "Invalid Func returned a non-default value");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CallbackContext_BindingFuncWithDefaultSyncContext_ThrowsInvalidOperationException()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var action = context.Bind(() => { return 13; }, new SynchronizationContext());
            }
        }

        /// <summary>
        /// Note: this is not a supported scenario! The "skip verification" overload is intended for efficiency reasons only.
        /// </summary>
        [TestMethod]
        public void FuncBoundToDefaultSyncContext_Invoked_Executes()
        {
            using (CallbackContext context = new CallbackContext())
            using (ManualResetEvent evt = new ManualResetEvent(false))
            {
                var action = context.Bind(() => { evt.Set(); return 13; }, new SynchronizationContext(), false);
                int result = action();
                bool signalled = evt.WaitOne(100);
                Assert.IsTrue(signalled, "Action did not run");
            }
        }

        [TestMethod]
        public void BoundObjectsyncedFunc_Invoked_Executes()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { sawAction = true; return 13; }, syncObject);
                int result = action();
                Assert.IsTrue(sawAction, "Bound action did not run");
            }
        }

        [TestMethod]
        public void BoundObjectsyncedFunc_Invoked_ReturnsValue()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { return 13; }, syncObject);
                int result = action();
                Assert.AreEqual(13, result, "Bound func did not return result");
            }
        }

        [TestMethod]
        public void BoundObjectsyncedFunc_Invoked_UsesSyncObject()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { return 13; }, syncObject);
                int result = action();
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
            }
        }

        [TestMethod]
        public void BoundObjectsyncedFunc_Invoked_SynchronizesWithSyncObject()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, syncObject);
                int result = action();
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not synchronized");
            }
        }

        [TestMethod]
        public void InvalidBoundObjectsyncedFunc_Invoked_SynchronizesWithSyncObject()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { return 13; }, syncObject);
                context.Reset();
                int result = action();
                Assert.IsTrue(syncObject.sawInvoke, "Bound action did not run through synchronizing object");
            }
        }

        [TestMethod]
        public void InvalidBoundObjectsyncedFunc_Invoked_DoesNotExecute()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, syncObject);
                context.Reset();
                int result = action();
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was executed");
            }
        }

        [TestMethod]
        public void InvalidBoundObjectsyncedFunc_Invoked_ReturnsDefault()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(true);
                var action = context.Bind(() => { return 13; }, syncObject);
                context.Reset();
                int result = action();
                Assert.AreEqual(0, result, "Invalid func returned non-default value");
            }
        }

        [TestMethod]
        public void BoundFakesyncedFunc_Invoked_Executes()
        {
            bool sawAction = false;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(false);
                var action = context.Bind(() => { sawAction = true; return 13; }, syncObject);
                int result = action();
                Assert.IsTrue(sawAction, "Bound action did not run");
            }
        }

        [TestMethod]
        public void BoundFakesyncedFunc_Invoked_ReturnsValue()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(false);
                var action = context.Bind(() => { return 13; }, syncObject);
                int result = action();
                Assert.AreEqual(13, result, "Bound func did not return result");
            }
        }

        [TestMethod]
        public void BoundFakesyncedFunc_Invoked_DoesNotUseSyncObject()
        {
            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(false);
                var action = context.Bind(() => { return 13; }, syncObject);
                int result = action();
                Assert.IsFalse(syncObject.sawInvoke, "Bound action did run through synchronizing object");
            }
        }

        [TestMethod]
        public void BoundFakesyncedFunc_Invoked_DoesNotSynchronizeWithSyncObject()
        {
            int sawActionThread = Thread.CurrentThread.ManagedThreadId;

            using (CallbackContext context = new CallbackContext())
            {
                var syncObject = new FakeFuncSynchronizingObject(false);
                var action = context.Bind(() => { sawActionThread = Thread.CurrentThread.ManagedThreadId; return 13; }, syncObject);
                int result = action();
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, sawActionThread, "Bound action was not run inline");
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
