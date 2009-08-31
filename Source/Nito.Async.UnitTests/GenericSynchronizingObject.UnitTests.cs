using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using System.ComponentModel;
using System.Reflection;

namespace UnitTests
{
    [TestClass]
    public class GenericSynchronizingObjectUnitTests
    {
        [TestMethod]
        public void TestInvokeRequiredWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                Assert.IsTrue(test.InvokeRequired, "GenericSynchronizingObject does not require invoke for ActionThread");

                // Capture InvokeRequired from the ActionThread's context
                bool nestedInvokeRequired = true;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { nestedInvokeRequired = test.InvokeRequired; evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                Assert.IsFalse(nestedInvokeRequired, "GenericSynchronizingObject does require invoke within ActionThread");
            }
        }

        [TestMethod]
        public void TestInvokeWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    test.Invoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; evt.Set(); }), null);
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "GenericSynchronizingObject.Invoke did not perform action");
                }

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
            }
        }

        [TestMethod]
        public void TestInvokeExceptionWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                Exception error = null;
                try
                {
                    test.Invoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; throw new MyException(); }), null);
                }
                catch (TargetInvocationException ex)
                {
                    error = ex;
                }

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
                Assert.IsInstanceOfType(error.InnerException, typeof(MyException), "Exception type not preserved");
            }
        }

        [TestMethod]
        public void TestBeginEndInvokeWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; evt.Set(); }), null);
                    Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                    Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                    test.EndInvoke(result);
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "GenericSynchronizingObject.Invoke did not perform action");
                    Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0), "AsyncWaitHandle should be signalled");
                }

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
            }
        }

        [TestMethod]
        public void TestBeginInvokeExplicitWaitWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; evt.Set(); }), null);
                    Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                    Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                    result.AsyncWaitHandle.WaitOne();
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "GenericSynchronizingObject.Invoke did not perform action");
                }

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
            }
        }

        [TestMethod]
        public void TestBeginInvokeMultipleExplicitWaitWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; evt.Set(); }), null);
                    Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                    Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                    result.AsyncWaitHandle.WaitOne();
                    result.AsyncWaitHandle.WaitOne();
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "GenericSynchronizingObject.Invoke did not perform action");
                }

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
            }
        }

        [TestMethod]
        public void TestBeginEndInvokeExplicitWaitWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; evt.Set(); }), null);
                    Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                    Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                    result.AsyncWaitHandle.WaitOne();
                    test.EndInvoke(result);
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "GenericSynchronizingObject.Invoke did not perform action");
                }

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
            }
        }

        [TestMethod]
        public void TestBeginEndInvokeExceptionWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; throw new MyException(); }), null);
                    Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                    Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");

                    Exception error = null;
                    try
                    {
                        test.EndInvoke(result);
                    }
                    catch (TargetInvocationException ex)
                    {
                        error = ex;
                    }

                    Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
                    Assert.IsInstanceOfType(error.InnerException, typeof(MyException), "Exception type not preserved");
                }
            }
        }

        [TestMethod]
        public void TestBeginEndPollingInvokeWithActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() => { test = new GenericSynchronizingObject(); evt.Set(); });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                // Ensure it will invoke on the correct thread
                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.AreNotEqual(thread.ManagedThreadId, actionThreadId, "ActionThread is this thread");

                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; evt.Set(); }), null);
                    Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                    Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                    while (!result.IsCompleted)
                    {
                        Thread.Sleep(20);
                    }
                    test.EndInvoke(result);
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "GenericSynchronizingObject.Invoke did not perform action");
                }

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
            }
        }

        [TestMethod]
        public void TestInvokeRequiredWithThreadPool()
        {
            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                ISynchronizeInvoke test = new GenericSynchronizingObject();

                Assert.IsFalse(test.InvokeRequired, "GenericSynchronizingObject does require invoke within thread pool");
            }
        }

        [TestMethod]
        public void TestInvokeWithThreadPool()
        {
            int threadId = ~Thread.CurrentThread.ManagedThreadId;

            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                ISynchronizeInvoke test = new GenericSynchronizingObject();
                test.Invoke((MethodInvoker)(() => { threadId = Thread.CurrentThread.ManagedThreadId; }), null);
            }

            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ThreadPool invoke did not operate synchronously");
        }

        [TestMethod]
        public void TestInvokeWithNullThreadPool()
        {
            int threadId = ~Thread.CurrentThread.ManagedThreadId;

            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(null))
            {
                ISynchronizeInvoke test = new GenericSynchronizingObject();
                test.Invoke((MethodInvoker)(() => { threadId = Thread.CurrentThread.ManagedThreadId; }), null);
            }

            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ThreadPool invoke did not operate synchronously");
        }

        [TestMethod]
        public void TestInvokeExceptionWithThreadPool()
        {
            int threadId = ~Thread.CurrentThread.ManagedThreadId;

            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                ISynchronizeInvoke test = new GenericSynchronizingObject();

                Exception error = null;
                try
                {
                    test.Invoke((MethodInvoker)(() => { threadId = Thread.CurrentThread.ManagedThreadId; throw new MyException(); }), null);
                }
                catch (TargetInvocationException ex)
                {
                    error = ex;
                }

                Assert.IsInstanceOfType(error.InnerException, typeof(MyException), "Exception type not preserved");
            }

            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ThreadPool invoke did not operate synchronously");
        }

        [TestMethod]
        public void TestBeginEndInvokeWithThreadPool()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                bool actionThreadIsThreadPoolThread = true;
                bool threadPoolThreadIsThreadPoolThread = false;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() =>
                        {
                            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
                            {
                                actionThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                                ISynchronizeInvoke test = new GenericSynchronizingObject();
                                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { threadPoolThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread; }), null);
                                Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                                Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                                test.EndInvoke(result);
                                Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0), "AsyncWaitHandle should be signalled");
                            }
                            evt.Set();
                        });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                Assert.IsFalse(actionThreadIsThreadPoolThread, "ActionThread is a thread pool thread");
                Assert.IsTrue(threadPoolThreadIsThreadPoolThread, "ThreadPool thread is not a thread pool thread");
            }
        }

        [TestMethod]
        public void TestBeginInvokeExplicitWaitWithThreadPool()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                bool actionThreadIsThreadPoolThread = true;
                bool threadPoolThreadIsThreadPoolThread = false;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() =>
                    {
                        using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
                        {
                            actionThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                            ISynchronizeInvoke test = new GenericSynchronizingObject();
                            IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { threadPoolThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread; }), null);
                            Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                            Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                            result.AsyncWaitHandle.WaitOne();
                        }
                        evt.Set();
                    });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                Assert.IsFalse(actionThreadIsThreadPoolThread, "ActionThread is a thread pool thread");
                Assert.IsTrue(threadPoolThreadIsThreadPoolThread, "ThreadPool thread is not a thread pool thread");
            }
        }

        [TestMethod]
        public void TestBeginEndInvokeExceptionWithThreadPool()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                bool actionThreadIsThreadPoolThread = true;
                bool threadPoolThreadIsThreadPoolThread = false;
                Exception error = null;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() =>
                    {
                        using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
                        {
                            actionThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                            ISynchronizeInvoke test = new GenericSynchronizingObject();
                            IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { threadPoolThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread; throw new MyException(); }), null);
                            Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                            Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");

                            try
                            {
                                test.EndInvoke(result);
                            }
                            catch (TargetInvocationException ex)
                            {
                                error = ex;
                            }
                        }
                        evt.Set();
                    });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                Assert.IsInstanceOfType(error.InnerException, typeof(MyException), "Exception type not preserved");
                Assert.IsFalse(actionThreadIsThreadPoolThread, "ActionThread is a thread pool thread");
                Assert.IsTrue(threadPoolThreadIsThreadPoolThread, "ThreadPool thread is not a thread pool thread");
            }
        }

        [TestMethod]
        public void TestBeginEndPollingInvokeWithThreadPool()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                bool actionThreadIsThreadPoolThread = true;
                bool threadPoolThreadIsThreadPoolThread = false;
                using (ManualResetEvent evt = new ManualResetEvent(false))
                {
                    thread.Do(() =>
                    {
                        using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
                        {
                            actionThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                            ISynchronizeInvoke test = new GenericSynchronizingObject();
                            IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { threadPoolThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread; }), null);
                            Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
                            Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
                            while (!result.IsCompleted)
                            {
                                Thread.Sleep(20);
                            }
                            test.EndInvoke(result);
                        }
                        evt.Set();
                    });
                    Assert.IsTrue(evt.WaitOne(TimeSpan.FromMilliseconds(100)), "ActionThread did not perform action");
                }

                Assert.IsFalse(actionThreadIsThreadPoolThread, "ActionThread is a thread pool thread");
                Assert.IsTrue(threadPoolThreadIsThreadPoolThread, "ThreadPool thread is not a thread pool thread");
            }
        }

        private class MyException : Exception
        {
        }
    }
}
