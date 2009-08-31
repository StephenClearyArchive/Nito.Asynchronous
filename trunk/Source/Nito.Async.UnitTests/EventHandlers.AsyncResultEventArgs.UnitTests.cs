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
    public class AsyncResultEventArgsUnitTests
    {
        [TestMethod]
        public void TestFailureResult()
        {
            Exception error = new Exception();

            var test = new AsyncResultEventArgs<int>(error);
            Assert.IsFalse(test.Cancelled, "Failed event args should not be cancelled");
            Assert.AreSame(error, test.Error, "Failed event args did not preserve exception");
        }

        [TestMethod]
        public void TestFailureResultWithUserState()
        {
            Exception error = new Exception();
            object state = new object();

            var test = new AsyncResultEventArgs<int>(0, error, false, state);
            Assert.IsFalse(test.Cancelled, "Failed event args should not be cancelled");
            Assert.AreSame(error, test.Error, "Failed event args did not preserve exception");
            Assert.AreSame(state, test.UserState, "Failed event args did not preserve user state");
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void TestFailureResultReadingResult()
        {
            var test = new AsyncResultEventArgs<int>(new Exception());
            Trace.WriteLine(test.Result);
        }

        [TestMethod]
        public void TestCancelledResult()
        {
            object state = new object();

            var test = new AsyncResultEventArgs<int>(0, null, true, state);
            Assert.IsTrue(test.Cancelled, "Cancelled event args should be cancelled");
            Assert.IsNull(test.Error, "Cancelled event args should not preserve exception");
            Assert.AreSame(state, test.UserState, "Cancelled event args did not preserve user state");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestCancelledReadingResult()
        {
            var test = new AsyncResultEventArgs<int>(0, null, true, null);
            Trace.WriteLine(test.Result);
        }

        [TestMethod]
        public void TestSuccessfulResult()
        {
            object state = new object();

            var test = new AsyncResultEventArgs<int>(13, null, false, state);
            Assert.IsFalse(test.Cancelled, "Successful event args should not be cancelled");
            Assert.IsNull(test.Error, "Successful event args should not preserve exception");
            Assert.AreSame(state, test.UserState, "Successful event args did not preserve user state");
            Assert.AreEqual(13, test.Result, "Successful event args did not preserve result");
        }
    }
}
