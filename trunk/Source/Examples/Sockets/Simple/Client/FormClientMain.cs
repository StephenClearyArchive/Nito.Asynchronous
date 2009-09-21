using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using Nito.Async.Sockets;
using Nito.Async;

namespace Client
{
    public partial class FormClientMain : Form
    {
        public FormClientMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The connected state of the socket.
        /// </summary>
        private enum SocketState
        {
            /// <summary>
            /// The socket is closed; we are not trying to connect.
            /// </summary>
            Closed,

            /// <summary>
            /// The socket is attempting to connect.
            /// </summary>
            Connecting,

            /// <summary>
            /// The socket is connected.
            /// </summary>
            Connected,

            /// <summary>
            /// The socket is attempting to disconnect.
            /// </summary>
            Disconnecting
        }

        /// <summary>
        /// The socket that connects to the server. This is null if ClientSocketState is SocketState.Closed.
        /// </summary>
        private SimpleClientTcpSocket ClientSocket;

        /// <summary>
        /// The connected state of the socket. If this is SocketState.Closed, then ClientSocket is null.
        /// </summary>
        private SocketState ClientSocketState;

        /// <summary>
        /// Closes and clears the socket, without causing exceptions.
        /// </summary>
        private void ResetSocket()
        {
            // Close the socket
            ClientSocket.Close();
            ClientSocket = null;

            // Indicate there is no socket connection
            ClientSocketState = SocketState.Closed;
        }

        /// <summary>
        /// Ensures the display matches the socket state.
        /// </summary>
        private void RefreshDisplay()
        {
            // If the socket is connected, don't allow connecting it; if it's not, then don't allow disconnecting it
            buttonConnect.Enabled = (ClientSocketState == SocketState.Closed);
            buttonDisconnect.Enabled = (ClientSocketState == SocketState.Connected);
            buttonAbortiveClose.Enabled = (ClientSocketState == SocketState.Connected);

            // We can only send messages if we have a connection
            buttonSendMessage.Enabled = (ClientSocketState == SocketState.Connected);
            buttonSendComplexMessage.Enabled = (ClientSocketState == SocketState.Connected);

            // Display status
            switch (ClientSocketState)
            {
                case SocketState.Closed:
                    toolStripStatusLabel.Text = "Stopped";
                    break;
                case SocketState.Connecting:
                    toolStripStatusLabel.Text = "Connecting";
                    break;
                case SocketState.Connected:
                    toolStripStatusLabel.Text = "Connected to " + ClientSocket.RemoteEndPoint.ToString();
                    break;
                case SocketState.Disconnecting:
                    toolStripStatusLabel.Text = "Disconnecting";
                    break;
            }
        }

