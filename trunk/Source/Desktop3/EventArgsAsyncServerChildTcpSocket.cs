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

    public sealed class EventArgsAsyncServerChildTcpSocket : IAsyncTcpConnection
    {
        /// <summary>
        /// The delegate scheduler used to synchronize callbacks.
        /// </summary>
        private readonly IAsyncDelegateScheduler scheduler;

        private readonly Socket socket;

        /// <summary>
        /// The state machine for the socket.
        /// </summary>
        private readonly SocketStateMachine state;

        internal EventArgsAsyncServerChildTcpSocket(IAsyncDelegateScheduler scheduler, Socket socket)
        {
            this.scheduler = scheduler;
            this.socket = socket;
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
            this.ReadCompleted = null;
            this.WriteCompleted = null;
            this.ShutdownCompleted = null;
            this.state.Close();
            this.socket.Close();
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

        public event Action<AsyncResultEventArgs<int>> ReadCompleted;

        public event Action<AsyncCompletedEventArgs> WriteCompleted;

        public event Action<AsyncCompletedEventArgs> ShutdownCompleted;
    }
}
