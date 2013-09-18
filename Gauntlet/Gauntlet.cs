using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using TradeLink.Common;
using System.IO;
using TradeLink.API;
using TradeLink.AppKit;

namespace WinGauntlet
{
    public partial class Gauntlet : AppTracker
    {


        HistSim h;
        BackgroundWorker bw = new BackgroundWorker();
        BackgroundWorker getsymwork = new BackgroundWorker();
        static GauntArgs args = new GauntArgs();
        public const string PROGRAM = "Gauntlet";
        StreamWriter indf;

        Log _log = new Log(PROGRAM);



        public Gauntlet()
        {
            TrackEnabled = Util.TrackUsage();
            Program = PROGRAM;
            InitializeComponent();
            Text += " " + Util.TLVersion();
            debug(Util.TLSIdentity());
            args.GotDebug += new DebugDelegate(args_GotDebug);
            args.ParseArgs(Environment.GetCommandLineArgs());
            FormClosing += new FormClosingEventHandler(Gauntlet_FormClosing);
            debug(RunTracker.CountNewGetPrettyRuns(PROGRAM, Util.PROGRAM));
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            getsymwork.WorkerSupportsCancellation = true;
            getsymwork.DoWork += new DoWorkEventHandler(getsymwork_DoWork);
            getsymwork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getsymwork_RunWorkerCompleted);
            getsymwork.RunWorkerAsync();
            getsymwork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getsymwork_RunWorkerCompleted);

            if (args.isUnattended)
            {
                ordersincsv.Checked = true;
                if (args.HideWindow) { this.ShowInTaskbar = false; this.WindowState = FormWindowState.Minimized; }
                //ShowWindow(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);
                bindresponseevents();
                queuebut_Click(null, null);
            }
            else
            {
                status("wait while tickdata is loaded...");
                UpdateResponses(ResponseLoader.GetResponseList(args.DllName));
            }

