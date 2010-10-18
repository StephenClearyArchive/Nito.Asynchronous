namespace Nito.Communication
{
    using System;
    using System.ComponentModel;
    using System.Net;

    /// <summary>
    /// Represents a client socket built on the asynchronous event-based model (see <see cref="IAsyncTcpConnection"/>).
    /// </summary>
    /// <remarks>
    /// <para>Client sockets must be connected before they can be used for any other operations.</para>
    /// </remarks>
    public interface IAsyncClientTcpSocket : IAsyncTcpConnection
    {
#if !SILVERLIGHT
        /// <summary>
        /// Binds to a local endpoint. This method is not normally used.
        /// </summary>
        /// <remarks>
        /// <para>This method may not be called after <see cref="O:Nito.Communication.IAsyncClientTcpSocket.ConnectAsync"/>.</para>
        /// </remarks>
        /// <param name="bindTo">The local endpoint.</param>
        void Bind(EndPoint bindTo);
#endif

        /// <summary>
        /// Initiates a connect operation.
        /// </summary>
        /// <remarks>
        /// <para>There may be only one connect operation for a client socket, and it must be the first operation performed.</para>
        /// <para>The connect operation will complete by invoking <see cref="ConnectCompleted"/>, unless the socket is closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>Connect operations are never cancelled.</para>
        /// </remarks>
        /// <param name="server">The address and port of the server to connect to.</param>
        void ConnectAsync(EndPoint server);

        /// <summary>
        /// Indicates the completion of a connect operation, either successfully or with error. 
        /// </summary>
        /// <remarks>
        /// <para>Connect operations are never cancelled.</para>
        /// <para>Connect operations will not complete if the socket is closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// <para>Generally, a handler of this event will call <see cref="IAsyncTcpConnection.ReadAsync"/> to start a read operation immediately.</para>
        /// <para>If a connect operation completes with error, the socket should be closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// </remarks>
        event Action<AsyncCompletedEventArgs> ConnectCompleted;
    }
}
