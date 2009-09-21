namespace Server
{
    partial class FormServerMain
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
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonSendMessage = new System.Windows.Forms.Button();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonDisplayIP = new System.Windows.Forms.Button();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonAbortiveClose = new System.Windows.Forms.Button();
            this.buttonSendComplexMessage = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip.Location = new System.Drawing.Point(0, 310);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(469, 22);
            this.statusStrip.TabIndex = 0;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(51, 17);
            this.toolStripStatusLabel1.Text = "Stopped";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(81, 17);
            this.toolStripStatusLabel2.Text = "0 connections";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(150, 10);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(95, 23);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Start &Listening";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(251, 10);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(98, 23);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.Text = "Sto&p Listening";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonSendMessage
            // 
            this.buttonSendMessage.Enabled = false;
            this.buttonSendMessage.Location = new System.Drawing.Point(12, 68);
            this.buttonSendMessage.Name = "buttonSendMessage";
            this.buttonSendMessage.Size = new System.Drawing.Size(96, 23);
            this.buttonSendMessage.TabIndex = 7;
            this.buttonSendMessage.Text = "&Send Message";
            this.buttonSendMessage.UseVisualStyleBackColor = true;
            this.buttonSendMessage.Click += new System.EventHandler(this.buttonSendMessage_Click);
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMessage.Location = new System.Drawing.Point(114, 70);
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(343, 20);
            this.textBoxMessage.TabIndex = 8;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 126);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Size = new System.Drawing.Size(445, 181);
            this.textBoxLog.TabIndex = 10;
            // 
            // buttonDisplayIP
            // 
            this.buttonDisplayIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDisplayIP.Location = new System.Drawing.Point(382, 39);
            this.buttonDisplayIP.Name = "buttonDisplayIP";
            this.buttonDisplayIP.Size = new System.Drawing.Size(75, 23);
            this.buttonDisplayIP.TabIndex = 6;
            this.buttonDisplayIP.Text = "&Display IP";
            this.buttonDisplayIP.UseVisualStyleBackColor = true;
            this.buttonDisplayIP.Click += new System.EventHandler(this.buttonDisplayIP_Click);
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(44, 12);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(100, 20);
            this.textBoxPort.TabIndex = 1;
            this.textBoxPort.Text = "7750";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Port";
            // 
            // buttonAbortiveClose
            // 
            this.buttonAbortiveClose.Enabled = false;
            this.buttonAbortiveClose.Location = new System.Drawing.Point(133, 39);
            this.buttonAbortiveClose.Name = "buttonAbortiveClose";
            this.buttonAbortiveClose.Size = new System.Drawing.Size(132, 23);
            this.buttonAbortiveClose.TabIndex = 5;
            this.buttonAbortiveClose.Text = "&Abortively Close Clients";
            this.buttonAbortiveClose.UseVisualStyleBackColor = true;
            this.buttonAbortiveClose.Click += new System.EventHandler(this.buttonAbortiveClose_Click);
            // 
            // buttonSendComplexMessage
            // 
            this.buttonSendComplexMessage.Enabled = false;
            this.buttonSendComplexMessage.Location = new System.Drawing.Point(12, 97);
            this.buttonSendComplexMessage.Name = "buttonSendComplexMessage";
            this.buttonSendComplexMessage.Size = new System.Drawing.Size(142, 23);
            this.buttonSendComplexMessage.TabIndex = 9;
            this.buttonSendComplexMessage.Text = "Send Complex &Message";
            this.buttonSendComplexMessage.UseVisualStyleBackColor = true;
            this.buttonSendComplexMessage.Click += new System.EventHandler(this.buttonSendComplexMessage_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Enabled = false;
            this.buttonDisconnect.Location = new System.Drawing.Point(12, 39);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(115, 23);
            this.buttonDisconnect.TabIndex = 4;
            this.buttonDisconnect.Text = "&Disconnect Clients";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // FormServerMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 332);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.buttonSendComplexMessage);
            this.Controls.Add(this.buttonAbortiveClose);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonDisplayIP);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.textBoxMessage);
            this.Controls.Add(this.buttonSendMessage);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.statusStrip);
            this.Name = "FormServerMain";
            this.Text = "Nito TCP Server";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonSendMessage;
        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button buttonDisplayIP;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonAbortiveClose;
        private System.Windows.Forms.Button buttonSendComplexMessage;
        private System.Windows.Forms.Button buttonDisconnect;
    }
}

