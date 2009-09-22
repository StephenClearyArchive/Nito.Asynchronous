using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;

// Copyright 2009 by Nito Programs.

// There are two layers of classes defined in this file: the lower layer (ending in *Impl) provides a thin translation from IAsyncResult
// notifications to event-based notifications, including thread synchronization. The second layer provides additional concurrency requirements,
// especially concerning socket shutdown (e.g., preventing events from being raised on a socket after Close has been called).

namespace Nito.Async.Sockets
{
    /// <summary>
    /// This is a special class that may be passed to some WriteAsync methods to indicate that WriteCompleted should not be called on success.
    /// </summary>
    public sealed class CallbackOnErrorsOnly { }

    /// <summary>
    /// Represents a connected data socket built on the asynchronous event-based model.
    /// </summary>
    /// <remarks>
    /// <para>No operations are ever cancelled. During a socket shutdown, some operations may not complete; see below for details.</para>
    /// <para>Only one read operation should be active on a data socket at any time.</para>
    /// <para>Multiple write operations may be active on a data socket; the data will be written to the socket in order.</para>
    /// <para>Disconnecting a socket may be done one of three ways: shutting down a socket, closing a socket, and abortively closing a socket.
    /// <para>Shutting down a socket performs a graceful disconnect. Once a socket starts shutting down, no read or write operations will complete; only the shutting down operation will complete.</para>
    /// <para>Closing a socket performs a graceful disconnect in the background. Once a socket is closed, no operations will complete.</para>
    /// <para>Abortively closing a socket performs an immediate hard close. This is not recommended in general practice, but is the fastest way to release system socket resources. Once a socket is abortively closed, no operations will complete.</para></para>
    /// <para>All operations must be initiated from a thread with a non-free-threaded synchronization context. This means that, e.g., GUI threads may call these methods, but free threads may not.</para>
    /// </remarks>
    public interface IAsyncTcpConnection : IDisposable
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
        /// Initiates a read operation.
        /// </summary>
        /// <param name="buffer">The buffer to receive the data.</param>
        /// <param name="offset">The offset into <paramref name="buffer"/> to write the received data.</param>
        /// <param name="size">The maximum number of bytes that may be written into <paramref name="buffer"/> at <paramref name="offset"/>.</param>
        /// <remarks>
        /// <para>There may be only one active read operation at any time.</para>
        /// <para>The read operation will complete by invoking <see cref="ReadCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="Close"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Read operations are never cancelled.</para>
        /// </remarks>
        void ReadAsync(byte[] buffer, int offset, int size);

        /// <summary>
        /// Indicates the completion of a read operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Read operations are never cancelled.</para>
        /// <para>Read operations will not complete if the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="Close"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Generally, a handler of this event will call <see cref="ReadAsync"/> to start another read operation immediately.</para>
        /// <para>If a read operation completes with error, the socket should be closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>The result of a read operation is the number of bytes read from the socket.</para>
        /// <para>Note that a successful read operation may complete even though it only read part of the buffer.</para>
        /// <para>A successful read operation may also complete with a 0-length read; this indicates the remote side has gracefully closed. The appropriate response to a 0-length read is to <see cref="Close"/> the socket.</para>
        /// </remarks>
        event Action<AsyncResultEventArgs<int>> ReadCompleted;

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
        /// <param name="buffer">The data to write to the socket.</param>
        void WriteAsync(byte[] buffer);

        /// <inheritdoc cref="WriteAsync(byte[])" />
        /// <remarks>
        /// <inheritdoc cref="WriteAsync(byte[])" />
        /// <para>If <paramref name="state"/> is an instance of <see cref="CallbackOnErrorsOnly"/>, then <see cref="WriteCompleted"/> is only invoked in an error situation; it is not invoked if the write completes successfully.</para>
        /// </remarks>
        /// <param name="buffer">The data to write to the socket.</param>
        /// <param name="state">The context, which is passed to <see cref="WriteCompleted"/> as <c>e.UserState</c>.</param>
        void WriteAsync(byte[] buffer, object state);

