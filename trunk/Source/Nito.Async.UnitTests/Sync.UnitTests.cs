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
        public void TestSyncAction()
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
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                action();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
            }
        }

        [TestMethod]
        public void TestSyncAction1()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
            int arg1 = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a) => { arg1 = a;  actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                action(13);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreEqual(13, arg1, "Action did not receive parameter 1");
            }
        }

        [TestMethod]
        public void TestSyncAction2()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
            int arg1 = 0;
            int arg2 = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a, int b) => { arg1 = a; arg2 = b; actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                action(13, 17);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreEqual(13, arg1, "Action did not receive parameter 1");
                Assert.AreEqual(17, arg2, "Action did not receive parameter 2");
            }
        }

        [TestMethod]
        public void TestSyncAction3()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
            int arg1 = 0;
            int arg2 = 0;
            int arg3 = 0;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                Action<int, int, int> action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAction((int a, int b, int c) => { arg1 = a; arg2 = b; arg3 = c; actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                action(13, 17, 19);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreEqual(13, arg1, "Action did not receive parameter 1");
                Assert.AreEqual(17, arg2, "Action did not receive parameter 2");
                Assert.AreEqual(19, arg3, "Action did not receive parameter 3");
            }
        }

        [TestMethod]
        public void TestSyncAction4()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
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
                    return Sync.SynchronizeAction((int a, int b, int c, int d) => { arg1 = a; arg2 = b; arg3 = c; arg4 = d; actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                action(13, 17, 19, 23);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreEqual(13, arg1, "Action did not receive parameter 1");
                Assert.AreEqual(17, arg2, "Action did not receive parameter 2");
                Assert.AreEqual(19, arg3, "Action did not receive parameter 3");
                Assert.AreEqual(23, arg4, "Action did not receive parameter 4");
            }
        }

        [TestMethod]
        public void TestSyncAsyncCallback()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
            IAsyncResult arg1 = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                AsyncCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeAsyncCallback((a) => { arg1 = a; actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                IAsyncResult param1 = new TestAsyncResult();
                action(param1);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreSame(param1, arg1, "Action did not receive parameter 1");
            }
        }

        [TestMethod]
        public void TestSyncTimerCallback()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
            object arg1 = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                TimerCallback action = thread.DoGet(() =>
                {
                   return Sync.SynchronizeTimerCallback((a) => { arg1 = a; actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                object param1 = new object();
                action(param1);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreSame(param1, arg1, "Action did not receive parameter 1");
            }
        }

        [TestMethod]
        public void TestSyncWaitCallback()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
            object arg1 = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitCallback((a) => { arg1 = a; actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                object param1 = new object();
                action(param1);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreSame(param1, arg1, "Action did not receive parameter 1");
            }
        }

        [TestMethod]
        public void TestSyncWaitOrTimerCallback()
        {
            int actionThreadId = Thread.CurrentThread.ManagedThreadId;
            object arg1 = null;
            bool arg2 = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Have the ActionThread set "action"
                WaitOrTimerCallback action = thread.DoGet(() =>
                {
                    return Sync.SynchronizeWaitOrTimerCallback((a, b) => { arg1 = a; arg2 = b; actionThreadId = Thread.CurrentThread.ManagedThreadId; });
                });

                // The action should be run in the context of the ActionThread
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, actionThreadId, "Action should not have run already");
                object param1 = new object();
                action(param1, true);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "Action did not run on ActionThread");
                Assert.AreSame(param1, arg1, "Action did not receive parameter 1");
                Assert.IsTrue(arg2, "Action did not receive parameter 2");
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
