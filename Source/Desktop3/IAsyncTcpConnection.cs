using System;

namespace Nito.Communication
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;

    using Async;

    /// <summary>
    /// Represents a connected data socket using the asynchronous event-based model.
    /// </summary>
    /// <remarks>
    /// <para>No operations are ever cancelled. During a socket shutdown, some operations may not complete; see below for details.</para>
    /// <para>Only one read operation should be active on a data socket at any time.</para>
    /// <para>Multiple write operations may be active at the same time.</para>
    /// <para>Disconnecting a socket may be done one of three ways: shutting down a socket, closing a socket, and abortively closing a socket.
    /// <para>Shutting down a socket performs a graceful disconnect. Once a socket starts shutting down, no read or write operations will complete; only the shutting down operation will complete.</para>
    /// <para>Closing a socket performs a graceful disconnect in the background. Once a socket is closed, no operations will complete.</para>
    /// <para>Abortively closing a socket performs an immediate hard close. This is not recommended in general practice, but is the fastest way to release system socket resources. Once a socket is abortively closed, no operations will complete.</para></para>
    /// <para>All operations must be initiated from a thread with a non-free-threaded synchronization context. This means that, e.g., GUI threads may call these methods, but free threads may not.</para>
    /// <para>Implementors must implement <see cref="IDisposable.Dispose"/> such that it closes the socket, ensuring that no operations will complete.</para>
    /// </remarks>
    public interface IAsyncTcpConnection : IDisposable
    {
        /// <summary>
        /// Gets the endpoint on this side of the connection.
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the endpoint on the remote side of the connection.
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets or sets a value indicating the Nagle algorithm has been disabled.
        /// </summary>
        /// <remarks>
        /// <para>The default is false. Generally, this should be left to its default value.</para>
        /// </remarks>
        bool NoDelay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether (and how long) a graceful shutdown will be performed in the background.
        /// </summary>
        /// <remarks>
        /// <para>Setting LingerState to enabled with a 0 timeout will make all calls to <see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/> act as though <see cref="InterfaceExtensions.AbortiveClose"/> was called. Generally, this should be left to its default value.</para>
        /// </remarks>
        LingerOption LingerState { get; set; }

        /// <summary>
        /// Initiates a read operation.
        /// </summary>
        /// <param name="buffer">The buffer to receive the data.</param>
        /// <param name="offset">The offset into <paramref name="buffer"/> to write the received data.</param>
        /// <param name="size">The maximum number of bytes that may be written into <paramref name="buffer"/> at <paramref name="offset"/>.</param>
        /// <remarks>
        /// <para>There may be only one active read operation at any time.</para>
        /// <para>The read operation will complete by invoking <see cref="ReadCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>Read operations are never cancelled.</para>
        /// </remarks>
        void ReadAsync(byte[] buffer, int offset, int size);

        /// <summary>
        /// Initiates a read operation.
        /// </summary>
        /// <param name="buffers">The buffers containing the data to receive the data.</param>
        /// <remarks>
        /// <para>There may be only one active read operation at any time.</para>
        /// <para>The read operation will complete by invoking <see cref="ReadCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>Read operations are never cancelled.</para>
        /// </remarks>
        void ReadAsync(IList<ArraySegment<byte>> buffers);

        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <remarks>
        /// <para>Multiple write operations may be active at the same time.</para>
        /// <para>The write operation will complete by invoking <see cref="WriteCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>Write operations are never cancelled.</para>
        /// <para>If <paramref name="state"/> is an instance of <see cref="CallbackOnErrorsOnly"/>, then <see cref="WriteCompleted"/> is only invoked in an error situation; it is not invoked if the write completes successfully.</para>
        /// </remarks>
        /// <param name="buffer">The buffer containing the data to write to the socket.</param>
        /// <param name="offset">The offset of the data within <paramref name="buffer"/>.</param>
        /// <param name="size">The number of bytes of data, at <paramref name="offset"/> within <paramref name="buffer"/>.</param>
        /// <param name="state">The context, which is passed to <see cref="WriteCompleted"/> as <c>e.UserState</c>.</param>
        void WriteAsync(byte[] buffer, int offset, int size, object state = null);

        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <param name="buffers">The buffers containing the data to write to the socket.</param>
        /// <param name="state">The context, which is passed to <see cref="WriteCompleted"/> as <c>e.UserState</c>.</param>
        /// <remarks>
        /// 	<para>Multiple write operations may be active at the same time.</para>
        /// 	<para>The write operation will complete by invoking <see cref="IAsyncTcpConnection.WriteCompleted"/>, unless the socket is shut down (<see cref="IAsyncTcpConnection.ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// 	<para>Write operations are never cancelled.</para>
        /// 	<para>If <paramref name="state"/> is an instance of <see cref="CallbackOnErrorsOnly"/>, then <see cref="IAsyncTcpConnection.WriteCompleted"/> is only invoked in an error situation; it is not invoked if the write completes successfully.</para>
        /// </remarks>
        void WriteAsync(IList<ArraySegment<byte>> buffers, object state = null);

        /// <summary>
        /// Initiates a shutdown operation. Once a shutdown operation is initiated, only the shutdown operation will complete.
        /// </summary>
        /// <remarks>
        /// <para>The shutdown operation will complete by invoking <see cref="ShutdownCompleted"/>.</para>
        /// <para>Shutdown operations are never cancelled.</para>
        /// </remarks>
        void ShutdownAsync();

        /// <summary>
        /// Indicates the completion of a read operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Read operations are never cancelled.</para>
        /// <para>Read operations will not complete if the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>Generally, a handler of this event will call <see cref="ReadAsync"/> to start another read operation immediately.</para>
        /// <para>If a read operation completes with error, the socket should be closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>The result of a read operation is the number of bytes read from the socket.</para>
        /// <para>Note that a successful read operation may complete even though it only read part of the buffer.</para>
        /// <para>A successful read operation may also complete with a 0-length read; this indicates the remote side has gracefully closed. The appropriate response to a 0-length read is to <see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/> the socket.</para>
        /// </remarks>
        event Action<AsyncResultEventArgs<int>> ReadCompleted;

        /// <summary>
        /// Indicates the completion of a write operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Write operations are never cancelled.</para>
        /// <para>Write operations will not complete if the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>Note that even though a write operation completes, the data may not have been received by the remote end. However, it is still important to handle <see cref="WriteCompleted"/>, because errors may be reported.</para>
        /// <para>If a write operation completes with error, the socket should be closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// </remarks>
        event Action<AsyncCompletedEventArgs> WriteCompleted;

        /// <summary>
        /// Indicates the completion of a shutdown operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Shutdown operations are never cancelled.</para>
        /// <para>Generally, a shutdown completing with error is handled the same as a shutdown completing successfully: the normal response in both situations is to <see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/> the socket.</para>
        /// </remarks>
        event Action<AsyncCompletedEventArgs> ShutdownCompleted;
    }
}
