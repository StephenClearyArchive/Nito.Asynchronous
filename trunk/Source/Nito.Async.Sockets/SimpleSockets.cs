using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Timers;
using System.Net.Sockets;

// Copyright 2009 by Nito Programs.

namespace Nito.Async.Sockets
{
    /// <summary>
    /// Represents a connected data socket built on the asynchronous event-based model using a simple packet/keepalive protocol.
    /// </summary>
    /// <remarks>
    /// <para>No operations are ever cancelled. During a socket shutdown, some operations may not complete; see below for details.</para>
    /// <para>Read operations are always running on a connected socket, so <see cref="PacketArrived"/> should always have a handler.</para>
    /// <para>Keepalive packets are automatically written as needed to the data socket, so <see cref="WriteCompleted"/> should always have a handler at least to detect packet write errors.</para>
    /// <para>Multiple write operations may be active on a data socket; the data will be written to the socket in order.</para>
    /// <para>Packets may not be larger than 2 GB.</para>
    /// <para>Disconnecting a socket may be done one of three ways: shutting down a socket, closing a socket, and abortively closing a socket.</para>
    /// <para>Shutting down a socket performs a graceful disconnect. Once a socket starts shutting down, no read or write operations will complete; only the shutting down operation will complete.</para>
    /// <para>Closing a socket performs a graceful disconnect in the background. Once a socket is closed, no operations will complete.</para>
    /// <para>Abortively closing a socket performs an immediate hard close. This is not recommended in general practice, but is the fastest way to release system socket resources. Once a socket is abortively closed, no operations will complete.</para>
    /// <para>All operations must be initiated from a thread with a non-free-threaded synchronization context. This means that, e.g., GUI threads may call these methods, but free threads may not.</para>
    /// </remarks>
    public interface ISimpleAsyncTcpConnection : IDisposable
    {
        #region Connection properties

        /// <summary>
        /// Returns the IP address and port on this side of the connection.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Returns the IP address and port on the remote side of the connection.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// The amount of time the socket waits before sending a keepalive packet.
        /// </summary>
        TimeSpan KeepaliveTimeout { get; set; }

        /// <summary>
        /// True if the Nagle algorithm has been disabled.
        /// </summary>
        /// <remarks>
        /// <para>The default is false. Generally, this should be left to its default value.</para>
        /// </remarks>
        bool NoDelay { get; set; }

        /// <summary>
        /// If and how long a graceful shutdown will be performed in the background.
        /// </summary>
        /// <remarks>
        /// <para>Setting LingerState to enabled with a 0 timeout will make all calls to <see cref="Close"/> act as though <see cref="AbortiveClose"/> was called. Generally, this should be left to its default value.</para>
        /// </remarks>
        LingerOption LingerState { get; set; }

        #endregion

        #region Read operation

        /// <summary>
        /// Indicates the arrival of a packet, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>These operations are never cancelled.</para>
        /// <para>If this event is invoked with an error, the socket should be closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>The arrival of a null packet indicates the remote side has gracefully closed. The appropriate response to a null packet read is to <see cref="Close"/> the socket.</para>
        /// <para>This event is not invoked after the socket is shut down (<see cref="ShutdownAsync"/>, closed (<see cref="Close"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// </remarks>
        event Action<AsyncResultEventArgs<byte[]>> PacketArrived;

        #endregion

        #region Write operation

        /// <overloads>
        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <remarks>
        /// <para>Multiple write operations may be active at the same time.</para>
        /// <para>The write operation will complete by invoking <see cref="WriteCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="Close"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Write operations are never cancelled.</para>
        /// </remarks>
        /// </overloads>
        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <remarks>
        /// <para>Multiple write operations may be active at the same time.</para>
        /// <para>The write operation will complete by invoking <see cref="WriteCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="Close"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Write operations are never cancelled.</para>
        /// </remarks>
        /// <param name="packet">The packet to write to the socket.</param>
        void WriteAsync(byte[] packet);