        /// <inheritdoc cref="WriteAsync(byte[])" />
        /// <param name="buffer">The buffer containing the data to write to the socket.</param>
        /// <param name="offset">The offset of the data within <paramref name="buffer"/>.</param>
        /// <param name="size">The number of bytes of data, at <paramref name="offset"/> within <paramref name="buffer"/>.</param>
        void WriteAsync(byte[] buffer, int offset, int size);

        /// <inheritdoc cref="WriteAsync(byte[], object)" />
        /// <param name="buffer">The buffer containing the data to write to the socket.</param>
        /// <param name="offset">The offset of the data within <paramref name="buffer"/>.</param>
        /// <param name="size">The number of bytes of data, at <paramref name="offset"/> within <paramref name="buffer"/>.</param>
        /// <param name="state">The context, which is passed to <see cref="WriteCompleted"/> as <c>e.UserState</c>.</param>
        void WriteAsync(byte[] buffer, int offset, int size, object state);

        /// <summary>
        /// Indicates the completion of a write operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Write operations are never cancelled.</para>
        /// <para>Write operations will not complete if the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="Close"/>), or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Note that even though a write operation completes, the data may not have been received by the remote end. However, it is still important to handle <see cref="WriteCompleted"/>, because errors may be reported.</para>
        /// <para>If a write operation completes with error, the socket should be closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
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

    // The standard way of translating an IAsyncResult-based notification to event-based notification is as follows:
    //   1) Define the end-user event to fire, e.g.:
    //      public Action<AsyncResultEventArgs<int>> ReadCompleted { get; set; }
    //   2) Define a (private) method that just invokes the end-user event, e.g.:
    //      // Always runs in User thread
    //      private void InvokeReadCompleted(object args)
    //      {
    //          if (ReadCompleted != null)
    //              ReadCompleted((AsyncResultEventArgs<int>)args);
    //      }
    //   3) Define a (private readonly) delegate that just wraps the Invoke method, e.g.:
    //      private readonly SendOrPostCallback InvokeReadCompleted_;
    //   4) Of course, set it during the contructor, e.g.:
    //          InvokeReadCompleted_ = new SendOrPostCallback(InvokeReadCompleted);
    //   5) Start the operation using AsyncOperation, e.g.:
    //      // Always runs in User thread
    //      public void ReadAsync(byte[] buffer, int offset, int size)
    //      {
    //          // Note: the user-defined "state" object *must* be unique!
    //          AsyncOperation oper = AsyncOperationManager.CreateOperation(state);
    //          Socket.BeginReceive(buffer, offset, size, SocketFlags.None, SocketReadComplete, oper);
    //      }
    //   6) Finally, define the completion method that captures the results of the operation and synchronizes it with the user thread, e.g.:
    //      // Always runs in ThreadPool thread
    //      private void SocketReadComplete(IAsyncResult asyncResult)
    //      {
    //          AsyncOperation oper = (AsyncOperation)asyncResult.AsyncState;
    //          try
    //          {
    //              int result = Socket.EndReceive(asyncResult);
    //              oper.PostOperationCompleted(InvokeReadCompleted_, new AsyncResultEventArgs<int>(result));
    //          }
    //          catch (Exception ex)
    //          {
    //              oper.PostOperationCompleted(InvokeReadCompleted_, new AsyncResultEventArgs<int>(ex));
    //          }
    //      }

    // However, the Nito.Async library makes it much easier:
    //   1) Define the end-user event to fire, e.g.:
    //      public Action<AsyncCompletedEventArgs> WriteCompleted { get; set; }
    //   2) Start the operation using Nito.Async.Sync.SynchronizeAsyncCallback with Nito.Async.Async.InvokeAndCallback, e.g.:
    //      // Always runs in User thread
    //      public void ReadAsync(byte[] buffer, int offset, int size)
    //      {
    //          Socket.BeginReceive(buffer, offset, size, SocketFlags.None, Sync.SynchronizeAsyncCallback((asyncResult) =>
    //              {
    //                  Async.InvokeAndCallback(() => Socket.EndReceive(asyncResult),
    //                      ReadCompleted, null);
    //              }), null);
    //      }
    // This approach has the advantage of a simpler implementation, especially considering that all code runs within a single thread context.

