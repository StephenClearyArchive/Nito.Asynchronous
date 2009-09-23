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
        public void CancelledProperty_ForFailedArgs_IsFalse()
        {
            var test = new AsyncResultEventArgs<int>(new Exception());
            Assert.IsFalse(test.Cancelled, "Failed event args should not be cancelled");
        }

        [TestMethod]
        public void CancelledProperty_ForFailedArgsFullConstructor_IsFalse()
        {
            var test = new AsyncResultEventArgs<int>(0, new Exception(), false, null);
            Assert.IsFalse(test.Cancelled, "Failed event args should not be cancelled");
        }

        [TestMethod]
        public void Exception_ForFailedArgs_IsPreserved()
        {
            Exception error = new Exception();
            var test = new AsyncResultEventArgs<int>(error);
            Assert.AreSame(error, test.Error, "Failed event args did not preserve exception");
        }

        [TestMethod]
        public void Exception_ForFailedArgsFullConstructor_IsPreserved()
        {
            Exception error = new Exception();
            var test = new AsyncResultEventArgs<int>(0, error, false, null);
            Assert.AreSame(error, test.Error, "Failed event args did not preserve exception");
        }

        [TestMethod]
        public void UserState_ForFailedArgsFullConstructor_IsPreserved()
        {
            object state = new object();
            var test = new AsyncResultEventArgs<int>(0, new Exception(), false, state);
            Assert.AreSame(state, test.UserState, "Failed event args did not preserve user state");
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void ReadingResult_ForFailedArgs_ThrowsTargetInvocationException()
        {
            var test = new AsyncResultEventArgs<int>(new Exception());
            Trace.WriteLine(test.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void ReadingResult_ForFailedArgsFullConstructor_ThrowsTargetInvocationException()
        {
            var test = new AsyncResultEventArgs<int>(0, new Exception(), false, null);
            Trace.WriteLine(test.Result);
        }

        [TestMethod]
        public void CancelledProperty_ForCancelledArgsFullConstructor_IsTrue()
        {
            var test = new AsyncResultEventArgs<int>(0, null, true, null);
            Assert.IsTrue(test.Cancelled, "Cancelled event args should be cancelled");
        }

        [TestMethod]
        public void Error_ForCancelledArgsFullConstructor_IsNull()
        {
            var test = new AsyncResultEventArgs<int>(0, null, true, null);
            Assert.IsNull(test.Error, "Cancelled event args should not preserve exception");
        }

        [TestMethod]
        public void UserState_ForCancelledArgsFullConstructor_IsPreserved()
        {
            object state = new object();
            var test = new AsyncResultEventArgs<int>(0, null, true, state);
            Assert.AreSame(state, test.UserState, "Cancelled event args did not preserve user state");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadingResult_ForCancelledArgsFullConstructor_ThrowsInvalidOperationException()
        {
            var test = new AsyncResultEventArgs<int>(0, null, true, null);
            Trace.WriteLine(test.Result);
        }

        [TestMethod]
        public void CancelledProperty_ForSuccessfulArgs_IsFalse()
        {
            var test = new AsyncResultEventArgs<int>(13);
            Assert.IsFalse(test.Cancelled, "Successful event args should not be cancelled");
        }

        [TestMethod]
        public void CancelledProperty_ForSuccessfulArgsFullConstructor_IsFalse()
        {
            var test = new AsyncResultEventArgs<int>(13, null, false, null);
            Assert.IsFalse(test.Cancelled, "Successful event args should not be cancelled");
        }

        [TestMethod]
        public void Error_ForSuccessfulArgs_IsNull()
        {
            var test = new AsyncResultEventArgs<int>(13);
            Assert.IsNull(test.Error, "Successful event args should not preserve exception");
        }

        [TestMethod]
        public void Error_ForSuccessfulArgsFullConstructor_IsNull()
        {
            var test = new AsyncResultEventArgs<int>(13, null, false, null);
            Assert.IsNull(test.Error, "Successful event args should not preserve exception");
        }

        [TestMethod]
        public void UserState_ForSuccessfulArgsFullConstructor_IsPreserved()
        {
            object state = new object();
            var test = new AsyncResultEventArgs<int>(13, null, false, state);
            Assert.AreSame(state, test.UserState, "Successful event args did not preserve user state");
        }

        [TestMethod]
        public void Result_ForSuccessfulArgs_IsPreserved()
        {
            var test = new AsyncResultEventArgs<int>(13);
            Assert.AreEqual(13, test.Result, "Successful event args did not preserve result");
        }

        [TestMethod]
        public void Result_ForSuccessfulArgsFullConstructor_IsPreserved()
        {
            var test = new AsyncResultEventArgs<int>(13, null, false, null);
            Assert.AreEqual(13, test.Result, "Successful event args did not preserve result");
        }
    }
}
