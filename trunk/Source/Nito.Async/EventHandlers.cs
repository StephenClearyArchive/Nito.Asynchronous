// <copyright file="EventHandlers.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Provides data for the asynchronous event handlers that have one result.
    /// </summary>
    /// <typeparam name="T">The type of the result of the asynchronous operation.</typeparam>
    public class AsyncResultEventArgs<T> : AsyncCompletedEventArgs
    {
        /// <summary>
        /// The result of the asynchronous operation.
        /// </summary>
        private T result;

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
            this.result = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResultEventArgs{T}"/> class indicating a successful completion.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        public AsyncResultEventArgs(T result)
            : this(result, null, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResultEventArgs{T}"/> class indicating an unsuccessful operation.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        public AsyncResultEventArgs(Exception error)
            : this(default(T), error, false, null)
        {
        }

        /// <summary>
        /// Gets the result of the asynchronous operation. This property may only be read if <see cref="AsyncCompletedEventArgs.Error"/> is null.
        /// </summary>
        public T Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return this.result;
            }
        }
    }
}
