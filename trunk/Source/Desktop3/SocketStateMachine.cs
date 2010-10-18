using System;
using System.Collections.Generic;

namespace Nito.Communication
{
    /// <summary>
    /// A state machine for an asynchronous TCP/IP socket.
    /// </summary>
    internal sealed class SocketStateMachine
    {
        /// <summary>
        /// The write request queue.
        /// </summary>
        private readonly Queue<WriteRequest> writeQueue = new Queue<WriteRequest>();

        /// <summary>
        /// The current transfer state of the socket.
        /// </summary>
        private SocketTransferState state;

        /// <summary>
        /// Transitions the state machine to the <c>Reading</c> state. Throws <see cref="InvalidOperationException"/> if the state transition is invalid.
        /// </summary>
        public void Read()
        {
            if ((this.state & SocketTransferState.Reading) == SocketTransferState.Reading)
            {
                throw new InvalidOperationException("Cannot start reading from a socket that is already doing an asynchronous read.");
            }

            if (this.state == SocketTransferState.Closing)
            {
                throw new InvalidOperationException("Cannot start reading from a socket that is closing.");
            }

            this.state |= SocketTransferState.Reading;
        }

        /// <summary>
        /// Transitions the state machine out of the <c>Reading</c> state. Throws <see cref="InvalidOperationException"/> if the state transition is invalid.
        /// </summary>
        public void ReadComplete()
        {
            if (this.state == SocketTransferState.Idle)
            {
                throw new InvalidOperationException("Cannot complete reading when no read was started.");
            }

            if (this.state == SocketTransferState.Closing)
            {
                throw new InvalidOperationException("Cannot complete reading from a socket that is closing.");
            }

            this.state &= ~SocketTransferState.Reading;
        }

        /// <summary>
        /// Transitions the state machine to the <c>Writing</c> state for the specified write request. This request will be queued if another write is already in progress. If the request is queued, this method returns <c>false</c>.
        /// </summary>
        /// <param name="request">The write request causing this state transition.</param>
        /// <returns><c>true</c> if the write request should be sent out immediately; <c>false</c> if the write request was queued.</returns>
        public bool Write(WriteRequest request)
        {
            if (this.state == SocketTransferState.Closing)
            {
                throw new InvalidOperationException("Cannot start writing to a socket that is closing.");
            }

            var ret = ((this.state & SocketTransferState.Writing) != SocketTransferState.Writing);
            this.state |= SocketTransferState.Writing;

            if (!ret)
            {
                this.writeQueue.Enqueue(request);
            }

            return ret;
        }

        /// <summary>
        /// Notifies the state machine that the write has completed. If there is another write request ready, this method will dequeue it and return it (and stay in the <c>Writing</c> state). If this method returns <c>null</c>, then the state machine has transitioned out of the <c>Writing</c> state.
        /// </summary>
        /// <returns>The next write request, or <c>null</c> if there is no next write request.</returns>
        public WriteRequest WriteComplete()
        {
            if (this.writeQueue.Count == 0)
            {
                this.state &= ~SocketTransferState.Writing;
                return null;
            }

            return this.writeQueue.Dequeue();
        }

        /// <summary>
        /// Transitions the state machine to the <c>Closing</c> state.
        /// </summary>
        public void Close()
        {
            this.state = SocketTransferState.Closing;
        }

        /// <summary>
        /// The transfer state of the socket.
        /// </summary>
        [Flags]
        private enum SocketTransferState
        {
            /// <summary>
            /// The socket is idle (open, with no active reads or writes).
            /// </summary>
            Idle = 0x0,

            /// <summary>
            /// The socket has an asynchronous read in progress.
            /// </summary>
            Reading = 0x1,

            /// <summary>
            /// The socket has an asynchronous write in progress.
            /// </summary>
            Writing = 0x2,

            /// <summary>
            /// The socket is closed. This state may not be combined with any other states.
            /// </summary>
            Closing = 0x4,
        }
    }
}
