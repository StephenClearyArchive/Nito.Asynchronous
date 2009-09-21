using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Nito.Async;
using Nito.Async.Sockets;

namespace Server
{
    public partial class FormServerMain : Form
    {
        // Always runs on GUI thread
        public FormServerMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The socket that listens for connections. This is null if we are not listening.
        /// </summary>
        private SimpleServerTcpSocket ListeningSocket;

        /// <summary>
        /// The state of a child socket connection.
        /// </summary>
        private enum ChildSocketState
        {
            /// <summary>
            /// The child socket has an established connection.
            /// </summary>
            Connected,

            /// <summary>
            /// The child socket is disconnecting.
            /// </summary>
            Disconnecting
        }

        /// <summary>
        /// A mapping of sockets (with established connections) to their state.
        /// </summary>
        private Dictionary<SimpleServerChildTcpSocket, ChildSocketState> ChildSockets = new Dictionary<SimpleServerChildTcpSocket, ChildSocketState>();

        /// <summary>
        /// Closes and clears the listening socket and all connected sockets, without causing exceptions.
        /// </summary>
        private void ResetListeningSocket()
        {
            // Close all child sockets
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> socket in ChildSockets)
                socket.Key.Close();
            ChildSockets.Clear();

            // Close the listening socket
            ListeningSocket.Close();
            ListeningSocket = null;
        }

        /// <summary>
        /// Closes and clears a child socket (established connection), without causing exceptions.
        /// </summary>
        /// <param name="childSocket">The child socket to close. May be null.</param>
        private void ResetChildSocket(SimpleServerChildTcpSocket childSocket)
        {
            // Close the child socket if possible
            if (childSocket != null)
                childSocket.Close();

            // Remove it from the list of child sockets
            ChildSockets.Remove(childSocket);
        }

        private void RefreshDisplay()
        {
            // If the server socket is running, don't allow starting it; if it's not, then don't allow stopping it
            buttonStart.Enabled = (ListeningSocket == null);
            buttonStop.Enabled = (ListeningSocket != null);

            // We can only send messages or disconnect if we have connected clients
            buttonSendMessage.Enabled = (ChildSockets.Count != 0);
            buttonSendComplexMessage.Enabled = (ChildSockets.Count != 0);
            buttonDisconnect.Enabled = (ChildSockets.Count != 0);
            buttonAbortiveClose.Enabled = (ChildSockets.Count != 0);

            // Display status
            if (ListeningSocket == null)
                toolStripStatusLabel1.Text = "Stopped";
            else
                toolStripStatusLabel1.Text = "Listening on " + ListeningSocket.LocalEndPoint.ToString();
            toolStripStatusLabel2.Text = ChildSockets.Count + " connections";
        }

