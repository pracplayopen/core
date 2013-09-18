namespace TikConverter
{
    partial class GenericConvert
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenericConvert));
            this.button1 = new System.Windows.Forms.Button();
            this.filelist = new System.Windows.Forms.Label();
            this.inputfields = new System.Windows.Forms.ListBox();
            this.outputfields = new System.Windows.Forms.ListBox();
            this._stat = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ignore = new System.Windows.Forms.Button();
            this.moveup = new System.Windows.Forms.Button();
            this.movedown = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.importtype = new System.Windows.Forms.ComboBox();
            this.addfield = new System.Windows.Forms.Button();
            this.ignoreinvalid = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.convertexample = new System.Windows.Forms.ListBox();
            this.dupecol = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "select file(s)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // filelist
            // 
            this.filelist.AutoSize = true;
            this.filelist.Location = new System.Drawing.Point(111, 18);
            this.filelist.Name = "filelist";
            this.filelist.Size = new System.Drawing.Size(88, 13);
            this.filelist.TabIndex = 1;
            this.filelist.Text = "No files selected.";
            // 
            // inputfields
            // 
            this.inputfields.FormattingEnabled = true;
            this.inputfields.Location = new System.Drawing.Point(104, 72);
            this.inputfields.Name = "inputfields";
            this.inputfields.Size = new System.Drawing.Size(184, 238);
            this.inputfields.TabIndex = 2;
            this.toolTip1.SetToolTip(this.inputfields, "order your CSV columns to match up with tradelink fields.  To affect tradelink fi" +
        "elds, change type.");
            // 
            // outputfields
            // 
            this.outputfields.FormattingEnabled = true;
            this.outputfields.Location = new System.Drawing.Point(294, 72);
            this.outputfields.Name = "outputfields";
            this.outputfields.Size = new System.Drawing.Size(187, 238);
            this.outputfields.TabIndex = 3;
            this.toolTip1.SetToolTip(this.outputfields, "data that will make it to tradelink (change by changing type)");
            // 
            // _stat
            // 
            this._stat.AutoSize = true;
            this._stat.Location = new System.Drawing.Point(29, 319);
            this._stat.Name = "_stat";
            this._stat.Size = new System.Drawing.Size(130, 13);
            this._stat.TabIndex = 4;
            this._stat.Text = "Select a CSV file to begin.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(134, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "CSV columns";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(312, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Import to Tradelink fields";
            // 
            // ignore
            // 
            this.ignore.Location = new System.Drawing.Point(13, 186);
            this.ignore.Name = "ignore";
            this.ignore.Size = new System.Drawing.Size(85, 23);
            this.ignore.TabIndex = 7;
            this.ignore.Text = "ignore";
            this.ignore.UseVisualStyleBackColor = true;
            this.ignore.Click += new System.EventHandler(this.ignore_Click);
            // 
            // moveup
            // 
            this.moveup.Location = new System.Drawing.Point(13, 157);
            this.moveup.Name = "moveup";
            this.moveup.Size = new System.Drawing.Size(85, 23);
            this.moveup.TabIndex = 8;
            this.moveup.Text = "move up";
            this.moveup.UseVisualStyleBackColor = true;
            this.moveup.Click += new System.EventHandler(this.moveup_Click);
            // 
            // movedown
            // 
            this.movedown.Location = new System.Drawing.Point(13, 215);
            this.movedown.Name = "movedown";
            this.movedown.Size = new System.Drawing.Size(85, 23);
            this.movedown.TabIndex = 9;
            this.movedown.Text = "move down";
            this.movedown.UseVisualStyleBackColor = true;
            this.movedown.Click += new System.EventHandler(this.movedown_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(13, 287);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "Import";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // importtype
            // 
            this.importtype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.importtype.FormattingEnabled = true;
            this.importtype.Location = new System.Drawing.Point(12, 56);
            this.importtype.Name = "importtype";
            this.importtype.Size = new System.Drawing.Size(85, 21);
            this.importtype.TabIndex = 11;
            this.toolTip1.SetToolTip(this.importtype, "type of CSV data to import");
            this.importtype.SelectedIndexChanged += new System.EventHandler(this.importtype_SelectedIndexChanged);
            // 
            // addfield
            // 
            this.addfield.Location = new System.Drawing.Point(13, 128);
            this.addfield.Name = "addfield";
            this.addfield.Size = new System.Drawing.Size(85, 23);
            this.addfield.TabIndex = 12;
            this.addfield.Text = "add column";
            this.addfield.UseVisualStyleBackColor = true;
            this.addfield.Click += new System.EventHandler(this.addfield_Click);
            // 
            // ignoreinvalid
            // 
            this.ignoreinvalid.Appearance = System.Windows.Forms.Appearance.Button;
            this.ignoreinvalid.AutoSize = true;
            this.ignoreinvalid.Checked = true;
            this.ignoreinvalid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ignoreinvalid.Location = new System.Drawing.Point(12, 258);
            this.ignoreinvalid.Name = "ignoreinvalid";
            this.ignoreinvalid.Size = new System.Drawing.Size(79, 23);
            this.ignoreinvalid.TabIndex = 13;
            this.ignoreinvalid.Text = "ignore invalid";
            this.toolTip1.SetToolTip(this.ignoreinvalid, "imported bars and ticks that are invalid will be ignored");
            this.ignoreinvalid.UseVisualStyleBackColor = true;
            this.ignoreinvalid.CheckedChanged += new System.EventHandler(this.ignoreinvalid_CheckedChanged);
            // 
            // convertexample
            // 
            this.convertexample.FormattingEnabled = true;
            this.convertexample.Location = new System.Drawing.Point(487, 72);
            this.convertexample.Name = "convertexample";
            this.convertexample.Size = new System.Drawing.Size(187, 238);
            this.convertexample.TabIndex = 15;
            this.toolTip1.SetToolTip(this.convertexample, "Bars will show as 4 OHLC ticks");
            // 
            // dupecol
            // 
            this.dupecol.Location = new System.Drawing.Point(13, 99);
            this.dupecol.Name = "dupecol";
            this.dupecol.Size = new System.Drawing.Size(85, 23);
            this.dupecol.TabIndex = 14;
            this.dupecol.Text = "dupe column";
            this.dupecol.UseVisualStyleBackColor = true;
            this.dupecol.Click += new System.EventHandler(this.dupecol_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(496, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Sample Convert (Bar = 4 ticks)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Type";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Align";
            // 
            // GenericConvert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(697, 352);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.convertexample);
            this.Controls.Add(this.dupecol);
            this.Controls.Add(this.ignoreinvalid);
            this.Controls.Add(this.addfield);
            this.Controls.Add(this.importtype);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.movedown);
            this.Controls.Add(this.moveup);
            this.Controls.Add(this.ignore);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._stat);
            this.Controls.Add(this.outputfields);
            this.Controls.Add(this.inputfields);
            this.Controls.Add(this.filelist);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GenericConvert";
            this.Text = "Convert CSVs of SAME TYPE ONLY";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label filelist;
        private System.Windows.Forms.ListBox inputfields;
        private System.Windows.Forms.ListBox outputfields;
        private System.Windows.Forms.Label _stat;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ignore;
        private System.Windows.Forms.Button moveup;
        private System.Windows.Forms.Button movedown;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox importtype;
        private System.Windows.Forms.Button addfield;
        private System.Windows.Forms.CheckBox ignoreinvalid;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button dupecol;
        private System.Windows.Forms.ListBox convertexample;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}