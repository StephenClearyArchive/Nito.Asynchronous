using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

// Copyright 2009 by Nito Programs.

namespace Nito.Async
{
    /// <summary>
    /// Provides data for the asynchronous event handlers that have one result.
    /// </summary>
    /// <typeparam name="T">The type of the result of the asynchronous operation.</typeparam>
    public class AsyncResultEventArgs<T> : AsyncCompletedEventArgs
    {
        /// <summary>
        /// The result of the asynchronous operation.
        /// </summary>
        private T Result_;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResultEventArgs{T}"/> class.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        /// <param name="error">Any error that occurred. Null if no error.</param>
        /// <param name="cancelled">Whether the operation was cancelled.</param>
        /// <param name="userState">The optional user-defined state object.</param>
        public AsyncResultEventArgs(T result, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            Result_ = result;
        }

        /// <summary>
        /// Creates an arguments object indicating a successful completion.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        public AsyncResultEventArgs(T result) : this(result, null, false, null) { }

        /// <summary>
        /// Creates an arguments object indicating an unsuccessful operation.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        public AsyncResultEventArgs(Exception error) : this(default(T), error, false, null) { }

        /// <summary>
        /// The result of the asynchronous operation. This property may only be read if <see cref="AsyncCompletedEventArgs.Error"/> is null.
        /// </summary>
        public T Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return Result_;
            }
        }
    }

    public static partial class Sync
    {
        /// <summary>
        /// Runs <paramref name="action"/> followed by <paramref name="callback"/> with arguments indicating success. If <paramref name="action"/>
        /// raises an exception, <paramref name="callback"/> is invoked with arguments indicating the error.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="callback">The callback to indicate success or error.</param>
        /// <param name="state">The user state to include in the arguments to the callback. May be null.</param>
        /// <remarks>
        /// <para>This method does not support argments indicating cancellation.</para>
        /// </remarks>
        public static void InvokeAndCallback(Action action, Action<AsyncCompletedEventArgs> callback, object state)
        {
            try
            {
                action();
                if (callback != null)
                    callback(new AsyncCompletedEventArgs(null, false, state));
            }
            catch (Exception ex)
            {
                if (callback != null)
                    callback(new AsyncCompletedEventArgs(ex, false, state));
            }
        }

        /// <summary>
        /// Runs <paramref name="action"/> followed by <paramref name="callback"/> with arguments indicating success,
        /// including its return value. If <paramref name="action"/> raises an exception, <paramref name="callback"/>
        /// is invoked with arguments indicating the error.
        /// </summary>
        /// <typeparam name="T">The type of the result of the action.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="callback">The callback to indicate success or error.</param>
        /// <param name="state">The user state to include in the arguments to the callback. May be null.</param>
        /// <remarks>
        /// <para>This method does not support argments indicating cancellation.</para>
        /// </remarks>
        public static void InvokeAndCallback<T>(Func<T> action, Action<AsyncResultEventArgs<T>> callback, object state)
        {
            try
            {
                T result = action();
                if (callback != null)
                    callback(new AsyncResultEventArgs<T>(result, null, false, state));
            }
            catch (Exception ex)
            {
                if (callback != null)
                    callback(new AsyncResultEventArgs<T>(default(T), ex, false, state));
            }
        }
    }
}
