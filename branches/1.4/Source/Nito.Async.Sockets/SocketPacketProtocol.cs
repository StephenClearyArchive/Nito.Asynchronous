// <copyright file="SocketPacketProtocol.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Async.Sockets
{
    using System;

    /// <summary>
    /// Maintains the necessary buffers for applying a packet protocol over a stream-based socket.
    /// </summary>
    /// <remarks>
    /// <para>This class uses a 4-byte signed integer length prefix, which allows for packet sizes up to 2 GB with single-bit error detection.</para>
    /// <para>Keepalive packets are supported as packets with a length prefix of 0; <see cref="PacketArrived"/> is never called when keepalive packets are returned.</para>
    /// <para>Once <see cref="Start"/> is called, this class continuously reads from the underlying socket, calling <see cref="PacketArrived"/> when packets are received. To stop reading, close the underlying socket.</para>
    /// <para>Reading will also automatically stop when a read error or graceful close is detected.</para>
    /// </remarks>
    public class SocketPacketProtocol
    {
        /// <summary>
        /// The buffer for the length prefix; this is always 4 bytes long.
        /// </summary>
        private byte[] lengthBuffer;

        /// <summary>
        /// The buffer for the data; this is null if we are receiving the length prefix buffer.
        /// </summary>
        private byte[] dataBuffer;

        /// <summary>
        /// The number of bytes already read into the buffer (the length buffer if <see cref="dataBuffer"/> is null, otherwise the data buffer).
        /// </summary>
        private int bytesReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketPacketProtocol"/> class bound to a given socket connection.
        /// </summary>
        /// <param name="socket">The socket used for communication.</param>
        public SocketPacketProtocol(IAsyncTcpConnection socket)
        {
            this.Socket = socket;
            this.lengthBuffer = new byte[sizeof(int)];
        }

        /// <summary>
        /// Indicates the completion of a packet read from the socket.
        /// </summary>
        /// <remarks>
        /// <para>This may be called with a null packet, indicating that the other end graciously closed the connection.</para>
        /// </remarks>
        public event Action<AsyncResultEventArgs<byte[]>> PacketArrived;

        /// <summary>
        /// Gets the socket used for communication.
        /// </summary>
        public IAsyncTcpConnection Socket { get; private set; }

        /// <overloads>
        /// <summary>Sends a packet to a socket.</summary>
        /// <remarks>
        /// <para>Generates a length prefix for the packet and writes the length prefix and packet to the socket.</para>
        /// </remarks>
        /// </overloads>
        /// <summary>Sends a packet to a socket.</summary>
        /// <remarks>
        /// <para>Generates a length prefix for the packet and writes the length prefix and packet to the socket.</para>
        /// </remarks>
        /// <param name="socket">The socket used for communication.</param>
        /// <param name="packet">The packet to send.</param>
        /// <param name="state">The user-defined state that is passed to WriteCompleted. May be null.</param>
        public static void WritePacketAsync(IAsyncTcpConnection socket, byte[] packet, object state)
        {
            // Get the length prefix for the message
            byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);

            // We use the special CallbackOnErrorsOnly object to tell the socket we don't want
            //  WriteCompleted to be invoked (it would confuse socket users if they see WriteCompleted
            //  events for writes they never started).
            socket.WriteAsync(lengthPrefix, new CallbackOnErrorsOnly());

            // Send the actual message, this time enabling the normal callback.
            socket.WriteAsync(packet, state);
        }

        /// <inheritdoc cref="WritePacketAsync(IAsyncTcpConnection, byte[], object)" />
        /// <param name="socket">The socket used for communication.</param>
        /// <param name="packet">The packet to send.</param>
        public static void WritePacketAsync(IAsyncTcpConnection socket, byte[] packet)
        {
            WritePacketAsync(socket, packet, null);
        }

        /// <summary>
        /// Sends a keepalive (0-length) packet to the socket.
        /// </summary>
        /// <param name="socket">The socket used for communication.</param>
        public static void WriteKeepaliveAsync(IAsyncTcpConnection socket)
        {
            // We use CallbackOnErrorsOnly to indicate that the WriteCompleted callback should only be
            //  called if there was an error.
            socket.WriteAsync(BitConverter.GetBytes((int)0), new CallbackOnErrorsOnly());
        }

        /// <summary>
        /// Begins reading from the socket.
        /// </summary>
        public void Start()
        {
            this.Socket.ReadCompleted += this.SocketReadCompleted;
            this.ContinueReading();
        }

        /// <summary>
        /// Requests a read directly into the correct buffer.
        /// </summary>
        private void ContinueReading()
        {
            // Read into the appropriate buffer: length or data
            if (this.dataBuffer != null)
            {
                this.Socket.ReadAsync(this.dataBuffer, this.bytesReceived, this.dataBuffer.Length - this.bytesReceived);
            }
            else
            {
                this.Socket.ReadAsync(this.lengthBuffer, this.bytesReceived, this.lengthBuffer.Length - this.bytesReceived);
            }
        }

        /// <summary>
        /// Called when a socket read completes. Parses the received data and calls <see cref="PacketArrived"/> if necessary.
        /// </summary>
        /// <param name="e">Argument object containing the number of bytes read.</param>
        /// <exception cref="System.IO.InvalidDataException">If the data received is not a packet.</exception>
        private void SocketReadCompleted(AsyncResultEventArgs<int> e)
        {
            // Pass along read errors verbatim
            if (e.Error != null)
            {
                if (this.PacketArrived != null)
                {
                    this.PacketArrived(new AsyncResultEventArgs<byte[]>(e.Error));
                }

                return;
            }

            // Get the number of bytes read into the buffer
            this.bytesReceived += e.Result;

            // If we get a zero-length read, then that indicates the remote side graciously closed the connection
            if (e.Result == 0)
            {
                if (this.PacketArrived != null)
                {
                    this.PacketArrived(new AsyncResultEventArgs<byte[]>((byte[])null));
                }

                return;
            }

            if (this.dataBuffer == null)
            {
                // (We're currently receiving the length buffer)
                if (this.bytesReceived != sizeof(int))
                {
                    // We haven't gotten all the length buffer yet
                    this.ContinueReading();
                }
                else
                {
                    // We've gotten the length buffer
                    int length = BitConverter.ToInt32(this.lengthBuffer, 0);

                    // Sanity check for length < 0
                    //  This check will catch 50% of transmission errors that make it past both the IP and Ethernet checksums
                    if (length < 0)
                    {
                        if (this.PacketArrived != null)
                        {
                            this.PacketArrived(new AsyncResultEventArgs<byte[]>(new System.IO.InvalidDataException("Packet length less than zero (corrupted message)")));
                        }

                        return;
                    }

                    // Zero-length packets are allowed as keepalives
                    if (length == 0)
                    {
                        this.bytesReceived = 0;
                        this.ContinueReading();
                    }
                    else
                    {
                        // Create the data buffer and start reading into it
                        this.dataBuffer = new byte[length];
                        this.bytesReceived = 0;
                        this.ContinueReading();
                    }
                }
            }
            else
            {
                if (this.bytesReceived != this.dataBuffer.Length)
                {
                    // We haven't gotten all the data buffer yet
                    this.ContinueReading();
                }
                else
                {
                    // We've gotten an entire packet
                    if (this.PacketArrived != null)
                    {
                        this.PacketArrived(new AsyncResultEventArgs<byte[]>(this.dataBuffer));
                    }

                    // Start reading the length buffer again
                    this.dataBuffer = null;
                    this.bytesReceived = 0;
                    this.ContinueReading();
                }
            }
        }
    }
}
