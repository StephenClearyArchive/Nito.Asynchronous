using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;

// Copyright 2009 by Nito Programs.

namespace Nito.Async
{
    /// <summary>
    /// Provides utility methods for implementing asynchronous operations.
    /// </summary>
    public static partial class Sync
    {
        // Note: these do not overload a single "Synchronize" method, because we want to allow argument type inference for lambda expressions.
        //  If Synchronize(AsyncCallback) and Synchronize(WaitCallback) were both defined, then Synchronize((x) => MessageBox.Show("test"));
        //    would not compile. Declaring the parameter types would work, e.g., Synchronize((object x) => MessageBox.Show("test"));
        //  However, if Synchronize(TimerCallback) and Synchronize(WaitCallback) were both defined, then
        //    Synchronize((object x) => MessageBox.Show("test")); would not compile. This makes explicit parameter type declaration brittle, as
        //    other callback types may be added to Synchronize in the future. This is because of how C# treats delegates (WaitCallback and
        //    TimerCallback are completely different delegate types and may not be cast to each other).
        //  The only long-term solution would be to cast the lambda expression to the required type, e.g.,
        //    Synchronize((WaitCallback)((x) => MessageBox.Show("test"))); but this is no better than:
        //    SynchronizeWaitCallback((x) => MessageBox.Show("test"));
        //  When considering the generic (Action<...>) overloads, the problem only grows worse.

        /// <summary>
        /// Returns an <see cref="Action"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        public static Action SynchronizeAction(Action callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on another thread, probably a ThreadPool thread
            return delegate()
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(), null);
            };
        }

        /// <summary>
        /// Returns an <see cref="Action{T}"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <typeparam name="T">The type of the parameter to the callback.</typeparam>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        public static Action<T> SynchronizeAction<T>(Action<T> callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on another thread, probably a ThreadPool thread
            return delegate(T arg)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(arg), null);
            };
        }

        /// <summary>
        /// Returns an <see cref="Action{T1, T2}"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter to the callback.</typeparam>
        /// <typeparam name="T2">The type of the second parameter to the callback.</typeparam>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        public static Action<T1, T2> SynchronizeAction<T1, T2>(Action<T1, T2> callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on another thread, probably a ThreadPool thread
            return delegate(T1 arg1, T2 arg2)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(arg1, arg2), null);
            };
        }

        /// <summary>
        /// Returns an <see cref="Action{T1, T2, T3}"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter to the callback.</typeparam>
        /// <typeparam name="T2">The type of the second parameter to the callback.</typeparam>
        /// <typeparam name="T3">The type of the third parameter to the callback.</typeparam>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        public static Action<T1, T2, T3> SynchronizeAction<T1, T2, T3>(Action<T1, T2, T3> callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on another thread, probably a ThreadPool thread
            return delegate(T1 arg1, T2 arg2, T3 arg3)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(arg1, arg2, arg3), null);
            };
        }

        /// <summary>
        /// Returns an <see cref="Action{T1, T2, T3, T4}"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter to the callback.</typeparam>
        /// <typeparam name="T2">The type of the second parameter to the callback.</typeparam>
        /// <typeparam name="T3">The type of the third parameter to the callback.</typeparam>
        /// <typeparam name="T4">The type of the third parameter to the callback.</typeparam>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        public static Action<T1, T2, T3, T4> SynchronizeAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on another thread, probably a ThreadPool thread
            return delegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(arg1, arg2, arg3, arg4), null);
            };
        }

        /// <summary>
        /// Returns an <see cref="AsyncCallback"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        /// <remarks>
        /// <para>This is intended for use within a call to BeginXXX methods, e.g., <code>socket.BeginConnect(remoteEP, Sync.SynchronizeAsyncCallback(callback), state);</code></para>
        /// </remarks>
        public static AsyncCallback SynchronizeAsyncCallback(AsyncCallback callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on another thread, probably a ThreadPool thread
            return delegate(IAsyncResult asyncResult)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(asyncResult), null);
            };
        }

        /// <summary>
        /// Returns a <see cref="TimerCallback"/> that executes in the context of the thread that called this method (if that
        /// thread exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        /// <remarks>
        /// <para>This is intended for use within a call to the <see cref="System.Threading.Timer"/> constructor, e.g., <code>new Timer(Sync.Synchronize(callback));</code></para>
        /// </remarks>
        public static TimerCallback SynchronizeTimerCallback(TimerCallback callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on a ThreadPool thread
            return delegate(object state)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(state), null);
            };
        }

        /// <summary>
        /// Returns a <see cref="WaitCallback"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        public static WaitCallback SynchronizeWaitCallback(WaitCallback callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on a ThreadPool thread
            return delegate(object state)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(state), null);
            };
        }

        /// <summary>
        /// Returns a <see cref="WaitOrTimerCallback"/> that executes in the context of the thread that called this method (if that thread
        /// exposes <see cref="SynchronizationContext"/>).
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A synchronized callback.</returns>
        /// <remarks>
        /// <para>This is intended for use within a call to <see cref="ThreadPool"/>'s RegisterWaitForSingleObject methods, e.g., <code>ThreadPool.RegisterWaitForSingleObject(waitObject, Sync.SynchronizeWaitOrTimerCallback(callback), state, ...);</code></para>
        /// </remarks>
        public static WaitOrTimerCallback SynchronizeWaitOrTimerCallback(WaitOrTimerCallback callback)
        {
            // Create the operation, capturing the current thread's synchronization context
            AsyncOperation operation = AsyncOperationManager.CreateOperation(new object());

            // This delegate will be executed on a ThreadPool thread
            return delegate(object state, bool timedOut)
            {
                // Synchronize the operation back to the originating thread
                operation.PostOperationCompleted((unusedState) => callback(state, timedOut), null);
            };
        }
    }
}
