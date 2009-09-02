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
    public class SyncUnitTests
    {
        [TestMethod]
        public void Action_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction(() => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedAction_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action();
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void ActionWithOneArgument_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedActionWithOneArgument_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(13);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedActionWithOneArgument_Invoked_ReceivesParameters()
        {
            int arg1 = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1) => { arg1 = a1; });
                });

                action(13);
                thread.Join();

                Assert.AreEqual(13, arg1, "Action did not receive parameter");
            }
        }

        [TestMethod]
        public void ActionWithTwoArguments_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedActionWithTwoArguments_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(13, 17);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedActionWithTwoArguments_Invoked_ReceivesParameters()
        {
            int arg1 = 0;
            int arg2 = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2) => { arg1 = a1; arg2 = a2; });
                });

                action(13, 17);
                thread.Join();

                Assert.AreEqual(13, arg1, "Action did not receive parameter");
                Assert.AreEqual(17, arg2, "Action did not receive parameter");
            }
        }

        [TestMethod]
        public void ActionWithThreeArguments_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2, int a3) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedActionWithThreeArguments_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2, int a3) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(13, 17, 19);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedActionWithThreeArguments_Invoked_ReceivesParameters()
        {
            int arg1 = 0;
            int arg2 = 0;
            int arg3 = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2, int a3) => { arg1 = a1; arg2 = a2; arg3 = a3; });
                });

                action(13, 17, 19);
                thread.Join();

                Assert.AreEqual(13, arg1, "Action did not receive parameter");
                Assert.AreEqual(17, arg2, "Action did not receive parameter");
                Assert.AreEqual(19, arg3, "Action did not receive parameter");
            }
        }

        [TestMethod]
        public void ActionWithFourArguments_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int, int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2, int a3, int a4) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedActionWithFourArguments_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int, int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2, int a3, int a4) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(13, 17, 19, 23);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedActionWithFourArguments_Invoked_ReceivesParameters()
        {
            int arg1 = 0;
            int arg2 = 0;
            int arg3 = 0;
            int arg4 = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int, int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a1, int a2, int a3, int a4) => { arg1 = a1; arg2 = a2; arg3 = a3; arg4 = a4; });
                });

                action(13, 17, 19, 23);
                thread.Join();

                Assert.AreEqual(13, arg1, "Action did not receive parameter");
                Assert.AreEqual(17, arg2, "Action did not receive parameter");
                Assert.AreEqual(19, arg3, "Action did not receive parameter");
                Assert.AreEqual(23, arg4, "Action did not receive parameter");
            }
        }

        [TestMethod]
        public void AsyncCallback_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                AsyncCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAsyncCallback((IAsyncResult a1) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedAsyncCallback_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                AsyncCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAsyncCallback((IAsyncResult a1) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(null);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedAsyncCallback_Invoked_ReceivesParameter()
        {
            TestAsyncResult parameter1 = new TestAsyncResult();
            IAsyncResult arg1 = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                AsyncCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAsyncCallback((IAsyncResult a1) => { arg1 = a1; });
                });

                action(parameter1);
                thread.Join();

                Assert.AreSame(parameter1, arg1, "Action did not receive parameter");
            }
        }

        [TestMethod]
        public void TimerCallback_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                TimerCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeTimerCallback((a1) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedTimerCallback_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                TimerCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeTimerCallback((a1) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(null);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedTimerCallback_Invoked_ReceivesParameter()
        {
            object parameter1 = new object();
            object arg1 = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                TimerCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeTimerCallback((a1) => { arg1 = a1; });
                });

                action(parameter1);
                thread.Join();

                Assert.AreSame(parameter1, arg1, "Action did not receive parameter");
            }
        }

        [TestMethod]
        public void WaitCallback_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitCallback((a1) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedWaitCallback_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitCallback((a1) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(null);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedWaitCallback_Invoked_ReceivesParameter()
        {
            object parameter1 = new object();
            object arg1 = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitCallback((a1) => { arg1 = a1; });
                });

                action(parameter1);
                thread.Join();

                Assert.AreSame(parameter1, arg1, "Action did not receive parameter");
            }
        }

        [TestMethod]
        public void WaitOrTimerCallback_AfterSync_HasNotRun()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitOrTimerCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitOrTimerCallback((a1, a2) => { sawAction = true; });
                });

                // The action should be run in the context of the ActionThread
                Assert.IsFalse(sawAction, "Action should not have run already");
            }
        }

        [TestMethod]
        public void SyncedWaitOrTimerCallback_Invoked_RunsSynchronized()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitOrTimerCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitOrTimerCallback((a1, a2) => { actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                action(null, false);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run synchronized");
            }
        }

        [TestMethod]
        public void SyncedWaitOrTimerCallback_Invoked_ReceivesParameter()
        {
            object parameter1 = new object();
            object arg1 = null;
            bool arg2 = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitOrTimerCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitOrTimerCallback((a1, a2) => { arg1 = a1; arg2 = a2; });
                });

                action(parameter1, true);
                thread.Join();

                Assert.AreSame(parameter1, arg1, "Action did not receive parameter");
                Assert.IsTrue(arg2, "Action did not receive parameter");
            }
        }

        private sealed class TestAsyncResult : IAsyncResult
        {
            #region IAsyncResult Members

            public object AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }
    }
}
