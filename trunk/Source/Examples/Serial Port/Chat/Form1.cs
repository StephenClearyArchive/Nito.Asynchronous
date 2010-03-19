using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nito.Async;
using System.IO;

namespace Chat
{
    public partial class Form1 : Form
    {
        private SerialPort port;
        private long received, sent;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBoxSerialPorts.Items.AddRange(SerialPort.PortNames.ToArray());
        }

        private void EnableDisableControls()
        {
            bool open = this.port == null ? false : this.port.IsOpen;

            this.comboBoxSerialPorts.Enabled = !open;
            this.buttonOpen.Enabled = !open;
            this.buttonClose.Enabled = open;
            this.buttonAbortiveClose.Enabled = open;
            this.buttonSend.Enabled = open;
            this.buttonSendFile.Enabled = open;

            if (!open)
            {
                this.toolStripStatusLabel.Text = "Closed";
            }
            else
            {
                this.toolStripStatusLabel.Text = "Opened: " + this.comboBoxSerialPorts.SelectedItem;
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            try
            {
                this.port = new SerialPort(new SerialPort.Config((string)this.comboBoxSerialPorts.SelectedItem, 115200) { Handshake = System.IO.Ports.Handshake.RequestToSend });
                this.port.DataArrived += data =>
                {
                    //this.textBoxReceiving.AppendText("Incoming data: " + data.Length + " bytes" + Environment.NewLine);
                    this.received += data.Length;
                    this.toolStripStatusLabelReceived.Text = this.received.ToString();
                };
                this.port.ErrorReceived += error => this.textBoxReceiving.AppendText("Error detected: " + error + Environment.NewLine);
                this.port.WriteError += error => this.textBoxReceiving.AppendText("Write error detected: " + error.Message + Environment.NewLine);
                //this.port.PinChanged += pin => this.textBoxReceiving.AppendText("Pin change detected: " + pin + Environment.NewLine);
                this.port.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: [" + ex.GetType().Name + "] " + ex.Message);
            }

            this.EnableDisableControls();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.port.Close();
            this.EnableDisableControls();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            byte[] data = Encoding.ASCII.GetBytes(this.textBoxSending.Text);
            this.port.Write(data);
            this.sent += data.Length;
            this.toolStripStatusLabelSent.Text = this.sent.ToString();
        }

        private void buttonSendFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                // Read the file
                byte[] file = File.ReadAllBytes(openFileDialog.FileName);

                // Send the file
                this.port.Write(file);

                this.sent += file.Length;
                this.toolStripStatusLabelSent.Text = this.sent.ToString();
            }
            catch (Exception ex)
            {
                this.port.Close();
                MessageBox.Show("Error: [" + ex.GetType().Name + "] " + ex.Message);
            }

            this.EnableDisableControls();
        }

        private void buttonAbortiveClose_Click(object sender, EventArgs e)
        {
            this.port.DiscardOutputBuffer();
            this.port.Close();
            this.EnableDisableControls();
        }
    }
}
