namespace Client
{
    partial class FormClientMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonDisplayIP = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.buttonSendMessage = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonSendComplexMessage = new System.Windows.Forms.Button();
            this.textBoxIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.buttonAbortiveClose = new System.Windows.Forms.Button();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.checkBoxKeepalive = new System.Windows.Forms.CheckBox();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonDisplayIP
            // 
            this.buttonDisplayIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDisplayIP.Location = new System.Drawing.Point(359, 38);
            this.buttonDisplayIP.Name = "buttonDisplayIP";
            this.buttonDisplayIP.Size = new System.Drawing.Size(75, 23);
            this.buttonDisplayIP.TabIndex = 6;
            this.buttonDisplayIP.Text = "Display &IP";
            this.buttonDisplayIP.UseVisualStyleBackColor = true;
            this.buttonDisplayIP.Click += new System.EventHandler(this.buttonDisplayIP_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 123);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Size = new System.Drawing.Size(422, 88);
            this.textBoxLog.TabIndex = 10;
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMessage.Location = new System.Drawing.Point(114, 67);
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(320, 20);
            this.textBoxMessage.TabIndex = 8;
            // 
            // buttonSendMessage
            // 
            this.buttonSendMessage.Enabled = false;
            this.buttonSendMessage.Location = new System.Drawing.Point(12, 65);
            this.buttonSendMessage.Name = "buttonSendMessage";
            this.buttonSendMessage.Size = new System.Drawing.Size(96, 23);
            this.buttonSendMessage.TabIndex = 7;
            this.buttonSendMessage.Text = "&Send Message";
            this.buttonSendMessage.UseVisualStyleBackColor = true;
            this.buttonSendMessage.Click += new System.EventHandler(this.buttonSendMessage_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Enabled = false;
            this.buttonDisconnect.Location = new System.Drawing.Point(93, 38);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(75, 23);
            this.buttonDisconnect.TabIndex = 4;
            this.buttonDisconnect.Text = "&Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(12, 38);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 3;
            this.buttonConnect.Text = "&Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 214);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(446, 22);
            this.statusStrip.TabIndex = 7;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(51, 17);
            this.toolStripStatusLabel.Text = "Stopped";
            // 
            // buttonSendComplexMessage
            // 
            this.buttonSendComplexMessage.Enabled = false;
            this.buttonSendComplexMessage.Location = new System.Drawing.Point(12, 94);
            this.buttonSendComplexMessage.Name = "buttonSendComplexMessage";
            this.buttonSendComplexMessage.Size = new System.Drawing.Size(142, 23);
            this.buttonSendComplexMessage.TabIndex = 9;
            this.buttonSendComplexMessage.Text = "Send Complex &Message";
            this.buttonSendComplexMessage.UseVisualStyleBackColor = true;
            this.buttonSendComplexMessage.Click += new System.EventHandler(this.buttonSendComplexMessage_Click);
            // 
            // textBoxIPAddress
            // 
            this.textBoxIPAddress.Location = new System.Drawing.Point(76, 12);
            this.textBoxIPAddress.Name = "textBoxIPAddress";
            this.textBoxIPAddress.Size = new System.Drawing.Size(133, 20);
            this.textBoxIPAddress.TabIndex = 1;
            this.textBoxIPAddress.Text = "127.0.0.1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "IP Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Port";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(247, 12);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(100, 20);
            this.textBoxPort.TabIndex = 2;
            this.textBoxPort.Text = "7750";
            // 
            // buttonAbortiveClose
            // 
            this.buttonAbortiveClose.Enabled = false;
            this.buttonAbortiveClose.Location = new System.Drawing.Point(174, 38);
            this.buttonAbortiveClose.Name = "buttonAbortiveClose";
            this.buttonAbortiveClose.Size = new System.Drawing.Size(107, 23);
            this.buttonAbortiveClose.TabIndex = 5;
            this.buttonAbortiveClose.Text = "&Abortively Close";
            this.buttonAbortiveClose.UseVisualStyleBackColor = true;
            this.buttonAbortiveClose.Click += new System.EventHandler(this.buttonAbortiveClose_Click);
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 5000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // checkBoxKeepalive
            // 
            this.checkBoxKeepalive.AutoSize = true;
            this.checkBoxKeepalive.Checked = true;
            this.checkBoxKeepalive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxKeepalive.Location = new System.Drawing.Point(161, 98);
            this.checkBoxKeepalive.Name = "checkBoxKeepalive";
            this.checkBoxKeepalive.Size = new System.Drawing.Size(106, 17);
            this.checkBoxKeepalive.TabIndex = 18;
            this.checkBoxKeepalive.Text = "Send Keepalives";
            this.checkBoxKeepalive.UseVisualStyleBackColor = true;
            this.checkBoxKeepalive.CheckedChanged += new System.EventHandler(this.checkBoxKeepalive_CheckedChanged);
            // 
            // FormClientMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 236);
            this.Controls.Add(this.checkBoxKeepalive);
            this.Controls.Add(this.buttonAbortiveClose);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxIPAddress);
            this.Controls.Add(this.buttonSendComplexMessage);
            this.Controls.Add(this.buttonDisplayIP);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.textBoxMessage);
            this.Controls.Add(this.buttonSendMessage);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.statusStrip);
            this.Name = "FormClientMain";
            this.Text = "Nito TCP Client";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonDisplayIP;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.Button buttonSendMessage;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Button buttonSendComplexMessage;
        private System.Windows.Forms.TextBox textBoxIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Button buttonAbortiveClose;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.CheckBox checkBoxKeepalive;
    }
}

