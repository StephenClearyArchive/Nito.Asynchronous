// <copyright file="EventHandlers.Sync.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

    /// <content>
    /// Utility methods for asynchronous event handlers.
    /// </content>
    public static partial class Sync
    {
        /// <summary>
        /// Runs <paramref name="action"/> followed by <paramref name="callback"/> with arguments indicating success. If <paramref name="action"/>
        /// raises an exception, <paramref name="callback"/> is invoked with arguments indicating the error.
        /// </summary>
        /// <param name="action">The action to perform. May not be <c>null</c>.</param>
        /// <param name="callback">The callback to indicate success or error.</param>
        /// <param name="state">The user state to include in the arguments to the callback. May be null.</param>
        /// <remarks>
        /// <para>This method does not support argments indicating cancellation.</para>
        /// </remarks>
        public static void InvokeAndCallback(Action action, Action<AsyncCompletedEventArgs> callback, object state)
        {
            Contract.Requires(action != null);

            try
            {
                action();
                if (callback != null)
                {
                    callback(new AsyncCompletedEventArgs(null, false, state));
                }
            }
            catch (Exception ex)
            {
                if (callback != null)
                {
                    callback(new AsyncCompletedEventArgs(ex, false, state));
                }
            }
        }

        /// <summary>
        /// Runs <paramref name="action"/> followed by <paramref name="callback"/> with arguments indicating success,
        /// including its return value. If <paramref name="action"/> raises an exception, <paramref name="callback"/>
        /// is invoked with arguments indicating the error.
        /// </summary>
        /// <typeparam name="T">The type of the result of the action.</typeparam>
        /// <param name="action">The action to perform. May not be <c>null</c>.</param>
        /// <param name="callback">The callback to indicate success or error.</param>
        /// <param name="state">The user state to include in the arguments to the callback. May be null.</param>
        /// <remarks>
        /// <para>This method does not support argments indicating cancellation.</para>
        /// </remarks>
        public static void InvokeAndCallback<T>(Func<T> action, Action<AsyncResultEventArgs<T>> callback, object state)
        {
            Contract.Requires(action != null);

            try
            {
                T result = action();
                if (callback != null)
                {
                    callback(new AsyncResultEventArgs<T>(result, null, false, state));
                }
            }
            catch (Exception ex)
            {
                if (callback != null)
                {
                    callback(new AsyncResultEventArgs<T>(default(T), ex, false, state));
                }
            }
        }
    }
}