        /// <inheritdoc cref="WriteAsync(byte[])" />
        /// <remarks>
        /// <inheritdoc cref="WriteAsync(byte[])" />
        /// <para>If <paramref name="state"/> is an instance of <see cref="CallbackOnErrorsOnly"/>, then <see cref="WriteCompleted"/> is only invoked in an error situation; it is not invoked if the write completes successfully.</para>
        /// </remarks>
        /// <param name="packet">The packet to write to the socket.</param>
        /// <param name="state">The context, which is passed to <see cref="WriteCompleted"/> as <c>e.UserState</c>.</param>
        void WriteAsync(byte[] packet, object state);

        /// <summary>
        /// Indicates the completion of a write operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Write operations are never cancelled.</para>
        /// <para>Write operations will not complete if the socket is shut down (<see cref="ShutdownAsync"/>, closed (<see cref="Close"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Note that even though a write operation completes, the data may not have been received by the remote end. However, it is still important to handle <see cref="WriteCompleted"/>, because errors may be reported.</para>
        /// <para>If a write operation completes with error, the socket should be closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Note that write operations may "complete" with error, even if WriteAsync was not called; this is how keepalive packet errors are detected. For this reason, <see cref="WriteCompleted"/> should always have a handler.</para>
        /// </remarks>
        event Action<AsyncCompletedEventArgs> WriteCompleted;

        #endregion

        #region Shutdown operation

        /// <summary>
        /// Initiates a shutdown operation. Once a shutdown operation is initiated, only the shutdown operation will complete.
        /// </summary>
        /// <remarks>
        /// <para>The shutdown operation will complete by invoking <see cref="ShutdownCompleted"/>.</para>
        /// <para>Shutdown operations are never cancelled.</para>
        /// </remarks>
        void ShutdownAsync();

        /// <summary>
        /// Indicates the completion of a shutdown operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Shutdown operations are never cancelled.</para>
        /// <para>Generally, a shutdown completing with error is handled the same as a shutdown completing successfully: the normal response in both situations is to <see cref="Close"/> the socket.</para>
        /// </remarks>
        event Action<AsyncCompletedEventArgs> ShutdownCompleted;

        /// <summary>
        /// Gracefully or abortively closes the socket. Once this method is called, no operations will complete.
        /// </summary>
        /// <remarks>
        /// <para>This method performs a graceful shutdown of the underlying socket; however, this is performed in the background, so the application never receives notification of its completion. <see cref="ShutdownAsync"/> performs a graceful shutdown with completion.</para>
        /// <para>Note that exiting the process after calling this method but before the background shutdown completes will result in an abortive close.</para>
        /// <para><see cref="LingerState"/> will determine whether this method will perform a graceful or abortive close.</para>
        /// </remarks>
        void Close();

        /// <summary>
        /// Abortively closes the socket. Once this method is called, no operations will complete.
        /// </summary>
        /// <remarks>
        /// <para>This method provides the fastest way to reclaim socket resources; however, its use is not generally recommended; <see cref="Close"/> should usually be used instead of this method.</para>
        /// </remarks>
        void AbortiveClose();

        #endregion
    }

    /// <summary>
    /// Represents a client socket built on the asynchronous event-based model using a simple packet/keepalive protocol (see <see cref="ISimpleAsyncTcpConnection"/>).
    /// </summary>
    /// <remarks>
    /// <para>Client sockets must be connected before they can be used for any other operations. Once connected, they automatically read and send keepalive packets.</para>
    /// </remarks>
    public sealed class SimpleClientTcpSocket : ISimpleAsyncTcpConnection
    {
        /// <summary>
        /// The underlying client socket.
        /// </summary>
        private ClientTcpSocket Socket;

        /// <summary>
        /// The packetizer that is used to buffer socket data read from <see cref="Socket"/>.
        /// </summary>
        private SocketPacketProtocol Packetizer;