    /// <summary>
    /// Provides a wrapper around a <see cref="Socket"/>, translating the <see cref="IAsyncResult"/>-based notifications to event-based notifications,
    /// including thread synchronization. This is used for client sockets and children of server sockets.
    /// </summary>
    internal class TcpSocketImpl : IDisposable
    {
        /// <summary>
        /// The socket for this connection.
        /// </summary>
        protected Socket Socket { get; private set; }

        /// <summary>
        /// Initializes a new socket wrapper for the given socket.
        /// </summary>
        /// <param name="socket">The socket to wrap.</param>
        // Always runs in User thread
        public TcpSocketImpl(Socket socket)
        {
            Socket = socket;
        }

        /// <summary>
        /// Closes the underlying socket and frees the socket resources. Be sure to clear all events before calling this method!
        /// The socket will be gracefully closed in the background by the WinSock dll unless <see cref="SetAbortive"/> has been called,
        /// in which case the socket will be abortively closed and immediately freed. If the process exits shortly after calling this
        /// method, the socket will be abortively closed when the WinSock dll is unloaded.
        /// </summary>
        // Always runs in User thread
        public void Dispose()
        {
            // Do the actual close
            Socket.Close();
        }

        /// <summary>
        /// Sets a flag in the socket to indicate that the close (performed by <see cref="Dispose"/>) should be done abortively.
        /// </summary>
        // Always runs in User thread
        public void SetAbortive()
        {
            Socket.LingerState = new LingerOption(true, 0);
        }

        #region Connection properties

        /// <summary>
        /// Returns the IP address and port on this side of the connection.
        /// </summary>
        public IPEndPoint LocalEndPoint { get { return (IPEndPoint)Socket.LocalEndPoint; } }

        /// <summary>
        /// Returns the IP address and port on the remote side of the connection.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get { return (IPEndPoint)Socket.RemoteEndPoint; } }

        /// <summary>
        /// True if the Nagle algorithm has been disabled. The default is false. Generally, this should be left to its default value.
        /// </summary>
        public bool NoDelay { get { return Socket.NoDelay; } set { Socket.NoDelay = value; } }

        /// <summary>
        /// If and how long the a graceful shutdown will be performed in the background. Setting LingerState to enabled with a 0 timeout will make all calls
        /// to Close act as though AbortiveClose was called. Generally, this should be left to its default value.
        /// </summary>
        public LingerOption LingerState { get { return Socket.LingerState; } set { Socket.LingerState = value; } }

        #endregion

        #region Write operation

        /// <summary>
        /// Delegate to invoke when the write operation completes.
        /// </summary>
        public Action<AsyncCompletedEventArgs> WriteCompleted { get; set; }

        /// <summary>
        /// Calls <see cref="WriteCompleted"/> if necessary.
        /// </summary>
        /// <param name="args">The arguments to pass to <see cref="WriteCompleted"/>.</param>
        private void OnWriteCompleted(AsyncCompletedEventArgs args)
        {
            // If there's no error and the user state is CallbackOnErrorsOnly, then don't issue the callback
            if (args.Error == null && args.UserState is CallbackOnErrorsOnly)
                return;
            if (WriteCompleted != null)
                WriteCompleted(args);
        }

        /// <summary>
        /// Initiates a write operation on the socket. The operation will be completed via <see cref="WriteCompleted"/>.
        /// </summary>
        /// <param name="buffer">Buffer containing the data to write.</param>
        /// <param name="offset">Offset in <paramref name="buffer"/> where the data begins.</param>
        /// <param name="size">Size of the data.</param>
        /// <param name="state">User-defined state object. May be null.</param>
        // Always runs in User thread
        public void WriteAsync(byte[] buffer, int offset, int size, object state)
        {
            Socket.BeginSend(buffer, offset, size, SocketFlags.None, Sync.SynchronizeAsyncCallback((asyncResult) =>
                {
                    Sync.InvokeAndCallback(() => Socket.EndSend(asyncResult),
                        OnWriteCompleted, asyncResult.AsyncState);
                }), state);
        }

