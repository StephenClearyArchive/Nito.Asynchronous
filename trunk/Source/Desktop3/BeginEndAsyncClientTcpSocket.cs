namespace Nito.Communication
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;

    using Async;

    /// <summary>
    /// An asynchronous client TCP/IP socket that uses the <c>Begin*/End*</c> socket functions to work asynchronously.
    /// </summary>
    public sealed class BeginEndAsyncClientTcpSocket : IAsyncClientTcpSocket
    {
        /// <summary>
        /// The delegate scheduler used to synchronize callbacks.
        /// </summary>
        private readonly IAsyncDelegateScheduler scheduler;

        /// <summary>
        /// The underlying socket.
        /// </summary>
        private readonly Socket socket;

        /// <summary>
        /// The state machine for the socket.
        /// </summary>
        private readonly SocketStateMachine state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginEndAsyncClientTcpSocket"/> class with the specified delegate scheduler.
        /// </summary>
        /// <param name="scheduler">The delegate scheduler used to synchronize callbacks.</param>
        public BeginEndAsyncClientTcpSocket(IAsyncDelegateScheduler scheduler)
        {
            this.scheduler = scheduler;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.state = new SocketStateMachine();
        }

        public EndPoint LocalEndPoint
        {
            get { return this.socket.LocalEndPoint; }
        }

        public EndPoint RemoteEndPoint
        {
            get { return this.socket.RemoteEndPoint; }
        }

        public bool NoDelay
        {
            get { return this.socket.NoDelay; }
            set { this.socket.NoDelay = value; }
        }

        public LingerOption LingerState
        {
            get { return this.socket.LingerState; }
            set { this.socket.LingerState = value; }
        }

        /// <summary>
        /// Binds to a local endpoint before connecting. This method is not normally used.
        /// </summary>
        /// <param name="bindTo">The local endpoint.</param>
        /// <remarks>
        /// This method may not be called after <see cref="O:Nito.Communication.BeginEndAsyncClientTcpSocket.ConnectAsync"/>.
        /// </remarks>
        public void Bind(EndPoint bindTo)
        {
            this.socket.Bind(bindTo);
        }

        /// <summary>
        /// Initiates a connect operation.
        /// </summary>
        /// <param name="server">The address and port of the server to connect to.</param>
        /// <remarks>
        /// 	<para>There may be only one connect operation for a client socket, and it must be the first operation performed.</para>
        /// 	<para>The connect operation will complete by invoking <see cref="ConnectCompleted"/>, unless the socket is closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// 	<para>Connect operations are never cancelled.</para>
        /// </remarks>
        public void ConnectAsync(EndPoint server)
        {
            this.socket.BeginConnect(
                server,
                asyncResult =>
                {
                    try
                    {
                        this.socket.EndConnect(asyncResult);
                        this.scheduler.Schedule(() => this.OnConnectComplete());
                    }
                    catch (Exception ex)
                    {
                        this.scheduler.Schedule(() => this.OnConnectComplete(ex));
                    }
                },
                null);
        }

        /// <summary>
        /// Initiates a read operation.
        /// </summary>
        /// <param name="buffer">The buffer to receive the data.</param>
        /// <param name="offset">The offset into <paramref name="buffer"/> to write the received data.</param>
        /// <param name="size">The maximum number of bytes that may be written into <paramref name="buffer"/> at <paramref name="offset"/>.</param>
        /// <remarks>
        /// 	<para>There may be only one active read operation at any time.</para>
        /// 	<para>The read operation will complete by invoking <see cref="ReadCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// 	<para>Read operations are never cancelled.</para>
        /// </remarks>
        public void ReadAsync(byte[] buffer, int offset, int size)
        {
            this.state.Read();
            this.socket.BeginReceive(
                buffer,
                offset,
                size,
                SocketFlags.None,
                asyncResult =>
                {
                    try
                    {
                        var result = this.socket.EndReceive(asyncResult);
                        this.scheduler.Schedule(() => this.OnReadComplete(result: result));
                    }
                    catch (Exception ex)
                    {
                        this.scheduler.Schedule(() => this.OnReadComplete(ex: ex));
                    }
                },
                null);
        }

        /// <summary>
        /// Initiates a read operation.
        /// </summary>
        /// <param name="buffers">The buffers to receive the data.</param>
        /// <remarks>
        /// 	<para>There may be only one active read operation at any time.</para>
        /// 	<para>The read operation will complete by invoking <see cref="ReadCompleted"/>, unless the socket is shut down (<see cref="ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// 	<para>Read operations are never cancelled.</para>
        /// </remarks>
        public void ReadAsync(IList<ArraySegment<byte>> buffers)
        {
            this.state.Read();
            this.socket.BeginReceive(
                buffers,
                SocketFlags.None,
                asyncResult =>
                {
                    try
                    {
                        var result = this.socket.EndReceive(asyncResult);
                        this.scheduler.Schedule(() => this.OnReadComplete(result: result));
                    }
                    catch (Exception ex)
                    {
                        this.scheduler.Schedule(() => this.OnReadComplete(ex: ex));
                    }
                },
                null);
        }

        /// <summary>
        /// Initiates a write operation.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write to the socket.</param>
        /// <param name="offset">The offset of the data within <paramref name="buffer"/>.</param>
        /// <param name="size">The number of bytes of data, at <paramref name="offset"/> within <paramref name="buffer"/>.</param>
        /// <param name="state">The context, which is passed to <see cref="WriteCompleted"/> as <c>e.UserState</c>.</param>
        /// <remarks>
        /// 	<para>Multiple write operations may be active at the same time.</para>
        /// 	<para>The write operation will complete by invoking <see cref="IAsyncTcpConnection.WriteCompleted"/>, unless the socket is shut down (<see cref="IAsyncTcpConnection.ShutdownAsync"/>), closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>), or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// 	<para>Write operations are never cancelled.</para>
        /// 	<para>If <paramref name="state"/> is an instance of <see cref="CallbackOnErrorsOnly"/>, then <see cref="IAsyncTcpConnection.WriteCompleted"/> is only invoked in an error situation; it is not invoked if the write completes successfully.</para>
        /// </remarks>
        public void WriteAsync(byte[] buffer, int offset, int size, object state)
        {
            if (this.state.Write(new WriteRequest(buffer, offset, size, state)))
            {
                this.Write(buffer, offset, size, state);
            }
        }

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
        public void WriteAsync(IList<ArraySegment<byte>> buffers, object state = null)
        {
            if (this.state.Write(new WriteRequest(buffers, state)))
            {
                this.Write(buffers, state);
            }
        }

        /// <summary>
        /// Initiates a shutdown operation. Once a shutdown operation is initiated, only the shutdown operation will complete.
        /// </summary>
        /// <remarks>
        /// 	<para>The shutdown operation will complete by invoking <see cref="ShutdownCompleted"/>.</para>
        /// 	<para>Shutdown operations are never cancelled.</para>
        /// </remarks>
        public void ShutdownAsync()
        {
            this.state.Close();
            this.ConnectCompleted = null;
            this.ReadCompleted = null;
            this.WriteCompleted = null;
            this.socket.BeginDisconnect(false, asyncResult =>
            {
                try
                {
                    this.socket.EndDisconnect(asyncResult);
                    this.scheduler.Schedule(() => this.OnShutdownComplete());
                }
                catch (Exception ex)
                {
                    this.scheduler.Schedule(() => this.OnShutdownComplete(ex));
                }
            }, null);
        }

        /// <summary>
        /// Closes the listening socket immediately and frees all resources.
        /// </summary>
        /// <remarks>
        /// <para>No events will be raised once this method is called.</para>
        /// </remarks>
        public void Dispose()
        {
            this.ConnectCompleted = null;
            this.ReadCompleted = null;
            this.WriteCompleted = null;
            this.ShutdownCompleted = null;
            this.state.Close();
            this.socket.Close();
        }

        private void OnConnectComplete(Exception ex = null)
        {
            if (this.ConnectCompleted != null)
            {
                this.ConnectCompleted(new AsyncCompletedEventArgs(ex, false, null));
            }
        }

        private void OnReadComplete(Exception ex = null, int result = 0)
        {
            this.state.ReadComplete();
            if (this.ReadCompleted != null)
            {
                this.ReadCompleted(ex == null ? new AsyncResultEventArgs<int>(result) : new AsyncResultEventArgs<int>(ex));
            }
        }

        private void OnWriteComplete(object state, Exception ex = null)
        {
            this.ContinueWriting();
            if (this.WriteCompleted != null)
            {
                this.WriteCompleted(new AsyncCompletedEventArgs(ex, false, state));
            }
        }

        private void OnShutdownComplete(Exception ex = null)
        {
            if (this.ShutdownCompleted != null)
            {
                this.ShutdownCompleted(new AsyncCompletedEventArgs(ex, false, null));
            }
        }

        private void Write(byte[] buffer, int offset, int size, object state)
        {
            this.socket.BeginSend(
                buffer,
                offset,
                size,
                SocketFlags.None,
                asyncResult =>
                {
                    try
                    {
                        var result = this.socket.EndSend(asyncResult);
                        if (result < size)
                        {
                            this.scheduler.Schedule(
                                () =>
                                {
                                    try
                                    {
                                        this.Write(buffer, offset + result, size - result, state);
                                    }
                                    catch (Exception ex)
                                    {
                                        this.OnWriteComplete(state, ex);
                                    }
                                });
                        }
                        else
                        {
                            this.scheduler.Schedule(() => this.OnWriteComplete(state));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.scheduler.Schedule(() => this.OnWriteComplete(state, ex));
                    }
                },
                state);
        }

        private void Write(IList<ArraySegment<byte>> buffers, object state)
        {
            this.socket.BeginSend(
                buffers,
                SocketFlags.None,
                asyncResult =>
                {
                    try
                    {
                        var result = this.socket.EndSend(asyncResult);
                        var remainingBuffers = SocketHelpers.RemainingBuffers(buffers, result);
                        if (remainingBuffers.Count != 0)
                        {
                            this.scheduler.Schedule(
                                () =>
                                {
                                    try
                                    {
                                        this.Write(remainingBuffers, state);
                                    }
                                    catch (Exception ex)
                                    {
                                        this.OnWriteComplete(state, ex);
                                    }
                                });
                        }
                        else
                        {
                            this.scheduler.Schedule(() => this.OnWriteComplete(state));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.scheduler.Schedule(() => this.OnWriteComplete(state, ex));
                    }
                },
                state);
        }

        private void ContinueWriting()
        {
            var nextWrite = this.state.WriteComplete();
            if (nextWrite == null)
            {
                return;
            }

            if (nextWrite.Buffers != null)
            {
                this.Write(nextWrite.Buffers, nextWrite.State);
            }
            else
            {
                this.Write(nextWrite.Buffer, nextWrite.Offset, nextWrite.Size, nextWrite.State);
            }
        }

        /// <summary>
        /// Indicates the completion of a connect operation, either successfully or with error.
        /// </summary>
        /// <remarks>
        /// 	<para>Connect operations are never cancelled.</para>
        /// 	<para>Connect operations will not complete if the socket is closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// 	<para>Generally, a handler of this event will call <see cref="IAsyncTcpConnection.ReadAsync"/> to start a read operation immediately.</para>
        /// 	<para>If a connect operation completes with error, the socket should be closed (<see cref="InterfaceExtensions.Close(IAsyncTcpConnection)"/>) or abortively closed (<see cref="InterfaceExtensions.AbortiveClose"/>).</para>
        /// </remarks>
        public event Action<AsyncCompletedEventArgs> ConnectCompleted;

        public event Action<AsyncResultEventArgs<int>> ReadCompleted;

        public event Action<AsyncCompletedEventArgs> WriteCompleted;

        public event Action<AsyncCompletedEventArgs> ShutdownCompleted;
    }
}
