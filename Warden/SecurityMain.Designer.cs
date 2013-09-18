namespace Warden
{
    partial class SecurityMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecurityMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.help = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this._s3bucket = new System.Windows.Forms.TextBox();
            this.sync = new System.Windows.Forms.Button();
            this.okbut = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.export = new System.Windows.Forms.Button();
            this.import = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.stat = new System.Windows.Forms.Label();
            this.delbut = new System.Windows.Forms.Button();
            this._program = new System.Windows.Forms.TextBox();
            this._awsid = new System.Windows.Forms.TextBox();
            this.addbut = new System.Windows.Forms.Button();
            this._awskey = new System.Windows.Forms.TextBox();
            this._user = new System.Windows.Forms.TextBox();
            this._regcode = new System.Windows.Forms.TextBox();
            this.acllist = new System.Windows.Forms.ListBox();
            this.testurl = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.testurl);
            this.splitContainer1.Panel1.Controls.Add(this.help);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this._s3bucket);
            this.splitContainer1.Panel1.Controls.Add(this.sync);
            this.splitContainer1.Panel1.Controls.Add(this.okbut);
            this.splitContainer1.Panel1.Controls.Add(this.button2);
            this.splitContainer1.Panel1.Controls.Add(this.export);
            this.splitContainer1.Panel1.Controls.Add(this.import);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.stat);
            this.splitContainer1.Panel1.Controls.Add(this.delbut);
            this.splitContainer1.Panel1.Controls.Add(this._program);
            this.splitContainer1.Panel1.Controls.Add(this._awsid);
            this.splitContainer1.Panel1.Controls.Add(this.addbut);
            this.splitContainer1.Panel1.Controls.Add(this._awskey);
            this.splitContainer1.Panel1.Controls.Add(this._user);
            this.splitContainer1.Panel1.Controls.Add(this._regcode);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.acllist);
            this.splitContainer1.Size = new System.Drawing.Size(468, 358);
            this.splitContainer1.SplitterDistance = 156;
            this.splitContainer1.TabIndex = 0;
            // 
            // help
            // 
            this.help.Image = ((System.Drawing.Image)(resources.GetObject("help.Image")));
            this.help.Location = new System.Drawing.Point(432, 128);
            this.help.Name = "help";
            this.help.Size = new System.Drawing.Size(24, 23);
            this.help.TabIndex = 22;
            this.help.UseVisualStyleBackColor = true;
            this.help.Click += new System.EventHandler(this.help_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 7);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "aws bucket";
            // 
            // _s3bucket
            // 
            this._s3bucket.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Warden.Properties.Settings.Default, "s3baseurl", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._s3bucket.Location = new System.Drawing.Point(15, 23);
            this._s3bucket.Name = "_s3bucket";
            this._s3bucket.Size = new System.Drawing.Size(110, 20);
            this._s3bucket.TabIndex = 20;
            this._s3bucket.Text = global::Warden.Properties.Settings.Default.s3baseurl;
            // 
            // sync
            // 
            this.sync.Location = new System.Drawing.Point(346, 57);
            this.sync.Name = "sync";
            this.sync.Size = new System.Drawing.Size(110, 23);
            this.sync.TabIndex = 19;
            this.sync.Text = "get users";
            this.sync.UseVisualStyleBackColor = true;
            this.sync.Click += new System.EventHandler(this.sync_Click);
            // 
            // okbut
            // 
            this.okbut.Location = new System.Drawing.Point(103, 96);
            this.okbut.Name = "okbut";
            this.okbut.Size = new System.Drawing.Size(33, 23);
            this.okbut.TabIndex = 18;
            this.okbut.Text = "ok";
            this.toolTip1.SetToolTip(this.okbut, "accept the current user/regcode entry and add to list (may need to delete old ent" +
        "ry)");
            this.okbut.UseVisualStyleBackColor = true;
            this.okbut.Click += new System.EventHandler(this.okbut_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(346, 86);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(110, 23);
            this.button2.TabIndex = 17;
            this.button2.Text = "apply";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // export
            // 
            this.export.Location = new System.Drawing.Point(404, 12);
            this.export.Name = "export";
            this.export.Size = new System.Drawing.Size(52, 23);
            this.export.TabIndex = 16;
            this.export.Text = "export";
            this.export.UseVisualStyleBackColor = true;
            this.export.Click += new System.EventHandler(this.export_Click);
            // 
            // import
            // 
            this.import.Location = new System.Drawing.Point(346, 12);
            this.import.Name = "import";
            this.import.Size = new System.Drawing.Size(52, 23);
            this.import.TabIndex = 15;
            this.import.Text = "import";
            this.import.UseVisualStyleBackColor = true;
            this.import.Click += new System.EventHandler(this.import_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(277, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "regcode";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(170, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "user";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(277, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "awskey";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(170, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "awsid";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Folder/User List:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(401, 128);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(25, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // stat
            // 
            this.stat.AutoSize = true;
            this.stat.Location = new System.Drawing.Point(12, 128);
            this.stat.Name = "stat";
            this.stat.Size = new System.Drawing.Size(147, 13);
            this.stat.TabIndex = 8;
            this.stat.Text = "The warden will see you now.";
            // 
            // delbut
            // 
            this.delbut.Location = new System.Drawing.Point(56, 96);
            this.delbut.Name = "delbut";
            this.delbut.Size = new System.Drawing.Size(41, 23);
            this.delbut.TabIndex = 7;
            this.delbut.Text = "del";
            this.toolTip1.SetToolTip(this.delbut, "delete the selected ACL from list below");
            this.delbut.UseVisualStyleBackColor = true;
            this.delbut.Click += new System.EventHandler(this.delbut_Click);
            // 
            // _program
            // 
            this._program.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._program.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._program.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Warden.Properties.Settings.Default, "program", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._program.Location = new System.Drawing.Point(108, 57);
            this._program.Name = "_program";
            this._program.Size = new System.Drawing.Size(172, 20);
            this._program.TabIndex = 0;
            this._program.Text = global::Warden.Properties.Settings.Default.program;
            this._program.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this._program.Leave += new System.EventHandler(this.textBox1_Leave);
            // 
            // _awsid
            // 
            this._awsid.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Warden.Properties.Settings.Default, "accessid", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._awsid.Location = new System.Drawing.Point(140, 23);
            this._awsid.Name = "_awsid";
            this._awsid.Size = new System.Drawing.Size(98, 20);
            this._awsid.TabIndex = 1;
            this._awsid.Text = global::Warden.Properties.Settings.Default.accessid;
            // 
            // addbut
            // 
            this.addbut.Location = new System.Drawing.Point(12, 96);
            this.addbut.Name = "addbut";
            this.addbut.Size = new System.Drawing.Size(38, 23);
            this.addbut.TabIndex = 5;
            this.addbut.Text = "add";
            this.toolTip1.SetToolTip(this.addbut, "clear current user/regcode entry in preperation for a new entry");
            this.addbut.UseVisualStyleBackColor = true;
            this.addbut.Click += new System.EventHandler(this.addbut_Click);
            // 
            // _awskey
            // 
            this._awskey.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Warden.Properties.Settings.Default, "accesskey", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._awskey.Location = new System.Drawing.Point(244, 23);
            this._awskey.Name = "_awskey";
            this._awskey.PasswordChar = '*';
            this._awskey.Size = new System.Drawing.Size(96, 20);
            this._awskey.TabIndex = 2;
            this._awskey.Text = global::Warden.Properties.Settings.Default.accesskey;
            this._awskey.UseSystemPasswordChar = true;
            // 
            // _user
            // 
            this._user.Location = new System.Drawing.Point(140, 96);
            this._user.Name = "_user";
            this._user.Size = new System.Drawing.Size(98, 20);
            this._user.TabIndex = 4;
            // 
            // _regcode
            // 
            this._regcode.Location = new System.Drawing.Point(244, 96);
            this._regcode.Name = "_regcode";
            this._regcode.Size = new System.Drawing.Size(98, 20);
            this._regcode.TabIndex = 3;
            // 
            // acllist
            // 
            this.acllist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.acllist.FormattingEnabled = true;
            this.acllist.Location = new System.Drawing.Point(0, 0);
            this.acllist.Name = "acllist";
            this.acllist.Size = new System.Drawing.Size(468, 198);
            this.acllist.TabIndex = 0;
            this.acllist.SelectedIndexChanged += new System.EventHandler(this.acllist_SelectedIndexChanged);
            // 
            // testurl
            // 
            this.testurl.Location = new System.Drawing.Point(346, 128);
            this.testurl.Name = "testurl";
            this.testurl.Size = new System.Drawing.Size(49, 23);
            this.testurl.TabIndex = 23;
            this.testurl.Text = "test";
            this.toolTip1.SetToolTip(this.testurl, "verify your program can read this url");
            this.testurl.UseVisualStyleBackColor = true;
            this.testurl.Click += new System.EventHandler(this.testurl_Click);
            // 
            // SecurityMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(468, 358);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SecurityMain";
            this.Text = "Warden";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox _awskey;
        private System.Windows.Forms.TextBox _awsid;
        private System.Windows.Forms.TextBox _program;
        private System.Windows.Forms.Button delbut;
        private System.Windows.Forms.Button addbut;
        private System.Windows.Forms.TextBox _user;
        private System.Windows.Forms.TextBox _regcode;
        private System.Windows.Forms.Label stat;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox acllist;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button export;
        private System.Windows.Forms.Button import;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button okbut;
        private System.Windows.Forms.Button sync;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox _s3bucket;
        private System.Windows.Forms.Button help;
        private System.Windows.Forms.Button testurl;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

