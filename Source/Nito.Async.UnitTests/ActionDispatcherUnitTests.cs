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
    public class ActionDispatcherUnitTests
    {
        [TestMethod]
        public void TestExitActionOnEmptyQueue()
        {
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                Assert.IsFalse(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread exited before ActionDispatcher.QueueExit");
                dispatcher.QueueExit();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
            }
        }

        [TestMethod]
        public void TestSingleAction()
        {
            bool sawAction = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueAction(() => { sawAction = true; });
                Assert.IsFalse(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread exited before ActionDispatcher.QueueExit");
                dispatcher.QueueExit();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
                Assert.IsTrue(sawAction, "ActionDispatcher did not execute action");
            }
        }

        [TestMethod]
        public void TestSingleActionWithoutTimeouts()
        {
            bool sawAction = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueAction(() => { sawAction = true; });
                dispatcher.QueueExit();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
                Assert.IsTrue(sawAction, "ActionDispatcher did not execute action");
            }
        }

        [TestMethod]
        public void TestMultipleActionsInQueueBeforeThreadStart()
        {
            bool sawAction1 = false;
            bool sawAction2 = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                dispatcher.QueueAction(() => { sawAction1 = true; });
                dispatcher.QueueAction(() => { sawAction2 = true; });
                thread.Start();
                Assert.IsFalse(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread exited before ActionDispatcher.QueueExit");
                dispatcher.QueueExit();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
                Assert.IsTrue(sawAction1, "ActionDispatcher did not execute the first action");
                Assert.IsTrue(sawAction2, "ActionDispatcher did not execute the second action");
            }
        }

        [TestMethod]
        public void TestMultipleActionsAndExitInQueueBeforeThreadStart()
        {
            bool sawAction1 = false;
            bool sawAction2 = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                dispatcher.QueueAction(() => { sawAction1 = true; });
                dispatcher.QueueAction(() => { sawAction2 = true; });
                dispatcher.QueueExit();
                thread.Start();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
                Assert.IsTrue(sawAction1, "ActionDispatcher did not execute the first action");
                Assert.IsTrue(sawAction2, "ActionDispatcher did not execute the second action");
            }
        }

        [TestMethod]
        public void TestCurrentPropertyInsideAction()
        {
            bool currentIsOk = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueAction(() => { if (ActionDispatcher.Current == dispatcher) currentIsOk = true; });
                dispatcher.QueueExit();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
                Assert.IsTrue(currentIsOk, "ActionDispatcher did not set Current correctly for action");
            }
        }

        [TestMethod]
        public void TestCurrentPropertyOutsideAction()
        {
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();

                Assert.IsNull(ActionDispatcher.Current, "ActionDispatcher.Current is not null outside Run()");

                dispatcher.QueueExit();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
            }
        }

        [TestMethod]
        public void TestSynchronizationContext()
        {
            bool contextIsOk = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueAction(() =>
                {
                    if (SynchronizationContext.Current != null &&
                        SynchronizationContext.Current.GetType() == typeof(ActionDispatcherSynchronizationContext))
                        contextIsOk = true;
                });
                dispatcher.QueueExit();
                Assert.IsTrue(thread.Join(TimeSpan.FromMilliseconds(100)), "Thread did not exit after ActionDispatcher.QueueExit");
                Assert.IsTrue(contextIsOk, "ActionDispatcher did not set SynchronizationContext.Current correctly for action");
            }
        }
    }
}
