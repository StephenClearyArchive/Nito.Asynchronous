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
        public void Action_Invoked_Runs()
        {
            bool sawAction = false;

            Sync.InvokeAndCallback(
                () => { sawAction = true; },
                (x) => { },
                null);

            Assert.IsTrue(sawAction, "Action did not run");
        }

        [TestMethod]
        public void Callback_ForSuccessfulAction_Runs()
        {
            bool sawCallback = false;

            Sync.InvokeAndCallback(
                () => { },
                (x) => { sawCallback = true; },
                null);

            Assert.IsTrue(sawCallback, "Callback did not run");
        }

        [TestMethod]
        public void CallbackArgs_ForSuccessfulAction_AreNotCancelled()
        {
            bool argsCancelled = true;

            Sync.InvokeAndCallback(
                () => { },
                (x) => { argsCancelled = x.Cancelled; }, null);

            Assert.IsFalse(argsCancelled, "Callback arguments indicate action was cancelled");
        }

        [TestMethod]
        public void CallbackArgs_ForSuccessfulAction_HaveNullError()
        {
            Exception argsError = new Exception();

            Sync.InvokeAndCallback(
                () => { },
                (x) => { argsError = x.Error; },
            null);

            Assert.IsNull(argsError, "Callback arguments indicate an error");
        }

        [TestMethod]
        public void CallbackArgs_ForSuccessfulAction_PreserveUserState()
        {
            object state = new object();
            object argsState = null;

            Sync.InvokeAndCallback(
                () => { },
                (x) => { argsState = x.UserState; },
                state);

            Assert.AreSame(state, argsState, "User state not preserved");
        }

        [TestMethod]
        public void Callback_ForFailedAction_Runs()
        {
            bool sawCallback = false;

            Sync.InvokeAndCallback(
                () => { throw new Exception(); },
                (x) => { sawCallback = true; },
                null);

            Assert.IsTrue(sawCallback, "Callback did not run");
        }

        [TestMethod]
        public void CallbackArgs_ForFailedAction_AreNotCancelled()
        {
            bool argsCancelled = true;

            Sync.InvokeAndCallback(
                () => { throw new Exception(); },
                (x) => { argsCancelled = x.Cancelled; }, null);

            Assert.IsFalse(argsCancelled, "Callback arguments indicate action was cancelled");
        }

        [TestMethod]
        public void CallbackArgs_ForFailedAction_PreserveError()
        {
            Exception actionException = new Exception();
            Exception argsError = null;

            Sync.InvokeAndCallback(
                () => { throw actionException; },
                (x) => { argsError = x.Error; },
            null);

            Assert.AreSame(actionException, argsError, "Failed arguments did not preserve error");
        }

        [TestMethod]
        public void CallbackArgs_ForFailedAction_PreserveUserState()
        {
            object state = new object();
            object argsState = null;

            Sync.InvokeAndCallback(
                () => { throw new Exception(); },
                (x) => { argsState = x.UserState; },
                state);

            Assert.AreSame(state, argsState, "User state not preserved");
        }

        [TestMethod]
        public void Func_Invoked_Runs()
        {
            bool sawAction = false;

            Sync.InvokeAndCallback(
                () => { sawAction = true; return 13; },
                (x) => { },
                null);

            Assert.IsTrue(sawAction, "Action did not run");
        }

        [TestMethod]
        public void Callback_ForSuccessfulFunc_Runs()
        {
            bool sawCallback = false;

            Sync.InvokeAndCallback(
                () => { return 13; },
                (x) => { sawCallback = true; },
                null);

            Assert.IsTrue(sawCallback, "Callback did not run");
        }

        [TestMethod]
        public void CallbackArgs_ForSuccessfulFunc_AreNotCancelled()
        {
            bool argsCancelled = true;

            Sync.InvokeAndCallback(
                () => { return 13; },
                (x) => { argsCancelled = x.Cancelled; }, null);

            Assert.IsFalse(argsCancelled, "Callback arguments indicate action was cancelled");
        }

        [TestMethod]
        public void CallbackArgs_ForSuccessfulFunc_HaveNullError()
        {
            Exception argsError = new Exception();

            Sync.InvokeAndCallback(
                () => { return 13; },
                (x) => { argsError = x.Error; },
            null);

            Assert.IsNull(argsError, "Callback arguments indicate an error");
        }

        [TestMethod]
        public void CallbackArgs_ForSuccessfulFunc_PreserveUserState()
        {
            object state = new object();
            object argsState = null;

            Sync.InvokeAndCallback(
                () => { return 13; },
                (x) => { argsState = x.UserState; },
                state);

            Assert.AreSame(state, argsState, "User state not preserved");
        }

        [TestMethod]
        public void CallbackArgs_ForSuccessfulFunc_PreserveResult()
        {
            int result = 0;

            Sync.InvokeAndCallback(
                () => { return 13; },
                (x) => { result = x.Result; },
                null);

            Assert.AreEqual(13, result, "Result not preserved");
        }

        [TestMethod]
        public void Callback_ForFailedFunc_Runs()
        {
            bool sawCallback = false;

            Sync.InvokeAndCallback(
                new Func<int>(() => { throw new Exception(); }),
                (x) => { sawCallback = true; },
                null);

            Assert.IsTrue(sawCallback, "Callback did not run");
        }

        [TestMethod]
        public void CallbackArgs_ForFailedFunc_AreNotCancelled()
        {
            bool argsCancelled = true;

            Sync.InvokeAndCallback(
                new Func<int>(() => { throw new Exception(); }),
                (x) => { argsCancelled = x.Cancelled; }, null);

            Assert.IsFalse(argsCancelled, "Callback arguments indicate action was cancelled");
        }

        [TestMethod]
        public void CallbackArgs_ForFailedFunc_PreserveError()
        {
            Exception actionException = new Exception();
            Exception argsError = null;

            Sync.InvokeAndCallback(
                new Func<int>(() => { throw actionException; }),
                (x) => { argsError = x.Error; },
            null);

            Assert.AreSame(actionException, argsError, "Failed arguments did not preserve error");
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void CallbackArgs_ForFailedFunc_ReadingResult_ThrowsTargetInvocationException()
        {
            int result = 0;

            Sync.InvokeAndCallback(
                new Func<int>(() => { throw new Exception(); }),
                (x) => { result = x.Result; },
            null);
        }

        [TestMethod]
        public void CallbackArgs_ForFailedFunc_PreserveUserState()
        {
            object state = new object();
            object argsState = null;

            Sync.InvokeAndCallback(
                new Func<int>(() => { throw new Exception(); }),
                (x) => { argsState = x.UserState; },
                state);

            Assert.AreSame(state, argsState, "User state not preserved");
        }
    }
}
