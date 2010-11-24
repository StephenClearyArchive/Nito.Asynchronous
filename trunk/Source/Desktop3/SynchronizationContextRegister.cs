// <copyright file="SynchronizationContextRegister.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
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
        /// The <see cref="SynchronizationContext"/> has exactly one managed thread associated with it. Any <see cref="SynchronizationContext"/> specifying <see cref="SpecificAssociatedThread"/> should also specify <see cref="Synchronized"/>.
        /// </summary>
        SpecificAssociatedThread = 0x10,

        /// <summary>
        /// The <see cref="SynchronizationContext"/> makes the standard guarantees (<see cref="NonReentrantPost"/>, <see cref="NonReentrantSend"/>, <see cref="Synchronized"/>, <see cref="Sequential"/>, and <see cref="SpecificAssociatedThread"/>). This is defined as a constant because most custom synchronization contexts do make these guarantees.
        /// </summary>
        Standard = NonReentrantPost | NonReentrantSend | Synchronized | Sequential | SpecificAssociatedThread,
    }

    /// <summary>
    /// A global register of <see cref="SynchronizationContextProperties"/> flags for <see cref="SynchronizationContext"/> types.
    /// </summary>
    public static class SynchronizationContextRegister
    {
        /// <summary>
        /// A mapping from synchronization context type names to their properties. We map from type names instead of actual types to avoid dependencies on unnecessary assemblies.
        /// </summary>
        private static readonly Dictionary<string, SynchronizationContextProperties> synchronizationContextProperties = PredefinedSynchronizationContextProperties();

        /// <summary>
        /// Registers a <see cref="SynchronizationContext"/> type claiming to provide certain guarantees.
        /// </summary>
        /// <param name="synchronizationContextType">The type derived from <see cref="SynchronizationContext"/>. May not be <c>null</c>.</param>
        /// <param name="properties">The guarantees provided by this type.</param>
        /// <remarks>
        /// <para>This method should be called once for each type of <see cref="SynchronizationContext"/>. It is not necessary to call this method for .NET <see cref="SynchronizationContext"/> types or <see cref="ActionDispatcherSynchronizationContext"/>.</para>
        /// <para>If this method is called more than once for a type, the new value of <paramref name="properties"/> replaces the old value. The flags are not merged.</para>
        /// </remarks>
        public static void Register(Type synchronizationContextType, SynchronizationContextProperties properties)
        {
            Contract.Requires(synchronizationContextType != null);

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
        /// Looks up the guarantees for a <see cref="SynchronizationContext"/> type.
        /// </summary>
        /// <param name="synchronizationContextType">The type derived from <see cref="SynchronizationContext"/> to test. May not be <c>null</c>.</param>
        /// <returns>The properties guaranteed by <paramref name="synchronizationContextType"/>.</returns>
        public static SynchronizationContextProperties Lookup(Type synchronizationContextType)
        {
            Contract.Requires(synchronizationContextType != null);

            lock (synchronizationContextProperties)
            {
                SynchronizationContextProperties supported = SynchronizationContextProperties.None;
                if (synchronizationContextProperties.ContainsKey(synchronizationContextType.FullName))
                {
                    supported = synchronizationContextProperties[synchronizationContextType.FullName];
                }

                return supported;
            }
        }

        /// <summary>
        /// Verifies that a <see cref="SynchronizationContext"/> satisfies the guarantees required by the calling code.
        /// </summary>
        /// <param name="synchronizationContextType">The type derived from <see cref="SynchronizationContext"/> to test. May not be <c>null</c>.</param>
        /// <param name="properties">The guarantees required by the calling code.</param>
        public static void Verify(Type synchronizationContextType, SynchronizationContextProperties properties)
        {
            Contract.Requires(synchronizationContextType != null);

            SynchronizationContextProperties supported = Lookup(synchronizationContextType);
            if ((supported & properties) != properties)
            {
                throw new InvalidOperationException("This asynchronous object cannot be used with this SynchronizationContext");
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

        /// <summary>
        /// Returns the mapping for all predefined (.NET) <see cref="SynchronizationContext"/> types.
        /// </summary>
        /// <returns>The mapping for all predefined (.NET) <see cref="SynchronizationContext"/> types.</returns>
        private static Dictionary<string, SynchronizationContextProperties> PredefinedSynchronizationContextProperties()
        {
            var ret = new Dictionary<string, SynchronizationContextProperties>
            {
                { "System.Threading.SynchronizationContext", SynchronizationContextProperties.NonReentrantPost },
                { "System.Windows.Forms.WindowsFormsSynchronizationContext", SynchronizationContextProperties.Standard },
                { "System.Windows.Threading.DispatcherSynchronizationContext", SynchronizationContextProperties.Standard }
            };

            // AspNetSynchronizationContext does not provide any guarantees at all, so it is not added here
            return ret;
        }
    }
}
