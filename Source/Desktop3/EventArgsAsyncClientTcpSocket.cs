using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.Communication
{
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;

    using Async;

    public sealed class EventArgsAsyncClientTcpSocket : IAsyncClientTcpSocket
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
        /// Initializes a new instance of the <see cref="EventArgsAsyncClientTcpSocket"/> class with the specified delegate scheduler.
        /// </summary>
        /// <param name="scheduler">The delegate scheduler used to synchronize callbacks.</param>
        public EventArgsAsyncClientTcpSocket(IAsyncDelegateScheduler scheduler)
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

        public void ConnectAsync(EndPoint server)
        {
            var args = new SocketAsyncEventArgs { RemoteEndPoint = server };
            args.Completed += (_, __) =>
            {
                var error = args.GetError();
                if (error == null)
                {
                    this.scheduler.Schedule(() => this.OnConnectComplete());
                }
                else
                {
                    this.scheduler.Schedule(() => this.OnConnectComplete(error));
                }
            };
            this.socket.ConnectAsync(args);
        }

        public void ReadAsync(byte[] buffer, int offset, int size)
        {
            this.state.Read();
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, offset, size);
            args.Completed += (_, __) =>
            {
                var error = args.GetError();
                if (error == null)
                {
                    var result = args.BytesTransferred;
                    this.scheduler.Schedule(() => this.OnReadComplete(result: result));
                }
                else
                {
                    this.scheduler.Schedule(() => this.OnReadComplete(ex: error));
                }
            };
        }

        public void ReadAsync(IList<ArraySegment<byte>> buffers)
        {
            this.state.Read();
            var args = new SocketAsyncEventArgs { BufferList = buffers };
            args.Completed += (_, __) =>
            {
                var error = args.GetError();
                if (error == null)
                {
                    var result = args.BytesTransferred;
                    this.scheduler.Schedule(() => this.OnReadComplete(result: result));
                }
                else
                {
                    this.scheduler.Schedule(() => this.OnReadComplete(ex: error));
                }
            };
        }

        public void WriteAsync(byte[] buffer, int offset, int size, object state = null)
        {
            if (this.state.Write(new WriteRequest(buffer, offset, size, state)))
            {
                this.Write(buffer, offset, size, state);
            }
        }

        public void WriteAsync(IList<ArraySegment<byte>> buffers, object state = null)
        {
            if (this.state.Write(new WriteRequest(buffers, state)))
            {
                this.Write(buffers, state);
            }
        }

        public void ShutdownAsync()
        {
            this.state.Close();
            this.ConnectCompleted = null;
            this.ReadCompleted = null;
            this.WriteCompleted = null;
            var args = new SocketAsyncEventArgs();
            args.Completed += (_, __) =>
            {
                var error = args.GetError();
                if (error == null)
                {
                    this.scheduler.Schedule(() => this.OnShutdownComplete());
                }
                else
                {
                    this.scheduler.Schedule(() => this.OnShutdownComplete(error));
                }
            };
            this.socket.DisconnectAsync(args);
        }

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
            var args = new SocketAsyncEventArgs { UserToken = state };
            args.SetBuffer(buffer, offset, size);
            args.Completed += (_, __) =>
            {
                var error = args.GetError();
                if (error == null)
                {
                    var result = args.BytesTransferred;
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
                else
                {
                    this.scheduler.Schedule(() => this.OnWriteComplete(state, error));
                }
            };
            this.socket.SendAsync(args);
        }

        private void Write(IList<ArraySegment<byte>> buffers, object state)
        {
            var args = new SocketAsyncEventArgs { BufferList = buffers, UserToken = state };
            args.Completed += (_, __) =>
            {
                var error = args.GetError();
                if (error == null)
                {
                    var result = args.BytesTransferred;
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
                else
                {
                    this.scheduler.Schedule(() => this.OnWriteComplete(state, error));
                }
            };
            this.socket.SendAsync(args);
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

        public event Action<AsyncCompletedEventArgs> ConnectCompleted;

        public event Action<AsyncResultEventArgs<int>> ReadCompleted;

        public event Action<AsyncCompletedEventArgs> WriteCompleted;

        public event Action<AsyncCompletedEventArgs> ShutdownCompleted;
    }
}
