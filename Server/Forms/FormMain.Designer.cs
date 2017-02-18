namespace Server.Forms
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.LabelNovaId = new System.Windows.Forms.Label();
            this.LabelPassword = new System.Windows.Forms.Label();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.TrayIcon_ContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Timer_KeepAlive = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.VpnUserBox = new System.Windows.Forms.TextBox();
            this.VPNPasswordBox = new System.Windows.Forms.TextBox();
            this.VPNConnect = new System.Windows.Forms.Button();
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.StatusVPNLabel = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button3 = new System.Windows.Forms.Button();
            this.DisconnectVPNBtn = new System.Windows.Forms.Button();
            this.TrayIcon_ContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Computer ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.LightGray;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Password:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.LightGray;
            this.label3.Location = new System.Drawing.Point(12, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Status:";
            // 
            // LabelNovaId
            // 
            this.LabelNovaId.AutoSize = true;
            this.LabelNovaId.Location = new System.Drawing.Point(103, 14);
            this.LabelNovaId.Name = "LabelNovaId";
            this.LabelNovaId.Size = new System.Drawing.Size(0, 17);
            this.LabelNovaId.TabIndex = 4;
            // 
            // LabelPassword
            // 
            this.LabelPassword.AutoSize = true;
            this.LabelPassword.Location = new System.Drawing.Point(85, 44);
            this.LabelPassword.Name = "LabelPassword";
            this.LabelPassword.Size = new System.Drawing.Size(0, 17);
            this.LabelPassword.TabIndex = 5;
            // 
            // LabelStatus
            // 
            this.LabelStatus.AutoSize = true;
            this.LabelStatus.Location = new System.Drawing.Point(64, 74);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(0, 17);
            this.LabelStatus.TabIndex = 6;
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.TrayIcon_ContextMenu;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "Nova Remote Assistance Tool";
            this.TrayIcon.Visible = true;
            // 
            // TrayIcon_ContextMenu
            // 
            this.TrayIcon_ContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.TrayIcon_ContextMenu.Name = "TrayIcon_ContextMenu";
            this.TrayIcon_ContextMenu.Size = new System.Drawing.Size(93, 26);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // Timer_KeepAlive
            // 
            this.Timer_KeepAlive.Enabled = true;
            this.Timer_KeepAlive.Tick += new System.EventHandler(this.Timer_KeepAlive_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 102);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(96, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Host";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.StartHosting);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(12, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "VPN User:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.LightGray;
            this.label5.Location = new System.Drawing.Point(12, 171);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 17);
            this.label5.TabIndex = 9;
            this.label5.Text = "VPN Password";
            // 
            // VpnUserBox
            // 
            this.VpnUserBox.Location = new System.Drawing.Point(111, 133);
            this.VpnUserBox.Name = "VpnUserBox";
            this.VpnUserBox.Size = new System.Drawing.Size(100, 25);
            this.VpnUserBox.TabIndex = 10;
            this.VpnUserBox.Text = "vpn1";
            // 
            // VPNPasswordBox
            // 
            this.VPNPasswordBox.Location = new System.Drawing.Point(111, 164);
            this.VPNPasswordBox.Name = "VPNPasswordBox";
            this.VPNPasswordBox.Size = new System.Drawing.Size(100, 25);
            this.VPNPasswordBox.TabIndex = 11;
            this.VPNPasswordBox.Text = "Casper123";
            // 
            // VPNConnect
            // 
            this.VPNConnect.Location = new System.Drawing.Point(9, 227);
            this.VPNConnect.Name = "VPNConnect";
            this.VPNConnect.Size = new System.Drawing.Size(96, 23);
            this.VPNConnect.TabIndex = 12;
            this.VPNConnect.Text = "Connect VPN";
            this.VPNConnect.UseVisualStyleBackColor = true;
            this.VPNConnect.Click += new System.EventHandler(this.OnConnectVPN);
            // 
            // StatusVPNLabel
            // 
            this.StatusVPNLabel.AutoSize = true;
            this.StatusVPNLabel.BackColor = System.Drawing.Color.LightGray;
            this.StatusVPNLabel.Location = new System.Drawing.Point(12, 200);
            this.StatusVPNLabel.Name = "StatusVPNLabel";
            this.StatusVPNLabel.Size = new System.Drawing.Size(46, 17);
            this.StatusVPNLabel.TabIndex = 13;
            this.StatusVPNLabel.Text = "Status:";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "4",
            "8",
            "16",
            "32"});
            this.comboBox1.Location = new System.Drawing.Point(269, 14);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 25);
            this.comboBox1.TabIndex = 14;
            this.comboBox1.Text = "8";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.LightSlateGray;
            this.button3.Location = new System.Drawing.Point(269, 45);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(121, 46);
            this.button3.TabIndex = 15;
            this.button3.Text = "Update Color Depth";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.OnUpdateColor);
            // 
            // DisconnectVPNBtn
            // 
            this.DisconnectVPNBtn.Location = new System.Drawing.Point(115, 225);
            this.DisconnectVPNBtn.Name = "DisconnectVPNBtn";
            this.DisconnectVPNBtn.Size = new System.Drawing.Size(96, 23);
            this.DisconnectVPNBtn.TabIndex = 16;
            this.DisconnectVPNBtn.Text = "DisConnect VPN";
            this.DisconnectVPNBtn.UseVisualStyleBackColor = true;
            this.DisconnectVPNBtn.Click += new System.EventHandler(this.DisconnectVPNBtn_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(402, 260);
            this.Controls.Add(this.DisconnectVPNBtn);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.StatusVPNLabel);
            this.Controls.Add(this.VPNConnect);
            this.Controls.Add(this.VPNPasswordBox);
            this.Controls.Add(this.VpnUserBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.LabelStatus);
            this.Controls.Add(this.LabelPassword);
            this.Controls.Add(this.LabelNovaId);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "Remote Desktop Server";
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.TrayIcon_ContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label LabelNovaId;
        private System.Windows.Forms.Label LabelPassword;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenuStrip TrayIcon_ContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Timer Timer_KeepAlive;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox VpnUserBox;
        private System.Windows.Forms.TextBox VPNPasswordBox;
        private System.Windows.Forms.Button VPNConnect;
        private System.Windows.Forms.Label StatusVPNLabel;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button3;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.Button DisconnectVPNBtn;
    }
}