        /// <summary>
        /// The timer used to send keepalive packets if necessary.
        /// </summary>
        private Timer KeepaliveTimer;

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleClientTcpSocket"/>.
        /// </summary>
        public SimpleClientTcpSocket()
        {
            // Create the underlying socket and hook up its Connect event.
            Socket = new ClientTcpSocket();
            Socket.ConnectCompleted += SocketConnectCompleted;

            // Create the receive buffer for the socket.
            Packetizer = new SocketPacketProtocol(Socket);

            // Initialize the keepalive timer and its default value.
            KeepaliveTimer = new Timer();
            KeepaliveTimer.Elapsed += KeepaliveTimerTimeout;
            KeepaliveTimer.Interval = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Gracefully or abortively closes the socket connection. See <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            KeepaliveTimer.Dispose();
            Socket.Dispose();
        }

        /// <summary>
        /// Responds to the socket's ConnectCompleted event.
        /// </summary>
        /// <param name="e">Argument object for this event.</param>
        private void SocketConnectCompleted(AsyncCompletedEventArgs e)
        {
            // Note: this is called just before the user-defined ConnectCompleted

            // If this was a connection error, then do nothing
            if (e.Error != null)
                return;

            // Start reading
            Packetizer.Start();

            // Start writing keepalive packets as necessary
            KeepaliveTimer.SetPeriodic(KeepaliveTimer.Interval);
        }

        /// <summary>
        /// Responds to the keepalive timer's Timeout event.
        /// </summary>
        private void KeepaliveTimerTimeout()
        {
            // Write a keepalive packet
            SocketPacketProtocol.WriteKeepaliveAsync(Socket);
        }

