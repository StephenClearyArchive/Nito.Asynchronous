using System;

namespace Nito.Communication
{
    using System.Net;

    using Async;

    public interface IAsyncServerTcpSocket : IDisposable
    {
        /// <summary>
        /// Gets the endpoint of the listening socket.
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// <param name="bindTo">The local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        void Bind(EndPoint bindTo, int backlog);

        /// <summary>
        /// Initiates an accept operation.
        /// </summary>
        /// <remarks>
        /// <para>There may be only one accept operation at a time for a listening socket.</para>
        /// <para>The accept operation will complete by invoking <see cref="AcceptCompleted"/> unless the socket is closed (<see cref="InterfaceExtensions.Close(IAsyncServerTcpSocket)"/>).</para>
        /// <para>Accept operations are never cancelled.</para>
        /// </remarks>
        void AcceptAsync();

        /// <summary>
        /// Indicates the completion of an accept operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Accept operations are never cancelled.</para>
        /// <para>Accept operations will not complete if the socket is closed (<see cref="InterfaceExtensions.Close(IAsyncServerTcpSocket)"/>).</para>
        /// <para>The result of the accept operation is a new socket connection.</para>
        /// <para>Generally, a handler of this event will call <see cref="AcceptAsync"/> to continue accepting other connections.</para>
        /// <para>If an accept operation completes with error, no action is necessary other than continuing to accept other connections.</para>
        /// </remarks>
        event Action<AsyncResultEventArgs<IAsyncTcpConnection>> AcceptCompleted;
    }
}
