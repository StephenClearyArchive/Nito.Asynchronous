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
        public void Run_WithoutExitAction_DoesNotReturn()
        {
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
       
                bool threadExited = thread.Join(TimeSpan.FromMilliseconds(100));
                Assert.IsFalse(threadExited, "Thread exited before ActionDispatcher.QueueExit");

                dispatcher.QueueExit();
                thread.Join();
            }
        }

        [TestMethod]
        public void ExitAction_OnEmptyQueueAfterThreadStarted_CausesRunToReturn()
        {
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueExit();

                bool threadExited = thread.Join(TimeSpan.FromMilliseconds(100));
                Assert.IsTrue(threadExited, "Thread did not exit after ActionDispatcher.QueueExit");
            }
        }

        [TestMethod]
        public void ExitAction_OnEmptyQueueBeforeThreadStarted_CausesRunToReturn()
        {
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                dispatcher.QueueExit();
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();

                bool threadExited = thread.Join(TimeSpan.FromMilliseconds(100));
                Assert.IsTrue(threadExited, "Thread did not exit after ActionDispatcher.QueueExit");
            }
        }

        [TestMethod]
        public void Action_QueuedToActionDispatcher_IsExecutedByRun()
        {
            bool sawAction = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueAction(() => { sawAction = true; });
                dispatcher.QueueExit();
                thread.Join();

                Assert.IsTrue(sawAction, "ActionDispatcher did not execute action");
            }
        }

        [TestMethod]
        public void MultipleActions_QueuedBeforeThreadStart_AreExecutedByRun()
        {
            bool sawAction1 = false;
            bool sawAction2 = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                dispatcher.QueueAction(() => { sawAction1 = true; });
                dispatcher.QueueAction(() => { sawAction2 = true; });
                thread.Start();
                dispatcher.QueueExit();
                thread.Join();

                Assert.IsTrue(sawAction1, "ActionDispatcher did not execute the first action");
                Assert.IsTrue(sawAction2, "ActionDispatcher did not execute the second action");
            }
        }

        [TestMethod]
        public void Action_QueuedAfterExitAction_IsNotExecutedByRun()
        {
            bool sawAction = false;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                dispatcher.QueueExit();
                dispatcher.QueueAction(() => { sawAction = true; });
                thread.Start();
                thread.Join();

                Assert.IsFalse(sawAction, "ActionDispatcher did execute the action");
            }
        }

        [TestMethod]
        public void Current_FromInsideAction_IsActionDispatcherForThatAction()
        {
            ActionDispatcher innerDispatcher = null;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueAction(() => { innerDispatcher = ActionDispatcher.Current; });
                dispatcher.QueueExit();
                thread.Join();

                Assert.AreSame(dispatcher, innerDispatcher, "ActionDispatcher did not set Current correctly for action");
            }
        }

        [TestMethod]
        public void Current_FromOutsideAction_IsNull()
        {
            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();

                Assert.IsNull(ActionDispatcher.Current, "ActionDispatcher.Current is not null outside Run()");

                dispatcher.QueueExit();
                thread.Join();
            }
        }

        [TestMethod]
        public void CurrentSynchronizationContext_FromInsideAction_IsActionDispatcherSynchronizationContext()
        {
            SynchronizationContext innerContext = null;

            using (ActionDispatcher dispatcher = new ActionDispatcher())
            {
                Thread thread = new Thread(() => dispatcher.Run());
                thread.Start();
                dispatcher.QueueAction(() => { innerContext = SynchronizationContext.Current; });
                dispatcher.QueueExit();
                thread.Join();

                Assert.IsInstanceOfType(innerContext, typeof(ActionDispatcherSynchronizationContext), "ActionDispatcher did not set SynchronizationContext.Current correctly for action");
            }
        }
    }
}