        private void ListeningSocket_ConnectionArrived(AsyncResultEventArgs<SimpleServerChildTcpSocket> e)
        {
            // Check for errors
            if (e.Error != null)
            {
                ResetListeningSocket();
                textBoxLog.AppendText("Socket error during Accept: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                RefreshDisplay();
                return;
            }

            SimpleServerChildTcpSocket socket = e.Result;

            try
            {
                // Save the new child socket connection
                ChildSockets.Add(socket, ChildSocketState.Connected);

                socket.PacketArrived += (args) => ChildSocket_PacketArrived(socket, args);
                socket.WriteCompleted += (args) => ChildSocket_WriteCompleted(socket, args);
                socket.ShutdownCompleted += (args) => ChildSocket_ShutdownCompleted(socket, args);

                // Display the connection information
                textBoxLog.AppendText("Connection established to " + socket.RemoteEndPoint.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetChildSocket(socket);
                textBoxLog.AppendText("Socket error accepting connection: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void ChildSocket_PacketArrived(SimpleServerChildTcpSocket socket, AsyncResultEventArgs<byte[]> e)
        {
            try
            {
                // Check for errors
                if (e.Error != null)
                {
                    textBoxLog.AppendText("Client socket error during Read from " + socket.RemoteEndPoint.ToString() + ": [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                    ResetChildSocket(socket);
                }
                else if (e.Result == null)
                {
                    // PacketArrived completes with a null packet when the other side gracefully closes the connection
                    textBoxLog.AppendText("Socket graceful close detected from " + socket.RemoteEndPoint.ToString() + Environment.NewLine);

                    // Close the socket and remove it from the list
                    ResetChildSocket(socket);
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
                        textBoxLog.AppendText("Socket read got a string message from " + socket.RemoteEndPoint.ToString() + ": " + stringMessage.Message + Environment.NewLine);
                        return;
                    }

                    Messages.ComplexMessage complexMessage = message as Messages.ComplexMessage;
                    if (complexMessage != null)
                    {
                        textBoxLog.AppendText("Socket read got a complex message from " + socket.RemoteEndPoint.ToString() + ": (UniqueID = " + complexMessage.UniqueID.ToString() +
                            ", Time = " + complexMessage.Time.ToString() + ", Message = " + complexMessage.Message + ")" + Environment.NewLine);
                        return;
                    }

                    textBoxLog.AppendText("Socket read got an unknown message from " + socket.RemoteEndPoint.ToString() + " of type " + message.GetType().Name + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                textBoxLog.AppendText("Error reading from socket " + socket.RemoteEndPoint.ToString() + ": [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
                ResetChildSocket(socket);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void ChildSocket_ShutdownCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SimpleServerChildTcpSocket socket = (SimpleServerChildTcpSocket)sender;

            // Check for errors
            if (e.Error != null)
            {
                textBoxLog.AppendText("Socket error during Shutdown of " + socket.RemoteEndPoint.ToString() + ": [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                ResetChildSocket(socket);
            }
            else
            {
                textBoxLog.AppendText("Socket shutdown completed on " + socket.RemoteEndPoint.ToString() + Environment.NewLine);

                // Close the socket and remove it from the list
                ResetChildSocket(socket);
            }

            RefreshDisplay();
        }

        private void ChildSocket_WriteCompleted(SimpleServerChildTcpSocket socket, AsyncCompletedEventArgs e)
        {
            // Check for errors
            if (e.Error != null)
            {
                // Note: WriteCompleted may be called as the result of a normal write (SocketPacketizer.WritePacketAsync),
                //  or as the result of a call to SocketPacketizer.WriteKeepaliveAsync. However, WriteKeepaliveAsync
                //  will never invoke WriteCompleted if the write was successful; it will only invoke WriteCompleted if
                //  the keepalive packet failed (indicating a loss of connection).

                // If you want to get fancy, you can tell if the error is the result of a write failure or a keepalive
                //  failure by testing e.UserState, which is set by normal writes.
                if (e.UserState is string)
                    textBoxLog.AppendText("Socket error during Write to " + socket.RemoteEndPoint.ToString() + ": [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                else
                    textBoxLog.AppendText("Socket error detected by keepalive to " + socket.RemoteEndPoint.ToString() + ": [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);

                ResetChildSocket(socket);
            }
            else
            {
                string description = (string)e.UserState;
                textBoxLog.AppendText("Socket write completed to " + socket.RemoteEndPoint.ToString() + " for message " + description + Environment.NewLine);
            }

            RefreshDisplay();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // Read the port number
            int port;
            if (!int.TryParse(textBoxPort.Text, out port))
            {
                MessageBox.Show("Invalid port number: " + textBoxPort.Text);
                textBoxPort.Focus();
                return;
            }

            try
            {
                // Define the socket, bind to the port, and start accepting connections
                ListeningSocket = new SimpleServerTcpSocket();
                ListeningSocket.ConnectionArrived += ListeningSocket_ConnectionArrived;
                ListeningSocket.Listen(port);

                textBoxLog.AppendText("Listening on port " + port.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetListeningSocket();
                textBoxLog.AppendText("Error creating listening socket on port " + port.ToString() + ": [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            RefreshDisplay();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            // Close the listening socket cleanly
            ResetListeningSocket();
            RefreshDisplay();
        }

        private void buttonSendMessage_Click(object sender, EventArgs e)
        {
            // This function sends a simple (string) message to all connected clients
            Messages.StringMessage message = new Messages.StringMessage();
            message.Message = textBoxMessage.Text;

            string description = "<string message: " + message.Message + ">";

            // Serialize it to a binary array
            byte[] binaryObject = Messages.Util.Serialize(message);

            // Keep a list of all errors for child sockets
            Dictionary<SimpleServerChildTcpSocket, Exception> SocketErrors = new Dictionary<SimpleServerChildTcpSocket, Exception>();

            // Start a send on each child socket
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket in ChildSockets)
            {
                // Ignore sockets that are disconnecting
                if (childSocket.Value != ChildSocketState.Connected)
                    continue;

                try
                {
                    textBoxLog.AppendText("Sending to " + childSocket.Key.RemoteEndPoint.ToString() + ": " + description + Environment.NewLine);
                    childSocket.Key.WriteAsync(binaryObject, description);
                }
                catch (Exception ex)
                {
                    // Make a note of the error to handle later
                    SocketErrors.Add(childSocket.Key, ex);
                }
            }

            // Handle all errors. This is done outside the enumeration loop because the child socket
            //  error recovery will remove the socket from the list of child sockets.
            foreach (KeyValuePair<SimpleServerChildTcpSocket, Exception> error in SocketErrors)
            {
                textBoxLog.AppendText("Child Socket error sending message to " + error.Key.RemoteEndPoint.ToString() + ": [" + error.Value.GetType().Name + "] " + error.Value.Message + Environment.NewLine);
                ResetChildSocket(error.Key);
            }

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

        private void buttonSendComplexMessage_Click(object sender, EventArgs e)
        {
            // This function sends a complex message to all connected clients
            Messages.ComplexMessage message = new Messages.ComplexMessage();
            message.UniqueID = Guid.NewGuid();
            message.Time = DateTimeOffset.Now;
            message.Message = textBoxMessage.Text;

            string description = "<complex message: " + message.UniqueID + ">";

            // Serialize it to a binary array
            byte[] binaryObject = Messages.Util.Serialize(message);

            // Keep a list of all errors for child sockets
            Dictionary<SimpleServerChildTcpSocket, Exception> SocketErrors = new Dictionary<SimpleServerChildTcpSocket, Exception>();

            // Start a send on each child socket
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket in ChildSockets)
            {
                // Ignore sockets that are disconnecting
                if (childSocket.Value != ChildSocketState.Connected)
                    continue;

                try
                {
                    textBoxLog.AppendText("Sending to " + childSocket.Key.RemoteEndPoint.ToString() + ": " + description + Environment.NewLine);
                    childSocket.Key.WriteAsync(binaryObject, description);
                }
                catch (Exception ex)
                {
                    // Make a note of the error to handle later
                    SocketErrors.Add(childSocket.Key, ex);
                }
            }

            // Handle all errors. This is done outside the enumeration loop because the child socket
            //  error recovery will remove the socket from the list of child sockets.
            foreach (KeyValuePair<SimpleServerChildTcpSocket, Exception> error in SocketErrors)
            {
                textBoxLog.AppendText("Child Socket error sending message to " + error.Key.RemoteEndPoint.ToString() + ": [" + error.Value.GetType().Name + "] " + error.Value.Message + Environment.NewLine);
                ResetChildSocket(error.Key);
            }

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            // Initiate a graceful disconnect for all clients
            SimpleServerChildTcpSocket[] children = new SimpleServerChildTcpSocket[ChildSockets.Keys.Count];
            ChildSockets.Keys.CopyTo(children, 0);
            foreach (SimpleServerChildTcpSocket child in children)
            {
                try
                {
                    child.ShutdownAsync();
                    ChildSockets[child] = ChildSocketState.Disconnecting;
                }
                catch (Exception ex)
                {
                    textBoxLog.AppendText("Child Socket error disconnecting from " + child.RemoteEndPoint.ToString() + ": [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
                    ResetChildSocket(child);
                }
            }

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

        private void buttonAbortiveClose_Click(object sender, EventArgs e)
        {
            // Keep a list of all errors for child sockets
            Dictionary<SimpleServerChildTcpSocket, Exception> SocketErrors = new Dictionary<SimpleServerChildTcpSocket, Exception>();

            // Abortively close all clients
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket in ChildSockets)
            {
                try
                {
                    childSocket.Key.AbortiveClose();
                }
                catch (Exception ex)
                {
                    // Make a note of the error to handle later
                    SocketErrors.Add(childSocket.Key, ex);
                }
            }

            // Handle all errors. This is done outside the enumeration loop because the child socket
            //  error recovery will remove the socket from the list of child sockets.
            foreach (KeyValuePair<SimpleServerChildTcpSocket, Exception> error in SocketErrors)
            {
                textBoxLog.AppendText("Child Socket error aborting " + error.Key.RemoteEndPoint.ToString() + ": [" + error.Value.GetType().Name + "] " + error.Value.Message + Environment.NewLine);
                ResetChildSocket(error.Key);
            }

            ChildSockets.Clear();

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

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
