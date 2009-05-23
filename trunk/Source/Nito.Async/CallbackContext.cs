using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

// Copyright 2009 by Nito Programs.

namespace Nito.Async
{
    /// <summary>
    /// Provides a context to which delegates may be bound.
    /// </summary>
    /// <remarks>
    /// <para>A bound delegate acts as a delegate wrapper around the original (inner) delegate. When executed, the bound delegate first checks if it is <i>valid</i>. If it is valid, then it executes the inner delegate. If it is invalid, then it will return without executing the inner delegate. Invalid bound delegates simply return; they do not throw an exception.</para>
    /// <para>All bound delegates are valid when they are bound to a context by calling <see cref="O:Nito.Async.CallbackContext.Bind"/>. When <see cref="Reset"/> (or <see cref="Dispose"/>) is called on the context, all previously-bound delegates are invalidated.</para>
    /// <para>The context keeps track of whether there is at least one delegate bound to it. <see cref="Invalidated"/> is true if there are no bound delegates; it is false if there is at least one. There is no way to query the validity of a particular delegate.</para>
    /// <para>If the innter delegate raises an exeption, then the bound delegate will propogate that exception.</para>
    /// </remarks>
    /// <threadsafety>
    /// <para>Instance members of this type are not thread-safe.</para>
    /// <para>Furthermore, delegates bound to an instance of this type must be synchronized with any other access to that instance.</para>
    /// </threadsafety>
    public sealed class CallbackContext : IDisposable
    {
        /// <summary>
        /// The current object-context.
        /// </summary>
        private object context;

        /// <summary>
        /// Creates a new context.
        /// </summary>
        public CallbackContext()
        {
        }

        /// <summary>
        /// Returns true if all delegates previously bound to this context have been invalidated. Returns false if there is at least one delegate
        /// that is valid.
        /// </summary>
        public bool Invalidated { get { return (context == null); } }

        /// <summary>
        /// Resets a context. This invalidates all delegates currently bound to this context.
        /// </summary>
        /// <remarks>
        /// <para>After this method returns, other delegates may be bound to this context, and they will be valid. This method only invalidates currently-bound delegates.</para>
        /// </remarks>
        public void Reset()
        {
            context = null;
        }

        /// <summary>
        /// Binds a delegate to this context, and returns the bound, valid delegate.
        /// </summary>
        /// <remarks>
        /// <para>The bound delegate will first determine if it is still valid. If the bound delegate is valid, then it will invoke the contained delegate. If the bound delegate is invalid, it will do nothing.</para>
        /// <para>To invalidate all bound delegates, call the <see cref="Reset"/> method.</para>
        /// </remarks>
        /// <param name="action">The contained delegate.</param>
        /// <returns>A valid delegate bound to the current context.</returns>
        /// <threadsafety>
        /// <para>The execution of the bound delegate must be synchronized with any other access of its bound <see cref="CallbackContext"/>.</para>
        /// </threadsafety>
        public Action Bind(Action action)
        {
            // Create the object-context if it doesn't already exist.
            if (context == null)
                context = new object();

            // Make a (reference) copy of the current object-context; this is necessary because lambda expressions bind to variables, not values
            object boundContext = context;
            return () =>
                {
                    // Compare the bound object-context to the current object-context; if they differ, then do nothing
                    // Use the static object.Equals instead of the instance object.Equals because the current object-context may be null
                    if (object.Equals(boundContext, context))
                        action();
                };
        }

        /// <summary>
        /// Binds a delegate to this context, and returns the bound, valid delegate.
        /// </summary>
        /// <remarks>
        /// <para>The bound delegate will first determine if it is still valid. If the bound delegate is valid, then it will invoke the contained delegate. If the bound delegate is invalid, it will only return the default value for <typeparamref name="T"/>.</para>
        /// <para>To invalidate all bound delegates, call the <see cref="Reset"/> method.</para>
        /// </remarks>
        /// <typeparam name="T">The return value of the contained and bound delegates.</typeparam>
        /// <param name="func">The contained delegate.</param>
        /// <returns>A valid delegate bound to the current context.</returns>
        /// <threadsafety>
        /// <para>The execution of the bound delegate must be synchronized with any other access of its bound <see cref="CallbackContext"/>.</para>
        /// </threadsafety>
        public Func<T> Bind<T>(Func<T> func)
        {
            // Create the object-context if it doesn't already exist.
            if (context == null)
                context = new object();

            // Make a (reference) copy of the current object-context; this is necessary because lambda expressions bind to variables, not values
            object boundContext = context;
            return () =>
                {
                    // Compare the bound object-context to the current object-context; if they differ, then do nothing
                    // Use the static object.Equals instead of the instance object.Equals because the current object-context may be null
                    if (object.Equals(boundContext, context))
                        return func();
                    return default(T);
                };
        }

