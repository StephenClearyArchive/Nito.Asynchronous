using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;

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
        public void TestImplicitJoin()
        {
            ActionThread thread = new ActionThread();
            using (thread)
            {
                thread.Start();
            }

            Assert.IsFalse(thread.IsAlive, "ActionThread did not implicitly join");
        }

        [TestMethod]
        public void TestImplicitJoinWithoutStart()
        {
            ActionThread thread = new ActionThread();
            using (thread)
            {
            }

            Assert.IsFalse(thread.IsAlive, "ActionThread did not implicitly join");
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
                actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                // Use the SynchronizationContext to give the ActionThread more work to do
                actionThreadSyncContext.Post((state) => { sawAction = true; }, null);

                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                Assert.IsTrue(sawAction, "ActionThread did not perform action from SynchronizationContext");
            }
        }

        [TestMethod]
        public void TestSynchronousAction()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                int threadId = Thread.CurrentThread.ManagedThreadId;
                thread.DoSynchronously(() => { threadId = Thread.CurrentThread.ManagedThreadId; });
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        public void TestSynchronousFunc()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                int threadId = Thread.CurrentThread.ManagedThreadId;
                object obj = thread.DoGet(() => { threadId = Thread.CurrentThread.ManagedThreadId; return new object(); });
                Assert.IsNotNull(obj, "ActionThread did not return result");
                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        public void TestIsAlive()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.IsFalse(thread.IsAlive, "ActionThread is alive before starting");

                thread.Start();
                Assert.IsTrue(thread.IsAlive, "ActionThread is not alive after starting");

                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not join");
                Assert.IsFalse(thread.IsAlive, "ActionThread is alive after joining");
            }
        }

        [TestMethod]
        public void TestIsBackgroundSetBeforeStart()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.IsFalse(thread.IsBackground, "ActionThread should not be a background thread by default");

                thread.IsBackground = true;
                Assert.IsTrue(thread.IsBackground, "ActionThread did not remember IsBackground");

                thread.IsBackground = false;
                Assert.IsFalse(thread.IsBackground, "ActionThread did not remember IsBackground");
            }
        }

        [TestMethod]
        public void TestIsBackgroundSetAfterStart()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                Assert.IsFalse(thread.IsBackground, "ActionThread should not be a background thread by default");

                thread.IsBackground = true;
                Assert.IsTrue(thread.IsBackground, "ActionThread did not remember IsBackground");

                thread.IsBackground = false;
                Assert.IsFalse(thread.IsBackground, "ActionThread did not remember IsBackground");
            }
        }

        [TestMethod]
        public void TestNameSetBeforeStart()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.IsNull(thread.Name, "ActionThread has a name without being set");

                thread.Name = "Bob";
                Assert.AreEqual("Bob", thread.Name, "ActionThread did not remember name");
            }
        }

        [TestMethod]
        public void TestNameSetAfterStart()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.IsNull(thread.Name, "ActionThread has a name without being set");

                thread.Start();
                thread.Name = "Bob";
                Assert.AreEqual("Bob", thread.Name, "ActionThread did not remember name");

                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not join");
                Assert.AreEqual("Bob", thread.Name, "ActionThread did not remember name after joining");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNameMultiSet()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Name = "Bob";
                thread.Name = "Sue";
            }
        }
    }
}
