namespace Client.Forms
{
    partial class FormConnect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConnect));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TextBox_Password = new System.Windows.Forms.TextBox();
            this.IdBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ButtonConnect = new System.Windows.Forms.Button();
            this.hostButton = new System.Windows.Forms.Button();
            this.ColorDepthBox = new System.Windows.Forms.TextBox();
            this.ColorDepthButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.VPNConnect = new System.Windows.Forms.Button();
            this.statusUpdateBox = new System.Windows.Forms.RichTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(56, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password:";
            // 
            // TextBox_Password
            // 
            this.TextBox_Password.Location = new System.Drawing.Point(87, 104);
            this.TextBox_Password.Name = "TextBox_Password";
            this.TextBox_Password.Size = new System.Drawing.Size(152, 25);
            this.TextBox_Password.TabIndex = 5;
            // 
            // IdBox
            // 
            this.IdBox.Location = new System.Drawing.Point(87, 67);
            this.IdBox.Name = "IdBox";
            this.IdBox.Size = new System.Drawing.Size(152, 25);
            this.IdBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(153)))));
            this.label3.Location = new System.Drawing.Point(70, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(256, 30);
            this.label3.TabIndex = 7;
            this.label3.Text = "Connect to a Server/Client";
            // 
            // ButtonConnect
            // 
            this.ButtonConnect.Location = new System.Drawing.Point(87, 135);
            this.ButtonConnect.Name = "ButtonConnect";
            this.ButtonConnect.Size = new System.Drawing.Size(73, 27);
            this.ButtonConnect.TabIndex = 8;
            this.ButtonConnect.Text = "Connect";
            this.ButtonConnect.UseVisualStyleBackColor = true;
            this.ButtonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // hostButton
            // 
            this.hostButton.Location = new System.Drawing.Point(166, 135);
            this.hostButton.Name = "hostButton";
            this.hostButton.Size = new System.Drawing.Size(73, 27);
            this.hostButton.TabIndex = 9;
            this.hostButton.Text = "Host";
            this.hostButton.UseVisualStyleBackColor = true;
            this.hostButton.Click += new System.EventHandler(this.hostButton_Click);
            // 
            // ColorDepthBox
            // 
            this.ColorDepthBox.Location = new System.Drawing.Point(87, 195);
            this.ColorDepthBox.Name = "ColorDepthBox";
            this.ColorDepthBox.Size = new System.Drawing.Size(73, 25);
            this.ColorDepthBox.TabIndex = 10;
            this.ColorDepthBox.Text = "8";
            // 
            // ColorDepthButton
            // 
            this.ColorDepthButton.Location = new System.Drawing.Point(87, 229);
            this.ColorDepthButton.Name = "ColorDepthButton";
            this.ColorDepthButton.Size = new System.Drawing.Size(73, 27);
            this.ColorDepthButton.TabIndex = 11;
            this.ColorDepthButton.Text = "Update";
            this.ColorDepthButton.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 198);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "Color Depth";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(153)))));
            this.label5.Location = new System.Drawing.Point(358, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(225, 30);
            this.label5.TabIndex = 13;
            this.label5.Text = "Connect VPN for WAN";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(431, 99);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(152, 25);
            this.textBox1.TabIndex = 17;
            this.textBox1.Text = "Casper123";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(431, 62);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(152, 25);
            this.textBox2.TabIndex = 16;
            this.textBox2.Text = "vpn1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(356, 99);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 17);
            this.label6.TabIndex = 15;
            this.label6.Text = "VPN Pwd:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(356, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 17);
            this.label7.TabIndex = 14;
            this.label7.Text = "VPN User:";
            // 
            // VPNConnect
            // 
            this.VPNConnect.Location = new System.Drawing.Point(431, 135);
            this.VPNConnect.Name = "VPNConnect";
            this.VPNConnect.Size = new System.Drawing.Size(73, 27);
            this.VPNConnect.TabIndex = 18;
            this.VPNConnect.Text = "Connect";
            this.VPNConnect.UseVisualStyleBackColor = true;
            this.VPNConnect.Click += new System.EventHandler(this.VPNConnect_Click);
            // 
            // statusUpdateBox
            // 
            this.statusUpdateBox.BackColor = System.Drawing.SystemColors.Info;
            this.statusUpdateBox.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.statusUpdateBox.Location = new System.Drawing.Point(242, 185);
            this.statusUpdateBox.Name = "statusUpdateBox";
            this.statusUpdateBox.Size = new System.Drawing.Size(181, 96);
            this.statusUpdateBox.TabIndex = 19;
            this.statusUpdateBox.Text = "";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(274, 163);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(106, 17);
            this.label8.TabIndex = 20;
            this.label8.Text = "Status Messages";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(2, 215);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 17);
            this.label9.TabIndex = 21;
            this.label9.Text = "4/8/16/24/32";
            // 
            // FormConnect
            // 
            this.AcceptButton = this.ButtonConnect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 293);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.statusUpdateBox);
            this.Controls.Add(this.VPNConnect);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ColorDepthButton);
            this.Controls.Add(this.ColorDepthBox);
            this.Controls.Add(this.hostButton);
            this.Controls.Add(this.ButtonConnect);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TextBox_Password);
            this.Controls.Add(this.IdBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "FormConnect";
            this.Text = "Remote Desktop - Connect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TextBox_Password;
        private System.Windows.Forms.TextBox IdBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ButtonConnect;
        private System.Windows.Forms.Button hostButton;
        private System.Windows.Forms.TextBox ColorDepthBox;
        private System.Windows.Forms.Button ColorDepthButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button VPNConnect;
        private System.Windows.Forms.RichTextBox statusUpdateBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
    }
}