        /// <summary>
        /// Synchronizes a delegate and then binds it to this context, and returns the synchronized, bound, valid delegate.
        /// </summary>
        /// <remarks>
        /// <para>The bound delegate will first determine if it is still valid. If the bound delegate is valid, then it will invoke the contained delegate. If the bound delegate is invalid, it will do nothing.</para>
        /// <para>To invalidate all bound delegates, call the <see cref="Reset"/> method.</para>
        /// </remarks>
        /// <param name="action">The contained delegate. This delegate should not raise exceptions.</param>
        /// <param name="synchronizingObject">The object to use for synchronizing the delegate if necessary.</param>
        /// <returns>A valid delegate bound to the current context.</returns>
        /// <threadsafety>
        /// <para>The returned delegate may be executed on any thread; it will synchronize itself with this <see cref="CallbackContext"/>.</para>
        /// </threadsafety>
        public Action Bind(Action action, ISynchronizeInvoke synchronizingObject)
        {
            // Create the bound delegate
            Action boundAction = Bind(action);

            // Return a synchronized wrapper for the bound delegate
            return () =>
                {
                    // We synchronously invoke rather than async (BeginInvoke) because it's up to the implementation whether to require EndInvoke
                    if (synchronizingObject.InvokeRequired)
                        synchronizingObject.Invoke(action, null);
                    else
                        action();
                };
        }

        /// <summary>
        /// Synchronizes a delegate and then binds it to this context, and returns the synchronized, bound, valid delegate.
        /// </summary>
        /// <remarks>
        /// <para>The bound delegate will first determine if it is still valid. If the bound delegate is valid, then it will invoke the contained delegate. If the bound delegate is invalid, it will do nothing.</para>
        /// <para>To invalidate all bound delegates, call the <see cref="Reset"/> method.</para>
        /// </remarks>
        /// <param name="action">The contained delegate. This delegate should not raise exceptions.</param>
        /// <param name="synchronizationContext">The object to use for synchronizing the delegate if necessary.</param>
        /// <returns>A valid delegate bound to the current context.</returns>
        /// <threadsafety>
        /// <para>The returned delegate may be executed on any thread except the thread that owns <paramref name="synchronizationContext"/>; it will synchronize itself with this <see cref="CallbackContext"/>.</para>
        /// </threadsafety>
        public Action Bind(Action action, SynchronizationContext synchronizationContext)
        {
            // Create the bound delegate
            Action boundAction = Bind(action);

            // Return a synchronized wrapper for the bound delegate
            return () =>
                {
                    synchronizationContext.Post((state) => action(), null);
                };
        }

        /// <summary>
        /// Synchronizes a delegate and then binds it to this context, and returns the synchronized, bound, valid delegate.
        /// </summary>
        /// <remarks>
        /// <para>The bound delegate will first determine if it is still valid. If the bound delegate is valid, then it will invoke the contained delegate. If the bound delegate is invalid, it will do nothing.</para>
        /// <para>To invalidate all bound delegates, call the <see cref="Reset"/> method.</para>
        /// </remarks>
        /// <typeparam name="T">The return value of the contained and bound delegates.</typeparam>
        /// <param name="func">The contained delegate. This delegate should not raise exceptions.</param>
        /// <param name="synchronizingObject">The object to use for synchronizing the delegate if necessary.</param>
        /// <returns>A valid delegate bound to the current context.</returns>
        /// <threadsafety>
        /// <para>The returned delegate may be executed on any thread; it will synchronize itself with this <see cref="CallbackContext"/>.</para>
        /// </threadsafety>
        public Func<T> Bind<T>(Func<T> func, ISynchronizeInvoke synchronizingObject)
        {
            // Create the bound delegate
            Func<T> boundFunc = Bind(func);

            // Return a synchronized wrapper for the bound delegate
            return () =>
                {
                    if (synchronizingObject.InvokeRequired)
                        return (T)synchronizingObject.Invoke(func, null);
                    else
                        return func();
                };
        }

        /// <summary>
        /// Synchronizes a delegate and then binds it to this context, and returns the synchronized, bound, valid delegate.
        /// </summary>
        /// <remarks>
        /// <para>The bound delegate will first determine if it is still valid. If the bound delegate is valid, then it will invoke the contained delegate. If the bound delegate is invalid, it will do nothing.</para>
        /// <para>To invalidate all bound delegates, call the <see cref="Reset"/> method.</para>
        /// </remarks>
        /// <typeparam name="T">The return value of the contained and bound delegates.</typeparam>
        /// <param name="func">The contained delegate. This delegate should not raise exceptions.</param>
        /// <param name="synchronizationContext">The object to use for synchronizing the delegate.</param>
        /// <returns>A valid delegate bound to the current context.</returns>
        /// <threadsafety>
        /// <para>The returned delegate may be executed on any thread except the thread that owns <paramref name="synchronizationContext"/>; it will synchronize itself with this <see cref="CallbackContext"/>.</para>
        /// </threadsafety>
        public Func<T> Bind<T>(Func<T> func, SynchronizationContext synchronizationContext)
        {
            // Create the bound delegate
            Func<T> boundFunc = Bind(func);

            // Return a synchronized wrapper for the bound delegate
            return () =>
                {
                    T retVal = default(T);
                    synchronizationContext.Send((state) => retVal = func(), null);
                    return retVal;
                };
        }

        /// <summary>
        /// Invalidates all delegates bound to this context.
        /// </summary>
        /// <remarks>
        /// <para>This method is a synonym for <see cref="Reset"/>.</para>
        /// </remarks>
        public void Dispose()
        {
            Reset();
        }
    }
}