            _optdecision.Items.Clear();
            _optdecision.Items.AddRange(Optimize.GetDecideable().ToArray());
            if (_optdecision.Items.Count > 0)
                _optdecision.Text = "NetPL";

        }

        void getsymwork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
                status("completed loading tick data.");
            else
            {
                status("error loading data");
                debug(e.Error.Message + e.Error.StackTrace);
            }
            queuebut.Enabled = true;
            queuebut.Invalidate();
        }

        void getsymwork_DoWork(object sender, DoWorkEventArgs e)
        {
            queuebut.Enabled = false;
            queuebut.Invalidate();
            tickFileFilterControl1.SetSymbols(args.Folder);
        }

        void args_GotDebug(string msg)
        {
            Console.WriteLine(msg);
        }

        bool isresetonrun = true;

        // when run button is clicked, setup the simulation
        private void queuebut_Click(object sender, EventArgs e)
        {
            // see whether we're doing reset
            isresetonrun = resetonrun.Checked;
            // make sure we only have one background thread
            if (bw.IsBusy)
            {
                status("simulation already in progress.");
                return;
            }
            if ((opt != null) && opt.isRunning)
            {
                status("optimization in progress.");
                return;
            }
            // make sure response is valid
            if ((args.Response == null) || !args.Response.isValid)
            {
                status("No valid response was selected, quitting.");
                return;
            }

            // make sure tick folder is valid
            if (args.Folder=="")
            {
                string msg = "No tick folder option is configured.";
                status(msg);
                if (!args.isUnattended) MessageBox.Show(msg);
                return;
            }


            // prepare other arguments for the run
            if (!args.isUnattended)
            {
                args.Orders = ordersincsv.Checked;
                args.Indicators = _indicatcsv.Checked;
                args.Debugs = _debugs.Checked;
                args.Filter = tickFileFilterControl1.GetFilter();
            }

            // perform reset if requested
            if (isresetonrun)
                resetresponse();

            // clear results
            clearresults();
            // set names and times
            args.Name = args.ResponseName+uniquen;
            args.Started = DateTime.Now;

            // enable progress reporting
            ProgressBar1.Enabled = true;
            // disable more than one simulation at once
            queuebut.Enabled = false;

            // start the run in the background
            bw.RunWorkerAsync(args);

        }
        string uniquen { get { return DateTime.Now.ToString(".yyyMMdd.HHmm"); } }

        string LogFile(string logtype) { return OUTFOLD+args.Response.FullName+(_unique.Checked ? uniquen:"")+ "."+logtype+".csv"; }
        Broker SimBroker = new Broker();

        string lasttff = string.Empty;
        /// <summary>
        /// returns true if sim event binding is needed
        /// </summary>
        /// <param name="sim"></param>
        /// <param name="ga"></param>
        bool prepsim_needsbind(ref HistSim sim, GauntArgs ga)
        {
            // see if filter or folder has changed
            string thistff = TickFileFilter.Serialize(ga.Filter)+ga.Folder;
            bool fullreset = lasttff != thistff;

            if ((sim==null) || fullreset)
            {
                if (_portfoliosim.Checked)
                {
                    if (_siminmemory.Checked)
                    {
                        sim = new HistSimMemory(ga.Folder, ga.Filter);
                    }
                    else
                    {
                        debug("Using portfolio simulation. (realistic)");
                        sim = new MultiSimImpl(ga.Folder, ga.Filter);
                    }
                }
                else
                {
                    debug("Using sequential symbol simulation. (faster)");
                    sim = new SingleSimImpl(ga.Folder, ga.Filter);
                }
                lasttff = thistff;
                return true;

            }
            else
            {
                sim.Reset();
            }
            return false;
        }

        void bindsim(ref HistSim h)
        {
            h.GotDebug += new DebugDelegate(h_GotDebug);
            h.GotTick += new TickDelegate(h_GotTick);
        }

        void unbindsim(ref HistSim h)
        {
            if (h != null)
            {
                h.GotDebug -= new DebugDelegate(h_GotDebug);
                h.GotTick -= new TickDelegate(h_GotTick);
            }
        }

        // runs the simulation in background
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            FillCount = 0;
            // get simulation arguments
            GauntArgs ga = (GauntArgs)e.Argument;
            // notify user
            debug("Run started: " + ga.Name);
            status("Started: " + ga.ResponseName);
            // prepare simulator
            if (prepsim_needsbind(ref h, ga))
                bindsim(ref h);
            // prep broker
            SimBroker = new Broker();
            SimBroker.UseBidAskFills = _usebidask.Checked;
            SimBroker.UseHighLiquidityFillsEOD = _usehighliquidityEOD.Checked;

            SimBroker.GotFill += new FillDelegate(SimBroker_GotFill);
            SimBroker.GotOrder += new OrderDelegate(SimBroker_GotOrder);
            SimBroker.GotOrderCancel += new OrderCancelDelegate(SimBroker_GotOrderCancel);
            // start simulation
            try
            {
                h.PlayTo(ga.PlayTo);
            }
            catch (DirectoryNotFoundException ex)
            {
                debug("Error locating tick files,  err: " + ex.Message + ex.StackTrace);
                debug("You may want to ensure your tick files are in the TradeLinkTicks folder.");
            }
            // end simulation
            ga.Stopped = DateTime.Now;
            ga.TicksProcessed = h.TicksProcessed;
            ga.Executions = FillCount;
            // save result
            e.Result = ga;
        }

        void SimBroker_GotOrder(Order o)
        {
            try {
            args.Response.GotOrder(o);
            }
            catch (Exception ex) { debug("on order: "+o.ToString()+" response threw exception: " + ex.Message); }
        }
        int FillCount = 0;
        void SimBroker_GotFill(Trade t)
        {
            FillCount++;
            try {
            args.Response.GotFill(t);
            }
            catch (Exception ex) { debug("on fill: "+t.ToString()+" response threw exception: " + ex.Message); }
        }

        void SimBroker_GotOrderCancel(string sym, bool side, long id)
        {
            try {
            args.Response.GotOrderCancel(id);
            }
            catch (Exception ex) { debug("on cancel: "+id+" response threw exception: " + ex.Message); }
        }

        void clearresults()
        {
            FillCount = 0;
            tradeResults1.Clear();
            SimBroker.Reset();
        }


        void updatecompletedtrades(List<Trade> list, bool savetrades, bool saveorders)
        {
            
            tradeResults1.NewResultTrades(LogFile("Trades"), list);
            TradeResults.GetPortfolioPlot("Equity", _initialequity.Value, 0, 0, Util.ToTLDate(), Util.ToTLTime(), list, ref equitychart);
            if (savetrades)
            {
                debug("writing " + list.Count + " trades...");
                Util.ClosedPLToText(list, ',', LogFile("Trades"));
            }
            if (saveorders)
            {
                List<Order> olist = SimBroker.GetOrderList();
                debug("writing " + olist.Count + " orders...");
                StreamWriter sw = new StreamWriter(LogFile("Orders"), false);
                string[] cols = Enum.GetNames(typeof(OrderField));
                sw.WriteLine(string.Join(",", cols));
                for (int i = 0; i < olist.Count; i++)
                    sw.WriteLine(OrderImpl.Serialize(olist[i]));
                sw.Close();
            }
        }

        // runs after simulation is complete
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            debug(_sb.ToString());
            _sb = new System.Text.StringBuilder(10000000);
            GauntArgs gargs = (GauntArgs)e.Result;
            if (!e.Cancelled)
            {
                List<Trade> list = SimBroker.GetTradeList();
                updatecompletedtrades(list, gargs.Trades, gargs.Orders);
                string msg = "Done.  Ticks: " + gargs.TicksProcessed + " Speed:" + gargs.TicksSecond.ToString("N0") + " t/s  Fills: " + gargs.Executions.ToString();
                debug(msg);
                status(msg);

            }
            else debug("Canceled.");
            // close indicators
            if (indf != null)
            {
                indf.Close();
                indf = null;
            }

            // reset simulation
            h.Reset();
            count = 0;
            lastp = 0;
            if (args.isUnattended)
            {
                Close();
                return;
            }
            // enable new runs
            ProgressBar1.Enabled = false;
            ProgressBar1.Value = 0;
            queuebut.Enabled = true;
            Invalidate(true);
        }

        int count = 0;
        long lastp = 0;
        string nowtime = "0";
        void h_GotTick(Tick t)
        {
            
            nowtime = t.time.ToString();
            // execute open orders
            SimBroker.Execute(t);
            if (args.Response == null) return;
            if (t.depth > _depth) return;
            count++;
            try
            {
                args.Response.GotTick(t);
            }
            catch (Exception ex) { debug("response threw exception: " + ex.Message); }
            if (args.isUnattended) return;
            long percent = (long)((double)count*100 / h.TicksPresent);
            if ((percent!=lastp) && (percent % 5 == 0))
            {
                updatepercent(percent);
                lastp = percent;
            }

        }

        void updatepercent(long per)
        {
            if (InvokeRequired)
                Invoke(new LongDelegate(updatepercent), new object[] { per });
            else
            {
                try
                {
                    ProgressBar1.Value = (int)per;
                    ProgressBar1.Invalidate();
                }
                catch (Exception) { }
            }
        }


        void h_GotDebug(string msg)
        {
            debug(msg);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // tick folder
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = "Select the folder containing tick files";
            string folder = Util.TLTickDir;
            if (Directory.Exists(args.Folder))
                folder = args.Folder;
            fd.SelectedPath = folder;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                args.Folder = fd.SelectedPath;
                tickFileFilterControl1.SetSymbols(args.Folder);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // response library selection
            OpenFileDialog of = new OpenFileDialog();
            of.CheckPathExists = true;
            of.CheckFileExists = true;
            of.DefaultExt = ".dll";
            of.Filter = "Response DLL|*.dll";
            of.Multiselect = false;
            if (of.ShowDialog() == DialogResult.OK)
            {
                args.DllName = of.FileName;
            
                UpdateResponses(ResponseLoader.GetResponseList(of.FileName));

            }
        }

        void UpdateResponses(List<string> responses)
        {
            reslist.Items.Clear();
            for (int i = 0; i < responses.Count; i++)
                reslist.Items.Add(responses[i]);
        }

        private void messages_DoubleClick(object sender, EventArgs e)
        {
            messages.Clear();
        }

        string OUTFOLD = Util.ProgramData(PROGRAM)+ "\\";
        void debug(string message) 
        {
            if (InvokeRequired)
                Invoke(new DebugDelegate(debug), new object[] { message });
            else
            {
                messages.AppendText(message+Environment.NewLine);
                messages.Invalidate(true);
            }
            _log.GotDebug(message);
        }

        void status(string message)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new DebugDelegate(status), new object[] { message });
                }
                catch (ObjectDisposedException) { }
            }
            else
            {
                try
                {
                    lastmessage.Text = message;
                    lastmessage.Invalidate();
                }
                catch (ObjectDisposedException) { }
            }
        }

        private void savesettings_Click(object sender, EventArgs e)
        {
            WinGauntlet.Properties.Settings.Default.Save();
        }

        private void Gauntlet_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                bw.CancelAsync();
                getsymwork.CancelAsync();
                if (indf != null)
                    indf.Close();
                h.Stop();
            }
            catch { }
            try
            {
                if (saveonexit.Checked && !args.isUnattended)
                {
                    Properties.Settings.Default.tickfolder = args.Folder;
                    Properties.Settings.Default.boxdll = args.DllName;
                    WinGauntlet.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                debug("Error saving gauntlet configuration: " + ex.Message + ex.StackTrace);
            }
            _log.Stop();
        }



        private void button4_Click(object sender, EventArgs e)
        {
            WinGauntlet.Properties.Settings.Default.Reset();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WinGauntlet.Properties.Settings.Default.Reload();
        }


        string CurrentResponse { get { return (string)reslist.SelectedItem; } }

        private void boxlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            string rname = string.Empty;
            try
            {
                args.Response = ResponseLoader.FromDLL(CurrentResponse, args.DllName);
            }
            catch (Exception ex) 
            { 
                status("Response failed to load, quitting... (" + ex.Message + (ex.InnerException != null ? ex.InnerException.Message.ToString() : "") + ")"); 
            }
            if ((args==null) || (args.Response==null) || !args.Response.isValid) 
            { 
                status("Response did not load or loaded in a shutdown state. "+rname); 
                return; 
            }
            args.ResponseName = args.Response.FullName;
            _boundonce = false;
            bindresponseevents();
            if (!resetonrun.Checked)
                resetresponse();

            // update optimizeable params
            updateoptimize();
        }

        bool getsimhints(string folder, TickFileFilter tff, ref int date, ref string[] syms)
        {
            date = 0;
            syms = new string[0];
            try
            {
                string[] files = Directory.GetFiles(folder, TikConst.WILDCARD_EXT);
                var simfiles = tff.Allows(files);
                List<string> simsyms = new List<string>(simfiles.Length);
                Array.Sort(simfiles);
                // get earliest date
                var min = int.MaxValue;
                foreach (var sf in simfiles)
                {
                    var sec = SecurityImpl.SecurityFromFileName(sf);
                    if (!string.IsNullOrWhiteSpace(sec.symbol) && !simsyms.Contains(sec.symbol))
                        simsyms.Add(sec.symbol);
                    if ((sec.Date > 0) && (sec.Date < min))
                        min = sec.Date;
                }
                if (min != int.MaxValue)
                    date = min;
                syms = simsyms.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                debug("error getting sim hints: " + ex.Message + ex.StackTrace);
                return false;
            }

            
        }


        void resetresponse()
        {
            // test if response is valid
            if ((args.Response == null) || !args.Response.isValid)
                return;
            // notify
            debug("Resetting response: " + args.Response.FullName);
            // do work
            try {
                var r = args.Response;
                int date = 0;
                string[] syms = new string[0];
                if (getsimhints(args.Folder, args.Filter, ref date, ref syms))
                    ResponseLoader.SendSimulationHints(ref r, date, 0, syms, debug);
                args.Response.Reset();
            }
            catch (Exception ex) { debug("on reset, response threw exception: " + ex.Message); }
        }

        bool _boundonce = false;
        bool bindresponseevents()
        {
            if ((args.Response== null) || !args.Response.isValid)
                return false;
            if (_boundonce) return true;
            args.Response.ID = 0;
            args.Response.SendTicketEvent += new TicketDelegate(Response_SendTicketEvent);
            args.Response.SendMessageEvent += new MessageDelegate(Response_SendMessage);
            args.Response.SendIndicatorsEvent += new ResponseStringDel(Response_SendIndicators);
            args.Response.SendDebugEvent += new DebugDelegate(Response_GotDebug);
            args.Response.SendCancelEvent += new LongSourceDelegate(Response_CancelOrderSource);
            args.Response.SendOrderEvent += new OrderSourceDelegate(Response_SendOrder);
            args.Response.SendBasketEvent += new BasketDelegate(Response_SendBasket);
            args.Response.SendChartLabelEvent += new ChartLabelDelegate(Response_SendChartLabel);
            _boundonce = true;
            return true;
        }
        bool _sendticketwarn = false;
        void Response_SendTicketEvent(string space, string user, string password, string summary, string description, Priority pri, TicketStatus stat)
        {
            if (_sendticketwarn) return;
            debug("Sendticket not supported in gauntlet.");
            _sendticketwarn = true;
        }

        bool _sendbaskwarn = false;
        void Response_SendBasket(Basket b, int id)
        {
            if (_sendbaskwarn) return;
            debug("Sendbasket not supported in gauntlet.");
            debug("To specify trading symbols, select symbols via GUI.");
            _sendbaskwarn = true;
        }

        void Response_SendChartLabel(decimal price, int bar, string label, System.Drawing.Color c)
        {
            
        }
        int _depth = 0;
        void Response_SendMessage(MessageTypes type, long source, long dest, long id, string data, ref string response)
        {
            switch (type)
            {
                case MessageTypes.DOMREQUEST:
                    
                    int d = 0;
                    string[] r = MessageTracker.ParseRequest(data);
                    if (r.Length > 1)
                        if (int.TryParse(MessageTracker.RequestParam(data,1), out d))
                            _depth = d;
                    break;
            }
        }

        void Response_SendIndicators(int id, string param)
        {
            if (!args.Indicators) return;
            // prepare indicator output
            if (indf == null)
            {
                indf = new StreamWriter(LogFile("Indicators"), false);
                indf.WriteLine(string.Join(",", args.Response.Indicators));
                indf.AutoFlush = true;
            }
            indf.WriteLine(param);
        }


        void Response_SendOrder(Order o, int id)
        {
            if (h!=null)
                SimBroker.SendOrderStatus(o);
        }

        void mybroker_GotOrderCancel(string sym, bool side, long id)
        {
            try {
            if (args.Response != null)
                args.Response.GotOrderCancel(id);
            }
            catch (Exception ex) { debug("on cancel: "+id+" response threw exception: " + ex.Message); }
        }

        void Response_CancelOrderSource(long number, int id)
        {
            if (h!=null)
                SimBroker.CancelOrder(number);
            
        }

        System.Text.StringBuilder _sb = new System.Text.StringBuilder(10000000);

        void Response_GotDebug(string msg)
        {
           // _sb.AppendLine(msg.Msg);
            _sb.AppendFormat("{0}: {1}{2}", nowtime, msg, Environment.NewLine);
            
        }

        void Response_IndicatorUpdate(object[] parameters)
        {

        }



        private void _stopbut_Click(object sender, EventArgs e)
        {
            if (bw.IsBusy)
                bw.CancelAsync();
        }

        class GauntArgs
        {
            public GauntArgs() 
            {
                // if using default filter then make it inclusive
                _filter.DefaultDeny = false;
            }
            // command line arguments
            const int DLL = 1;
            const int RESPONSE = 2;
            const int FLAGS = 3;
            const int TICKFOLDER = 4;
            const int FILEFILTERFILE = 5;
            public string Name;
            public int TicksProcessed = 0;
            public int Executions = 0;
            public long PlayTo = MultiSimImpl.ENDSIM;
            public DateTime Started = DateTime.MaxValue;
            public DateTime Stopped = DateTime.MaxValue;
            public double Seconds { get { return Stopped.Subtract(Started).TotalSeconds; } }
            public double TicksSecond { get { return Seconds == 0 ? 0 : ((double)TicksProcessed / Seconds); } }

            bool _debugs = true;
            bool _indicators = false;
            bool _trades = true;
            bool _orders = false;
            
            bool _hideWindow = false;
            public bool Orders { get { return _orders; } set { _orders = value; } }
            public bool Trades { get { return _trades; } set { _trades = value; } }
            public bool Indicators { get { return _indicators; } set { _indicators = value; } }
            public bool Debugs { get { return _debugs; } set { _debugs = value; } }
            
            public bool HideWindow { get { return _hideWindow; } set { _hideWindow = value; } }
            string _dllname = !File.Exists(WinGauntlet.Properties.Settings.Default.boxdll) ? "Responses.dll" : WinGauntlet.Properties.Settings.Default.boxdll;
            string _resp = "";
            Response _response;
            string _folder = (Properties.Settings.Default.tickfolder == null) ? Util.TLTickDir : Properties.Settings.Default.tickfolder;
            TickFileFilter _filter = new TickFileFilter();
            string _filterloc = "";
            void D(string msg) { if (GotDebug != null) GotDebug(msg); }
            public event DebugDelegate GotDebug;
            public string DllName { get { return _dllname; } set { _dllname = value; } }
            public string ResponseName { get { return _resp; } set { _resp = value; } }
            public Response Response { get { return _response; } set { _response = value; } }
            public string Folder { get { return _folder; } set { _folder = value; } }
            public TickFileFilter Filter { get { return _filter; } set { _filter = value; } }
            public string FilterLocation { get { return _filterloc; } set { _filterloc = value; } }
            public bool hasPrereq { get { return (_response != null) && Directory.Exists(_folder); } }
            bool _background = false;
            public bool isUnattended { get { return _background; } }
            public override string ToString()
            {
                string[] r = new string[] { DllName, ResponseName, Folder, Flags,FilterLocation };
                return string.Join("|", r);
            }
            public void ParseArgs(string[] args)
            {
                int l = args.Length-1;
                if (l == FILEFILTERFILE)
                {
                    SetFilter(args[FILEFILTERFILE]);
                    SetFolder(args[TICKFOLDER]);
                    SetDll(args[DLL]);
                    SetResponse(args[RESPONSE]);
                    SetFlags(args[FLAGS]);
                }
                else if (l == TICKFOLDER)
                {
                    SetFolder(args[TICKFOLDER]);
                    SetDll(args[DLL]);
                    SetResponse(args[RESPONSE]);
                    SetFlags(args[FLAGS]);
                }
                else if (l == FLAGS)
                {
                    SetDll(args[DLL]);
                    SetResponse(args[RESPONSE]);
                    SetFlags(args[FLAGS]);
                }
                else if (l == RESPONSE)
                {
                    SetDll(args[DLL]);
                    SetResponse(args[RESPONSE]);
                }
                else if (l == DLL)
                {
                    SetDll(args[DLL]);

                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("GAUNTLET USAGE: ");
                    Console.WriteLine("gauntlet.exe [Response.dll] [ResponseName] [FLAGS] [TickPath] [TickFileFilterPath]");
                    Console.WriteLine("Flags: control output produced.  (O)rders (T)rades (I)ndicators (D)ebugs (C)apitalConnection (H)ideWindow");
                    Console.WriteLine(@"eg: gauntlet 'c:\users\administrator\my documents\MyStrategies.dll' ");
                    Console.WriteLine("\t\tMyStrategies.MyStrategy OTIF 'c:\\tradelink\\tickdata\\' ");
                    Console.WriteLine("\t\t'c:\\users\\administrator\\my documents\\filefilter.txt'");
                    Console.WriteLine("");
                }
                D("dll|resp|fold|flags|filt: " + this.ToString());
                if (hasPrereq)
                    _background = true;
                
            }
            string Flags { get { return (Orders ? "O" : "") + (Debugs ? "D" : "") + (Indicators ? "I" : "") + (Trades ? "T" : "") + (HideWindow ? "H" : ""); } }
            void SetFlags(string flags)
            {
                _orders = flags.Contains("O");
                _debugs = flags.Contains("F");
                _trades = flags.Contains("T");
                _indicators = flags.Contains("I");
                _hideWindow = flags.Contains("H");

                D("set flags: "+Flags);
            }
            bool SetFolder(string folder)
            {
                if (!Directory.Exists(folder))
                {
                    D("no folder exists: " + folder);
                    return false;
                }
                _folder = folder;
                D("tickfolder: " + folder);
                return true;
            }
            bool SetResponse(string name)
            {
                // dll must exist, otherwise quit
                if (DllName== "") return false;
                // response must exist in dll, otherwise quit
                if (!ResponseLoader.GetResponseList(DllName).Contains(name))
                    return false;
                _resp = name;
                try
                {
                    _response = ResponseLoader.FromDLL(_resp, DllName);
                }
                catch (Exception ex)
                {
                    D(ex.Message + ex.StackTrace);
                }
                bool r = _response.isValid;
                if (r)
                    D("loaded response: " + name);
                else
                    D("invalid response: " + name + " " + _response.FullName);
                return r;
            }

            bool SetDll(string file)
            {
                if (File.Exists(file))
                {
                    _dllname = file;
                    D("found dll: " + file);
                    return true;
                }
                else
                    D("no dll found: " + file);
                return false;

            }

            bool SetFilter(string file)
            {
                if (File.Exists(file))
                {
                    _filterloc = file;
                    _filter = TickFileFilter.FromFile(file);
                    D("found filter: " + file);
                    return true;
                }
                else D("no filter found: " + file);
                return false;
            }
        }

        private void _twithelp_Click(object sender, EventArgs e)
        {
            HelpReportCommunity.Help(PROGRAM);
            
        }

        private void _viewresults_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Util.ProgramData(PROGRAM));
        }

        
        void updateoptimize()
        {

            if (string.IsNullOrWhiteSpace(args.DllName) || string.IsNullOrWhiteSpace(args.ResponseName))
            {
                status("Select a response.");
                return;
            }
            // reset optimizeable params for this response
            _optimizename.Items.Clear();
            _optimizename.Items.AddRange(Optimize.GetOptimizeable(args.DllName, args.ResponseName).ToArray());

            if ((_optimizename.Items.Count > 0) && string.IsNullOrWhiteSpace(_optimizename.Text))
            {
                _optimizename.Text = _optimizename.Items[0].ToString();
            }


            
        }

        private void refreshoptimizechoices_Click(object sender, EventArgs e)
        {
            updateoptimize();
        }

        Optimize opt;

        private void startoptimize_Click(object sender, EventArgs e)
        {
            if ((opt != null) && opt.isRunning)
            {
                optstatus("Wait until current optimization completes...");
                return;
            }
            // unbind events
            unbindsim(ref h);
            // make sure sim is prepped
            prepsim_needsbind(ref h, args);
            // create an optimization
            opt = new Optimize(args.DllName, args.ResponseName, h);
            // configure everything
            opt.Advance = _optinc.Value;
            opt.StartAt = _optstart.Value;
            opt.StopAt = _optstop.Value;
            opt.OptimizeName = _optimizename.Text;
            opt.OptimizeDecisionsName = _optdecision.Text;
            opt.isHigherDecisionValuesBetter = higherisoptimimal.Checked;
            opt.SendEngineDebugEvent += new DebugDelegate(debug);
            opt.SendDebugEvent+=new DebugDelegate(debug);
            opt.SendStatusEvent += new DebugDelegate(opt_SendStatusEvent);
            opt.SendOptimizationCompleteEvent +=new ResultListDel(opt_SendOptimizationCompleteEvent);
            opt.SendOptimizationProgressEvent += new IntDelegate(opt_SendOptimizationProgressEvent);
            // start
            if (opt.Start())
            {
                tabs.SelectedIndex = 0;
            }

        }

        void opt_SendOptimizationCompleteEvent(List<Result> results)
        {
            // rebind non optimization events
            bindsim(ref h);
            // reset progress
            updatepercent(0);
            // show results
            tradeResults1.Clear();
            foreach (var res in results)
                tradeResults1.NewResult(res);
            
        }

        void opt_SendStatusEvent(string msg)
        {
            optstatus(msg);
            status(msg);
        }

        void opt_SendOptimizationProgressEvent(int val)
        {
            updatepercent(val);
        }


        void optstatus(string msg)
        {
            if (InvokeRequired)
                Invoke(new DebugDelegate(optstatus),new object[] { msg});
            {
                _optstat.Text = msg;
                _optstat.Invalidate();
            }
        }




        private void _optimizename_SelectedIndexChanged(object sender, EventArgs e)
        {

            // reset default advance
            bool isint = Optimize.isNameInt(args.DllName, args.ResponseName, _optimizename.Text);;
            _optinc.DecimalPlaces = isint ? 0 : 5;
            if (_optinc.Value ==0)
            {
                _optinc.Value = Optimize.GetMinAdvance(args.DllName, args.ResponseName, _optimizename.Text);
            }
        }




    }






}
