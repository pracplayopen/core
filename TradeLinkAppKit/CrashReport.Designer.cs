namespace TradeLink.AppKit
{
    partial class CrashReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrashReport));
            this._send = new System.Windows.Forms.Button();
            this._user = new System.Windows.Forms.TextBox();
            this._pass = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._desc = new System.Windows.Forms.TextBox();
            this._body = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this._stat = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _send
            // 
            this._send.Location = new System.Drawing.Point(218, 28);
            this._send.Margin = new System.Windows.Forms.Padding(2);
            this._send.Name = "_send";
            this._send.Size = new System.Drawing.Size(60, 21);
            this._send.TabIndex = 0;
            this._send.Text = "send";
            this._send.UseVisualStyleBackColor = true;
            this._send.Click += new System.EventHandler(this._email_Click);
            // 
            // _user
            // 
            this._user.Location = new System.Drawing.Point(75, 5);
            this._user.Margin = new System.Windows.Forms.Padding(2);
            this._user.Name = "_user";
            this._user.Size = new System.Drawing.Size(68, 20);
            this._user.TabIndex = 2;
            this.toolTip1.SetToolTip(this._user, "Valid google account (double-click to create one)");
            this._user.DoubleClick += new System.EventHandler(this._user_DoubleClick);
            // 
            // _pass
            // 
            this._pass.Location = new System.Drawing.Point(210, 5);
            this._pass.Margin = new System.Windows.Forms.Padding(2);
            this._pass.Name = "_pass";
            this._pass.PasswordChar = '*';
            this._pass.Size = new System.Drawing.Size(68, 20);
            this._pass.TabIndex = 3;
            this._pass.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(148, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Password:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Description";
            // 
            // _desc
            // 
            this._desc.Location = new System.Drawing.Point(75, 28);
            this._desc.Margin = new System.Windows.Forms.Padding(2);
            this._desc.Name = "_desc";
            this._desc.Size = new System.Drawing.Size(129, 20);
            this._desc.TabIndex = 7;
            // 
            // _body
            // 
            this._body.Location = new System.Drawing.Point(14, 73);
            this._body.Margin = new System.Windows.Forms.Padding(2);
            this._body.Multiline = true;
            this._body.Name = "_body";
            this._body.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._body.Size = new System.Drawing.Size(439, 366);
            this._body.TabIndex = 8;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(39, 8);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(32, 13);
            this.linkLabel1.TabIndex = 9;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "User:";
            this.toolTip1.SetToolTip(this.linkLabel1, "Click to create a new google account if you don\'t have one.");
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // _stat
            // 
            this._stat.AutoSize = true;
            this._stat.Location = new System.Drawing.Point(72, 51);
            this._stat.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this._stat.Name = "_stat";
            this._stat.Size = new System.Drawing.Size(300, 13);
            this._stat.TabIndex = 10;
            this._stat.Text = "Please describe what you were doing when this error occured.";
            // 
            // CrashReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(464, 450);
            this.Controls.Add(this._stat);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this._body);
            this.Controls.Add(this._desc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._pass);
            this.Controls.Add(this._user);
            this.Controls.Add(this._send);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CrashReport";
            this.Text = "Tradelink.org Issue Report";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _send;
        private System.Windows.Forms.TextBox _user;
        private System.Windows.Forms.TextBox _pass;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _desc;
        private System.Windows.Forms.TextBox _body;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label _stat;
    }
}