using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Windows.Threading;

namespace FrameworkTests
{
    /// <summary>
    /// Tests assumptions made regarding the .NET framework that are not documented on MSDN
    /// </summary>
    [TestClass]
    public class FrameworkAssumptions
    {
        [TestMethod]
        public void DefaultSyncContextPost_IsNotReentrant()
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            {
                SynchronizationContext syncContext = new SynchronizationContext();
                syncContext.Post((state) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                completed.WaitOne();

                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadID, "SynchronizationContext.Post is reentrant");
            }
        }

        [TestMethod]
        public void DefaultSyncContextSend_IsReentrant()
        {
            int threadID = ~Thread.CurrentThread.ManagedThreadId;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            {
                SynchronizationContext syncContext = new SynchronizationContext();
                syncContext.Send((state) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                completed.WaitOne();

                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, threadID, "SynchronizationContext.Send is not reentrant");
            }
        }

        /// <summary>
        /// This method will fail if running Visual Studio as non-Administrator.
        /// </summary>
        [TestMethod]
        [Timeout(20000)]
        [UrlToTest("http://localhost/")]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost(@"..\Test.FakeWebApplication", "/")]
        public void AspNetSyncContext_Post_IsReentrant()
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            int poolThreadID = Thread.CurrentThread.ManagedThreadId;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            {
                SynchronizationContext syncContext = SynchronizationContext.Current;
                ThreadPool.QueueUserWorkItem((state1) =>
                {
                    poolThreadID = Thread.CurrentThread.ManagedThreadId;
                    syncContext.Post((state2) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                });
                completed.WaitOne();

                Assert.AreEqual(poolThreadID, threadID, "AspNetSynchronizationContext.Post is not reentrant");
            }
        }

        /// <summary>
        /// This method will fail if running Visual Studio as non-Administrator.
        /// </summary>
        [TestMethod]
        [Timeout(20000)]
        [UrlToTest("http://localhost/")]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost(@"..\Test.FakeWebApplication", "/")]
        public void AspNetSyncContext_Send_IsReentrant()
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            int poolThreadID = Thread.CurrentThread.ManagedThreadId;

            using (ManualResetEvent completed = new ManualResetEvent(false))
            {
                SynchronizationContext syncContext = SynchronizationContext.Current;
                ThreadPool.QueueUserWorkItem((state1) =>
                {
                    poolThreadID = Thread.CurrentThread.ManagedThreadId;
                    syncContext.Send((state2) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                });
                completed.WaitOne();

                Assert.AreEqual(poolThreadID, threadID, "AspNetSynchronizationContext.Send is not reentrant");
            }
        }

        [TestMethod]
        public void WindowsFormsSyncContextPost_IsNotReentrant()
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            SynchronizationContext syncContext = null;

            using (AutoResetEvent completed = new AutoResetEvent(false))
            using (ManualResetEvent exit = new ManualResetEvent(false))
            {
                Thread thread = new Thread((state) =>
                {
                    // Force creation of a window handle, thus creating a WindowsFormsSynchronizationContext
                    Control control = new Control();
                    IntPtr handle = control.Handle;

                    // Capture the WindowsFormsSynchronizationContext for the test thread
                    syncContext = SynchronizationContext.Current;
                    completed.Set();

                    // Wait for an exit signal, processing window events
                    //  This is the poor man's version of Application.Run()
                    while (!exit.WaitOne(100))
                    {
                        Application.DoEvents();
                    }

                    completed.Set();
                });

                thread.Start();
                completed.WaitOne();

                syncContext.Post((state) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                completed.WaitOne();
                exit.Set();
                completed.WaitOne();

                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadID, "WindowsFormsSynchronizationContext.Post is reentrant");
            }
        }

        [TestMethod]
        public void WindowsFormsSyncContextSend_IsNotReentrant()
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            SynchronizationContext syncContext = null;

            using (AutoResetEvent completed = new AutoResetEvent(false))
            using (ManualResetEvent exit = new ManualResetEvent(false))
            {
                Thread thread = new Thread((state) =>
                {
                    // Force creation of a window handle, thus creating a WindowsFormsSynchronizationContext
                    Control control = new Control();
                    IntPtr handle = control.Handle;

                    // Capture the WindowsFormsSynchronizationContext for the test thread
                    syncContext = SynchronizationContext.Current;
                    completed.Set();

                    // Wait for an exit signal, processing window events
                    //  This is the poor man's version of Application.Run()
                    while (!exit.WaitOne(100))
                    {
                        Application.DoEvents();
                    }

                    completed.Set();
                });

                thread.Start();
                completed.WaitOne();

                syncContext.Send((state) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                completed.WaitOne();
                exit.Set();
                completed.WaitOne();

                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadID, "WindowsFormsSynchronizationContext.Send is reentrant");
            }
        }

        [TestMethod]
        public void DispatcherSyncContextPost_IsNotReentrant()
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            SynchronizationContext syncContext = null;
            Dispatcher dispatcher = null;

            using (AutoResetEvent completed = new AutoResetEvent(false))
            {
                Thread thread = new Thread((state) =>
                {
                    // Force creation of a Dispatcher and then capture the DispatcherSynchronizationContext
                    dispatcher = Dispatcher.CurrentDispatcher;
                    completed.Set();

                    // Process the dispatcher queue until the test thread causes us to exit
                    Dispatcher.Run();
                });

                thread.Start();
                completed.WaitOne();

                // Capture the DispatcherSynchronizationContext
                dispatcher.Invoke((MethodInvoker)(() => { syncContext = SynchronizationContext.Current; }), null);

                syncContext.Post((state) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                completed.WaitOne();
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);

                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadID, "DispatcherSynchronizationContext.Post is reentrant");
            }
        }

        [TestMethod]
        public void DispatcherSyncContextSend_IsNotReentrant()
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            SynchronizationContext syncContext = null;
            Dispatcher dispatcher = null;

            using (AutoResetEvent completed = new AutoResetEvent(false))
            {
                Thread thread = new Thread((state) =>
                {
                    // Force creation of a Dispatcher and then capture the DispatcherSynchronizationContext
                    dispatcher = Dispatcher.CurrentDispatcher;
                    completed.Set();

                    // Process the dispatcher queue until the test thread causes us to exit
                    Dispatcher.Run();
                });

                thread.Start();
                completed.WaitOne();

                // Capture the DispatcherSynchronizationContext
                dispatcher.Invoke((MethodInvoker)(() => { syncContext = SynchronizationContext.Current; }), null);

                syncContext.Send((state) => { threadID = Thread.CurrentThread.ManagedThreadId; completed.Set(); }, null);
                completed.WaitOne();
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);

                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadID, "DispatcherSynchronizationContext.Post is reentrant");
            }
        }
    }
}
