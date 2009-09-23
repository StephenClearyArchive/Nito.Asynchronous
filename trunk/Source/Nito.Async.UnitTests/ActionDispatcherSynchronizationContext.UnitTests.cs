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
        public void ActionDispatcherSyncContext_AfterConstruction_RegistersStandardPropertiesExceptSpecificAssociatedThread()
        {
            // Just calling "typeof" isn't sufficient to invoke the static constructor, so we create one
            // Ordinarily, this isn't necessary, since Verify is normally only called on types of initialized objects
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                new ActionDispatcherSynchronizationContext(dispatcher);
            }

            // This will throw an exception if the type doesn't support all Standard properties
            SynchronizationContextRegister.Verify(typeof(ActionDispatcherSynchronizationContext), SynchronizationContextProperties.NonReentrantPost | SynchronizationContextProperties.NonReentrantSend | SynchronizationContextProperties.Sequential | SynchronizationContextProperties.Synchronized);
        }

        [TestMethod]
        public void Send_IsSynchronous()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            using (ManualResetEvent wait = new ManualResetEvent(false))
            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext.
                actionDispatcherSyncContext = thread2.DoGet(() => { return SynchronizationContext.Current; });

                // Have the first thread do a synchronous Send to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                thread1.Do(() =>
                    {
                        actionDispatcherSyncContext.Send((state) => { wait.WaitOne(); }, null);
                        completed.Set();
                    });

                bool completedSignalled = completed.WaitOne(100);
                Assert.IsFalse(completedSignalled, "ActionDispatcherSynchronizationContext.Send is not synchronous");

                wait.Set();
            }
        }

        [TestMethod]
        public void Post_IsAsynchronous()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            using (ManualResetEvent wait = new ManualResetEvent(false))
            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext and signal this thread when it's captured.
                actionDispatcherSyncContext = thread2.DoGet(() => { return SynchronizationContext.Current; });

                // Have the first thread do an synchronous Post to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                thread1.Do(() =>
                {
                    actionDispatcherSyncContext.Post((state) => { wait.WaitOne(); }, null);
                    completed.Set();
                });

                bool completedSignalled = completed.WaitOne(100);
                Assert.IsTrue(completedSignalled, "ActionDispatcherSynchronizationContext.Post is not asynchronous");

                wait.Set();
            }
        }

        [TestMethod]
        public void Send_ToCopiedDispatcher_IsSynchronous()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            using (ManualResetEvent wait = new ManualResetEvent(false))
            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext and signal this thread when it's captured.
                actionDispatcherSyncContext = thread2.DoGet(() => { return SynchronizationContext.Current.CreateCopy(); });

                // Have the first thread do a synchronous Send to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                thread1.Do(() =>
                {
                    actionDispatcherSyncContext.Send((state) => { wait.WaitOne(); }, null);
                    completed.Set();
                });

                bool completedSignalled = completed.WaitOne(100);
                Assert.IsFalse(completedSignalled, "ActionDispatcherSynchronizationContext.Send is not synchronous");

                wait.Set();
            }
        }

        [TestMethod]
        public void Post_ToCopiedDispatcher_IsAsynchronous()
        {
            SynchronizationContext actionDispatcherSyncContext = null;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            using (ManualResetEvent wait = new ManualResetEvent(false))
            using (ActionThread thread1 = new ActionThread())
            using (ActionThread thread2 = new ActionThread())
            {
                thread1.Start();
                thread2.Start();

                // Capture the second thread's SynchronizationContext and signal this thread when it's captured.
                actionDispatcherSyncContext = thread2.DoGet(() => { return SynchronizationContext.Current.CreateCopy(); });

                // Have the first thread do an synchronous Post to the second thread and then trigger the "completed" event.
                // The action queued to the second thread will wait for the "wait" event.

                thread1.Do(() =>
                {
                    actionDispatcherSyncContext.Post((state) => { wait.WaitOne(); }, null);
                    completed.Set();
                });

                bool completedSignalled = completed.WaitOne(100);
                Assert.IsTrue(completedSignalled, "ActionDispatcherSynchronizationContext.Post is not asynchronous");

                wait.Set();
            }
        }
    }
}
