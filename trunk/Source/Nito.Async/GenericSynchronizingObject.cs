// <copyright file="GenericSynchronizingObject.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    /// <summary>
    /// Allows objects that use <see cref="ISynchronizeInvoke"/> (usually using a property named SynchronizingObject) to synchronize to a
    /// generic <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>If an exception is raied by the delegate passed to <see cref="Invoke"/> or <see cref="BeginInvoke"/>, then that exception is propogated back to the caller. The stack trace of the exception is wiped out in the process, but is saved in <see cref="Exception.Data"/> under the key "Previous Stack Trace".</para>
    /// <para>This class does not invoke <see cref="SynchronizationContext.OperationStarted"/> or <see cref="SynchronizationContext.OperationCompleted"/>, so for some synchronization contexts, these may need to be called explicitly in addition to using this class. ASP.NET do require them to be called; Windows Forms, WPF, and <see cref="ActionDispatcher"/> do not.</para>
    /// <para>The thread that synchronizes the operation is the "source" thread. This is the thread that owns the object with a SynchronizingObject property.</para>
    /// <para>The thread that executes the operation is the "target" thread. This is the thread that creates the <see cref="SynchronizationContext"/> used for synchronization.</para>
    /// </remarks>
    public sealed class GenericSynchronizingObject : ISynchronizeInvoke
    {
        /// <summary>
        /// The captured synchronization context.
        /// </summary>
        private SynchronizationContext synchronizationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSynchronizingObject"/> class, binding to <see cref="SynchronizationContext.Current">SynchronizationContext.Current</see>.
        /// </summary>
        /// <remarks>
        /// <para>This method always runs in the target thread.</para>
        /// </remarks>
        public GenericSynchronizingObject()
        {
            this.synchronizationContext = SynchronizationContext.Current;
            if (this.synchronizationContext == null)
            {
                this.synchronizationContext = new SynchronizationContext();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current thread must invoke a delegate.
        /// </summary>
        /// <remarks>
        /// <para>This method always runs in the source thread.</para>
        /// </remarks>
        public bool InvokeRequired
        {
            // TODO: invalid logic!!!
            get { return this.synchronizationContext != SynchronizationContext.Current; }
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
        /// <para>This method always runs in the source thread.</para>
        /// </remarks>
        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            IAsyncResult ret = new AsyncResult();
            this.synchronizationContext.Post(
                (SendOrPostCallback)delegate(object state)
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
                },
                ret);
            return ret;
        }

        /// <summary>
        /// Waits for the invocation of a delegate to complete, and returns the result of the delegate.
        /// This may only be called once for a given <see cref="IAsyncResult"/> object, from one thread.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> returned from a call to <see cref="BeginInvoke"/>.</param>
        /// <returns>The result of the delegate.</returns>
        /// <remarks>
        /// <para>This method may run in an arbitrary thread context.</para>
        /// </remarks>
        public object EndInvoke(IAsyncResult result)
        {
            AsyncResult asyncResult = (AsyncResult)result;
            asyncResult.WaitForAndDispose();
            if (asyncResult.Error != null)
            {
                string key = "Previous Stack Trace";
                while (asyncResult.Error.Data.Contains(key))
                {
                    key = "Previous " + key;
                }

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
        /// <para>This method always runs in the source thread.</para>
        /// </remarks>
        public object Invoke(Delegate method, object[] args)
        {
            ReturnValue ret = new ReturnValue();
            this.synchronizationContext.Send(
                delegate(object unusedState)
                {
                    try
                    {
                        ret.ReturnedValue = method.DynamicInvoke(args);
                    }
                    catch (Exception ex)
                    {
                        ret.Error = ex;
                    }
                },
                null);
            if (ret.Error != null)
            {
                string key = "Previous Stack Trace";
                while (ret.Error.Data.Contains(key))
                {
                    key = "Previous " + key;
                }

                ret.Error.Data.Add(key, ret.Error.StackTrace);
                throw ret.Error;
            }

            return ret.ReturnedValue;
        }

        /// <summary>
        /// A helper object that just wraps the return value, when the delegate is invoked synchronously.
        /// </summary>
        private sealed class ReturnValue
        {
            /// <summary>
            /// Gets or sets return value, if any. This is only valid if <see cref="Error"/> is not <c>null</c>. May be <c>null</c>, even if valid.
            /// </summary>
            public object ReturnedValue { get; set; }

            /// <summary>
            /// Gets or sets the error, if any. May be <c>null</c>.
            /// </summary>
            public Exception Error { get; set; }
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
            /// The wait handle, which may be null. Writes are synchronized using Interlocked access.
            /// </summary>
            private ManualResetEvent asyncWaitHandle;

            /// <summary>
            /// Whether the operation has completed. Synchronized using atomic reads/writes and Interlocked access.
            /// </summary>
            private bool isCompleted;

            /// <summary>
            /// Object used for synchronization.
            /// </summary>
            private object syncObject = new object();

            /// <summary>
            /// Gets or sets the return value. Must be set before calling <see cref="Done"/>.
            /// </summary>
            public object ReturnValue { get; set; }

            /// <summary>
            /// Gets or sets the error. Must be set before calling <see cref="Done"/>.
            /// </summary>
            public Exception Error { get; set; }

            /// <summary>
            /// Gets the user-defined state. Always returns <c>null</c>; user-defined state is not supported.
            /// </summary>
            /// <remarks>
            /// <para>This property may be accessed in an arbitrary thread context.</para>
            /// </remarks>
            public object AsyncState
            {
                get { return null; }
            }

            /// <summary>
            /// Gets a waitable handle for this operation.
            /// </summary>
            /// <remarks>
            /// <para>This property may be accessed in an arbitrary thread context.</para>
            /// </remarks>
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    lock (this.syncObject)
                    {
                        // If it already exists, return it
                        if (this.asyncWaitHandle != null)
                        {
                            return this.asyncWaitHandle;
                        }

                        // Create a new one
                        this.asyncWaitHandle = new ManualResetEvent(this.isCompleted);
                        return this.asyncWaitHandle;
                    }
                }
            }

            /// <summary>
            /// Gets a value indicating whether the operation completed synchronously. Always returns false; synchronous completion is not supported.
            /// </summary>
            /// <remarks>
            /// <para>This property may be accessed in an arbitrary thread context.</para>
            /// </remarks>
            public bool CompletedSynchronously
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether this operation has completed.
            /// </summary>
            /// <remarks>
            /// <para>This property may be accessed in an arbitrary thread context.</para>
            /// </remarks>
            public bool IsCompleted
            {
                get
                {
                    lock (this.syncObject)
                    {
                        return this.isCompleted;
                    }
                }
            }

            /// <summary>
            /// Marks the AsyncResult object as done. Should only be called once.
            /// </summary>
            /// <remarks>
            /// <para>This method always runs in the target thread.</para>
            /// </remarks>
            public void Done()
            {
                lock (this.syncObject)
                {
                    this.isCompleted = true;

                    // Set the wait handle, only if necessary
                    if (this.asyncWaitHandle != null)
                    {
                        this.asyncWaitHandle.Set();
                    }
                }
            }

            /// <summary>
            /// Waits for the pending operation to complete, if necessary, and frees all resources. Should only be called once.
            /// </summary>
            /// <remarks>
            /// <para>This method may run in an arbitrary thread context.</para>
            /// </remarks>
            public void WaitForAndDispose()
            {
                // First, do a simple check to see if it's completed
                if (this.IsCompleted)
                {
                    // Ensure the underlying wait handle is disposed if necessary
                    lock (this.syncObject)
                    {
                        if (this.asyncWaitHandle != null)
                        {
                            this.asyncWaitHandle.Close();
                            this.asyncWaitHandle = null;
                        }
                    }

                    return;
                }

                // Wait for the signal that it's completed, creating the signal if necessary
                this.AsyncWaitHandle.WaitOne();

                // Now that it's completed, dispose of the underlying wait handle
                lock (this.syncObject)
                {
                    this.asyncWaitHandle.Close();
                    this.asyncWaitHandle = null;
                }
            }
        }
    }
}