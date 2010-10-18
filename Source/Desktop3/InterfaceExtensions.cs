namespace Nito.Communication
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public static class InterfaceExtensions
    {
        /// <summary>
        /// The traditional maximum value was 5, according to Stevens' TCP/IP vol 1; the current maximum is several hundred, but this is rarely necessary, so we use a value of 2.
        /// </summary>
        private const int DefaultBacklog = 2;

        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <remarks>
        /// <para>Multiple write operations may be active at the same time.</para>
        /// <para>The write operation will complete by invoking <see cref="IAsyncTcpConnection.WriteCompleted"/>, unless the socket is shut down (<see cref="IAsyncTcpConnection.ShutdownAsync"/>), closed (<see cref="Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Write operations are never cancelled.</para>
        /// </remarks>
        /// <param name="connection">The connection to which to write.</param>
        /// <param name="buffer">The data to write to the socket.</param>
        public static void WriteAsync(this IAsyncTcpConnection connection, byte[] buffer)
        {
            connection.WriteAsync(buffer, 0, buffer.Length, null);
        }

        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <remarks>
        /// <para>Multiple write operations may be active at the same time.</para>
        /// <para>The write operation will complete by invoking <see cref="IAsyncTcpConnection.WriteCompleted"/>, unless the socket is shut down (<see cref="IAsyncTcpConnection.ShutdownAsync"/>), closed (<see cref="Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Write operations are never cancelled.</para>
        /// <para>If <paramref name="state"/> is an instance of <see cref="CallbackOnErrorsOnly"/>, then <see cref="IAsyncTcpConnection.WriteCompleted"/> is only invoked in an error situation; it is not invoked if the write completes successfully.</para>
        /// </remarks>
        /// <param name="connection">The connection to which to write.</param>
        /// <param name="buffer">The data to write to the socket.</param>
        /// <param name="state">The context, which is passed to <see cref="IAsyncTcpConnection.WriteCompleted"/> as <c>e.UserState</c>.</param>
        public static void WriteAsync(this IAsyncTcpConnection connection, byte[] buffer, object state)
        {
            connection.WriteAsync(buffer, 0, buffer.Length, state);
        }

        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <param name="connection">The connection to which to write.</param>
        /// <param name="buffers">The buffers containing the data to write to the socket.</param>
        /// <remarks>
        /// 	<para>Multiple write operations may be active at the same time.</para>
        /// 	<para>The write operation will complete by invoking <see cref="IAsyncTcpConnection.WriteCompleted"/>, unless the socket is shut down (<see cref="IAsyncTcpConnection.ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// 	<para>Write operations are never cancelled.</para>
        /// </remarks>
        public static void WriteAsync(this IAsyncTcpConnection connection, params ArraySegment<byte>[] buffers)
        {
            connection.WriteAsync(buffers, null);
        }

        /// <summary>
        /// Gracefully or abortively closes the socket. Once this method is called, no operations will complete.
        /// </summary>
        /// <remarks>
        /// <para>This method performs a graceful shutdown of the underlying socket; however, this is performed in the background, so the application never receives notification of its completion. <see cref="IAsyncTcpConnection.ShutdownAsync"/> performs a graceful shutdown with completion.</para>
        /// <para>Note that exiting the process after calling this method but before the background shutdown completes will result in an abortive close.</para>
        /// <para><see cref="IAsyncTcpConnection.LingerState"/> will determine whether this method will perform a graceful or abortive close.</para>
        /// </remarks>
        public static void Close(this IAsyncTcpConnection connection)
        {
            connection.Dispose();
        }

        /// <summary>
        /// Abortively closes the socket. Once this method is called, no operations will complete.
        /// </summary>
        /// <remarks>
        /// <para>This method provides the fastest way to reclaim socket resources; however, its use is not generally recommended; <see cref="Close(IAsyncTcpConnection)"/> should usually be used instead of this method.</para>
        /// </remarks>
        public static void AbortiveClose(this IAsyncTcpConnection connection)
        {
            connection.LingerState = new LingerOption(true, 0);
            connection.Dispose();
        }

        /// <summary>
        /// Initiates a connect operation.
        /// </summary>
        /// <remarks>
        /// <para>There may be only one connect operation for a client socket, and it must be the first operation performed.</para>
        /// <para>The connect operation will complete by invoking <see cref="IAsyncClientTcpSocket.ConnectCompleted"/>, unless the socket is closed (<see cref="Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Connect operations are never cancelled.</para>
        /// </remarks>
        /// <param name="socket">The socket on which to initiate the connect operation.</param>
        /// <param name="address">The address of the server to connect to.</param>
        /// <param name="port">The port of the server to connect to.</param>
        public static void ConnectAsync(this IAsyncClientTcpSocket socket, IPAddress address, int port)
        {
            socket.ConnectAsync(new IPEndPoint(address, port));
        }

#if DESKTOP4 || SILVERLIGHT3 || SILVERLIGHT4
        /// <summary>
        /// Initiates a connect operation.
        /// </summary>
        /// <remarks>
        /// <para>There may be only one connect operation for a client socket, and it must be the first operation performed.</para>
        /// <para>The connect operation will complete by invoking <see cref="IAsyncClientTcpSocket.ConnectCompleted"/>, unless the socket is closed (<see cref="Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Connect operations are never cancelled.</para>
        /// </remarks>
        /// <param name="socket">The socket on which to initiate the connect operation.</param>
        /// <param name="host">The hostname of the server to connect to.</param>
        /// <param name="port">The port of the server to connect to.</param>
        public static void ConnectAsync(this IAsyncClientTcpSocket socket, string host, int port)
        {
            socket.ConnectAsync(new DnsEndPoint(host, port));
        }
#endif

        /// <summary>
        /// Closes the listening socket immediately and frees all resources.
        /// </summary>
        /// <remarks>
        /// <para>No events will be raised once this method is called.</para>
        /// </remarks>
        /// <param name="socket">The socket to close.</param>
        public static void Close(this IAsyncServerTcpSocket socket)
        {
            socket.Dispose();
        }

        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// <param name="socket">The socket to bind.</param>
        /// <param name="address">The address of the local endpoint.</param>
        /// <param name="port">The port of the local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public static void Bind(this IAsyncServerTcpSocket socket, IPAddress address, int port, int backlog)
        {
            socket.Bind(new IPEndPoint(address, port), backlog);
        }

        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// <param name="socket">The socket to bind.</param>
        /// <param name="bindTo">The local endpoint.</param>
        public static void Bind(this IAsyncServerTcpSocket socket, IPEndPoint bindTo)
        {
            socket.Bind(bindTo, DefaultBacklog);
        }

        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// <param name="socket">The socket to bind.</param>
        /// <param name="address">The address of the local endpoint.</param>
        /// <param name="port">The port of the local endpoint.</param>
        public static void Bind(this IAsyncServerTcpSocket socket, IPAddress address, int port)
        {
            socket.Bind(new IPEndPoint(address, port), DefaultBacklog);
        }

        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// <param name="socket">The socket to bind.</param>
        /// <param name="port">The port of the local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public static void Bind(this IAsyncServerTcpSocket socket, int port, int backlog)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, port), backlog);
        }

        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// <param name="socket">The socket to bind.</param>
        /// <param name="port">The port of the local endpoint.</param>
        public static void Bind(this IAsyncServerTcpSocket socket, int port)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, port), DefaultBacklog);
        }
    }
}
