// <copyright file="ScopedSynchronizationContext.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async
{
    using System;
    using System.Threading;

    /// <summary>
    /// Replaces <see cref="SynchronizationContext.Current">SynchronizationContext.Current</see> temporarily, restoring the previous synchronization context when disposed.
    /// </summary>
    public sealed class ScopedSynchronizationContext : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedSynchronizationContext"/> class, replacing the current synchronization context with <paramref name="replacementContext"/>.
        /// </summary>
        /// <param name="replacementContext">The context to temporarily install as the current synchronization context. This may ne null.</param>
        public ScopedSynchronizationContext(SynchronizationContext replacementContext)
        {
            this.PreviousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(replacementContext);
        }

        /// <summary>
        /// Gets the previous synchronization context. This was the value of <see cref="SynchronizationContext.Current"/> at the time this object was initialized. This may be null.
        /// </summary>
        public SynchronizationContext PreviousContext { get; private set; }

        /// <summary>
        /// Restores <see cref="PreviousContext"/> as the current synchronization context.
        /// </summary>
        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(this.PreviousContext);
        }
    }
}
