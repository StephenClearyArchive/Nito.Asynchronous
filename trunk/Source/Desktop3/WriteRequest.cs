namespace Nito.Communication
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A write request for a socket.
    /// </summary>
    internal sealed class WriteRequest
    {
        private readonly IList<ArraySegment<byte>> buffers;
        private readonly byte[] buffer;
        private readonly int offset;
        private readonly int size;
        private readonly object state;

        public WriteRequest(IList<ArraySegment<byte>> buffers, object state)
        {
            this.buffers = buffers;
            this.state = state;
        }

        public WriteRequest(byte[] buffer, int offset, int size, object state)
        {
            this.buffer = buffer;
            this.offset = offset;
            this.size = size;
            this.state = state;
        }

        /// <summary>
        /// Gets the buffers to write. This may be <c>null</c> if there is only one buffer.
        /// </summary>
        public IList<ArraySegment<byte>> Buffers
        {
            get { return this.buffers; }
        }

        public byte[] Buffer
        {
            get { return this.buffer; }
        }

        public int Offset
        {
            get { return this.offset; }
        }

        public int Size
        {
            get { return this.size; }
        }

        /// <summary>
        /// Gets the user state for the write request.
        /// </summary>
        public Object State
        {
            get { return this.state; }
        }
    }
}