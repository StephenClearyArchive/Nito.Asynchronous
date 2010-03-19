using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace Nito.Async
{
    /// <summary>
    /// A serial port that has been opened for asynchronous reading and writing.
    /// </summary>
    public sealed class SerialPort : IDisposable
    {
        /// <summary>
        /// The <see cref="SynchronizationContext"/> used to synchronize this serial port's events.
        /// </summary>
        private readonly SynchronizationContext synchronizationContext;

        /// <summary>
        /// The handshake parameter used to open this serial port.
        /// </summary>
        private readonly Handshake handshake;

        /// <summary>
        /// The discard-null parameter used to open this serial port.
        /// </summary>
        private readonly bool discardNull;

        /// <summary>
        /// The internal <see cref="System.IO.Ports.SerialPort"/> that we wrap.
        /// </summary>
        private readonly System.IO.Ports.SerialPort port;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPort"/> class, opening the serial port with the specified parameters.
        /// </summary>
        /// <param name="config">The serial port configuration parameters.</param>
        public SerialPort(Config config)
        {
            // Capture the synchronization context
            this.synchronizationContext = SynchronizationContext.Current;
            if (this.synchronizationContext == null)
            {
                this.synchronizationContext = new SynchronizationContext();
            }

            // Verify that the synchronization context is synchronized
            SynchronizationContextRegister.Verify(this.synchronizationContext.GetType(), SynchronizationContextProperties.Synchronized);

            // Copy configuration parameters into the wrapped serial port (saving the ones that must be set after it is opened)
            this.port = new System.IO.Ports.SerialPort(config.PortName, config.BaudRate, config.Parity, config.DataBits, config.StopBits);
            this.port.ParityReplace = config.ParityReplace;
            this.port.ReadBufferSize = config.ReadBufferSize;
            this.port.WriteBufferSize = config.WriteBufferSize;
            this.handshake = config.Handshake;
            this.discardNull = config.DiscardNull;

            // Hook up the wrapped serial port events
            this.port.DataReceived += (_, e) => this.synchronizationContext.Post(__ => this.PortDataReceived(), null);
            this.port.PinChanged += (_, e) => this.synchronizationContext.Post(__ => this.PortPinChanged(e.EventType), null);
            this.port.ErrorReceived += (_, e) => this.synchronizationContext.Post(__ => this.PortErrorReceived(e.EventType), null);
        }

        /// <summary>
        /// Respond to data coming in on the port.
        /// </summary>
        private void PortDataReceived()
        {
            if (this.port.IsOpen)
            {
                byte[] data = new byte[this.port.BytesToRead];
                int bytesRead = this.port.Read(data, 0, data.Length);
                if (bytesRead != data.Length)
                {
                    Array.Resize(ref data, bytesRead);
                }

                if (this.DataArrived != null)
                {
                    this.DataArrived(data);
                }
            }
        }

        /// <summary>
        /// Respond to a pin change detection on the port.
        /// </summary>
        /// <param name="change">The pin that changed.</param>
        private void PortPinChanged(SerialPinChange change)
        {
            if (this.port.IsOpen && this.PinChanged != null)
            {
                this.PinChanged(change);
            }
        }

        /// <summary>
        /// Respond to a port-level error on the port.
        /// </summary>
        /// <param name="error">The error that was detected.</param>
        private void PortErrorReceived(SerialError error)
        {
            if (this.port.IsOpen && this.ErrorReceived != null)
            {
                this.ErrorReceived(error);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPort"/> class, opening the serial port with the standard 8-N-1 parameters.
        /// </summary>
        /// <param name="portName">Name of the port to open.</param>
        /// <param name="baudRate">The baud rate.</param>
        public SerialPort(string portName, int baudRate)
            : this(new Config(portName, baudRate))
        {
        }

        /// <summary>
        /// Gets a value indicating whether this serial port is open.
        /// </summary>
        public bool IsOpen
        {
            get { return this.port.IsOpen; }
        }

        /// <summary>
        /// Gets a value indicating whether the data-carrier-detect (DCD) pin is high.
        /// </summary>
        public bool DataCarrierDetect
        {
            get { return this.port.CDHolding; }
        }

        /// <summary>
        /// Gets a value indicating whether the clear-to-send (CTS) pin is high.
        /// </summary>
        public bool ClearToSend
        {
            get { return this.port.CtsHolding; }
        }

        /// <summary>
        /// Gets a value indicating whether the data-set-ready (DSR) pin is high.
        /// </summary>
        public bool DataSetReady
        {
            get { return this.port.DsrHolding; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the serial port is transmitting a break-state condition.
        /// </summary>
        public bool BreakState
        {
            get { return this.port.BreakState; }

            set { this.port.BreakState = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to set the data-terminal-ready (DTR) pin high.
        /// </summary>
        public bool DataTerminalReady
        {
            get { return this.port.DtrEnable; }

            set { this.port.DtrEnable = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to set the request-to-send (RTS) pin high.
        /// </summary>
        public bool RequestToSend
        {
            get { return this.port.RtsEnable; }

            set { this.port.RtsEnable = value; }
        }

        /// <summary>
        /// Closes the serial port. Aborts any outstanding writes.
        /// </summary>
        public void Dispose()
        {
            if (this.port.IsOpen)
            {
                this.port.DiscardOutBuffer();
            }

            this.port.Close();
        }

        /// <summary>
        /// Opens the serial port.
        /// </summary>
        public void Open()
        {
            this.port.Open();
            this.port.Handshake = this.handshake;
            this.port.DiscardNull = this.discardNull;
            this.port.DiscardInBuffer();
            this.port.DiscardOutBuffer();
        }

        /// <summary>
        /// Closes the serial port. Blocks until all outstanding writes are completed.
        /// </summary>
        public void Close()
        {
            this.port.Close();
        }

        /// <summary>
        /// Discards any outstanding writes.
        /// </summary>
        public void DiscardOutputBuffer()
        {
            this.port.DiscardOutBuffer();
        }

        /// <summary>
        /// Writes the specified data to the serial port asynchronously.
        /// </summary>
        /// <param name="data">The data to write to the serial port.</param>
        /// <param name="offset">The offset in the byte array at which to begin writing.</param>
        /// <param name="count">The number of bytes in the byte array to send.</param>
        public void Write(byte[] data, int offset, int count)
        {
            this.port.BaseStream.BeginWrite(
                data,
                offset,
                count,
                (result) =>
                {
                    try
                    {
                        this.port.BaseStream.EndWrite(result);
                    }
                    catch (Exception ex)
                    {
                        this.synchronizationContext.Post(_ => this.WriteErrorDetected(ex), null);
                    }
                },
                null);
        }

        /// <summary>
        /// Writes the specified data to the serial port asynchronously.
        /// </summary>
        /// <param name="data">The data to write to the serial port.</param>
        public void Write(byte[] data)
        {
            this.Write(data, 0, data.Length);
        }

        private void WriteErrorDetected(Exception ex)
        {
            if (this.port.IsOpen && this.WriteError != null)
            {
                this.WriteError(ex);
            }
        }

        /// <summary>
        /// Gets the valid serial port names for the local machine.
        /// </summary>
        public static IEnumerable<string> PortNames
        {
            get { return System.IO.Ports.SerialPort.GetPortNames(); }
        }

        /// <summary>
        /// An event that fires whenever data has been read from the serial port. This event may be triggered out of order with respect to <see cref="PinChanged"/> or <see cref="ErrorReceived"/>.
        /// </summary>
        public Action<byte[]> DataArrived;

        /// <summary>
        /// An event that fires whenever a pin change is detected. This event may be triggered out of order with respect to <see cref="DataArrived"/> or <see cref="ErrorReceived"/>.
        /// </summary>
        public Action<SerialPinChange> PinChanged;

        /// <summary>
        /// An event that fires whenever an error has been received. This event may be triggered out of order with respect to <see cref="DataArrived"/> or <see cref="PinChanged"/>.
        /// </summary>
        public Action<SerialError> ErrorReceived;

        /// <summary>
        /// An event that fires when a write completes with an error.
        /// </summary>
        public Action<Exception> WriteError;

        /// <summary>
        /// Serial port configuration parameters.
        /// </summary>
        public sealed class Config
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Config"/> class.
            /// </summary>
            public Config()
            {
                this.BaudRate = 9600;
                this.Parity = Parity.None;
                this.DataBits = 8;
                this.StopBits = StopBits.One;
                this.Handshake = Handshake.None;
                this.ReadBufferSize = 4096;
                this.WriteBufferSize = 2048;
                this.DataArrivedThreshold = 1;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Config"/> class with the default parameters.
            /// </summary>
            /// <param name="portName">Name of the port.</param>
            /// <param name="baudRate">The baud rate.</param>
            public Config(string portName, int baudRate)
                : this()
            {
                this.PortName = portName;
                this.BaudRate = baudRate;
            }

            /// <summary>
            /// Gets or sets the name of the port to open.
            /// </summary>
            public string PortName { get; set; }

            /// <summary>
            /// Gets or sets the baud rate.
            /// </summary>
            public int BaudRate { get; set; }

            /// <summary>
            /// Gets or sets the parity.
            /// </summary>
            public Parity Parity { get; set; }

            /// <summary>
            /// Gets or sets the parity replacement character.
            /// </summary>
            public byte ParityReplace { get; set; }

            /// <summary>
            /// Gets or sets the data bits.
            /// </summary>
            public int DataBits { get; set; }

            /// <summary>
            /// Gets or sets the stop bits.
            /// </summary>
            public StopBits StopBits { get; set; }

            /// <summary>
            /// Gets or sets the handshake.
            /// </summary>
            public Handshake Handshake { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether null bytes should be automatically discarded when reading.
            /// </summary>
            public bool DiscardNull { get; set; }

            /// <summary>
            /// Gets or sets the size of the read buffer passed to the serial port driver.
            /// </summary>
            public int ReadBufferSize { get; set; }

            /// <summary>
            /// Gets or sets the size of the write buffer passed to the serial port driver.
            /// </summary>
            public int WriteBufferSize { get; set; }

            /// <summary>
            /// Gets or sets the number of bytes to attempt to wait for before calling <see cref="DataArrived"/>.
            /// </summary>
            public int DataArrivedThreshold { get; set; }
        }
    }
}