        #endregion

        #region Read operation

        /// <summary>
        /// Delegate to invoke when the read operation completes.
        /// </summary>
        public Action<AsyncResultEventArgs<int>> ReadCompleted { get; set; }

        /// <summary>
        /// Initiates a read operation on the socket. The operation will be completed via <see cref="ReadCompleted"/>.
        /// </summary>
        /// <param name="buffer">Buffer to read the data into.</param>
        /// <param name="offset">Offset in <paramref name="buffer"/> to store the data.</param>
        /// <param name="size">Maximum amount of data to receive in this read operation.</param>
        // Always runs in User thread
        public void ReadAsync(byte[] buffer, int offset, int size)
        {
            Socket.BeginReceive(buffer, offset, size, SocketFlags.None, Sync.SynchronizeAsyncCallback((asyncResult) =>
                {
                    Sync.InvokeAndCallback(() => Socket.EndReceive(asyncResult),
                        ReadCompleted, null);
                }), null);
        }

        #endregion

        #region Shutdown operation

        /// <summary>
        /// Delegate to invoke when the shutdown operation completes.
        /// </summary>
        public Action<AsyncCompletedEventArgs> ShutdownCompleted { get; set; }

        /// <summary>
        /// Initiates a shutdown operation on the socket. The operation will be completed via <see cref="ShutdownCompleted"/>.
        /// </summary>
        // Always runs in User thread
        public void ShutdownAsync()
        {
            Socket.BeginDisconnect(false, Sync.SynchronizeAsyncCallback((asyncResult) =>
                {
                    Sync.InvokeAndCallback(() => Socket.EndDisconnect(asyncResult),
                        ShutdownCompleted, null);
                }), null);
        }

        #endregion
    }

    /// <summary>
    /// Provides a wrapper around a <see cref="Socket"/>, translating the <see cref="IAsyncResult"/>-based notifications to event-based notifications,
    /// including thread synchronization. This is used for client sockets.
    /// </summary>
    internal sealed class ClientTcpSocketImpl : TcpSocketImpl
    {
        /// <summary>
        /// Initializes a new client socket wrapper.
        /// </summary>
        // Always runs in User thread
        public ClientTcpSocketImpl()
            : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
        }

        #region Socket methods

        /// <summary>
        /// Binds to a local endpoint. This method is not normally used.
        /// </summary>
        /// <param name="bindTo">The local endpoint.</param>
        public void Bind(IPEndPoint bindTo)
        {
            Socket.Bind(bindTo);
        }

        #endregion

        #region Connect operation

        /// <summary>
        /// Delegate to invoke when the connect operation completes.
        /// </summary>
        public Action<AsyncCompletedEventArgs> ConnectCompleted { get; set; }

        /// <summary>
        /// Initiates a connect operation on the socket. The operation will be completed via <see cref="ConnectCompleted"/>.
        /// </summary>
        // Always runs in User thread
        public void ConnectAsync(IPEndPoint server)
        {
            Socket.BeginConnect(server, Sync.SynchronizeAsyncCallback((asyncResult) =>
                {
                    Sync.InvokeAndCallback(() => Socket.EndConnect(asyncResult),
                        ConnectCompleted, null);
                }), null);
        }

