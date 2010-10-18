using System;

namespace Nito.Communication
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Async;

    public abstract class AsyncTcpConnectionBase : IAsyncTcpConnection
    {
        /// <summary>
        /// The delegate scheduler used to synchronize callbacks.
        /// </summary>
        protected readonly IAsyncDelegateScheduler scheduler;

        /// <summary>
        /// The underlying socket.
        /// </summary>
        protected readonly Socket socket;

        /// <summary>
        /// The state machine for the socket.
        /// </summary>
        protected readonly SocketStateMachine state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginEndAsyncClientTcpSocket"/> class with the specified delegate scheduler.
        /// </summary>
        /// <param name="scheduler">The delegate scheduler used to synchronize callbacks.</param>
        protected AsyncTcpConnectionBase(IAsyncDelegateScheduler scheduler, Socket socket)
        {
            this.scheduler = scheduler;
            this.socket = socket;
            this.state = new SocketStateMachine();
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
            get { return this.socket.NoDelay; }
            set { this.socket.NoDelay = value; }
        }

        public LingerOption LingerState
        {
            get { return this.socket.LingerState; }
            set { this.socket.LingerState = value; }
        }

        public abstract void ReadAsync(byte[] buffer, int offset, int size);

        public abstract void WriteAsync(byte[] buffer, int offset, int size, object state);

        public abstract void WriteAsync(IList<ArraySegment<byte>> buffers, object state);

        public abstract void ShutdownAsync();

        public virtual void Dispose()
        {
            this.ReadCompleted = null;
            this.WriteCompleted = null;
            this.ShutdownCompleted = null;
            this.state.Close();
            this.socket.Close();
        }

        protected void OnReadComplete(Exception ex = null, int result = 0)
        {
            this.state.ReadComplete();
            if (this.ReadCompleted != null)
            {
                this.ReadCompleted(ex == null ? new AsyncResultEventArgs<int>(result) : new AsyncResultEventArgs<int>(ex));
            }
        }

        protected void OnWriteComplete(object state, Exception ex = null)
        {
            this.ContinueWriting();
            if (this.WriteCompleted != null)
            {
                this.WriteCompleted(new AsyncCompletedEventArgs(ex, false, state));
            }
        }

        protected void OnShutdownComplete(Exception ex = null)
        {
            if (this.ShutdownCompleted != null)
            {
                this.ShutdownCompleted(new AsyncCompletedEventArgs(ex, false, null));
            }
        }

        protected void PrepareForShutdown()
        {
            this.state.Close();
            this.ReadCompleted = null;
            this.WriteCompleted = null;
        }

        protected IList<ArraySegment<byte>> RemainingWrite(IList<ArraySegment<byte>> buffers, int bytesWritten)
        {
            var ret = new List<ArraySegment<byte>>();
            int index;
            for (index = 0; index != buffers.Count && bytesWritten != 0; ++index)
            {
                var buffer = buffers[index];
                if (bytesWritten >= buffer.Count)
                {
                    bytesWritten -= buffer.Count;
                }
                else
                {
                    ret.Add(new ArraySegment<byte>(buffer.Array, buffer.Offset + bytesWritten, buffer.Count - bytesWritten));
                    bytesWritten = 0;
                }
            }

            ret.AddRange(buffers.Skip(index));
            return ret;
        }

        protected abstract void ContinueWriting();

        public event Action<AsyncResultEventArgs<int>> ReadCompleted;

        public event Action<AsyncCompletedEventArgs> WriteCompleted;

        public event Action<AsyncCompletedEventArgs> ShutdownCompleted;
    }
}
