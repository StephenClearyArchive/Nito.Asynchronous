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
    public class ActionThreadUnitTests
    {
        [TestMethod]
        public void TestNoActions()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
            }
        }

        [TestMethod]
        public void TestSingleActionBeforeStart()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Do(() => threadId = Thread.CurrentThread.ManagedThreadId);
                thread.Start();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        public void TestSingleActionAfterStart()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Do(() => threadId = Thread.CurrentThread.ManagedThreadId);
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ThreadStateException))]
        public void TestDoubleStart()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Start();
            }
        }

        [TestMethod]
        public void TestSynchronizationContext()
        {
            SynchronizationContext actionThreadSyncContext = null;
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { actionThreadSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Use the SynchronizationContext to give the ActionThread more work to do
                actionThreadSyncContext.Post((state) => { sawAction = true; }, null);

                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.IsTrue(sawAction, "ActionThread did not perform action from SynchronizationContext");
            }
        }
    }
}