        private void ClientSocket_ConnectCompleted(AsyncCompletedEventArgs e)
        {
            try
            {
                // Check for errors
                if (e.Error != null)
                {
                    ResetSocket();
                    textBoxLog.AppendText("Socket error during Connect: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                    return;
                }

                // Adjust state
                ClientSocketState = SocketState.Connected;

                // Display the connection information
                textBoxLog.AppendText("Connection established to " + ClientSocket.RemoteEndPoint.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                textBoxLog.AppendText("Socket error during Connection: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void ClientSocket_WriteCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Check for errors
            if (e.Error != null)
            {
                // Note: WriteCompleted may be called as the result of a normal write or a keepalive packet.

                ResetSocket();

                // If you want to get fancy, you can tell if the error is the result of a write failure or a keepalive
                //  failure by testing e.UserState, which is set by normal writes.
                if (e.UserState is string)
                    textBoxLog.AppendText("Socket error during Write: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                else
                    textBoxLog.AppendText("Socket error detected by keepalive: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
            }
            else
            {
                string description = (string)e.UserState;
                textBoxLog.AppendText("Socket write completed for message " + description + Environment.NewLine);
            }
            
            RefreshDisplay();
        }

        private void ClientSocket_ShutdownCompleted(AsyncCompletedEventArgs e)
        {
            // Check for errors
            if (e.Error != null)
            {
                ResetSocket();
                textBoxLog.AppendText("Socket error during Shutdown: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
            }
            else
            {
                textBoxLog.AppendText("Socket shutdown completed" + Environment.NewLine);

                // Close the socket and set the socket state
                ResetSocket();
            }

            RefreshDisplay();
        }

        private void ClientSocket_PacketArrived(AsyncResultEventArgs<byte[]> e)
        {
            try
            {
                // Check for errors
                if (e.Error != null)
                {
                    ResetSocket();
                    textBoxLog.AppendText("Socket error during Read: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                }
                else if (e.Result == null)
                {
                    // PacketArrived completes with a null packet when the other side gracefully closes the connection
                    textBoxLog.AppendText("Socket graceful close detected" + Environment.NewLine);

                    // Close the socket and handle the state transition to disconnected.
                    ResetSocket();
                }
                else
                {
                    // At this point, we know we actually got a message.

                    // Deserialize the message
                    object message = Messages.Util.Deserialize(e.Result);

                    // Handle the message
                    Messages.StringMessage stringMessage = message as Messages.StringMessage;
                    if (stringMessage != null)
                    {
                        textBoxLog.AppendText("Socket read got a string message: " + stringMessage.Message + Environment.NewLine);
                        return;
                    }

                    Messages.ComplexMessage complexMessage = message as Messages.ComplexMessage;
                    if (complexMessage != null)
                    {
                        textBoxLog.AppendText("Socket read got a complex message: (UniqueID = " + complexMessage.UniqueID.ToString() +
                            ", Time = " + complexMessage.Time.ToString() + ", Message = " + complexMessage.Message + ")" + Environment.NewLine);
                        return;
                    }

                    textBoxLog.AppendText("Socket read got an unknown message of type " + message.GetType().Name + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                ResetSocket();
                textBoxLog.AppendText("Error reading from socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // Read the IP address
                IPAddress serverIPAddress;
                if (!IPAddress.TryParse(textBoxIPAddress.Text, out serverIPAddress))
                {
                    MessageBox.Show("Invalid IP address: " + textBoxIPAddress.Text);
                    textBoxIPAddress.Focus();
                    return;
                }

                // Read the port number
                int port;
                if (!int.TryParse(textBoxPort.Text, out port))
                {
                    MessageBox.Show("Invalid port number: " + textBoxPort.Text);
                    textBoxPort.Focus();
                    return;
                }

                // Begin connecting to the remote IP
                ClientSocket = new SimpleClientTcpSocket();
                ClientSocket.ConnectCompleted += ClientSocket_ConnectCompleted;
                ClientSocket.PacketArrived += ClientSocket_PacketArrived;
                ClientSocket.WriteCompleted += (args) => ClientSocket_WriteCompleted(ClientSocket, args);
                ClientSocket.ShutdownCompleted += ClientSocket_ShutdownCompleted;
                ClientSocket.ConnectAsync(serverIPAddress, port);
                ClientSocketState = SocketState.Connecting;
                textBoxLog.AppendText("Connecting socket to " + (new IPEndPoint(serverIPAddress, port)).ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                textBoxLog.AppendText("Error creating connecting socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                ClientSocket.ShutdownAsync();
                ClientSocketState = SocketState.Disconnecting;
                textBoxLog.AppendText("Disconnecting socket" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                textBoxLog.AppendText("Error disconnecting socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void buttonAbortiveClose_Click(object sender, EventArgs e)
        {
            try
            {
                ClientSocket.AbortiveClose();
                ClientSocket = null;
                ClientSocketState = SocketState.Closed;
                textBoxLog.AppendText("Abortively closed socket" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                textBoxLog.AppendText("Error aborting socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void buttonSendMessage_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the message to send
                Messages.StringMessage message = new Messages.StringMessage();
                message.Message = textBoxMessage.Text;

                // Serialize the message to a binary array
                byte[] binaryMessage = Messages.Util.Serialize(message);

                // Send the message; the state is used by ClientSocket_WriteCompleted to display an output to the log
                string description = "<string message: " + message.Message + ">";
                ClientSocket.WriteAsync(binaryMessage, description);

                textBoxLog.AppendText("Sending message " + description + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                textBoxLog.AppendText("Error sending message to socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void buttonSendComplexMessage_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the message to send
                Messages.ComplexMessage message = new Messages.ComplexMessage();
                message.UniqueID = Guid.NewGuid();
                message.Time = DateTimeOffset.Now;
                message.Message = textBoxMessage.Text;

                // Serialize the message to a binary array
                byte[] binaryMessage = Messages.Util.Serialize(message);

                // Send the message; the state is used by ClientSocket_WriteCompleted to display an output to the log
                string description = "<complex message: " + message.UniqueID + ">";
                ClientSocket.WriteAsync(binaryMessage, description);

                textBoxLog.AppendText("Sending message " + description + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                textBoxLog.AppendText("Error sending message to socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        // This is just a utility function for displaying all the IP(v4) addresses of a computer; it is not
        //  necessary in order to use ClientTcpSocket/ServerTcpSocket.
        private void buttonDisplayIP_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1)
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")");
                }
            }

            MessageBox.Show(sb.ToString());
        }
    }
}
