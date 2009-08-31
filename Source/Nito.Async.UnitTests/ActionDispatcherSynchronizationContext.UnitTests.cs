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
    public class ActionDispatcherSynchronizationContextUnitTests
    {
        [TestMethod]
        public void TestRegisteredProperties()
        {
            // Just calling "typeof" isn't sufficient to invoke the static constructor, so we create one
            // Ordinarily, this isn't necessary, since Verify is normally only called on types of initialized objects
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                new ActionDispatcherSynchronizationContext(dispatcher);
            }

            SynchronizationContextRegister.Verify(typeof(ActionDispatcherSynchronizationContext), SynchronizationContextProperties.Standard);
        }

        [TestMethod]
        public void TestSend()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread2.Do(() => { actionDispatcherSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Sanity check
                Assert.IsInstanceOfType(actionDispatcherSyncContext, typeof(ActionDispatcherSynchronizationContext), "Prerequisite failed: ActionThread is not using an ActionDispatcherSynchronizationContext");

                // Have the first thread do a synchronous Send to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                using (ManualResetEvent completed = new ManualResetEvent(false))
                using (ManualResetEvent wait = new ManualResetEvent(false))
                {
                    thread1.Do(() =>
                        {
                            actionDispatcherSyncContext.Send((state) => { wait.WaitOne(); }, null);
                            completed.Set();
                        });

                    Assert.IsFalse(completed.WaitOne(100), "ActionDispatcherSynchronizationContext.Send is not synchronous");

                    wait.Set();

                    Assert.IsTrue(thread1.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                    Assert.IsTrue(thread2.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                }
            }
        }

        [TestMethod]
        public void TestPost()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread2.Do(() => { actionDispatcherSyncContext = SynchronizationContext.Current; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Sanity check
                Assert.IsInstanceOfType(actionDispatcherSyncContext, typeof(ActionDispatcherSynchronizationContext), "Prerequisite failed: ActionThread is not using an ActionDispatcherSynchronizationContext");

                // Have the first thread do an synchronous Post to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                using (ManualResetEvent completed = new ManualResetEvent(false))
                using (ManualResetEvent wait = new ManualResetEvent(false))
                {
                    thread1.Do(() =>
                    {
                        actionDispatcherSyncContext.Post((state) => { wait.WaitOne(); }, null);
                        completed.Set();
                    });

                    Assert.IsTrue(completed.WaitOne(100), "ActionDispatcherSynchronizationContext.Post is not asynchronous");

                    wait.Set();

                    Assert.IsTrue(thread1.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                    Assert.IsTrue(thread2.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                }
            }
        }

        [TestMethod]
        public void TestSendToCopy()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread2.Do(() => { actionDispatcherSyncContext = SynchronizationContext.Current.CreateCopy(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Sanity check
                Assert.IsInstanceOfType(actionDispatcherSyncContext, typeof(ActionDispatcherSynchronizationContext), "Prerequisite failed: ActionThread is not using an ActionDispatcherSynchronizationContext");

                // Have the first thread do a synchronous Send to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                using (ManualResetEvent completed = new ManualResetEvent(false))
                using (ManualResetEvent wait = new ManualResetEvent(false))
                {
                    thread1.Do(() =>
                    {
                        actionDispatcherSyncContext.Send((state) => { wait.WaitOne(); }, null);
                        completed.Set();
                    });

                    Assert.IsFalse(completed.WaitOne(100), "ActionDispatcherSynchronizationContext.Send is not synchronous");

                    wait.Set();

                    Assert.IsTrue(thread1.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                    Assert.IsTrue(thread2.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                }
            }
        }

        [TestMethod]
        public void TestPostToCopy()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext and signal this thread when it's captured.
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread2.Do(() => { actionDispatcherSyncContext = SynchronizationContext.Current.CreateCopy(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Sanity check
                Assert.IsInstanceOfType(actionDispatcherSyncContext, typeof(ActionDispatcherSynchronizationContext), "Prerequisite failed: ActionThread is not using an ActionDispatcherSynchronizationContext");

                // Have the first thread do an synchronous Post to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                using (ManualResetEvent completed = new ManualResetEvent(false))
                using (ManualResetEvent wait = new ManualResetEvent(false))
                {
                    thread1.Do(() =>
                    {
                        actionDispatcherSyncContext.Post((state) => { wait.WaitOne(); }, null);
                        completed.Set();
                    });

                    Assert.IsTrue(completed.WaitOne(100), "ActionDispatcherSynchronizationContext.Post is not asynchronous");

                    wait.Set();

                    Assert.IsTrue(thread1.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                    Assert.IsTrue(thread2.Join(TimeSpan.FromMilliseconds(100)), "ActionThread did not Join");
                }
            }
        }
    }
}
