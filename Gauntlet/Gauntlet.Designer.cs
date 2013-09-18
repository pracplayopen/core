namespace WinGauntlet
{
    partial class Gauntlet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Gauntlet));
            this.button1 = new System.Windows.Forms.Button();
            this.tabs = new System.Windows.Forms.TabControl();
            this.studypage = new System.Windows.Forms.TabPage();
            this.button5 = new System.Windows.Forms.Button();
            this._viewresults = new System.Windows.Forms.Button();
            this._twithelp = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.lastmessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.queuebut = new System.Windows.Forms.Button();
            this.reslist = new System.Windows.Forms.ListBox();
            this.tickFileFilterControl1 = new TradeLink.AppKit.TickFileFilterControl();
            this.optionpage = new System.Windows.Forms.TabPage();
            this._siminmemory = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this._indicatcsv = new System.Windows.Forms.CheckBox();
            this._debugfile = new System.Windows.Forms.CheckBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.savesettings = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this._debugs = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this._initialequity = new System.Windows.Forms.NumericUpDown();
            this._usehighliquidityEOD = new System.Windows.Forms.CheckBox();
            this.resetonrun = new System.Windows.Forms.CheckBox();
            this._portfoliosim = new System.Windows.Forms.CheckBox();
            this._usebidask = new System.Windows.Forms.CheckBox();
            this._unique = new System.Windows.Forms.CheckBox();
            this.ordersincsv = new System.Windows.Forms.CheckBox();
            this.messagewrite = new System.Windows.Forms.CheckBox();
            this.clearmessages = new System.Windows.Forms.CheckBox();
            this.saveonexit = new System.Windows.Forms.CheckBox();
            this.optimizetab = new System.Windows.Forms.TabPage();
            this._optstat = new System.Windows.Forms.Label();
            this.startoptimize = new System.Windows.Forms.Button();
            this.refreshoptimizechoices = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.higherisoptimimal = new System.Windows.Forms.CheckBox();
            this._optdecision = new System.Windows.Forms.ComboBox();
            this._optinc = new System.Windows.Forms.NumericUpDown();
            this._optstop = new System.Windows.Forms.NumericUpDown();
            this._optstart = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this._optimizename = new System.Windows.Forms.ComboBox();
            this.equitycurvetab = new System.Windows.Forms.TabPage();
            this.equitychart = new TradeLink.AppKit.ChartControl();
            this._resulttab = new System.Windows.Forms.TabPage();
            this.tradeResults1 = new TradeLink.AppKit.TradeResults();
            this.messagepage = new System.Windows.Forms.TabPage();
            this.messages = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabs.SuspendLayout();
            this.studypage.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.optionpage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._initialequity)).BeginInit();
            this.optimizetab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._optinc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._optstop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._optstart)).BeginInit();
            this.equitycurvetab.SuspendLayout();
            this._resulttab.SuspendLayout();
            this.messagepage.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(228, 66);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(21, 18);
            this.button1.TabIndex = 0;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.studypage);
            this.tabs.Controls.Add(this.optionpage);
            this.tabs.Controls.Add(this.optimizetab);
            this.tabs.Controls.Add(this.equitycurvetab);
            this.tabs.Controls.Add(this._resulttab);
            this.tabs.Controls.Add(this.messagepage);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(613, 300);
            this.tabs.TabIndex = 1;
            // 
            // studypage
            // 
            this.studypage.BackColor = System.Drawing.SystemColors.Window;
            this.studypage.Controls.Add(this.button5);
            this.studypage.Controls.Add(this._viewresults);
            this.studypage.Controls.Add(this._twithelp);
            this.studypage.Controls.Add(this.statusStrip1);
            this.studypage.Controls.Add(this.queuebut);
            this.studypage.Controls.Add(this.reslist);
            this.studypage.Controls.Add(this.tickFileFilterControl1);
            this.studypage.Location = new System.Drawing.Point(4, 22);
            this.studypage.Name = "studypage";
            this.studypage.Padding = new System.Windows.Forms.Padding(3);
            this.studypage.Size = new System.Drawing.Size(605, 274);
            this.studypage.TabIndex = 0;
            this.studypage.Text = "Studies";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(8, 6);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(71, 23);
            this.button5.TabIndex = 21;
            this.button5.Text = "Responses";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button2_Click);
            // 
            // _viewresults
            // 
            this._viewresults.Location = new System.Drawing.Point(322, 201);
            this._viewresults.Margin = new System.Windows.Forms.Padding(2);
            this._viewresults.Name = "_viewresults";
            this._viewresults.Size = new System.Drawing.Size(127, 32);
            this._viewresults.TabIndex = 20;
            this._viewresults.Text = "Raw Results Folder";
            this.toolTip1.SetToolTip(this._viewresults, "View Results Folder");
            this._viewresults.UseVisualStyleBackColor = true;
            this._viewresults.Click += new System.EventHandler(this._viewresults_Click);
            // 
            // _twithelp
            // 
            this._twithelp.Image = ((System.Drawing.Image)(resources.GetObject("_twithelp.Image")));
            this._twithelp.Location = new System.Drawing.Point(496, 201);
            this._twithelp.Margin = new System.Windows.Forms.Padding(2);
            this._twithelp.Name = "_twithelp";
            this._twithelp.Size = new System.Drawing.Size(31, 32);
            this._twithelp.TabIndex = 19;
            this.toolTip1.SetToolTip(this._twithelp, "get help");
            this._twithelp.UseVisualStyleBackColor = true;
            this._twithelp.Click += new System.EventHandler(this._twithelp_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.White;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ProgressBar1,
            this.lastmessage});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(3, 246);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(599, 25);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Enabled = false;
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(100, 19);
            // 
            // lastmessage
            // 
            this.lastmessage.Name = "lastmessage";
            this.lastmessage.Size = new System.Drawing.Size(80, 20);
            this.lastmessage.Text = "No active runs.";
            // 
            // queuebut
            // 
            this.queuebut.Location = new System.Drawing.Point(112, 201);
            this.queuebut.Name = "queuebut";
            this.queuebut.Size = new System.Drawing.Size(205, 32);
            this.queuebut.TabIndex = 4;
            this.queuebut.Text = "Run the Gauntlet";
            this.toolTip1.SetToolTip(this.queuebut, "start the backtesting run");
            this.queuebut.UseVisualStyleBackColor = true;
            this.queuebut.Click += new System.EventHandler(this.queuebut_Click);
            // 
            // reslist
            // 
            this.reslist.ColumnWidth = 300;
            this.reslist.FormattingEnabled = true;
            this.reslist.Location = new System.Drawing.Point(8, 35);
            this.reslist.MultiColumn = true;
            this.reslist.Name = "reslist";
            this.reslist.Size = new System.Drawing.Size(183, 160);
            this.reslist.TabIndex = 2;
            this.toolTip1.SetToolTip(this.reslist, "select response to trade");
            this.reslist.SelectedIndexChanged += new System.EventHandler(this.boxlist_SelectedIndexChanged);
            // 
            // tickFileFilterControl1
            // 
            this.tickFileFilterControl1.Location = new System.Drawing.Point(195, 6);
            this.tickFileFilterControl1.Margin = new System.Windows.Forms.Padding(1);
            this.tickFileFilterControl1.Name = "tickFileFilterControl1";
            this.tickFileFilterControl1.Size = new System.Drawing.Size(405, 189);
            this.tickFileFilterControl1.TabIndex = 18;
            // 
            // optionpage
            // 
            this.optionpage.BackColor = System.Drawing.SystemColors.Window;
            this.optionpage.Controls.Add(this._siminmemory);
            this.optionpage.Controls.Add(this.label3);
            this.optionpage.Controls.Add(this._indicatcsv);
            this.optionpage.Controls.Add(this._debugfile);
            this.optionpage.Controls.Add(this.button4);
            this.optionpage.Controls.Add(this.button3);
            this.optionpage.Controls.Add(this.savesettings);
            this.optionpage.Controls.Add(this.label2);
            this.optionpage.Controls.Add(this.button2);
            this.optionpage.Controls.Add(this._debugs);
            this.optionpage.Controls.Add(this.label1);
            this.optionpage.Controls.Add(this.button1);
            this.optionpage.Controls.Add(this._initialequity);
            this.optionpage.Controls.Add(this._usehighliquidityEOD);
            this.optionpage.Controls.Add(this.resetonrun);
            this.optionpage.Controls.Add(this._portfoliosim);
            this.optionpage.Controls.Add(this._usebidask);
            this.optionpage.Controls.Add(this._unique);
            this.optionpage.Controls.Add(this.ordersincsv);
            this.optionpage.Controls.Add(this.messagewrite);
            this.optionpage.Controls.Add(this.clearmessages);
            this.optionpage.Controls.Add(this.saveonexit);
            this.optionpage.Location = new System.Drawing.Point(4, 22);
            this.optionpage.Name = "optionpage";
            this.optionpage.Padding = new System.Windows.Forms.Padding(3);
            this.optionpage.Size = new System.Drawing.Size(534, 342);
            this.optionpage.TabIndex = 1;
            this.optionpage.Text = "Options";
            // 
            // _siminmemory
            // 
            this._siminmemory.AutoSize = true;
            this._siminmemory.Checked = global::WinGauntlet.Properties.Settings.Default.isSimRunInMemory;
            this._siminmemory.CheckState = System.Windows.Forms.CheckState.Checked;
            this._siminmemory.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "isSimRunInMemory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._siminmemory.Location = new System.Drawing.Point(28, 231);
            this._siminmemory.Name = "_siminmemory";
            this._siminmemory.Size = new System.Drawing.Size(178, 17);
            this._siminmemory.TabIndex = 37;
            this._siminmemory.Text = "Sim in Memory (faster if possible)";
            this._siminmemory.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(279, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 36;
            this.label3.Text = "Starting Equity";
            // 
            // _indicatcsv
            // 
            this._indicatcsv.AutoSize = true;
            this._indicatcsv.Location = new System.Drawing.Point(228, 145);
            this._indicatcsv.Margin = new System.Windows.Forms.Padding(2);
            this._indicatcsv.Name = "_indicatcsv";
            this._indicatcsv.Size = new System.Drawing.Size(96, 17);
            this._indicatcsv.TabIndex = 28;
            this._indicatcsv.Text = "Indicators CSV";
            this.toolTip1.SetToolTip(this._indicatcsv, "save indicators to log for analysis");
            this._indicatcsv.UseVisualStyleBackColor = true;
            // 
            // _debugfile
            // 
            this._debugfile.AutoSize = true;
            this._debugfile.Location = new System.Drawing.Point(228, 123);
            this._debugfile.Margin = new System.Windows.Forms.Padding(2);
            this._debugfile.Name = "_debugfile";
            this._debugfile.Size = new System.Drawing.Size(118, 17);
            this._debugfile.TabIndex = 27;
            this._debugfile.Text = "Messages in log file";
            this.toolTip1.SetToolTip(this._debugfile, "Saves messages to a text file for review");
            this._debugfile.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(54, 6);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(51, 23);
            this.button4.TabIndex = 20;
            this.button4.Text = "Discard";
            this.toolTip1.SetToolTip(this.button4, "discard changes made since last save");
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(111, 6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(61, 23);
            this.button3.TabIndex = 19;
            this.button3.Text = "Defaults";
            this.toolTip1.SetToolTip(this.button3, "return to gauntlet default values");
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // savesettings
            // 
            this.savesettings.Location = new System.Drawing.Point(8, 6);
            this.savesettings.Name = "savesettings";
            this.savesettings.Size = new System.Drawing.Size(42, 23);
            this.savesettings.TabIndex = 13;
            this.savesettings.Text = "Save";
            this.toolTip1.SetToolTip(this.savesettings, "save these options");
            this.savesettings.UseVisualStyleBackColor = true;
            this.savesettings.Click += new System.EventHandler(this.savesettings_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(46, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Select Response DLL";
            this.toolTip1.SetToolTip(this.label2, "select response library used to populate response list on Studies tab");
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(23, 67);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(20, 17);
            this.button2.TabIndex = 3;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // _debugs
            // 
            this._debugs.AutoSize = true;
            this._debugs.Checked = true;
            this._debugs.CheckState = System.Windows.Forms.CheckState.Checked;
            this._debugs.Location = new System.Drawing.Point(28, 123);
            this._debugs.Name = "_debugs";
            this._debugs.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._debugs.Size = new System.Drawing.Size(129, 17);
            this._debugs.TabIndex = 2;
            this._debugs.Text = "Response Debugging";
            this.toolTip1.SetToolTip(this._debugs, "show your responses SendDebug messages in messages window");
            this._debugs.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(255, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select Tick Folder";
            this.toolTip1.SetToolTip(this.label1, "select folder that is scanned for historical data");
            // 
            // _initialequity
            // 
            this._initialequity.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::WinGauntlet.Properties.Settings.Default, "InitialEquity", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._initialequity.Location = new System.Drawing.Point(360, 9);
            this._initialequity.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
            this._initialequity.Name = "_initialequity";
            this._initialequity.Size = new System.Drawing.Size(79, 20);
            this._initialequity.TabIndex = 35;
            this._initialequity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._initialequity.ThousandsSeparator = true;
            this._initialequity.Value = global::WinGauntlet.Properties.Settings.Default.InitialEquity;
            // 
            // _usehighliquidityEOD
            // 
            this._usehighliquidityEOD.AutoSize = true;
            this._usehighliquidityEOD.Checked = global::WinGauntlet.Properties.Settings.Default.AssumeHighLiquidEOD;
            this._usehighliquidityEOD.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "AssumeHighLiquidEOD", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._usehighliquidityEOD.Location = new System.Drawing.Point(228, 211);
            this._usehighliquidityEOD.Name = "_usehighliquidityEOD";
            this._usehighliquidityEOD.Size = new System.Drawing.Size(250, 17);
            this._usehighliquidityEOD.TabIndex = 34;
            this._usehighliquidityEOD.Text = "Assume Highly Liquid Fills (EOD/daily data only)";
            this._usehighliquidityEOD.UseVisualStyleBackColor = true;
            // 
            // resetonrun
            // 
            this.resetonrun.AutoSize = true;
            this.resetonrun.Checked = global::WinGauntlet.Properties.Settings.Default.ResetOnRun;
            this.resetonrun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.resetonrun.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "ResetOnRun", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.resetonrun.Location = new System.Drawing.Point(28, 188);
            this.resetonrun.Name = "resetonrun";
            this.resetonrun.Size = new System.Drawing.Size(94, 17);
            this.resetonrun.TabIndex = 33;
            this.resetonrun.Text = "Reset On Run";
            this.resetonrun.UseVisualStyleBackColor = true;
            // 
            // _portfoliosim
            // 
            this._portfoliosim.AutoSize = true;
            this._portfoliosim.Checked = global::WinGauntlet.Properties.Settings.Default.PortfolioRealisticSim;
            this._portfoliosim.CheckState = System.Windows.Forms.CheckState.Checked;
            this._portfoliosim.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "PortfolioRealisticSim", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._portfoliosim.Location = new System.Drawing.Point(28, 208);
            this._portfoliosim.Margin = new System.Windows.Forms.Padding(2);
            this._portfoliosim.Name = "_portfoliosim";
            this._portfoliosim.Size = new System.Drawing.Size(161, 17);
            this._portfoliosim.TabIndex = 32;
            this._portfoliosim.Text = "Portfolio-realistic Sim (slower)";
            this._portfoliosim.UseVisualStyleBackColor = true;
            // 
            // _usebidask
            // 
            this._usebidask.AutoSize = true;
            this._usebidask.Checked = global::WinGauntlet.Properties.Settings.Default.UseBidAskFills;
            this._usebidask.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "UseBidAskFills", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._usebidask.Location = new System.Drawing.Point(228, 188);
            this._usebidask.Margin = new System.Windows.Forms.Padding(2);
            this._usebidask.Name = "_usebidask";
            this._usebidask.Size = new System.Drawing.Size(106, 17);
            this._usebidask.TabIndex = 29;
            this._usebidask.Text = "Use Bid/Ask Fills";
            this.toolTip1.SetToolTip(this._usebidask, "Use Bid/Ask to fill orders, otherwise last trade is used.  This should generally " +
        "be enabled for for-ex");
            this._usebidask.UseVisualStyleBackColor = true;
            // 
            // _unique
            // 
            this._unique.AutoSize = true;
            this._unique.Checked = global::WinGauntlet.Properties.Settings.Default.csvnamesunique;
            this._unique.CheckState = System.Windows.Forms.CheckState.Checked;
            this._unique.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "csvnamesunique", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._unique.Location = new System.Drawing.Point(228, 100);
            this._unique.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._unique.Name = "_unique";
            this._unique.Size = new System.Drawing.Size(107, 17);
            this._unique.TabIndex = 26;
            this._unique.Text = "Unique filenames";
            this.toolTip1.SetToolTip(this._unique, "ensure filenames never duplicate");
            this._unique.UseVisualStyleBackColor = true;
            // 
            // ordersincsv
            // 
            this.ordersincsv.AutoSize = true;
            this.ordersincsv.Checked = global::WinGauntlet.Properties.Settings.Default.ordersincsv;
            this.ordersincsv.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "ordersincsv", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ordersincsv.Location = new System.Drawing.Point(228, 167);
            this.ordersincsv.Name = "ordersincsv";
            this.ordersincsv.Size = new System.Drawing.Size(92, 17);
            this.ordersincsv.TabIndex = 22;
            this.ordersincsv.Text = "Orders in CSV";
            this.toolTip1.SetToolTip(this.ordersincsv, "save orders to excel or R-compatible file");
            this.ordersincsv.UseVisualStyleBackColor = true;
            // 
            // messagewrite
            // 
            this.messagewrite.AutoSize = true;
            this.messagewrite.Checked = global::WinGauntlet.Properties.Settings.Default.writeonmessages;
            this.messagewrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.messagewrite.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "writeonmessages", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.messagewrite.Location = new System.Drawing.Point(28, 167);
            this.messagewrite.Name = "messagewrite";
            this.messagewrite.Size = new System.Drawing.Size(142, 17);
            this.messagewrite.TabIndex = 18;
            this.messagewrite.Text = "Disable Message Editing";
            this.toolTip1.SetToolTip(this.messagewrite, "disable modifying or making notes in messages window");
            this.messagewrite.UseVisualStyleBackColor = true;
            // 
            // clearmessages
            // 
            this.clearmessages.AutoSize = true;
            this.clearmessages.Checked = global::WinGauntlet.Properties.Settings.Default.clearmessages;
            this.clearmessages.CheckState = System.Windows.Forms.CheckState.Checked;
            this.clearmessages.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "clearmessages", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.clearmessages.Location = new System.Drawing.Point(28, 145);
            this.clearmessages.Name = "clearmessages";
            this.clearmessages.Size = new System.Drawing.Size(139, 17);
            this.clearmessages.TabIndex = 17;
            this.clearmessages.Text = "Clear Messages on Run";
            this.toolTip1.SetToolTip(this.clearmessages, "clear messages window for each run");
            this.clearmessages.UseVisualStyleBackColor = true;
            // 
            // saveonexit
            // 
            this.saveonexit.AutoSize = true;
            this.saveonexit.Checked = global::WinGauntlet.Properties.Settings.Default.saveonexit;
            this.saveonexit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.saveonexit.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGauntlet.Properties.Settings.Default, "saveonexit", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.saveonexit.Location = new System.Drawing.Point(28, 100);
            this.saveonexit.Name = "saveonexit";
            this.saveonexit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.saveonexit.Size = new System.Drawing.Size(86, 17);
            this.saveonexit.TabIndex = 14;
            this.saveonexit.Text = "Save on Exit";
            this.toolTip1.SetToolTip(this.saveonexit, "save gauntlet options on exit");
            this.saveonexit.UseVisualStyleBackColor = true;
            // 
            // optimizetab
            // 
            this.optimizetab.BackColor = System.Drawing.SystemColors.Window;
            this.optimizetab.Controls.Add(this._optstat);
            this.optimizetab.Controls.Add(this.startoptimize);
            this.optimizetab.Controls.Add(this.refreshoptimizechoices);
            this.optimizetab.Controls.Add(this.label9);
            this.optimizetab.Controls.Add(this.label8);
            this.optimizetab.Controls.Add(this.label6);
            this.optimizetab.Controls.Add(this.label5);
            this.optimizetab.Controls.Add(this.higherisoptimimal);
            this.optimizetab.Controls.Add(this._optdecision);
            this.optimizetab.Controls.Add(this._optinc);
            this.optimizetab.Controls.Add(this._optstop);
            this.optimizetab.Controls.Add(this._optstart);
            this.optimizetab.Controls.Add(this.label4);
            this.optimizetab.Controls.Add(this._optimizename);
            this.optimizetab.Location = new System.Drawing.Point(4, 22);
            this.optimizetab.Name = "optimizetab";
            this.optimizetab.Padding = new System.Windows.Forms.Padding(3);
            this.optimizetab.Size = new System.Drawing.Size(534, 342);
            this.optimizetab.TabIndex = 7;
            this.optimizetab.Text = "Optimize";
            // 
            // _optstat
            // 
            this._optstat.AutoSize = true;
            this._optstat.Location = new System.Drawing.Point(9, 233);
            this._optstat.Name = "_optstat";
            this._optstat.Size = new System.Drawing.Size(130, 13);
            this._optstat.TabIndex = 13;
            this._optstat.Text = "No optimization is running.";
            // 
            // startoptimize
            // 
            this.startoptimize.Location = new System.Drawing.Point(12, 177);
            this.startoptimize.Name = "startoptimize";
            this.startoptimize.Size = new System.Drawing.Size(268, 31);
            this.startoptimize.TabIndex = 12;
            this.startoptimize.Text = "optimize the gauntlet";
            this.startoptimize.UseVisualStyleBackColor = true;
            this.startoptimize.Click += new System.EventHandler(this.startoptimize_Click);
            // 
            // refreshoptimizechoices
            // 
            this.refreshoptimizechoices.Location = new System.Drawing.Point(301, 33);
            this.refreshoptimizechoices.Name = "refreshoptimizechoices";
            this.refreshoptimizechoices.Size = new System.Drawing.Size(75, 23);
            this.refreshoptimizechoices.TabIndex = 11;
            this.refreshoptimizechoices.Text = "refresh";
            this.refreshoptimizechoices.UseVisualStyleBackColor = true;
            this.refreshoptimizechoices.Click += new System.EventHandler(this.refreshoptimizechoices_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 142);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(82, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Deciding Value:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 113);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Testing every :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Finishing at:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Starting at:";
            // 
            // higherisoptimimal
            // 
            this.higherisoptimimal.Appearance = System.Windows.Forms.Appearance.Button;
            this.higherisoptimimal.AutoSize = true;
            this.higherisoptimimal.Checked = true;
            this.higherisoptimimal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.higherisoptimimal.Location = new System.Drawing.Point(301, 137);
            this.higherisoptimimal.Name = "higherisoptimimal";
            this.higherisoptimimal.Size = new System.Drawing.Size(136, 23);
            this.higherisoptimimal.TabIndex = 6;
            this.higherisoptimimal.Text = "Higher values are optimal";
            this.higherisoptimimal.UseVisualStyleBackColor = true;
            // 
            // _optdecision
            // 
            this._optdecision.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._optdecision.FormattingEnabled = true;
            this._optdecision.Location = new System.Drawing.Point(112, 139);
            this._optdecision.Name = "_optdecision";
            this._optdecision.Size = new System.Drawing.Size(168, 21);
            this._optdecision.TabIndex = 5;
            // 
            // _optinc
            // 
            this._optinc.DecimalPlaces = 5;
            this._optinc.Location = new System.Drawing.Point(113, 113);
            this._optinc.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this._optinc.Minimum = new decimal(new int[] {
            100000000,
            0,
            0,
            -2147483648});
            this._optinc.Name = "_optinc";
            this._optinc.Size = new System.Drawing.Size(167, 20);
            this._optinc.TabIndex = 4;
            this._optinc.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._optinc.ThousandsSeparator = true;
            // 
            // _optstop
            // 
            this._optstop.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::WinGauntlet.Properties.Settings.Default, "OptimizeFinishAt", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._optstop.Location = new System.Drawing.Point(113, 87);
            this._optstop.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this._optstop.Minimum = new decimal(new int[] {
            100000000,
            0,
            0,
            -2147483648});
            this._optstop.Name = "_optstop";
            this._optstop.Size = new System.Drawing.Size(167, 20);
            this._optstop.TabIndex = 3;
            this._optstop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._optstop.ThousandsSeparator = true;
            this._optstop.Value = global::WinGauntlet.Properties.Settings.Default.OptimizeFinishAt;
            // 
            // _optstart
            // 
            this._optstart.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::WinGauntlet.Properties.Settings.Default, "OptimizeStartAt", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._optstart.Location = new System.Drawing.Point(113, 61);
            this._optstart.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this._optstart.Minimum = new decimal(new int[] {
            100000000,
            0,
            0,
            -2147483648});
            this._optstart.Name = "_optstart";
            this._optstart.Size = new System.Drawing.Size(167, 20);
            this._optstart.TabIndex = 2;
            this._optstart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._optstart.ThousandsSeparator = true;
            this._optstart.Value = global::WinGauntlet.Properties.Settings.Default.OptimizeStartAt;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Optimize Value:";
            // 
            // _optimizename
            // 
            this._optimizename.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._optimizename.FormattingEnabled = true;
            this._optimizename.Location = new System.Drawing.Point(113, 33);
            this._optimizename.Name = "_optimizename";
            this._optimizename.Size = new System.Drawing.Size(167, 21);
            this._optimizename.TabIndex = 0;
            this._optimizename.SelectedIndexChanged += new System.EventHandler(this._optimizename_SelectedIndexChanged);
            // 
            // equitycurvetab
            // 
            this.equitycurvetab.BackColor = System.Drawing.SystemColors.Window;
            this.equitycurvetab.Controls.Add(this.equitychart);
            this.equitycurvetab.Location = new System.Drawing.Point(4, 22);
            this.equitycurvetab.Name = "equitycurvetab";
            this.equitycurvetab.Size = new System.Drawing.Size(534, 342);
            this.equitycurvetab.TabIndex = 6;
            this.equitycurvetab.Text = "Equity";
            // 
            // equitychart
            // 
            this.equitychart.AutoUpdate = false;
            this.equitychart.BackColor = System.Drawing.SystemColors.Window;
            this.equitychart.Bars = null;
            this.equitychart.DisplayCursor = false;
            this.equitychart.DisplayInterval = true;
            this.equitychart.DisplayRightClick = false;
            this.equitychart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.equitychart.isUsingShadedCloses = true;
            this.equitychart.Location = new System.Drawing.Point(0, 0);
            this.equitychart.ManualColor = System.Drawing.Color.Turquoise;
            this.equitychart.MinOsc = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.equitychart.Name = "equitychart";
            this.equitychart.Size = new System.Drawing.Size(534, 342);
            this.equitychart.Symbol = "";
            this.equitychart.TabIndex = 0;
            // 
            // _resulttab
            // 
            this._resulttab.Controls.Add(this.tradeResults1);
            this._resulttab.Location = new System.Drawing.Point(4, 22);
            this._resulttab.Margin = new System.Windows.Forms.Padding(2);
            this._resulttab.Name = "_resulttab";
            this._resulttab.Padding = new System.Windows.Forms.Padding(2);
            this._resulttab.Size = new System.Drawing.Size(534, 342);
            this._resulttab.TabIndex = 5;
            this._resulttab.Text = "Results";
            this._resulttab.UseVisualStyleBackColor = true;
            // 
            // tradeResults1
            // 
            this.tradeResults1.AutoWatch = false;
            this.tradeResults1.BackColor = System.Drawing.Color.White;
            this.tradeResults1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tradeResults1.Location = new System.Drawing.Point(2, 2);
            this.tradeResults1.Margin = new System.Windows.Forms.Padding(1);
            this.tradeResults1.Name = "tradeResults1";
            this.tradeResults1.Path = "C:\\Users\\jfranta\\Documents";
            this.tradeResults1.Size = new System.Drawing.Size(530, 338);
            this.tradeResults1.SplitterDistance = 43;
            this.tradeResults1.TabIndex = 0;
            // 
            // messagepage
            // 
            this.messagepage.Controls.Add(this.messages);
            this.messagepage.Location = new System.Drawing.Point(4, 22);
            this.messagepage.Name = "messagepage";
            this.messagepage.Size = new System.Drawing.Size(534, 342);
            this.messagepage.TabIndex = 4;
            this.messagepage.Text = "Messages";
            this.messagepage.UseVisualStyleBackColor = true;
            // 
            // messages
            // 
            this.messages.BackColor = System.Drawing.SystemColors.Window;
            this.messages.DataBindings.Add(new System.Windows.Forms.Binding("ReadOnly", global::WinGauntlet.Properties.Settings.Default, "writeonmessages", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.messages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messages.Location = new System.Drawing.Point(0, 0);
            this.messages.Name = "messages";
            this.messages.ReadOnly = global::WinGauntlet.Properties.Settings.Default.writeonmessages;
            this.messages.Size = new System.Drawing.Size(534, 342);
            this.messages.TabIndex = 0;
            this.messages.Text = "";
            // 
            // Gauntlet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(613, 300);
            this.Controls.Add(this.tabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Gauntlet";
            this.Text = "Gauntlet";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Gauntlet_FormClosing);
            this.tabs.ResumeLayout(false);
            this.studypage.ResumeLayout(false);
            this.studypage.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.optionpage.ResumeLayout(false);
            this.optionpage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._initialequity)).EndInit();
            this.optimizetab.ResumeLayout(false);
            this.optimizetab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._optinc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._optstop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._optstart)).EndInit();
            this.equitycurvetab.ResumeLayout(false);
            this._resulttab.ResumeLayout(false);
            this.messagepage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage studypage;
        private System.Windows.Forms.TabPage optionpage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage messagepage;
        private System.Windows.Forms.Button queuebut;
        private System.Windows.Forms.ListBox reslist;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RichTextBox messages;
        private System.Windows.Forms.CheckBox saveonexit;
        private System.Windows.Forms.Button savesettings;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel lastmessage;
        private System.Windows.Forms.CheckBox clearmessages;
        private System.Windows.Forms.CheckBox messagewrite;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox ordersincsv;
        private System.Windows.Forms.CheckBox _unique;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox _debugfile;
        private System.Windows.Forms.CheckBox _indicatcsv;
        private TradeLink.AppKit.TickFileFilterControl tickFileFilterControl1;
        private System.Windows.Forms.Button _twithelp;
        private System.Windows.Forms.Button _viewresults;
        private System.Windows.Forms.CheckBox _debugs;
        private System.Windows.Forms.TabPage _resulttab;
        private TradeLink.AppKit.TradeResults tradeResults1;
        private System.Windows.Forms.CheckBox _usebidask;
        private System.Windows.Forms.CheckBox _portfoliosim;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.CheckBox resetonrun;
        private System.Windows.Forms.CheckBox _usehighliquidityEOD;
        private System.Windows.Forms.TabPage equitycurvetab;
        private TradeLink.AppKit.ChartControl equitychart;
        private System.Windows.Forms.NumericUpDown _initialequity;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage optimizetab;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox higherisoptimimal;
        private System.Windows.Forms.ComboBox _optdecision;
        private System.Windows.Forms.NumericUpDown _optinc;
        private System.Windows.Forms.NumericUpDown _optstop;
        private System.Windows.Forms.NumericUpDown _optstart;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox _optimizename;
        private System.Windows.Forms.Button startoptimize;
        private System.Windows.Forms.Button refreshoptimizechoices;
        private System.Windows.Forms.CheckBox _siminmemory;
        private System.Windows.Forms.Label _optstat;
    }
}