        #endregion
    }

    /// <summary>
    /// Represents a client socket built on the asynchronous event-based model (see <see cref="IAsyncTcpConnection"/>).
    /// </summary>
    /// <remarks>
    /// <para>Client sockets must be connected before they can be used for any other operations.</para>
    /// </remarks>
    public sealed class ClientTcpSocket : IAsyncTcpConnection
    {
        /// <summary>
        /// The actual socket connection, which may be disconnected (null).
        /// </summary>
        private ClientTcpSocketImpl Socket_;

        /// <summary>
        /// The socket connection, created on demand.
        /// </summary>
        private ClientTcpSocketImpl Socket
        {
            get
            {
                if (Socket_ != null)
                    return Socket_;

                // Create a new socket connection and subscribe to its events
                Socket_ = new ClientTcpSocketImpl();
                Socket_.ConnectCompleted = (e) => { if (ConnectCompleted != null) ConnectCompleted(e); };
                Socket_.ReadCompleted = (e) => { if (ReadCompleted != null) ReadCompleted(e); };
                Socket_.WriteCompleted = (e) => { if (WriteCompleted != null) WriteCompleted(e); };
                Socket_.ShutdownCompleted = (e) => { if (ShutdownCompleted != null) ShutdownCompleted(e); };

                return Socket_;
            }
        }

        /// <summary>
        /// Throws an exception if the socket has not yet been created.
        /// </summary>
        private void EnsureOpen()
        {
            if (Socket_ == null)
                throw new InvalidOperationException("Socket is not open.");
        }

        /// <summary>
        /// Disconnects all events for the underlying socket, except the shutdown event.
        /// </summary>
        private void DisconnectSocketEventsExceptShutdown()
        {
            Socket_.ConnectCompleted = null;
            Socket_.ReadCompleted = null;
            Socket_.WriteCompleted = null;
        }

        /// <summary>
        /// Disconnects all events for the underlying socket.
        /// </summary>
        private void DisconnectSocketEvents()
        {
            DisconnectSocketEventsExceptShutdown();
            Socket_.ShutdownCompleted = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTcpSocket"/> class.
        /// </summary>
        public ClientTcpSocket()
        {
            SynchronizationContextRegister.Verify(SynchronizationContextProperties.Standard);
        }

        /// <summary>
        /// Gracefully or abortively closes the socket connection. See <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #region Socket methods

        /// <summary>
        /// Binds to a local endpoint. This method is not normally used.
        /// </summary>
        /// <remarks>
        /// <para>This method may not be called after <see cref="O:Nito.Async.Sockets.ClientTcpSocket.ConnectAsync"/>.</para>
        /// </remarks>
        /// <param name="bindTo">The local endpoint.</param>
        public void Bind(IPEndPoint bindTo)
        {
            Socket.Bind(bindTo);
        }

        #endregion

        #region Connection properties

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                EnsureOpen();
                return Socket.LocalEndPoint;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                EnsureOpen();
                return Socket.RemoteEndPoint;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public bool NoDelay
        {
            get
            {
                EnsureOpen();
                return Socket.NoDelay;
            }
            set
            {
                EnsureOpen();
                Socket.NoDelay = value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// <inheritdoc />
        /// Only valid once the socket is connected.
        /// </summary>
        public LingerOption LingerState
        {
            get
            {
                EnsureOpen();
                return Socket.LingerState;
            }
            set
            {
                EnsureOpen();
                Socket.LingerState = value;
            }
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
            ConnectAsync(new IPEndPoint(address, port));
        }

        /// <summary>
        /// Indicates the completion of a connect operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Connect operations are never cancelled.</para>
        /// <para>Connect operations will not complete if the socket is closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// <para>Generally, a handler of this event will call <see cref="ReadAsync"/> to start a read operation immediately.</para>
        /// <para>If a connect operation completes with error, the socket should be closed (<see cref="Close"/>) or abortively closed (<see cref="AbortiveClose"/>).</para>
        /// </remarks>
        public event Action<AsyncCompletedEventArgs> ConnectCompleted;

        #endregion

        #region Read operation

        /// <inheritdoc />
        public void ReadAsync(byte[] buffer, int offset, int size)
        {
            Socket.ReadAsync(buffer, offset, size);
        }

        /// <inheritdoc />
        public event Action<AsyncResultEventArgs<int>> ReadCompleted;

        #endregion

        #region Write operation

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer)
        {
            WriteAsync(buffer, 0, buffer.Length, null);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer, object state)
        {
            WriteAsync(buffer, 0, buffer.Length, state);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer, int offset, int size)
        {
            WriteAsync(buffer, offset, size, null);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer, int offset, int size, object state)
        {
            Socket.WriteAsync(buffer, offset, size, state);
        }

        /// <inheritdoc />
        public event Action<AsyncCompletedEventArgs> WriteCompleted;

        #endregion

        #region Shutdown operation

        /// <inheritdoc />
        public void ShutdownAsync()
        {
            EnsureOpen();

            // Disconnect all events except the shutdown event
            DisconnectSocketEventsExceptShutdown();

            // Initiate the shutdown
            Socket.ShutdownAsync();
        }

        /// <inheritdoc />
        public event Action<AsyncCompletedEventArgs> ShutdownCompleted;

        /// <inheritdoc />
        /// <remarks>
        /// <inheritdoc />
        /// <para>This method is a noop if the socket was never connected.</para>
        /// </remarks>
        public void Close()
        {
            // Do nothing if the underlying socket isn't there.
            if (Socket_ == null)
                return;

            // Disconnect all socket events
            DisconnectSocketEvents();

            // By default, closing the socket handle will cause the socket to linger
            Socket_.Dispose();

            // GC won't abort the graceful closure, because the WinSock API will keep it alive for a time
            Socket_ = null;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <inheritdoc />
        /// <para>This method is a noop if the socket was never connected.</para>
        /// </remarks>
        public void AbortiveClose()
        {
            // Do nothing if the underlying socket isn't there.
            if (Socket_ == null)
                return;

            // Disconnect all socket events
            DisconnectSocketEvents();

            // Set up the socket for an abortive close.
            Socket_.SetAbortive();

            // Close the socket connection
            Socket_.Dispose();
            Socket_ = null;
        }

        #endregion
    }

    /// <summary>
    /// Represents a child connection of a listening server socket, built on the asynchronous event-based model (see <see cref="IAsyncTcpConnection"/>).
    /// </summary>
    public sealed class ServerChildTcpSocket : IAsyncTcpConnection
    {
        /// <summary>
        /// The actual socket connection.
        /// </summary>
        private readonly TcpSocketImpl Socket;

        /// <summary>
        /// Creates a <see cref="ServerChildTcpSocket"/> from a <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The new socket connection to use.</param>
        internal ServerChildTcpSocket(Socket socket)
        {
            Socket = new TcpSocketImpl(socket);
            Socket.ReadCompleted = (e) => { if (ReadCompleted != null) ReadCompleted(e); };
            Socket.WriteCompleted = (e) => { if (WriteCompleted != null) WriteCompleted(e); };
            Socket.ShutdownCompleted = (e) => { if (ShutdownCompleted != null) ShutdownCompleted(e); };
        }

        /// <summary>
        /// Disconnects all events for the underlying socket, except the shutdown event.
        /// </summary>
        private void DisconnectSocketEventsExceptShutdown()
        {
            Socket.ReadCompleted = null;
            Socket.WriteCompleted = null;
        }

        /// <summary>
        /// Disconnects all events for the underlying socket.
        /// </summary>
        private void DisconnectSocketEvents()
        {
            DisconnectSocketEventsExceptShutdown();
            Socket.ShutdownCompleted = null;
        }

        /// <summary>
        /// Gracefully or abortively closes the socket connection. See <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #region Connection properties

        /// <inheritdoc />
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return Socket.LocalEndPoint;
            }
        }

        /// <inheritdoc />
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return Socket.RemoteEndPoint;
            }
        }

        /// <inheritdoc />
        public bool NoDelay
        {
            get
            {
                return Socket.NoDelay;
            }
            set
            {
                Socket.NoDelay = value;
            }
        }

        /// <inheritdoc />
        public LingerOption LingerState
        {
            get
            {
                return Socket.LingerState;
            }
            set
            {
                Socket.LingerState = value;
            }
        }

        #endregion

        #region Read operation

        /// <inheritdoc />
        public void ReadAsync(byte[] buffer, int offset, int size)
        {
            Socket.ReadAsync(buffer, offset, size);
        }

        /// <inheritdoc />
        public event Action<AsyncResultEventArgs<int>> ReadCompleted;

        #endregion

        #region Write operation

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer)
        {
            Socket.WriteAsync(buffer, 0, buffer.Length, null);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer, object state)
        {
            Socket.WriteAsync(buffer, 0, buffer.Length, state);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer, int offset, int size)
        {
            Socket.WriteAsync(buffer, offset, size, null);
        }

        /// <inheritdoc />
        public void WriteAsync(byte[] buffer, int offset, int size, object state)
        {
            Socket.WriteAsync(buffer, offset, size, state);
        }

        /// <inheritdoc />
        public event Action<AsyncCompletedEventArgs> WriteCompleted;

        #endregion

        #region Shutdown operation

        /// <inheritdoc />
        public void ShutdownAsync()
        {
            DisconnectSocketEventsExceptShutdown();
            Socket.ShutdownAsync();
        }

        /// <inheritdoc />
        public event Action<AsyncCompletedEventArgs> ShutdownCompleted;

        /// <inheritdoc />
        public void Close()
        {
            // Disconnect all socket events
            DisconnectSocketEvents();

            // By default, closing the socket handle will cause the socket to linger
            Socket.Dispose();
        }

        /// <inheritdoc />
        public void AbortiveClose()
        {
            // Disconnect all socket events
            DisconnectSocketEvents();

            // Set up the socket for an abortive close.
            Socket.SetAbortive();

            // Close the socket connection
            Socket.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Provides a wrapper around a <see cref="Socket"/>, translating the <see cref="IAsyncResult"/>-based notifications to event-based notifications,
    /// including thread synchronization. This is used for server (listening) sockets.
    /// </summary>
    internal sealed class ServerTcpSocketImpl : IDisposable
    {
        /// <summary>
        /// The socket for this connection.
        /// </summary>
        private Socket Socket_;

        /// <summary>
        /// Initializes a new server socket wrapper.
        /// </summary>
        // Always runs in User thread
        public ServerTcpSocketImpl()
        {
            Socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Binds the socket to a local endpoint and begins listening. Note: does not start accepting.
        /// </summary>
        /// <param name="bindTo">The local endpoint to bind to.</param>
        /// <param name="backlog">The backlog for listening.</param>
        // Always runs in User thread
        public void Bind(IPEndPoint bindTo, int backlog)
        {
            Socket_.Bind(bindTo);
            Socket_.Listen(backlog);
        }

        /// <summary>
        /// Returns the IP address and port on this side of the connection.
        /// </summary>
        public IPEndPoint LocalEndPoint { get { return (IPEndPoint)Socket_.LocalEndPoint; } }

        /// <summary>
        /// Closes the underlying socket and frees the socket resources. Be sure to clear all events before calling this method!
        /// </summary>
        // Always runs in User thread
        public void Dispose()
        {
            // Do the actual close
            Socket_.Close();
        }

        #region Accept operation

        /// <summary>
        /// Delegate to invoke when the accept operation completes.
        /// </summary>
        public Action<AsyncResultEventArgs<ServerChildTcpSocket>> AcceptCompleted { get; set; }

        /// <summary>
        /// Initiates an accept operation on the socket. The operation will be completed via <see cref="AcceptCompleted"/>.
        /// </summary>
        // Always runs in User thread
        public void AcceptAsync()
        {
            Socket_.BeginAccept(Sync.SynchronizeAsyncCallback((asyncResult) =>
                {
                    Sync.InvokeAndCallback(() => new ServerChildTcpSocket(Socket_.EndAccept(asyncResult)),
                        AcceptCompleted, null);
                }), null);
        }

        #endregion
    }

    /// <summary>
    /// Represents a listening server socket built on the asynchronous event-based model.
    /// </summary>
    /// <remarks>
    /// <para>No operations are ever cancelled. When the socket is closed, active operations do not complete.</para>
    /// <para>Only one accept operation may be active on a listening socket at any time.</para>
    /// <para>All operations must be initiated from a thread with a non-free-threaded synchronization context. This means that, e.g., GUI threads may call these methods, but free threads may not.</para>
    /// </remarks>
    public sealed class ServerTcpSocket : IDisposable
    {
        /// <summary>
        /// The traditional maximum value was 5, according to Stevens' TCP/IP vol 1; the current default is several hundred, but this is rarely
        /// necessary, so we use a value of 2.
        /// </summary>
        private const int DefaultBacklog = 2;

        /// <summary>
        /// The actual listening socket, which may be disconnected (null).
        /// </summary>
        private ServerTcpSocketImpl Socket_;

        /// <summary>
        /// The listening socket, created on demand.
        /// </summary>
        private ServerTcpSocketImpl Socket
        {
            get
            {
                if (Socket_ != null)
                    return Socket_;

                // Create a new socket connection and subscribe to its events
                Socket_ = new ServerTcpSocketImpl();
                Socket_.AcceptCompleted = (e) => { if (AcceptCompleted != null) AcceptCompleted(e); };

                return Socket_;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTcpSocket"/> class.
        /// </summary>
        public ServerTcpSocket()
        {
            SynchronizationContextRegister.Verify(SynchronizationContextProperties.Standard);
        }

        /// <summary>
        /// Closes the listening socket. See <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Closes the listening socket immediately and frees all resources.
        /// </summary>
        /// <remarks>
        /// <para>No events will be raised once this method is called.</para>
        /// </remarks>
        public void Close()
        {
            // Do nothing if the underlying socket isn't there.
            if (Socket_ == null)
                return;

            // Disconnect all socket events
            Socket_.AcceptCompleted = null;

            // Close the socket
            Socket_.Dispose();
            Socket_ = null;
        }

        #region Socket methods

        /// <overloads>
        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// </overloads>
        /// <summary>
        /// Binds to a local endpoint and begins listening.
        /// </summary>
        /// <remarks>
        /// <para>Note that this does not begin accepting.</para>
        /// </remarks>
        /// <param name="bindTo">The local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public void Bind(IPEndPoint bindTo, int backlog)
        {
            Socket.Bind(bindTo, backlog);
        }

        /// <inheritdoc cref="Bind(IPEndPoint, int)" />
        /// <param name="address">The address of the local endpoint.</param>
        /// <param name="port">The port of the local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public void Bind(IPAddress address, int port, int backlog)
        {
            Bind(new IPEndPoint(address, port), backlog);
        }

        /// <inheritdoc cref="Bind(IPEndPoint, int)" />
        /// <param name="bindTo">The local endpoint.</param>
        public void Bind(IPEndPoint bindTo)
        {
            Bind(bindTo, DefaultBacklog);
        }

        /// <inheritdoc cref="Bind(IPEndPoint, int)" />
        /// <param name="address">The address of the local endpoint.</param>
        /// <param name="port">The port of the local endpoint.</param>
        public void Bind(IPAddress address, int port)
        {
            Bind(new IPEndPoint(address, port), DefaultBacklog);
        }

        /// <inheritdoc cref="Bind(IPEndPoint, int)" />
        /// <param name="port">The port of the local endpoint.</param>
        /// <param name="backlog">The number of backlog connections for listening.</param>
        public void Bind(int port, int backlog)
        {
            Bind(new IPEndPoint(IPAddress.Any, port), backlog);
        }

        /// <inheritdoc cref="Bind(IPEndPoint, int)" />
        /// <param name="port">The port of the local endpoint.</param>
        public void Bind(int port)
        {
            Bind(new IPEndPoint(IPAddress.Any, port), DefaultBacklog);
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

        #region Accept operation

        /// <summary>
        /// Initiates an accept operation.
        /// </summary>
        /// <remarks>
        /// <para>There may be only one accept operation at a time for a listening socket.</para>
        /// <para>The accept operation will complete by invoking <see cref="AcceptCompleted"/> unless the socket is closed (<see cref="Close"/>).</para>
        /// <para>Accept operations are never cancelled.</para>
        /// </remarks>
        public void AcceptAsync()
        {
            Socket.AcceptAsync();
        }

        /// <summary>
        /// Indicates the completion of an accept operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// <para>Accept operations are never cancelled.</para>
        /// <para>Accept operations will not complete if the socket is closed (<see cref="Close"/>).</para>
        /// <para>The result of the accept operation is a new socket connection.</para>
        /// <para>Generally, a handler of this event will call <see cref="AcceptAsync"/> to continue accepting other connections.</para>
        /// <para>If an accept operation completes with error, no action is necessary other than continuing to accept other connections.</para>
        /// </remarks>
        public event Action<AsyncResultEventArgs<ServerChildTcpSocket>> AcceptCompleted;

        #endregion
    }
}
