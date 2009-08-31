using Nito.Async;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Reflection;

namespace UnitTests
{
    [TestClass]
    public class EventHandlersSynchronizeUnitTests
    {
        [TestMethod]
        public void TestSuccessfulAction()
        {
            bool sawAction = false;
            object state = new object();
            bool argsCancelled = true;
            Exception argsError = new Exception();
            object argsState = null;

            Sync.InvokeAndCallback(() => { sawAction = true; }, (x) =>
                {
                    argsCancelled = x.Cancelled;
                    argsError = x.Error;
                    argsState = x.UserState;
                }, state);

            Assert.IsTrue(sawAction, "Action did not run");
            Assert.IsFalse(argsCancelled, "Completed arguments indicate action was cancelled");
            Assert.IsNull(argsError, "Completed arguments indicate an error");
            Assert.AreSame(state, argsState, "User state not preserved");
        }

        [TestMethod]
        public void TestFailedAction()
        {
            bool sawAction = false;
            Exception actionException = new Exception();
            object state = new object();
            bool argsCancelled = true;
            Exception argsError = null;
            object argsState = null;

            Sync.InvokeAndCallback(() => { sawAction = true; throw actionException; }, (x) =>
            {
                argsCancelled = x.Cancelled;
                argsError = x.Error;
                argsState = x.UserState;
            }, state);

            Assert.IsTrue(sawAction, "Action did not run");
            Assert.IsFalse(argsCancelled, "Failed arguments indicate action was cancelled");
            Assert.AreSame(actionException, argsError, "Failed arguments did not preserve error");
            Assert.AreSame(state, argsState, "User state not preserved");
        }

        [TestMethod]
        public void TestSuccessfulFunc()
        {
            bool sawAction = false;
            object state = new object();
            bool argsCancelled = true;
            Exception argsError = new Exception();
            object argsState = null;
            int argsResult = 0;

            Sync.InvokeAndCallback(() => { sawAction = true; return 13; }, (x) =>
            {
                argsCancelled = x.Cancelled;
                argsError = x.Error;
                argsState = x.UserState;
                argsResult = x.Result;
            }, state);

            Assert.IsTrue(sawAction, "Action did not run");
            Assert.IsFalse(argsCancelled, "Completed arguments indicate action was cancelled");
            Assert.IsNull(argsError, "Completed arguments indicate an error");
            Assert.AreSame(state, argsState, "User state not preserved");
            Assert.AreEqual(13, argsResult, "Result not returned");
        }

        [TestMethod]
        public void TestFailedFunc()
        {
            bool sawAction = false;
            Exception actionException = new Exception();
            object state = new object();
            bool argsCancelled = true;
            Exception argsError = null;
            object argsState = null;
            Exception argsResultException = null;

            Sync.InvokeAndCallback(() => { sawAction = true; throw actionException; return 13; }, (x) =>
            {
                argsCancelled = x.Cancelled;
                argsError = x.Error;
                argsState = x.UserState;
                try
                {
                    Trace.WriteLine(x.Result);
                }
                catch (Exception ex)
                {
                    argsResultException = ex;
                }
            }, state);

            Assert.IsTrue(sawAction, "Action did not run");
            Assert.IsFalse(argsCancelled, "Failed arguments indicate action was cancelled");
            Assert.AreSame(actionException, argsError, "Failed arguments did not preserve error");
            Assert.AreSame(state, argsState, "User state not preserved");
            Assert.IsInstanceOfType(argsResultException, typeof(TargetInvocationException), "Failed arguments allowed reading result");
        }
    }
}
