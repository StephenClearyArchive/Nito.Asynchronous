using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;

// Copyright 2009 by Nito Programs.

namespace Nito.Async
{
    /// <summary>
    /// Allows objects that use <see cref="ISynchronizeInvoke"/> (usually using a property named SynchronizingObject) to synchronize to a
    /// generic <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>If an exception is raied by the delegate passed to <see cref="Invoke"/> or <see cref="BeginInvoke"/>, then that exception is propogated back to the caller. The stack trace of the exception is wiped out in the process, but is saved in <see cref="Exception.Data"/> under the key "Previous Stack Trace".</para>
    /// <para>This class does not invoke <see cref="SynchronizationContext.OperationStarted"/> or <see cref="SynchronizationContext.OperationCompleted"/>, so for some synchronization contexts, these may need to be called explicitly in addition to using this class. ASP.NET do require them to be called; Windows Forms, WPF, and <see cref="ActionDispatcher"/> do not.</para>
    /// </remarks>
    public sealed class GenericSynchronizingObject : ISynchronizeInvoke
    {
        // The thread that synchronizes the operation is the "source" thread. This is the thread that has the object with a SynchronizingObject
        //  property.
        // The thread that executes the operation is the "target" thread. This is the thread that creates the SynchronizationContext.

        /// <summary>
        /// The captured synchronization context.
        /// </summary>
        private SynchronizationContext synchronizationContext;

        /// <summary>
        /// Creates a new synchronizing object, binding to <see cref="SynchronizationContext.Current">SynchronizationContext.Current</see>.
        /// </summary>
        // Always runs within the target thread context.
        public GenericSynchronizingObject()
        {
            synchronizationContext = SynchronizationContext.Current;
            if (synchronizationContext == null)
                synchronizationContext = new SynchronizationContext();
        }

        /// <summary>
        /// A helper object that just wraps the return value, when the delegate is invoked synchronously.
        /// </summary>
        private sealed class ReturnValue
        {
            public object ret;
            public Exception error;
        }

        // Note that our implementation of AsyncResult differs significantly from that presented in "Implementing the CLR Asynchronous
        //  Programming Model", MSDN 2007-03, Jeffrey Richter. They take a lock-free approach, while we use explicit locks.
        // Some of the major differences:
        //  1) Ours is simplified, not handling synchronous completion, user-defined states, or callbacks.
        //  2) We use a lock instead of interlocked variables for these reasons:
        //    a) Locks tend to scale better as the number of CPUs increase (they only affect a single thread while interlocked affects
        //       the instruction cache of every CPU).
        //    b) Code is easier to read and understand that there are no race conditions.
        //    c) We do handle the situation where a WaitHandle is created earlier but not immediately used for synchronization. This is
        //       rare in practice.
        //    d) Race conditions are handled more efficiently. This is also rare in practice.
        //  3) However, we do require the allocation of a lock for every AsyncResult instance, so our solution does use more resources.

        /// <summary>
        /// A helper object that holds the return value and also allows waiting for the asynchronous completion of a delegate.
        /// Note that calling <see cref="ISynchronizeInvoke.EndInvoke"/> is optional, and this class is optimized for that common use case.
        /// </summary>
        private sealed class AsyncResult : IAsyncResult
        {
            /// <summary>
            /// The return value. Must be set before calling <see cref="Done"/>.
            /// </summary>
            public object ReturnValue { get; set; }

            /// <summary>
            /// The error. Must be set before calling <see cref="Done"/>.
            /// </summary>
            public Exception Error { get; set; }

            /// <summary>
            /// The wait handle, which may be null. Writes are synchronized using Interlocked access.
            /// </summary>
            private ManualResetEvent AsyncWaitHandle_;

            /// <summary>
            /// Whether the operation has completed. Synchronized using atomic reads/writes and Interlocked access.
            /// </summary>
            private bool IsCompleted_;

            /// <summary>
            /// Object used for synchronization.
            /// </summary>
            private object SyncObject_ = new object();

            /// <summary>
            /// Marks the AsyncResult object as done. Should only be called once.
            /// </summary>
            // Always runs within the target thread context.
            public void Done()
            {
                lock (SyncObject_)
                {
                    IsCompleted_ = true;

                    // Set the wait handle, only if necessary
                    if (AsyncWaitHandle_ != null)
                        AsyncWaitHandle_.Set();
                }
            }

