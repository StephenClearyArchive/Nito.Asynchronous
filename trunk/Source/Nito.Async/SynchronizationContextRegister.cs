// <copyright file="SynchronizationContextRegister.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Flags that identify differences in behavior in various <see cref="SynchronizationContext"/> implementations.
    /// </summary>
    [Flags]
    public enum SynchronizationContextProperties
    {
        /// <summary>
        /// The <see cref="SynchronizationContext"/> makes no guarantees about any of the properties in <see cref="SynchronizationContextProperties"/>.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// <see cref="SynchronizationContext.Post"/> is guaranteed to be non-reentrant (if called from a thread that is not the <see cref="SynchronizationContext"/>'s specific associated thread, if any).
        /// </summary>
        NonReentrantPost = 0x1,

        /// <summary>
        /// <see cref="SynchronizationContext.Send"/> is guaranteed to be non-reentrant (if called from a thread that is not the <see cref="SynchronizationContext"/>'s specific associated thread, if any).
        /// </summary>
        NonReentrantSend = 0x2,

        /// <summary>
        /// Delegates queued to the <see cref="SynchronizationContext"/> are guaranteed to execute one at a time.
        /// </summary>
        Synchronized = 0x4,

        /// <summary>
        /// Delegates queued to the <see cref="SynchronizationContext"/> are guaranteed to execute in order. Any <see cref="SynchronizationContext"/> claiming to be <see cref="Sequential"/> should also claim to be <see cref="Synchronized"/>.
        /// </summary>
        Sequential = 0x8,

        /// <summary>
        /// The <see cref="SynchronizationContext"/> makes all guarantees.
        /// </summary>
        All = NonReentrantPost | NonReentrantSend | Synchronized | Sequential
    }

    /// <summary>
    /// A global register of <see cref="SynchronizationContextProperties"/> flags for <see cref="SynchronizationContext"/> types.
    /// </summary>
    public static class SynchronizationContextRegister
    {
        /// <summary>
        /// A mapping from synchronization context type names to their properties. We map from type names instead of actual types to avoid dependencies on unnecessary assemblies.
        /// </summary>
        private static Dictionary<string, SynchronizationContextProperties> synchronizationContextProperties;

        /// <summary>
        /// Initializes static members of the <see cref="SynchronizationContextRegister"/> class with all <see cref="SynchronizationContext"/> types built into the .NET framework.
        /// </summary>
        static SynchronizationContextRegister()
        {
            synchronizationContextProperties = new Dictionary<string, SynchronizationContextProperties>();
            synchronizationContextProperties.Add("System.Threading.SynchronizationContext", SynchronizationContextProperties.NonReentrantPost);
            synchronizationContextProperties.Add("System.Windows.Forms.WindowsFormsSynchronizationContext", SynchronizationContextProperties.All);
            synchronizationContextProperties.Add("System.Windows.Threading.DispatcherSynchronizationContext", SynchronizationContextProperties.All);
            
            // AspNetSynchronizationContext does not provide any guarantees at all, so it is not added here
        }

        /// <summary>
        /// Registers a <see cref="SynchronizationContext"/> type claiming to provide certain guarantees.
        /// </summary>
        /// <param name="synchronizationContextType">The type derived from <see cref="SynchronizationContext"/>.</param>
        /// <param name="properties">The guarantees provided by this type.</param>
        /// <remarks>
        /// <para>This method should be called once for each type of <see cref="SynchronizationContext"/>. It is not necessary to call this method for .NET <see cref="SynchronizationContext"/> types or <see cref="ActionDispatcherSynchronizationContext"/>.</para>
        /// </remarks>
        public static void Register(Type synchronizationContextType, SynchronizationContextProperties properties)
        {
            lock (synchronizationContextProperties)
            {
                if (synchronizationContextProperties.ContainsKey(synchronizationContextType.FullName))
                {
                    synchronizationContextProperties[synchronizationContextType.FullName] = properties;
                }
                else
                {
                    synchronizationContextProperties.Add(synchronizationContextType.FullName, properties);
                }
            }
        }

        /// <summary>
        /// Verifies that a <see cref="SynchronizationContext"/> satisfies the guarantees required by the calling code.
        /// </summary>
        /// <param name="synchronizationContextType">The type derived from <see cref="SynchronizationContext"/> to test.</param>
        /// <param name="properties">The guarantees required by the calling code.</param>
        public static void Verify(Type synchronizationContextType, SynchronizationContextProperties properties)
        {
            lock (synchronizationContextProperties)
            {
                SynchronizationContextProperties supported = SynchronizationContextProperties.None;
                if (synchronizationContextProperties.ContainsKey(synchronizationContextType.FullName))
                {
                    supported = synchronizationContextProperties[synchronizationContextType.FullName];
                }

                if ((supported & properties) != properties)
                {
                    throw new InvalidOperationException("This asynchronous object cannot be used with this SynchronizationContext");
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="SynchronizationContext.Current"/> satisfies the guarantees required by the calling code.
        /// </summary>
        /// <param name="properties">The guarantees required by the calling code.</param>
        public static void Verify(SynchronizationContextProperties properties)
        {
            if (SynchronizationContext.Current == null)
            {
                Verify(typeof(SynchronizationContext), properties);
            }
            else
            {
                Verify(SynchronizationContext.Current.GetType(), properties);
            }
        }
    }
}
