namespace ServerEsignal
{
    partial class EsignalMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EsignalMain));
            this.label1 = new System.Windows.Forms.Label();
            this._acctapp = new System.Windows.Forms.TextBox();
            this._ok = new System.Windows.Forms.Button();
            this._msg = new System.Windows.Forms.Button();
            this._report = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Account/App:";
            // 
            // _acctapp
            // 
            this._acctapp.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ServerEsignal.Properties.Settings.Default, "accountorappname", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._acctapp.Location = new System.Drawing.Point(86, 22);
            this._acctapp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._acctapp.Name = "_acctapp";
            this._acctapp.Size = new System.Drawing.Size(133, 20);
            this._acctapp.TabIndex = 0;
            this._acctapp.Text = global::ServerEsignal.Properties.Settings.Default.accountorappname;
            // 
            // _ok
            // 
            this._ok.Location = new System.Drawing.Point(107, 44);
            this._ok.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._ok.Name = "_ok";
            this._ok.Size = new System.Drawing.Size(48, 29);
            this._ok.TabIndex = 2;
            this._ok.Text = "ok";
            this._ok.UseVisualStyleBackColor = true;
            this._ok.Click += new System.EventHandler(this._ok_Click);
            // 
            // _msg
            // 
            this._msg.Location = new System.Drawing.Point(159, 44);
            this._msg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._msg.Name = "_msg";
            this._msg.Size = new System.Drawing.Size(27, 29);
            this._msg.TabIndex = 3;
            this._msg.Text = "!";
            this._msg.UseVisualStyleBackColor = true;
            this._msg.Click += new System.EventHandler(this._msg_Click);
            // 
            // _report
            // 
            this._report.Image = global::ServerEsignal.Properties.Resources.bug;
            this._report.Location = new System.Drawing.Point(190, 44);
            this._report.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._report.Name = "_report";
            this._report.Size = new System.Drawing.Size(29, 29);
            this._report.TabIndex = 4;
            this._report.UseVisualStyleBackColor = true;
            this._report.Click += new System.EventHandler(this._report_Click);
            // 
            // EsignalMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(230, 100);
            this.Controls.Add(this._report);
            this.Controls.Add(this._msg);
            this.Controls.Add(this._ok);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._acctapp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "EsignalMain";
            this.Text = "Esignal Connector";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _acctapp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.Button _msg;
        private System.Windows.Forms.Button _report;
    }
}

