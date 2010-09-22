using System;

namespace Nito.Communication
{
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;

    using Async;

    public sealed class BeginEndAsyncServerChildTcpSocket : IAsyncTcpConnection
    {
        private readonly IAsyncDelegateScheduler scheduler;
        private readonly Socket socket;

        internal BeginEndAsyncServerChildTcpSocket(IAsyncDelegateScheduler scheduler, Socket socket)
        {
            this.scheduler = scheduler;
            this.socket = socket;
        }

        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)this.socket.LocalEndPoint; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint)this.socket.RemoteEndPoint; }
        }

        public bool NoDelay
        {
            get
            {
                return this.socket.NoDelay;
            }

            set
            {
                this.socket.NoDelay = value;
            }
        }

        public LingerOption LingerState
        {
            get
            {
                return this.socket.LingerState;
            }

            set
            {
                this.socket.LingerState = value;
            }
        }

        public void ReadAsync(byte[] buffer, int offset, int size)
        {
            this.socket.BeginReceive(buffer, offset, size, SocketFlags.None, asyncResult =>
            {
                try
                {
                    var result = this.socket.EndReceive(asyncResult);
                    this.scheduler.Schedule(() =>
                    {
                        if (this.ReadCompleted != null)
                        {
                            this.ReadCompleted(new AsyncResultEventArgs<int>(result));
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.scheduler.Schedule(() =>
                    {
                        if (this.ReadCompleted != null)
                        {
                            this.ReadCompleted(new AsyncResultEventArgs<int>(ex));
                        }
                    });
                }
            }, null);
        }

        public void WriteAsync(byte[] buffer, int offset, int size, object state)
        {
            this.socket.BeginSend(buffer, offset, size, SocketFlags.None, asyncResult =>
            {
                try
                {
                    var result = this.socket.EndSend(asyncResult);
                    if (result < size)
                    {
                        this.scheduler.Schedule(() =>
                        {
                            try
                            {
                                this.WriteAsync(buffer, offset + result, size - result, state);
                            }
                            catch (Exception ex)
                            {
                                if (this.WriteCompleted != null)
                                {
                                    this.WriteCompleted(new AsyncCompletedEventArgs(ex, false, state));
                                }
                            }
                        });
                    }
                    else
                    {
                        this.scheduler.Schedule(() =>
                        {
                            if (this.WriteCompleted != null)
                            {
                                this.WriteCompleted(new AsyncCompletedEventArgs(null, false, state));
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    this.scheduler.Schedule(() =>
                    {
                        if (this.WriteCompleted != null)
                        {
                            this.WriteCompleted(new AsyncCompletedEventArgs(ex, false, state));
                        }
                    });
                }
            }, state);
        }

        public void ShutdownAsync()
        {
            this.ReadCompleted = null;
            this.WriteCompleted = null;
            this.socket.BeginDisconnect(false, asyncResult =>
            {
                try
                {
                    this.socket.EndDisconnect(asyncResult);
                    this.scheduler.Schedule(() =>
                    {
                        if (this.ShutdownCompleted != null)
                        {
                            this.ShutdownCompleted(new AsyncCompletedEventArgs(null, false, null));
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.scheduler.Schedule(() =>
                    {
                        if (this.ShutdownCompleted != null)
                        {
                            this.ShutdownCompleted(new AsyncCompletedEventArgs(ex, false, null));
                        }
                    });
                }
            }, null);
        }

        public void Dispose()
        {
            this.ReadCompleted = null;
            this.WriteCompleted = null;
            this.ShutdownCompleted = null;
            this.socket.Close();
        }

        public event Action<AsyncResultEventArgs<int>> ReadCompleted;

        public event Action<AsyncCompletedEventArgs> WriteCompleted;

        public event Action<AsyncCompletedEventArgs> ShutdownCompleted;
    }
}