            /// <summary>
            /// Waits for the pending operation to complete, if necessary, and frees all resources. Should only be called once.
            /// </summary>
            // May run in an arbitrary thread context.
            public void WaitForAndDispose()
            {
                // First, do a simple check to see if it's completed
                if (IsCompleted)
                {
                    // Ensure the underlying wait handle is disposed if necessary
                    lock (SyncObject_)
                    {
                        if (AsyncWaitHandle_ != null)
                        {
                            AsyncWaitHandle_.Close();
                            AsyncWaitHandle_ = null;
                        }
                    }

                    return;
                }

                // Wait for the signal that it's completed, creating the signal if necessary
                AsyncWaitHandle.WaitOne();

                // Now that it's completed, dispose of the underlying wait handle
                lock (SyncObject_)
                {
                    AsyncWaitHandle_.Close();
                    AsyncWaitHandle_ = null;
                }
            }

            /// <summary>
            /// User-defined state is not supported.
            /// </summary>
            // May run in an arbitrary thread context.
            public object AsyncState
            {
                get { return null; }
            }

            /// <summary>
            /// Returns a waitable handle for this operation.
            /// </summary>
            // May run in an arbitrary thread context.
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    lock (SyncObject_)
                    {
                        // If it already exists, return it
                        if (AsyncWaitHandle_ != null)
                            return AsyncWaitHandle_;

                        // Create a new one
                        AsyncWaitHandle_ = new ManualResetEvent(IsCompleted_);
                        return AsyncWaitHandle_;
                    }
                }
            }

            /// <summary>
            /// Synchronous completion is not supported.
            /// </summary>
            // May run in an arbitrary thread context.
            public bool CompletedSynchronously
            {
                get { return false; }
            }

            /// <summary>
            /// Whether this operation has completed.
            /// </summary>
            // May run in an arbitrary thread context.
            public bool IsCompleted
            {
                get
                {
                    lock (SyncObject_)
                    {
                        return IsCompleted_;
                    }
                }
            }
        }

        /// <summary>
        /// Starts the invocation of a delegate on the thread that created this <see cref="GenericSynchronizingObject"/>.
        /// A corresponding call to <see cref="EndInvoke"/> is not required.
        /// </summary>
        /// <param name="method">The delegate to run.</param>
        /// <param name="args">The arguments to pass to <paramref name="method"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that can be used to detect completion of the delegate.</returns>
        /// <remarks>
        /// <para>The thread that created this <see cref="GenericSynchronizingObject"/> must have a non-null <see cref="SynchronizationContext"/>.</para>
        /// </remarks>
        // Always runs within the source thread context.
        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            IAsyncResult ret = new AsyncResult();
            synchronizationContext.Post((SendOrPostCallback)delegate(object state)
            {
                AsyncResult result = (AsyncResult)state;
                try
                {
                    result.ReturnValue = method.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    result.Error = ex;
                }
                result.Done();
            }, ret);
            return ret;
        }

        /// <summary>
        /// Waits for the invocation of a delegate to complete, and returns the result of the delegate.
        /// This may only be called once for a given <see cref="IAsyncResult"/> object, from one thread.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> returned from a call to <see cref="BeginInvoke"/>.</param>
        /// <returns>The result of the delegate.</returns>
        // May run in an arbitrary thread context.
        public object EndInvoke(IAsyncResult result)
        {
            AsyncResult asyncResult = (AsyncResult)result;
            asyncResult.WaitForAndDispose();
            if (asyncResult.Error != null)
            {
                string key = "Previous Stack Trace";
                while (asyncResult.Error.Data.Contains(key))
                    key = "Previous " + key;
                asyncResult.Error.Data.Add(key, asyncResult.Error.StackTrace);
                throw asyncResult.Error;
            }
            return asyncResult.ReturnValue;
        }

        /// <summary>
        /// Invokes a delegate on the thread that created this <see cref="GenericSynchronizingObject"/>.
        /// </summary>
        /// <param name="method">The delegate to invoke.</param>
        /// <param name="args">The parameters for <paramref name="method"/>.</param>
        /// <returns>The result of the delegate.</returns>
        /// <remarks>
        /// <para>The thread that created this <see cref="GenericSynchronizingObject"/> must have a non-null <see cref="SynchronizationContext"/>.</para>
        /// </remarks>
        // Always runs within the source thread context.
        public object Invoke(Delegate method, object[] args)
        {
            ReturnValue ret = new ReturnValue();
            synchronizationContext.Send(delegate(object unusedState)
            {
                try
                {
                    ret.ret = method.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    ret.error = ex;
                }
            }, null);
            if (ret.error != null)
            {
                string key = "Previous Stack Trace";
                while (ret.error.Data.Contains(key))
                    key = "Previous " + key;
                ret.error.Data.Add(key, ret.error.StackTrace);
                throw ret.error;
            }
            return ret.ret;
        }

        /// <summary>
        /// Whether the current thread must invoke a delegate.
        /// </summary>
        // Always runs within the source thread context.
        public bool InvokeRequired
        {
            get { return (synchronizationContext != SynchronizationContext.Current); }
        }
    }
}