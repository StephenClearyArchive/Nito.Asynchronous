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
        public void ActionThreadGSO_WithinActionThread_DoesNotRequireInvoke()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture InvokeRequired from the ActionThread's context
                bool nestedInvokeRequired = thread.DoGet(() => { return new GenericSynchronizingObject().InvokeRequired; });
                
                Assert.IsFalse(nestedInvokeRequired, "GenericSynchronizingObject does require invoke within ActionThread");
            }
        }

        [TestMethod]
        public void ActionThreadGSO_OutsideActionThread_DoesRequireInvoke()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });
                
                Assert.IsTrue(test.InvokeRequired, "GenericSynchronizingObject does not require invoke for ActionThread");
            }
        }

        [TestMethod]
        public void Action_InvokedThroughActionThreadGSO_Runs()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                test.Invoke((MethodInvoker)(() => { sawAction = true; }), null);

                Assert.AreEqual(true, sawAction, "GenericSynchronizingObject.Invoke did not execute action");
            }
        }

        [TestMethod]
        public void Action_InvokedThroughActionThreadGSO_RunsOnTheActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                test.Invoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; }), null);

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.Invoke did not synchronize");
            }
        }

        [TestMethod]
        public void Action_InvokedThroughActionThreadGSO_ReceivesParameters()
        {
            object parameter = new object();
            object argument = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                test.Invoke((Action<object>)((arg) => { argument = arg; }), new [] { parameter });

                Assert.AreSame(parameter, argument, "GenericSynchronizingObject.Invoke did not pass parameter");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void FailingAction_InvokedThroughActionThreadGSO_ThrowsTargetInvocationException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                test.Invoke((MethodInvoker)(() => { throw new Exception(); }), null);
            }
        }

        [TestMethod]
        public void FailingAction_InvokedThroughActionThreadGSO_PreservesExceptionAsInnerException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                Exception errorToThrow = new MyException();
                Exception innerErrorCaught = null;
                try
                {
                    test.Invoke((MethodInvoker)(() => { throw errorToThrow; }), null);
                }
                catch (TargetInvocationException ex)
                {
                    innerErrorCaught = ex.InnerException;
                }

                Assert.AreSame(errorToThrow, innerErrorCaught, "Exception not preserved");
            }
        }

        [TestMethod]
        public void Action_BeginInvokeThroughActionThreadGSO_Runs()
        {
            bool sawAction = false;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { sawAction = true; }), null);
                test.EndInvoke(result);

                Assert.IsTrue(sawAction, "GenericSynchronizingObject.BeginInvoke did not execute action");
            }
        }

        [TestMethod]
        public void Action_BeginInvokeThroughActionThreadGSO_RunsOnTheActionThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                int actionThreadId = Thread.CurrentThread.ManagedThreadId;
                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { actionThreadId = Thread.CurrentThread.ManagedThreadId; }), null);
                test.EndInvoke(result);

                Assert.AreEqual(thread.ManagedThreadId, actionThreadId, "GenericSynchronizingObject.BeginInvoke did not synchronize");
            }
        }

        [TestMethod]
        public void Action_BeginInvokeThroughGSO_ReceivesParameters()
        {
            object parameter = new object();
            object argument = null;

            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((Action<object>)((arg) => { argument = arg; }), new [] { parameter });
                test.EndInvoke(result);

                Assert.AreSame(parameter, argument, "GenericSynchronizingObject.BeginInvoke did not pass parameter");
            }
        }

        [TestMethod]
        public void AsyncResult_FromBeginInvoke_HasNullAsyncState()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                Assert.IsNull(result.AsyncState, "IAsyncResult.AsyncState is not used and should be null");
            }
        }

        [TestMethod]
        public void AsyncResult_FromBeginInvoke_DidNotCompleteSynchronously()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                Assert.IsFalse(result.CompletedSynchronously, "IAsyncResult.CompletedSynchronously is not used and should be false");
            }
        }

        [TestMethod]
        public void AsyncResultWaitHandle_AfterEndInvoke_IsSignalled()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                test.EndInvoke(result);
                bool signalled = result.AsyncWaitHandle.WaitOne(0);
                Assert.IsTrue(signalled, "AsyncWaitHandle should be signalled");
            }
        }

        /// <summary>
        /// Added for code coverage.
        /// </summary>
        [TestMethod]
        public void AsyncResultWaitHandle_AfterEndInvoke_IsSignalledOnSecondWait()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                test.EndInvoke(result);
                result.AsyncWaitHandle.WaitOne(0);

                bool signalled = result.AsyncWaitHandle.WaitOne(0);
                Assert.IsTrue(signalled, "AsyncWaitHandle should be signalled");
            }
        }

        /// <summary>
        /// Added for code coverage.
        /// </summary>
        [TestMethod]
        public void AsyncResult_AfterWait_ReturnsImmediatelyFromEndInvoke()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                result.AsyncWaitHandle.WaitOne(100);
                test.EndInvoke(result);
            }
        }

        /// <summary>
        /// Added for code coverage.
        /// </summary>
        [TestMethod]
        public void AsyncResult_AfterEndInvoke_ReturnsImmediatelyFromEndInvoke()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                test.EndInvoke(result);
                test.EndInvoke(result);
            }
        }

        [TestMethod]
        public void AsyncResultWaitHandle_FromBeginInvoke_IsWaitable()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                bool signalled = result.AsyncWaitHandle.WaitOne(100);
                Assert.IsTrue(signalled, "AsyncWaitHandle should be signalled");
            }
        }

        [TestMethod]
        public void AsyncResultWaitHandle_FromBeginInvoke_IsPollable()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                ISynchronizeInvoke test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { }), null);
                while (!result.IsCompleted)
                {
                    Thread.Sleep(20);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void FailingAction_BeginInvokedThroughActionThreadGSO_ThrowsTargetInvocationException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { throw new Exception(); }), null);
                test.EndInvoke(result);
            }
        }

        [TestMethod]
        public void FailingAction_BeginInvokedThroughActionThreadGSO_PreservesExceptionAsInnerException()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                // Capture GenericSynchronizingObject
                GenericSynchronizingObject test = thread.DoGet(() => { return new GenericSynchronizingObject(); });

                Exception errorToThrow = new MyException();
                Exception innerErrorCaught = null;
                IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { throw errorToThrow; }), null);
                try
                {
                    test.EndInvoke(result);
                }
                catch (TargetInvocationException ex)
                {
                    innerErrorCaught = ex.InnerException;
                }

                Assert.AreSame(errorToThrow, innerErrorCaught, "Exception not preserved");
            }
        }

        [TestMethod]
        public void GSO_FromThreadPool_DoesNotRequireInvoke()
        {
            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                GenericSynchronizingObject test = new GenericSynchronizingObject();
                Assert.IsFalse(test.InvokeRequired, "GenericSynchronizingObject does require invoke within thread pool");
            }
        }

        [TestMethod]
        public void ThreadPoolGSO_Invoked_ExecutesSynchronously()
        {
            int threadId = ~Thread.CurrentThread.ManagedThreadId;

            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                GenericSynchronizingObject test = new GenericSynchronizingObject();
                test.Invoke((MethodInvoker)(() => { threadId = Thread.CurrentThread.ManagedThreadId; }), null);
            }

            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ThreadPool invoke did not operate synchronously");
        }

        [TestMethod]
        public void ThreadPoolGSOFromNullSyncContext_Invoked_ExecutesSynchronously()
        {
            int threadId = ~Thread.CurrentThread.ManagedThreadId;

            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(null))
            {
                GenericSynchronizingObject test = new GenericSynchronizingObject();
                test.Invoke((MethodInvoker)(() => { threadId = Thread.CurrentThread.ManagedThreadId; }), null);
            }

            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, threadId, "ThreadPool invoke did not operate synchronously");
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void FailingAction_InvokedThroughThreadPoolGSO_ThrowsTargetInvocationException()
        {
            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                GenericSynchronizingObject test = new GenericSynchronizingObject();
                test.Invoke((MethodInvoker)(() => { throw new MyException(); }), null);
            }
        }

        [TestMethod]
        public void FailingAction_InvokedThroughThreadPoolGSO_PreservesExceptionAsInnerException()
        {
            using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
            {
                GenericSynchronizingObject test = new GenericSynchronizingObject();

                Exception errorToThrow = new MyException();
                Exception innerErrorCaught = null;
                try
                {
                    test.Invoke((MethodInvoker)(() => { throw errorToThrow; }), null);
                }
                catch (TargetInvocationException ex)
                {
                    innerErrorCaught = ex.InnerException;
                }

                Assert.AreSame(errorToThrow, innerErrorCaught, "Exception not preserved");
            }
        }

        [TestMethod]
        public void Action_BeginInvokeThroughThreadPoolGSO_Runs()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                bool sawAction = false;
                thread.DoSynchronously(() =>
                {
                    using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
                    {
                        GenericSynchronizingObject test = new GenericSynchronizingObject();
                        IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { sawAction = true; }), null);
                        test.EndInvoke(result);
                    }
                });

                Assert.IsTrue(sawAction, "BeginInvoke did not execute action.");
            }
        }

        [TestMethod]
        public void Action_BeginInvokeThroughThreadPoolGSO_RunsOnAThreadPoolThread()
        {
            using (ActionThread thread = new ActionThread())
            {
                thread.Start();

                bool threadPoolThreadIsThreadPoolThread = false;
                thread.DoSynchronously(() =>
                    {
                        using (ScopedSynchronizationContext x = new ScopedSynchronizationContext(new SynchronizationContext()))
                        {
                            GenericSynchronizingObject test = new GenericSynchronizingObject();
                            IAsyncResult result = test.BeginInvoke((MethodInvoker)(() => { threadPoolThreadIsThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread; }), null);
                            test.EndInvoke(result);
                        }
                    });

                Assert.IsTrue(threadPoolThreadIsThreadPoolThread, "ThreadPool thread is not a thread pool thread");
            }
        }

        private class MyException : Exception
        {
        }
    }
}