        #region Connection properties

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return Socket.LocalEndPoint; }
        }

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return Socket.RemoteEndPoint; }
        }

        /// <inheritdoc />
        public TimeSpan KeepaliveTimeout
        {
            get
            {
                return KeepaliveTimer.Interval;
            }
            set
            {
                KeepaliveTimer.SetPeriodic(value);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public bool NoDelay
        {
            get { return Socket.NoDelay; }
            set { Socket.NoDelay = value; }
        }

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public LingerOption LingerState
        {
            get { return Socket.LingerState; }
            set { Socket.LingerState = value; }
        }

        #endregion

        #region Socket methods

        /// <summary>
        /// Binds to a local endpoint. This method is not normally used.
        /// </summary>
        /// <remarks>
        /// <para>This method may not be called after <see cref="O:Nito.Async.Sockets.SimpleClientTcpSocket.ConnectAsync"/>.</para>
        /// </remarks>
        /// <param name="bindTo">The local endpoint.</param>
        public void Bind(IPEndPoint bindTo)
        {
            Socket.Bind(bindTo);
        }

        #endregion

        #region Connect operation

        /// <overloads>
        /// <summary>
        /// Initiates a connect operation.
        /// </summary>
        /// <remarks>
        /// <para>There may be only one connect operation for a client socket, and it must be the first operation performed.</para>
        /// <para>The connect operation will complete by invoking <see cref="ConnectCompleted"/>, unless the socket is closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Connect operations are never cancelled.</para>
        /// </remarks>
        /// </overloads>
        /// <summary>
        /// Initiates a connect operation.
        /// </summary>
        /// <remarks>
        /// <para>There may be only one connect operation for a client socket, and it must be the first operation performed.</para>
        /// <para>The connect operation will complete by invoking <see cref="ConnectCompleted"/>, unless the socket is closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Connect operations are never cancelled.</para>
        /// </remarks>
        /// <param name="server">The address and port of the server to connect to.</param>
        public void ConnectAsync(IPEndPoint server)
        {
            Socket.ConnectAsync(server);
        }

        /// <inheritdoc cref="ConnectAsync(IPEndPoint)" />
        /// <param name="address">The address of the server to connect to.</param>
        /// <param name="port">The port of the server to connect to.</param>
        public void ConnectAsync(IPAddress address, int port)
        {
            Socket.ConnectAsync(address, port);
        }

        /// <summary>
        /// Indicates the completion of a connect operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Connect operations are never cancelled.</para>
        /// <para>Connect operations will not complete if the socket is closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>A handler of this event should assign handlers to recurring events (<see cref="PacketArrived"/> and <see cref="WriteCompleted"/>) if they do not already have handlers. Otherwise, data and error events may be lost.</para>
        /// <para>If a connect operation completes with error, the socket should be closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// </remarks>
        public event Action<AsyncCompletedEventArgs> ConnectCompleted
        {
            add
            {
                Socket.ConnectCompleted += value;
            }
            remove
            {
                Socket.ConnectCompleted -= value;
            }
        }

        #endregion

        #region Read operation

        /// <inheritdoc />
        // The packetizer already ignores keepalive packets, so we don't have to handle them specially.
        public event Action<AsyncResultEventArgs<byte[]>> PacketArrived
        {
            add
            {
                Packetizer.PacketArrived += value;
            }
            remove
            {
                Packetizer.PacketArrived -= value;
            }
        }

        #endregion

        #region Write operation

        /// <inheritdoc />
        public void WriteAsync(byte[] packet)
        {
            KeepaliveTimer.Restart();
            SocketPacketProtocol.WritePacketAsync(Socket, packet);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] packet, object state)
        {
            KeepaliveTimer.Restart();
            SocketPacketProtocol.WritePacketAsync(Socket, packet, state);
        }

        /// <inheritdoc />
        // We queue keepalive packets using CallbackOnErrorsOnly, so we don't have to handle this specially
        public event Action<AsyncCompletedEventArgs> WriteCompleted
        {
            add
            {
                Socket.WriteCompleted += value;
            }
            remove
            {
                Socket.WriteCompleted -= value;
            }
        }

        #endregion

        #region Shutdown operation

        /// <inheritdoc />
        public void ShutdownAsync()
        {
            KeepaliveTimer.Cancel();
            Socket.ShutdownAsync();
        }

        /// <inheritdoc />
        public event Action<AsyncCompletedEventArgs> ShutdownCompleted
        {
            add
            {
                Socket.ShutdownCompleted += value;
            }
            remove
            {
                Socket.ShutdownCompleted -= value;
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            KeepaliveTimer.Cancel();
            Socket.Close();
        }

        /// <inheritdoc />
        public void AbortiveClose()
        {
            KeepaliveTimer.Cancel();
            Socket.AbortiveClose();
        }

        #endregion
    }

    /// <summary>
    /// Represents a child connection of a listening server socket, built on the asynchronous event-based model using a simple packet/keepalive protocol (see <see cref="ISimpleAsyncTcpConnection"/>).
    /// </summary>
    public sealed class SimpleServerChildTcpSocket : ISimpleAsyncTcpConnection
    {
        /// <summary>
        /// The underlying socket connection.
        /// </summary>
        private ServerChildTcpSocket Socket;

        /// <summary>
        /// The packetizer that is used to buffer socket data read from <see cref="Socket"/>.
        /// </summary>
        private SocketPacketProtocol Packetizer;

        /// <summary>
        /// The timer used to send keepalive packets if necessary.
        /// </summary>
        private Timer KeepaliveTimer;

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleServerChildTcpSocket"/>.
        /// </summary>
        /// <param name="socket">The socket to wrap.</param>
        internal SimpleServerChildTcpSocket(ServerChildTcpSocket socket)
        {
            Socket = socket;

            // Create the receive buffer for the socket
            Packetizer = new SocketPacketProtocol(Socket);

            // Initialize the keepalive timer
            KeepaliveTimer = new Timer();
            KeepaliveTimer.Elapsed += KeepaliveTimerTimeout;

            // Start reading
            Packetizer.Start();

            // Start writing keepalive packets as necessary
            KeepaliveTimer.SetPeriodic(TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Gracefully or abortively closes the socket connection. See <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            KeepaliveTimer.Dispose();
            Socket.Dispose();
        }

        /// <summary>
        /// Responds to the keepalive timer's Timeout event.
        /// </summary>
        private void KeepaliveTimerTimeout()
        {
            SocketPacketProtocol.WriteKeepaliveAsync(Socket);
        }

        #region Connection properties

        /// <inheritdoc />
        public IPEndPoint LocalEndPoint
        {
            get { return Socket.LocalEndPoint; }
        }

        /// <inheritdoc />
        public IPEndPoint RemoteEndPoint
        {
            get { return Socket.RemoteEndPoint; }
        }

        /// <inheritdoc />
        public TimeSpan KeepaliveTimeout
        {
            get
            {
                return KeepaliveTimer.Interval;
            }
            set
            {
                KeepaliveTimer.SetPeriodic(value);
            }
        }

        /// <inheritdoc />
        public bool NoDelay
        {
            get { return Socket.NoDelay; }
            set { Socket.NoDelay = value; }
        }

        /// <inheritdoc />
        public LingerOption LingerState
        {
            get { return Socket.LingerState; }
            set { Socket.LingerState = value; }
        }

        #endregion

        #region Read operation

        /// <inheritdoc />
        // The packetizer already ignores keepalive packets, so we don't have to handle them specially.
        public event Action<AsyncResultEventArgs<byte[]>> PacketArrived
        {
            add
            {
                Packetizer.PacketArrived += value;
            }
            remove
            {
                Packetizer.PacketArrived -= value;
            }
        }

        #endregion

        #region Write operation

        /// <inheritdoc />
        public void WriteAsync(byte[] packet)
        {
            KeepaliveTimer.Restart();
            SocketPacketProtocol.WritePacketAsync(Socket, packet);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] packet, object state)
        {
            KeepaliveTimer.Restart();
            SocketPacketProtocol.WritePacketAsync(Socket, packet, state);
        }

        /// <inheritdoc />
        // We queue keepalive packets using CallbackOnErrorsOnly, so we don't have to handle this specially
        public event Action<AsyncCompletedEventArgs> WriteCompleted
        {
            add
            {
                Socket.WriteCompleted += value;
            }
            remove
            {
                Socket.WriteCompleted -= value;
            }
        }

        #endregion

        #region Shutdown operation

        /// <inheritdoc />
        public void ShutdownAsync()
        {
            KeepaliveTimer.Cancel();
            Socket.ShutdownAsync();
        }

        /// <inheritdoc />
        public event Action<AsyncCompletedEventArgs> ShutdownCompleted
        {
            add
            {
                Socket.ShutdownCompleted += value;
            }
            remove
            {
                Socket.ShutdownCompleted -= value;
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            KeepaliveTimer.Cancel();
            Socket.Close();
        }

        /// <inheritdoc />
        public void AbortiveClose()
        {
            KeepaliveTimer.Cancel();
            Socket.AbortiveClose();
        }

        #endregion
    }

    /// <summary>
    /// Represents a listening server socket built on the asynchronous event-based model.
    /// </summary>
    /// <remarks>
    /// <para>No operations are ever cancelled. When the socket is closed, active operations do not complete.</para>
    /// <para>Listening is started only once; established connections are reported via <see cref="ConnectionArrived"/>.</para>
    /// <para>The listening operation must be initiated from a thread with a non-free-threaded synchronization context. This means that, e.g., GUI threads may call these methods, but free threads may not.</para>
    /// </remarks>
    public sealed class SimpleServerTcpSocket : IDisposable
    {
        /// <summary>
        /// The underlying listening socket.
        /// </summary>
        private ServerTcpSocket Socket;

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleServerTcpSocket"/>.
        /// </summary>
        public SimpleServerTcpSocket()
        {
            // Create the underlying socket
            Socket = new ServerTcpSocket();
            Socket.AcceptCompleted += SocketAcceptCompleted;
        }

        /// <summary>
        /// Closes the listening socket. See <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Socket.Dispose();
        }

        /// <summary>
        /// Closes the listening socket immediately and frees all resources.
        /// </summary>
        /// <remarks>
        /// <para>No events will be raised once this method is called.</para>
        /// </remarks>
        public void Close()
        {
            Socket.Close();
        }

        #region Socket methods

        /// <overloads>
        /// <summary>
        /// Binds to a local endpoint and begins listening and accepting connections.
        /// </summary>
        /// </overloads>
        /// <summary>
        /// Binds to a local endpoint and begins listening and accepting connections.
        /// </summary>
        /// <param name="bindTo">The local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public void Listen(IPEndPoint bindTo, int backlog)
        {
            Socket.Bind(bindTo, backlog);
            Socket.AcceptAsync();
        }

        /// <inheritdoc cref="Listen(IPEndPoint, int)" />
        /// <param name="address">The address of the local endpoint.</param>
        /// <param name="port">The port of the local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public void Listen(IPAddress address, int port, int backlog)
        {
            Socket.Bind(address, port, backlog);
            Socket.AcceptAsync();
        }

        /// <inheritdoc cref="Listen(IPEndPoint, int)" />
        /// <param name="bindTo">The local endpoint.</param>
        public void Listen(IPEndPoint bindTo)
        {
            Socket.Bind(bindTo);
            Socket.AcceptAsync();
        }

        /// <inheritdoc cref="Listen(IPEndPoint, int)" />
        /// <param name="address">The address of the local endpoint.</param>
        /// <param name="port">The port of the local endpoint.</param>
        public void Listen(IPAddress address, int port)
        {
            Socket.Bind(address, port);
            Socket.AcceptAsync();
        }

        /// <inheritdoc cref="Listen(IPEndPoint, int)" />
        /// <param name="port">The port of the local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public void Listen(int port, int backlog)
        {
            Socket.Bind(port, backlog);
            Socket.AcceptAsync();
        }

        /// <inheritdoc cref="Listen(IPEndPoint, int)" />
        /// <param name="port">The port of the local endpoint.</param>
        public void Listen(int port)
        {
            Socket.Bind(port);
            Socket.AcceptAsync();
        }

        #endregion

        #region Socket properties

        /// <summary>
        /// Returns the IP address and port of the listening socket.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return Socket.LocalEndPoint;
            }
        }

        #endregion

        #region Listen operation

        /// <summary>
        /// Converts the results of an accept operation on a <see cref="ServerTcpSocket"/> to the results of a listen operation on a
        /// <see cref="SimpleServerTcpSocket"/>.
        /// </summary>
        /// <param name="e">The results of the accept operation.</param>
        /// <returns>The results of the listen operation.</returns>
        private static AsyncResultEventArgs<SimpleServerChildTcpSocket> ConvertArgs(AsyncResultEventArgs<ServerChildTcpSocket> e)
        {
            if (e.Error != null)
                return new AsyncResultEventArgs<SimpleServerChildTcpSocket>(e.Error);
            return new AsyncResultEventArgs<SimpleServerChildTcpSocket>(new SimpleServerChildTcpSocket(e.Result));
        }

        /// <summary>
        /// Handles the <see cref="ServerTcpSocket.AcceptCompleted"/> event by restarting the accept operation and notifying the user of a listen
        /// operation completion.
        /// </summary>
        /// <param name="e">The results of the accept operation.</param>
        private void SocketAcceptCompleted(AsyncResultEventArgs<ServerChildTcpSocket> e)
        {
            Socket.AcceptAsync();
            if (ConnectionArrived != null)
                ConnectionArrived(ConvertArgs(e));
        }

        /// <summary>
        /// Indicates the arrival of a new socket connection, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>These operations are never cancelled.</para>
        /// <para>This event is not invoked after the socket is closed (<see cref="Close"/>).</para>
        /// <para>A handler of this event should assign handlers to the new socket's <see cref="SimpleServerChildTcpSocket.PacketArrived"/> and <see cref="SimpleServerChildTcpSocket.WriteCompleted"/>. Otherwise, data and error events may be lost.</para>
        /// <para>If this event is invoked with an error, no action is necessary.</para>
        /// </remarks>
        public event Action<AsyncResultEventArgs<SimpleServerChildTcpSocket>> ConnectionArrived;

        #endregion
    }
}
