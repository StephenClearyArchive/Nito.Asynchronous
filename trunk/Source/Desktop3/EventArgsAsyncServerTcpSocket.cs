using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.Communication
{
    using System.Net;
    using System.Net.Sockets;

    using Async;

    public sealed class EventArgsAsyncServerTcpSocket : IAsyncServerTcpSocket
    {
        private readonly IAsyncDelegateScheduler scheduler;
        private readonly Socket socket;

        public EventArgsAsyncServerTcpSocket(IAsyncDelegateScheduler scheduler)
        {
            this.scheduler = scheduler;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public EndPoint LocalEndPoint
        {
            get { return this.socket.LocalEndPoint; }
        }

        public void Bind(EndPoint bindTo, int backlog)
        {
            this.socket.Bind(bindTo);
            this.socket.Listen(backlog);
        }

        public void Dispose()
        {
            this.AcceptCompleted = null;
            this.socket.Close();
        }

        public void AcceptAsync()
        {
            var args = new SocketAsyncEventArgs();
            args.Completed += (_, __) =>
            {
                var error = args.GetError();
                if (error == null)
                {
                    this.scheduler.Schedule(() => this.OnAcceptComplete(result: new EventArgsAsyncServerChildTcpSocket(this.scheduler, args.AcceptSocket)));
                }
                else
                {
                    this.scheduler.Schedule(() => this.OnAcceptComplete(ex: error));
                }
            };
        }

        private void OnAcceptComplete(Exception ex = null, IAsyncTcpConnection result = null)
        {
            if (this.AcceptCompleted != null)
            {
                this.AcceptCompleted(ex == null ? new AsyncResultEventArgs<IAsyncTcpConnection>(result) : new AsyncResultEventArgs<IAsyncTcpConnection>(ex));
            }
        }

        public event Action<AsyncResultEventArgs<IAsyncTcpConnection>> AcceptCompleted;
    }
}
