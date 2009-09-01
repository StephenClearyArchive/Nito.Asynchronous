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
        public void JoinWithTimeout_WithoutActions_Joins()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                bool signalled = thread.Join(TimeSpan.FromMilliseconds(100));
                Assert.IsTrue(signalled, "ActionThread did not Join");
            }
        }

        [TestMethod]
        public void Join_WithAction_DoesNotJoin()
        {
            using (ManualResetEvent evt = new ManualResetEvent(false))
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Do(() => evt.WaitOne());

                bool signalled = thread.Join(TimeSpan.FromMilliseconds(100));
                Assert.IsFalse(signalled, "ActionThread joined");

                evt.Set();
            }
        }

        [TestMethod]
        public void Join_WithoutActions_Joins()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Join();
            }
        }

        [TestMethod]
        public void Join_BeforeStart_Joins()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Join();
            }
        }

        [TestMethod]
        public void JoinWithTimeout_BeforeStart_JoinsImmediately()
        {
            using (ActionThread thread = new ActionThread())
            {
                bool signalled = thread.Join(TimeSpan.FromMilliseconds(0));
                Assert.IsTrue(signalled, "ActionThread did not join");
            }
        }

        [TestMethod]
        public void Dispose_WithoutExplicitJoin_Joins()
        {
            ActionThread thread = new ActionThread();
            using (thread)
            {
                thread.Start();
            }

            Assert.IsFalse(thread.IsAlive, "ActionThread did not implicitly join");
        }

        [TestMethod]
        public void Dispose_WithoutStart_Joins()
        {
            ActionThread thread = new ActionThread();
            using (thread)
            {
            }

            Assert.IsFalse(thread.IsAlive, "ActionThread did not implicitly join");
        }

        [TestMethod]
        public void Action_QueuedBeforeStart_IsExecutedByThread()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Do(() => threadId = Thread.CurrentThread.ManagedThreadId);
                thread.Start();
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        public void Action_QueuedAfterStart_IsExecutedByThread()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Do(() => threadId = Thread.CurrentThread.ManagedThreadId);
                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ThreadStateException))]
        public void Start_OnStartedThread_ThrowsThreadStateException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Start();
            }
        }

        [TestMethod]
        public void SyncContext_FromInsideAction_PostsToSameThread()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                // Use the SynchronizationContext to give the ActionThread more work to do
                actionThreadSyncContext.Post((state) => { threadId = Thread.CurrentThread.ManagedThreadId; }, null);

                thread.Join();

                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        public void SyncContext_FromInsideAction_SendsToSameThread()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext
                SynchronizationContext actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                // Use the SynchronizationContext to give the ActionThread more work to do
                actionThreadSyncContext.Send((state) => { threadId = Thread.CurrentThread.ManagedThreadId; }, null);

                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        public void SyncContext_FromInsideAction_IsActionDispatcherSyncContext()
        {
            SynchronizationContext actionThreadSyncContext = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture the thread's SynchronizationContext and signal this thread when it's captured.
                actionThreadSyncContext = thread.DoGet(() => { return SynchronizationContext.Current; });

                Assert.IsInstanceOfType(actionThreadSyncContext, typeof(ActionDispatcherSynchronizationContext), "ActionThread did not provide an ActionDispatcherSynchronizationContext");
            }
        }

        [TestMethod]
        public void SyncAction_QueuedAfterStart_IsExecutedByThread()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.DoSynchronously(() => { threadId = Thread.CurrentThread.ManagedThreadId; });

                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ThreadStateException))]
        public void SyncAction_QueuedBeforeStart_ThrowsThreadStateException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.DoSynchronously(() => { });
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ThreadStateException))]
        public void SyncAction_QueuedAfterJoin_ThrowsThreadStateException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Join();
                thread.DoSynchronously(() => { });
            }
        }

        [TestMethod]
        public void SyncFunc_QueuedAfterStart_IsExecutedByThread()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                object obj = thread.DoGet(() => { threadId = Thread.CurrentThread.ManagedThreadId; return new object(); });

                Assert.AreEqual(thread.ManagedThreadId, threadId, "ActionThread ran in wrong thread context");
            }
        }

        [TestMethod]
        public void SyncFunc_QueuedAfterStart_PreservesReturnValue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                object obj = thread.DoGet(() => { return new object(); });
                Assert.IsNotNull(obj, "ActionThread did not return result");
            }
        }

        [TestMethod]
        public void IsAliveProperty_BeforeStart_IsFalse()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.IsFalse(thread.IsAlive, "ActionThread is alive before starting");
            }
        }

        [TestMethod]
        public void IsAliveProperty_AfterStart_IsTrue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                Assert.IsTrue(thread.IsAlive, "ActionThread is not alive after starting");
            }
        }

        [TestMethod]
        public void IsAliveProperty_AfterStartAndJoin_IsFalse()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Join();

                Assert.IsFalse(thread.IsAlive, "ActionThread is alive after joining");
            }
        }

        [TestMethod]
        public void IsBackgroundProperty_InitialValue_IsFalse()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.IsFalse(thread.IsBackground, "ActionThread should not be a background thread by default");
            }
        }

        [TestMethod]
        public void IsBackgroundProperty_BeforeStart_CanBeSetToTrue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.IsBackground = true;
                Assert.IsTrue(thread.IsBackground, "ActionThread did not remember IsBackground");
            }
        }

        [TestMethod]
        public void IsBackgroundProperty_AfterStart_CanBeSetToTrue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                thread.IsBackground = true;
                Assert.IsTrue(thread.IsBackground, "ActionThread did not remember IsBackground");
            }
        }

        [TestMethod]
        public void Name_InitialValue_IsNull()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.IsNull(thread.Name, "ActionThread has a name without being set");
            }
        }

        [TestMethod]
        public void NameSet_BeforeStart_RemembersValue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Name = "Bob";
                Assert.AreEqual("Bob", thread.Name, "ActionThread did not remember name");
            }
        }

        [TestMethod]
        public void NameSet_AfterStart_RemembersValue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Name = "Bob";
                Assert.AreEqual("Bob", thread.Name, "ActionThread did not remember name");
            }
        }

        [TestMethod]
        public void NameSet_AfterJoin_RemembersValue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Name = "Bob";
                thread.Join();

                Assert.AreEqual("Bob", thread.Name, "ActionThread did not remember name after joining");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Name_OnMultipleSets_ThrowsInvalidOperationException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Name = "Bob";
                thread.Name = "Sue";
            }
        }

        [TestMethod]
        public void Priority_InitialValue_IsNormal()
        {
            using (ActionThread thread = new ActionThread())
            {
                Assert.AreEqual(ThreadPriority.Normal, thread.Priority, "ActionThread did not start with ThreadPriority.Normal");
            }
        }

        [TestMethod]
        public void PrioritySet_BeforeStart_RemembersValue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Priority = ThreadPriority.Highest;
                Assert.AreEqual(ThreadPriority.Highest, thread.Priority, "ActionThread did not remember Priority");
            }
        }

        [TestMethod]
        public void PrioritySet_AfterStart_RemembersValue()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();
                thread.Priority = ThreadPriority.BelowNormal;
                Assert.AreEqual(ThreadPriority.BelowNormal, thread.Priority, "ActionThread did not remember Priority");
            }
        }
    }
}
