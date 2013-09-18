using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TradeLink.API;
using TradeLink.AppKit;
using TradeLink.Common;


namespace TradeLink.AppKit
{
    public partial class GenericView<T> : UserControl, GenericViewI where T : Model
    {
        int _id = -1;
        public int id { get { return _id; } set { _id = value; } }

        // grids
        protected DataTable dt = new DataTable();
        protected DataGridView dv = new DataGridView();
        SafeBindingSource bs = new SafeBindingSource(false);
        

        protected string[] symbols2add = new string[0];
        bool _trycache = true;
        public bool TryCache { get { return _trycache; } set { _trycache = value; showdefaults(); } }
        



        protected GenericViewItem<T> mygvi = null;

        public GenericView(GenericViewItem<T> gvi) 
        {
            InitializeComponent();
            mygvi = gvi;
            MouseUp += new MouseEventHandler(NOMV_MouseUp);

            


            dv.KeyUp += new KeyEventHandler(dv_KeyUp);


            Dock = DockStyle.Fill;
            Reset();

            
        }

        /// <summary>
        /// clears and resets the grid settings
        /// </summary>
        public virtual void Reset()
        {
            if (InvokeRequired)
                Invoke(new VoidDelegate(Reset));
            else
            {
                initgrid(mygvi);
                //clearcontextmenu();
                CreateRightClick();
            }
        }


        public event VoidDelegate SendPostCreationEvent;
        bool showonce = true;
        bool postcreate = true;
        void showdefaults()
        {
            bool v = (SendDebugEvent!=null);
            if (showonce && v)
            {
                debug("User: " + Owner);
                debug("Cache: " + (TryCache ? "ON" : "disabled") + ".");
                debug("Default Sleep: " + mysleep);
                debug("Right click for menu.");
                
                showonce = false;
            }
            if (postcreate)
            {
                if (SendPostCreationEvent != null)
                    SendPostCreationEvent();
            }
        }

        public event PositionArrayDelegate GetPositionsEvent;

        protected PositionTracker GetCurrentPositions()
        {
            if (GetPositionsEvent != null)
            {
                var poss = GetPositionsEvent();
                var pt = PositionTracker.FromPosition(poss);
                return pt;
            }
            else
                debug("No position tracking defined, using empty positions.");
            return new PositionTracker();
        }

        

        protected virtual bool updaterow(int dtrow_or_modidx, T model)
        {
            try
            {
                if ((dtrow_or_modidx < 0) || (dtrow_or_modidx >= models.Count) || (dtrow_or_modidx>=dt.Rows.Count))
                {
                    return false;
                }
                models[dtrow_or_modidx] = model;
                dt.Rows[dtrow_or_modidx].ItemArray = ToData(model).ToArray();
                return true;
            }
            catch (Exception ex)
            {
                v("error occured updating row: " + dtrow_or_modidx + " with: " + model.ToString()+" err: "+ex.Message+ex.StackTrace);

            }
            return false;
        }


        /// <summary>
        /// adds option to dump serialized models to right click menu
        /// </summary>
        protected void rightadddump2clipboard()
        {
            rightadd("dump model 2 clipboard", rightexportselobject);
        }

        /// <summary>
        /// dumps a serialized version of selected objects to clipboard
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void rightexportselobject(string clicktext)
        {
            // get mods
            var mods = getselectedmods();
            // prepare place to hold them
            StringBuilder sb = new StringBuilder();
            // process them all
            int c = 0;
            foreach (var mod in mods)
            {
                string msg = string.Empty;
                try
                {
                    // serialize object
                    msg = Util.Serialize<T>(mod, debug);
                }
                catch (Exception ex)
                {
                    debug(mod.symbol + " error serializing: " + mod.ToString() + " err: " + ex.Message + ex.StackTrace);
                    continue;
                }
                c++;
                // save it
                sb.AppendLine(msg);
            }
            // copy to clipboard
            Clipboard.SetText(sb.ToString());
            debug("exported " + c + " models to clipboard.");
        }

        /// <summary>
        /// copies symbol column values of all selected models to clipboard
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void rightselectedsymclip(string clicktext)
        {
            var syms = getselectedsymbols();
            if (syms.Count == 0)
            {
                status("Select one or more symbols/models to copy.");
                return;
            }
            syms = new List<string>( new BasketImpl(syms.ToArray()).ToSymArray());
            string data = string.Join(",",syms);
            System.Windows.Forms.Clipboard.SetText(data);
            debug("copied " + syms.Count + " symbols to clipboard: " + data);
        }
        /// <summary>
        /// adds right click option to copy allsymbols to clipboard
        /// </summary>
        protected void rightaddsymclipall()
        {
            rightadd("copy all syms to clipboard", rightallsymclip);
        }
        /// <summary>
        /// adds right click option to copy selected ymbols to clipboard
        /// </summary>
        protected void rightaddsymclipsel()
        {
            rightadd("syms to clipboard", rightselectedsymclip);
        }





        /// <summary>
        /// opens a program and passes a list of symbols as an argument
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="program"></param>
        /// <param name="allsymsinoneshot">whether one instance of program can handle all symbols</param>
        protected virtual void openprogram(string[] symbols, string program, bool allsymsinoneshot)
        {
            if (!System.IO.File.Exists(program))
            {
                debug(program+" does not exist.");
                return;
            }
            if (allsymsinoneshot)
            {
                string arg = "'" + string.Join(",", symbols) + "'";
                System.Diagnostics.Process.Start(program,arg );
                debug(program + " started with argument: " + arg);
            }
            else
            {
                foreach (string sym in symbols)
                {
                    System.Diagnostics.Process.Start(program, sym);
                    debug(program + " started with argument: " + sym);
                }

            }
        }

        public event GetAvailableGenericViewNamesDel GetAvailViewsEvent;

        List<string> getallviews()
        {
            if (GetAvailViewsEvent != null)
                return GetAvailViewsEvent();
            return new List<string>();
        }



        string _owner = string.Empty;
        /// <summary>
        /// current user of this application
        /// </summary>
        public string Owner { get { return _owner; } set { _owner = value; } }
        /// <summary>
        /// whether current user is valid
        /// </summary>
        public bool isOwnerValid { get { return !string.IsNullOrWhiteSpace(Owner); } }

        /// <summary>
        /// listen to hide/show request sfor debug window
        /// </summary>
        public event VoidDelegate SendDebugVisibleToggleEvent;

        /// <summary>
        /// add right click option to request hiding/showing of debug window
        /// </summary>
        protected void rightadddebugtogrequest()
        {
            rightadd("debugging");
            rightadd("show/hide", toggledebugrequest);
            rightadd("VerboseDebugging", toggleverboseclick);
            rightadd();
        }

        protected void toggleverboseclick(string click)
        {
            VerboseDebugging = !VerboseDebugging;
            status("Verbose debugging: " + (VerboseDebugging ? "ON." : "disabled."));
            debug("Verbose debugging: " + (VerboseDebugging ? "ON." : "disabled."));
        }

        /// <summary>
        /// request hidding showing of debug window
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected virtual void toggledebugrequest(string clicktext)
        {
            if (SendDebugVisibleToggleEvent != null)
                SendDebugVisibleToggleEvent();
            else
                debug("No debugging toggle event is attached.");
        }

        protected void rightaddpos() { rightadd("from positions", rightaddpos); }
        protected virtual void rightaddpos(string clicktext)
        {
            addsymbols(getsymbolspositions());
        }
      
        /// <summary>
        /// adds models for any symbols in clipboard to the view
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected virtual void rightaddclip(string clicktext)
        {
            addsymbols(getsymbolsclipboard());
        }
        /// <summary>
        /// adds right click option to prompt users for symbols
        /// </summary>
        protected void rightaddsym_user()
        {
            rightadd("add user", rightaddsym_user);
        }
        /// <summary>
        /// adds right click option that prompts user for symbols and disables parsing of those symbols
        /// </summary>
        protected void rightaddsym_user_raw()
        {
            rightadd("add user (long symbols)", rightaddsym_user_raw);
        }
        /// <summary>
        /// adds right click option to add symbols from clipboard
        /// </summary>
        protected void rightaddsym_clip() { rightadd("add from clipboard", rightaddclip); }
        /// <summary>
        /// prompts user for symbols to create models for
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected virtual void rightaddsym_user(string clicktext)
        {
            addsymbols(getsymbols_popup());
        }
        /// <summary>
        /// require that symbol entered are treated in raw form
        /// </summary>
        /// <param name="clicktext"></param>
        protected virtual void rightaddsym_user_raw(string clicktext)
        {
            addsymbols(getsymbols_popup_raw());
        }
        /// <summary>
        /// override this method to supply models to the grid, whenever symbols are added 
        /// (symbols added via addsymsnow)
        /// </summary>
        /// <returns></returns>
        protected virtual List<T> getmodels()
        {
            return new List<T>();
        }

        /// <summary>
        /// called by most of the symbol/model adding methods
        /// </summary>
        protected virtual void addsymsnow()
        {
            if (InvokeRequired)
                Invoke(new VoidDelegate(addsymsnow));
            else
            {
                var mods = getmodels();
                ShowItems(mods, false);
                addsymsnow_post();
            }
        }
        /// <summary>
        /// called when adding symbols, after models have been created and added to view
        /// </summary>
        protected virtual void addsymsnow_post()
        {
        }



        /// <summary>
        /// start a thread to add models for symbols to view
        /// </summary>
        /// <param name="symbols"></param>
        public virtual void addsymbols(params string[] symbols)
        {
            if (symbols.Length == 0)
            {
                debug("No symbols specified.");
                return;
            }
            Array.Sort(symbols);
            symbols2add = symbols;
            RunHelper.run(addsymsnow, null, debug, "adding symbols");
        }

        /// <summary>
        /// copies all model symbols to clipboard
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void rightallsymclip(string clicktext)
        {
            List<string> syms = new List<string>();
            foreach (var fe in models)
                syms.Add(fe.symbol);
            if (syms.Count == 0)
            {
                status("Add symbols first before copying them.");
                return;
            }
            string data = string.Join(",", syms.ToArray());
            System.Windows.Forms.Clipboard.SetText(data);
            debug("copied " + syms.Count + " symbols to clipboard: " + data);
        }

        /// <summary>
        /// toggles caching flag (not used by default)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void righttogcache(string clicktext)
        {
            TryCache = !TryCache;
            debug("cache: " + (TryCache ? "ON." : "disabled."));
        }

        /// <summary>
        /// whether all objects have been selected by user
        /// </summary>
        protected bool selectall = false;
        /// <summary>
        /// implements default select all behavior
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void dv_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.A))
            {

                if (selectall)
                    selectallrows_clear();
                else
                    selectallrows();
                selectall = !selectall;
                e.Handled = true;

            }
        }

        /// <summary>
        /// gets google finance base url
        /// </summary>
        /// <returns></returns>
        protected string getgooglefinanceurl()
        {
            return @"https://www.google.com/finance?q=";
        }
        /// <summary>
        /// gets general search url
        /// </summary>
        /// <returns></returns>
        protected string getgooglesearchurl()
        {
            return @"https://www.google.com/search?q=";
        }
        /// <summary>
        /// gets yahoo finance base url
        /// </summary>
        /// <returns></returns>
        protected string getyahoofinanceurl()
        {
            return @"http://finance.yahoo.com/q/is?&annual&s=";
        }
        /// <summary>
        /// gets seeking alpha base url
        /// </summary>
        /// <returns></returns>
        protected string getseekingalphaurl()
        {
            return @"http://seekingalpha.com/symbol/";
        }

        /// <summary>
        /// opens browser sessions for a list of queries (queries = symbols, company names, etc)
        /// </summary>
        /// <param name="urlappendquery"></param>
        /// <param name="baseurl"></param>
        protected void openbrowserurls(List<string> urlappendquery, string baseurl) { openbrowserurls(urlappendquery, baseurl, "Select one or more symbols/models."); }
        protected void openbrowserurls(List<string> urlappendquery,string baseurl, string nosymerror)
        {
            if (urlappendquery.Count == 0)
            {
                status(nosymerror);
                return;
            }
            foreach (string query in urlappendquery)
            {
                try
                {
                    // encode query
                    var q = System.Web.HttpUtility.UrlEncode(query);
                    System.Diagnostics.Process.Start(baseurl+q);
                }
                catch { }
            }
        }


        /// <summary>
        /// adds right click option to google finance selected symbols
        /// </summary>
        protected void rightaddgooglesym()
        {
            rightadd("show in google", rightgoogleselected);
        }


        /// <summary>
        /// adds right click menu to lookup symbol in yahoo finance
        /// </summary>
        protected void rightaddyahoosym()
        {
            rightadd("show in yahoo", rightyahooselected);
        }

        /// <summary>
        /// opens symbols in google finance
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void rightgoogleselected(string clicktext)
        {
            openbrowserurls(getselectedsymbols(), getgooglefinanceurl());
        }



        /// <summary>
        /// opens symbols in yahoo finance
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void rightyahooselected(string clicktext)
        {
            openbrowserurls(getselectedsymbols(), getyahoofinanceurl());
        }


        /// <summary>
        /// adds seperate to right click menu
        /// </summary>
        protected void rightaddsep()
        {
            if (lastmenu == null)
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
            else
                lastmenu.DropDownItems.Add(new ToolStripSeparator());
        }

        bool _verb = true;

        /// <summary>
        /// whether extended debugging is heard
        /// </summary>
        public bool VerboseDebugging { get { return _verb; } set { _verb = value; } }

        protected virtual void v(string msg)
        {
            if (VerboseDebugging)
            {
                debug(msg);
            }
        }


        ToolStripMenuItem lastmenu = null;

        /// <summary>
        /// reset current menu level
        /// </summary>
        protected void rightadd() { rightadd(string.Empty, null); }


        /// <summary>
        /// add a menu level
        /// </summary>
        /// <param name="name"></param>
        protected void rightadd(string name) { rightadd(name, null); }

        protected void clearcontextmenu()
        {
            ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            ContextMenuStrip.ShowCheckMargin = false;
            ContextMenuStrip.ShowImageMargin = false;
            ContextMenuStrip.ShowItemToolTips = true;
            lastmenu = null;
        }


        /// <summary>
        /// add a clickable item under most recent menu level
        /// </summary>
        /// <param name="name"></param>
        /// <param name="viewclick"></param>
        protected void rightadd(string name, DebugDelegate viewclick)
        {
            if (ContextMenuStrip == null)
                clearcontextmenu();
            if (viewclick == null)
            {
                // see if we're under a menu or at top level
                if (name == string.Empty)
                {
                    // next item will be at top level unless specified otherwise
                    lastmenu = null;
                    // don't do anything else
                    return;
                }
                // see if level has changed
                if ((lastmenu == null) || (lastmenu.Text != name))
                {
                    // create a new menu
                    lastmenu = new ToolStripMenuItem(name);
                    lastmenu.DropDownItems.Clear();
                    ((ToolStripDropDownMenu)lastmenu.DropDown).ShowCheckMargin = false;
                    ((ToolStripDropDownMenu)lastmenu.DropDown).ShowImageMargin = false;
                    ContextMenuStrip.Items.Add(lastmenu);


                }
            }
            if (viewclick != null)
            {
                if (lastmenu == null)
                {
                    ContextMenuStrip.Items.Add(name, null, contextclick);
                    ContextMenuStrip.Items[ContextMenuStrip.Items.Count - 1].Tag = viewclick;
                }
                else
                {
                    lastmenu.DropDownItems.Add(name, null, contextclick);
                    lastmenu.DropDownItems[lastmenu.DropDownItems.Count - 1].Tag = viewclick;
                }
            }
        }

        protected void rightaddorders_all()
        {
            rightadd("orders");
            rightaddticket();
            rightadddest();
            rightaddaccountset();
            rightadd();
        }

        protected void rightaddticket()
        {
            rightadd("new order ticket", rightaddticket);
        }
        /// <summary>
        /// default order size for an order ticket
        /// </summary>
        public int DefaultTicketSize = 100;

        protected void rightaddticket(string click)
        {
            // get selected symbols
            var syms = getselectedsymbols();
            if (syms.Count == 0)
            {
                status("Select one or more symbols/models.");
                return;
            }
            // get starting location
            var start = Parent.Location;
            // open tickets
            for (int i = 0; i<syms.Count; i++)
            {
                string sym = syms[i];
                if (string.IsNullOrWhiteSpace(sym))
                {
                    continue;
                }
                Ticket ticket = new Ticket(new BuyLimit(sym, DefaultTicketSize, 0));
                // tile multiple tickets
                ticket.Top = start.Y + (i * ticket.Height);
                ticket.SendOrder += new OrderDelegate(sendorder);
                ticket.Show();
            }
            debug("Opened order tickets for: " + Util.join(syms));
        }
        /// <summary>
        /// sends orders from view-originated tickets
        /// </summary>
        public event OrderDelegate SendOrderEvent;

        protected virtual void sendorder(Order o)
        {
            if (string.IsNullOrWhiteSpace(o.ex) && !string.IsNullOrWhiteSpace(DefaultDestination))
                o.ex = DefaultDestination;
            if (string.IsNullOrWhiteSpace(o.Account) && !string.IsNullOrWhiteSpace(DefaultAccount))
                o.Account = DefaultAccount;
            if (SendOrderEvent != null)
                SendOrderEvent(o);
            else
                debug("View: " + ViewName + " is not enabled to send orders.");
        }

        protected void rightadd_defaults() { rightadd_defaults(true); }
        protected void rightadd_defaults(bool donewithexports)
        {
            rightadd("columns");
            rightadd("freeze cols++", colfreeze_inc);
            rightadd("freeze cols--", colfreeze_dec);
            rightadd("export");
            rightadd("all to file", rightexport);
            rightaddsymclipsel();
            if (donewithexports)
                rightadd();
        }


        /// <summary>
        /// default right click menu
        /// </summary>
        
        public virtual void CreateRightClick()
        {
            rightadd_defaults(true);
        }


        string _name = "GenericView";
        /// <summary>
        /// name of this view
        /// (used when exporting models to files)
        /// </summary>
        public string ViewName { get { return _name; } set { _name = value; } }

        /// <summary>
        /// convert a model to an exportable format
        /// </summary>
        /// <param name="it"></param>
        /// <returns></returns>
        protected virtual List<string> ToItems(T it)
        {
            if (mygvi != null)
                return mygvi.ToItems(it);
            return it.ToItem();
            
        }


        /// <summary>
        /// grab symbols from text
        /// </summary>
        /// <param name="symtext"></param>
        /// <returns></returns>
        protected string[] parsesymbols(string symtext) { return parsesymbols(symtext, true); }
        /// <summary>
        /// grab symols from text, control whether lowercase is allowed
        /// </summary>
        /// <param name="symtext"></param>
        /// <param name="toupper"></param>
        /// <returns></returns>
        protected string[] parsesymbols(string symtext, bool toupper)
        {
            if (toupper)
                symtext = symtext.ToUpper();
            return BasketImpl.parsedata(symtext, false, false, null).ToSymArray();
        }
        protected virtual string[] getsymbols_popup_raw() { return getsymbols_popup(true, true); }
        /// <summary>
        /// get a popup prompting user for symbols
        /// </summary>
        /// <returns></returns>
        protected virtual string[] getsymbols_popup() { return getsymbols_popup(false,true); }
        /// <summary>
        /// get a popup prompting user for symbols, control whether lowercase allowed
        /// </summary>
        /// <param name="toupper"></param>
        /// <returns></returns>
        protected virtual string[] getsymbols_popup(bool forceraw, bool toupper)
        {
            // get list of symbols
            string symtext = TextPrompt.Prompt("Model Symbol List", "Enter symbols to add, seperated by commas." + Environment.NewLine + "eg: IBM,GOOG,GE");
            if (forceraw || symtext.Contains(','))
            {
                debug("Read symbols in raw form: " + symtext);
                var b = BasketImpl.FromString(symtext);
                return b.ToSymArrayFull();
            }
            else
            {
                if (toupper)
                    symtext = symtext.ToUpper();
                var syms2add = removesymbolheader(BasketImpl.parsedata(symtext, false, false, null).ToSymArray());
                debug("No commas found, parsing symbols as: " + Util.cjoin(syms2add));
                return syms2add;
            }

        }
        /// <summary>
        /// open a chart for a symbol
        /// </summary>
        /// <param name="sym"></param>
        protected void openchart(string sym)
        {
            var chart = BarListImpl.GetChart(sym,1,TryCache,200,debug);
            chart.DefaultInterval = BarInterval.Day;
            Chart c = new Chart(chart);
            
            c.Show();
        }
        /// <summary>
        /// open chart for many symbols
        /// </summary>
        /// <param name="syms"></param>
        protected void openchart(List<string> syms)
        {
            if (syms.Count == 0)
            {
                status("Select one or more symbols/models to chart.");
                return;
            }
            foreach (string sym in syms)
                openchart(sym);
        }
        /// <summary>
        /// add right click option to open chart for selected symbols
        /// </summary>
        protected void rightaddopenchart()
        {
            rightadd("open chart", openchartselected);
        }
        /// <summary>
        /// opens chart for selected symbols
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void openchartselected(string clicktext)
        {
            openchart(getselectedsymbols());
        }
        /// <summary>
        /// gets any symbols from clipboard
        /// </summary>
        /// <returns></returns>
        protected virtual string[] getsymbolsclipboard() { return getsymbolsclipboard(true); }
        protected virtual string[] getsymbolsclipboard(bool toupper)
        {
            // get clipboard symbols
            string symtext = System.Windows.Forms.Clipboard.GetText().ToUpper();
            var syms2add = BasketImpl.parsedata(symtext, false, false, null).ToSymArray();
            return removesymbolheader(syms2add);
        }

        public bool isFlatPositionsIncluded = false;

        protected virtual string[] getsymbolspositions()
        {
            var pt = GetCurrentPositions();
            List<string> symbols = new List<string>();
            for (int i = 0; i < pt.Count; i++)
            {
                var p = pt[i];
                if ((p.isFlat && isFlatPositionsIncluded) || !p.isFlat)
                {
                    if (!symbols.Contains(p.symbol))
                        symbols.Add(p.symbol);

                }
            }
            return symbols.ToArray();
        }

        /// <summary>
        /// removes header row from symbols copied from clipboard
        /// </summary>
        /// <param name="syms"></param>
        /// <returns></returns>
        protected string[] removesymbolheader(string[] syms)
        {
            List<string> fsyms = new List<string>(syms.Length);
            foreach (string s in syms)
                if (s.ToLower() == "symbol")
                    continue;
                else
                    fsyms.Add(s);
            return fsyms.ToArray();
        }

        /// <summary>
        /// dumps entire grid in an exportable format
        /// </summary>
        /// <returns></returns>
        protected virtual List<List<string>> ToAllItems()
        {
            List<List<string>> items = new List<List<string>>();
            foreach (T item in models)
                items.Add(ToItems(item));
            return items;
        }
        /// <summary>
        /// exports entire grid to a csv file
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void rightexport(string clicktext)
        {
            var all = ToAllItems();
            if (all.Count==0)
            {
                status("Add symbols before exporting.");
                return;
            }
            string fn = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+Util.ToTLDate()+"."+ViewName+".csv";
            if (Util.SaveCsv(colnames, all, fn, debug))
                debug("exported to desktop: " + fn);
            else
                debug("export error.");
        }

        protected void rightaddremove_both()
        {
            rightadd("remove");
            rightaddremovesel();
            rightaddremoveall();
            rightadd();
        }

        /// <summary>
        /// adds right click option to remove selected models
        /// </summary>
        protected void rightaddremovesel()
        {
            rightadd("remove selected", rightremove);
        }
        /// <summary>
        /// add right click option to remove all models
        /// </summary>
        protected void rightaddremoveall()
        {
            rightadd("remove all", rightclear);
        }
        
        /// <summary>
        /// removes all models (w/confirmation)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected virtual void rightclear(string clicktext)
        {
            if (models.Count == 0)
            {
                status("Add symbols/models before removing.");
                return;
            }
            if (MessageBox.Show("Remove all " + models.Count + " items?", "Confirm Remove", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Clear();
            }
            
        }

        public int Count { get { return models.Count; } }

        /// <summary>
        /// clear view
        /// </summary>
        public void Clear()
        {
            var syms = getallsymbols();
            if (ModelRemoveStartedEvent != null)
                ModelRemoveStartedEvent();
            dt.Clear();
            models.Clear();
            if (SendRemoveModelEvent != null)
            {
                for (int i = 0; i < syms.Count; i++)
                {
                    SendRemoveModelEvent(syms[i]);
                }
            }
            if (ModelRemoveCompleteEvent != null)
                ModelRemoveCompleteEvent();
                
            refreshnow();
        }

        public event VoidDelegate SendNextViewRequestEvent;
        public event VoidDelegate SendPreviousViewRequestEvent;
        public event DebugDelegate SendViewRequestEvent;

        protected void requestnextview(string clicktext)
        {
            if (SendNextViewRequestEvent != null)
                SendNextViewRequestEvent();
            else
                debug("No view request event defined.");
        }

        protected void requestprevview(string clicktext)
        {
            if (SendPreviousViewRequestEvent != null)
                SendPreviousViewRequestEvent();
            else
                debug("No previous view request event defined.");
        }


        void contextclick(object send, EventArgs e)
        {
            // get event
            var evt = (TradeLink.API.DebugDelegate)((ToolStripItem)send).Tag;
            // get clicked text
            var txt = (string)((ToolStripItem)send).Text;
            if (evt != null)
                evt(txt);

        }

        protected string myex = string.Empty;
        public string DefaultDestination { get { return myex; } set { myex = value; } }

        protected string myaccount = string.Empty;
        public string DefaultAccount { get { return myaccount; } set { myaccount = value; } }

        protected void rightaddaccountset()
        {
            rightadd("change account", rightaddaccountset);
        }
        protected void rightaddaccountset(string click)
        {
            var org = DefaultAccount;
            DefaultAccount = TextPrompt.Prompt("Change Account", "Your account name is currently set to: '" + DefaultAccount + "'. Enter new account: ", DefaultAccount);
            if (org != DefaultAccount)
                debug("user changed account to: " + DefaultAccount);
            else
                debug("user canceled account change.");
        }

        protected void rightadddest()
        {
            rightadd("change destination", rightadddest);
        }
        protected void rightadddest(string click)
        {
            var org = DefaultDestination;
            DefaultDestination = TextPrompt.Prompt("Change Destination/Exchange", "Your default destination is currently set to: '" + DefaultDestination + "'. Enter new destination or exchange: ", DefaultDestination);
            if (org != DefaultDestination)
                debug("user changed destination to: " + DefaultDestination);
            else
                debug("user canceled destination change.");
        }

        

        /// <summary>
        /// add right click for all views
        /// </summary>
        protected void rightaddviewsall() { rightaddviewsall(false); }
        /// <summary>
        /// add right click for all views with prev/next control
        /// </summary>
        /// <param name="prevandnext"></param>
        protected void rightaddviewsall(bool prevandnext)
        {
            var _views = getallviews();
            if (_views.Count > 1)
            {
                rightadd("views");

                foreach (var v in _views)
                {
                    // don't add view to oneself
                    if (v == ViewName)
                        continue;
                    rightadd(v, requestview);
                }
                if (prevandnext)
                {
                    rightadd("next", requestnextview);
                    rightadd("previous", requestprevview);
                }
                rightadd();
            }
        }

        protected virtual void requestview(string view)
        {

            if (SendViewRequestEvent != null)
                SendViewRequestEvent(view);
            else
                debug("No next view request event defined.");
        }

        /// <summary>
        /// adds right click option to add symbols from file
        /// </summary>
        protected void rightaddsym_file()
        {
            rightadd("add from file", rightsymfile);
        }

        /// <summary>
        /// adds models from symbols found in a file
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected virtual void rightsymfile(string clicktext)
        {
            symbols2add = getsymbols_file();
            addsymsnow();
        }

        /// <summary>
        /// gets symbols from a file chosen by user
        /// </summary>
        /// <returns></returns>
        protected virtual string[] getsymbols_file() { return getsymbols_file(false, true); }
        protected virtual string[] getsymbols_file(bool forceraw, bool toupper)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var sr = new System.IO.StreamReader(ofd.OpenFile());
                string symtext = sr.ReadToEnd();
                sr.Close();

                if (forceraw || symtext.Contains(','))
                {
                    debug("Read symbols in raw form: " + symtext+" from: "+ofd.FileName);
                    var b = BasketImpl.FromString(symtext);
                    return b.ToSymArrayFull();
                }
                else
                {
                    if (toupper)
                        symtext = symtext.ToUpper();
                    var syms2add = removesymbolheader(BasketImpl.parsedata(symtext, false, false, null).ToSymArray());
                    debug("No commas found in: "+ofd.FileName+", parsing symbols as: " + Util.cjoin(syms2add));
                    return syms2add;
                }

                

            }
            else
                debug("user canceled symbol add");
            return new string[0];
        }




        /// <summary>
        /// removes selected symbols (w/user confirm)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected virtual void rightremove(string clicktext)
        {
            
            int[] rows = selectedrows(dv);
            if (rows.Length == 0)
            {
                status("Select symbols/models to remove.");
                return;
            }
            // confirm 
            if (MessageBox.Show("Remove " + rows.Length + " items?", "Confirm Remove", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // ensure they're sorted
                Array.Sort(rows);
                // reverse
                Array.Reverse(rows);
                if (ModelRemoveStartedEvent!= null)
                    ModelRemoveStartedEvent();
                // delete them
                for (int i = 0; i<rows.Length; i++)
                {
                    var r = rows[i];
                    removemodel(r);
                }
                if (ModelRemoveCompleteEvent != null)
                    ModelRemoveCompleteEvent();
                selectallrows_clear();
                refreshnow();
            }
            else
                debug("Remove canceled by user.");
        }

        
        public bool removemodel(int row)
        {
            var r = row;
            if ((r < 0) || (r >= dt.Rows.Count))
            {
                debug("unable to delete invalid row: " + r + " max: " + dt.Rows.Count);
                return false;
            }
            var sym = models[r].symbol;
            dt.Rows.RemoveAt(r);
            models.RemoveAt(r);
            debug("deleted row: " + r);
            // notify
            if (SendRemoveModelEvent != null)
                SendRemoveModelEvent(sym);
            return true;
        }

        public event DebugDelegate SendRemoveModelEvent;
        public event VoidDelegate ModelRemoveCompleteEvent;
        public event VoidDelegate ModelRemoveStartedEvent;

        /// <summary>
        /// listen to view debugging information
        /// </summary>
        public event DebugDelegate SendDebugEvent;
        /// <summary>
        /// send view debugging information
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);
        }

        /// <summary>
        /// send status info
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void status(string msg)
        {
            if (SendStatusEvent != null)
                SendStatusEvent(msg);
        }

        public new void Hide()
        {
            if (InvokeRequired)
                Invoke(new VoidDelegate(Show));
            else
            {
                Visible = false;
                Invalidate();
            }
        }

        public new void Show()
        {
            if (InvokeRequired)
                Invoke(new VoidDelegate(Show));
            else
            {
                Visible = true;
                Invalidate();
            }
        }

        public void Toggle()
        {
            if (Visible)
                Hide();
            else
                Show();
        }


        /// <summary>
        /// select all models
        /// </summary>
        protected void selectallrows()
        {
            dv.SelectAll();

        }
        /// <summary>
        /// unselect all models
        /// </summary>
        protected void selectallrows_clear()
        {
            dv.ClearSelection();
        }



        internal void NOMV_MouseUp(object sender, MouseEventArgs e)
        {
            
        }


        protected int mysleep = 250;
        /// <summary>
        /// sleep (milliseconds) between option requests
        /// </summary>
        public int Sleep
        {
            get { return mysleep; }
            set
            {
                
                if (value == 0)
                    debug("disabling sleeping between data pulls.");
                else
                    debug("between data pulls, sleeping millisec: " + value);
                mysleep = value;
                showdefaults();
            }
        }



        /// <summary>
        /// dump model as native/raw objects
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual List<object> ToData(T item)
        {
            if (mygvi != null)
                return mygvi.ToData(item);
            return item.ToData();
        }

        /// <summary>
        /// holds formatting index (created by genericviewitem)
        /// </summary>
        protected int[] modformatcols = new int[0];
        /// <summary>
        /// holds formatting information (created by genericviewitem)
        /// </summary>
        protected string[] formats = new string[0];

        /// <summary>
        /// add models
        /// </summary>
        /// <param name="gts"></param>
        protected virtual void addmodels(List<T> gts)
        {
            refreshnow(true, false);
            
            for (int i = 0; i < gts.Count; i++)
            {
                var mod = gts[i];
                // ensure owner
                if (string.IsNullOrWhiteSpace(mod.owner))
                    mod.owner = Owner;
                gts[i] = mod;
                addmodel(mod, false);
            }
            
            refreshnow(false, true);
            setdgsize();
        }

        /// <summary>
        /// adds models
        /// </summary>
        /// <param name="model"></param>
        protected virtual void addmodel(T model) { addmodel(model, true); }
        /// <summary>
        /// adds models with screen refresh control
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="refresh"></param>
        protected virtual void addmodel(T mod, bool refresh)
        {
            try
            {
                object[] data = ToData(mod).ToArray();
                dt.Rows.Add(data);
                
                v("added model: "+mod.ToString()+" ["+mod.symbol+"/"+mod.GetType().Name+"]");
            }
            catch (Exception ex)
            {
                debug("error adding model: " + mod.ToString() + " [" + mod.symbol + "/" + mod.GetType().Name + "]" 
                    + " err: " + ex.Message + ex.StackTrace);
            }
            // perform refresh if requested
            if (refresh)
                refreshnow();


            
        }

        static string stripnamespace(string text)
        {
            var startidx = text.IndexOf('.');
            if (startidx < 0)
                return text;
            if (startidx==text.Length)
                return text;
            return text.Substring(startidx + 1, text.Length - startidx - 1);
        }

        protected void setcolfreeze(int freezecols)
        {
            try
            {
                var o = colfreeze;
                colfreeze = freezecols;
                if (colfreeze == dv.Columns.Count)
                    colfreeze--;
                if (colfreeze < 0)
                    colfreeze = 0;
                if (colfreeze != o)
                {
                    if (colfreeze > o)
                    {
                        for (int c = o; c < colfreeze + 1; c++)
                            dv.Columns[c].Frozen = true;
                    }
                    else
                        for (int c = o; c > colfreeze - 1; c--)
                            dv.Columns[c].Frozen = false;


                }
            }
            catch (IndexOutOfRangeException ex)
            {
                debug("unable to freeze " + freezecols + " columns, err: " + ex.Message + ex.StackTrace);
            }
        }

        int colfreeze = 1;
        /// <summary>
        /// freeze or unfree columns
        /// </summary>
        public int ColFreeze { get { return colfreeze; } set { setcolfreeze(value); } }

        /// <summary>
        /// freezes more columsn
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void colfreeze_inc(string clicktext)
        {
            ColFreeze++;
            
            debug("freezing: " + ColFreeze + " columns.");
        }
        /// <summary>
        /// freezes fewer columns
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void colfreeze_dec(string clicktext)
        {
            ColFreeze--;

            debug("freezing: " + ColFreeze + " columns.");
        }

        /// <summary>
        /// show model
        /// </summary>
        /// <param name="model"></param>
        public void ShowItems(T model)
        {
            models.Add(model);
            addmodel(model);
        }

        /// <summary>
        /// show models
        /// </summary>
        /// <param name="Noms"></param>
        public void ShowItems(List<T> Noms) { ShowItems(Noms, true); }
        /// <summary>
        ///  show models with ability to clear previosu models
        /// </summary>
        /// <param name="Noms"></param>
        /// <param name="clearcurrent"></param>
        public void ShowItems(List<T> Noms, bool clearcurrent)
        {
            // check if we're appending or not
            if (clearcurrent)
            {
                models.Clear();
                dt.Clear();
                models = Noms;
            }
            else
                models.AddRange(Noms);

            addmodels(Noms);

        }


        delegate void BoolDelegate(bool v, bool v2);

        /// <summary>
        /// refresh view now (force)
        /// </summary>
        public virtual void refreshnow() { refreshnow(false); }

        /// <summary>
        /// refresh now with control
        /// </summary>
        /// <param name="continuebatch"></param>
        protected virtual void refreshnow(bool continuebatch) { refreshnow(continuebatch, true); }
        protected virtual void refreshnow(bool continuebatch, bool forcerefresh)
        {

            if (dv.InvokeRequired)
            {
                try
                {
                    dv.Invoke(new BoolDelegate(refreshnow), new object[] { continuebatch, forcerefresh});
                }
                catch (ObjectDisposedException) { }
            }
            else
            {
                SafeBindingSource.refreshgrid(dv, bs,!continuebatch);
                if (forcerefresh)
                    Invalidate(true);
            }
        }

        /// <summary>
        /// models used by view
        /// </summary>
        protected List<T> models = new List<T>();
        /// <summary>
        /// gets zero-based incremental index of given length
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        int[] genindex(int len) { List<int> f = new List<int>(); for (int i = 0; i < len; i++) f.Add(i); return f.ToArray(); }

        void initformat() { initformat(modformatcols, formats); }
        delegate void formatdel(int[] modformats, string[] formats);
        bool didformat = false;
        
        /// <summary>
        /// sets up view formatting
        /// </summary>
        /// <param name="modformats"></param>
        /// <param name="formats"></param>
        protected virtual void initformat(int[] modformats, string[] formats)
        {
            if (dv.InvokeRequired)
                dv.Invoke(new formatdel(initformat),new object[] { modformats, formats});
            else
            {
                // only do once
                if (didformat)
                    return;
                // columns must exist in grid, not just table
                if (dv.Columns.Count != colnames.Count)
                    return;
                // ensure we have proper formats
                if ((modformats.Length == 0) && (formats.Length == colnames.Count) && (colnames.Count!=0))
                    modformats = genindex(colnames.Count);
                // apply formats
                DataGridViewCellStyle defaultstyle = new DataGridViewCellStyle();
                for (int i = 0; i < modformats.Length; i++)
                {
                    int col = modformats[i];
                    string colname = colnames[col];
                    var format = formats[i];
                    if (defaultstyle.Format != format)
                        dv.Columns[col].DefaultCellStyle.Format = format;
                    
                }
                // mark as done
                didformat = true;
                

            }
        }
        /// <summary>
        /// whether coloring is used for negative numbers and percentages
        /// </summary>
        bool _enablecolors = true;

        public bool isColoringEnabled { get { return _enablecolors; } set { _enablecolors = value; } }

        // init model information
        protected List<string> colnames = new List<string>();
        protected List<Type> coltypes = new List<Type>();

        protected List<bool> ispctfmt = new List<bool>();

        delegate void initdel(GenericViewItem<T> gvi);

        DataGridViewCellStyle gridstyle = new DataGridViewCellStyle();
        DataGridViewCellStyle negativestyle = new DataGridViewCellStyle();
        DataGridViewCellStyle positivestyle = new DataGridViewCellStyle();
        /// <summary>
        /// whether column sorting in the view is allowed
        /// </summary>
        public bool isSortAllowed = false;

        /// <summary>
        /// decimal places to display
        /// </summary>
        public int DefaultDecimalPlaces = config.DecimalPlaces;
        /// <summary>
        /// display decimals
        /// </summary>
        protected string DecimalFormatDisplay { get { return "N" + DefaultDecimalPlaces; } }
        /// <summary>
        /// export decimals
        /// </summary>
        protected string DecimalFormatExport { get { return "F" + DefaultDecimalPlaces; } }

        protected virtual void initgrid(GenericViewItem<T> gvi) 
        {
            if (dv.InvokeRequired)
                Invoke(new initdel(initgrid), new object[] { gvi});
            else
            {
                colnames = gvi.DimNames;
                coltypes = gvi.DimTypes;
                formats = gvi.DimFormats.ToArray();
                // init grid objects
                models.Clear();
                dt.Clear();
                dt = new System.Data.DataTable();
                dv = new DataGridView();
                bs = new SafeBindingSource(isSortAllowed);

                // create columns and default formats
                if (formats.Length != colnames.Count)
                    formats = new string[colnames.Count];
                for (int i = 0; i < colnames.Count; i++)
                {
                    var t = coltypes[i];
                    dt.Columns.Add(colnames[i], t);
                    // get suggested format
                    var fmt = formats[i];
                    // if format is supplied, use it
                    if (!string.IsNullOrWhiteSpace(fmt))
                        continue;
                    else // otherwise guess based on type
                    {
                        if (t == typeof(int) || t == typeof(long))
                            fmt = "N0";
                        else if (t == typeof(decimal) || (t == typeof(double)))
                            fmt = DecimalFormatDisplay;
                        else // otherwise use default
                            fmt = string.Empty;
                        formats[i] = fmt;
                    }
                }

                ispctfmt.Clear();
                foreach (var fmt in formats)
                {
                    ispctfmt.Add(fmt.Contains('P'));
                }

                // configure grid
                BackColor = Color.White;
                ForeColor = Color.Black;
                dv.BorderStyle = System.Windows.Forms.BorderStyle.None;
                dv.ForeColor = ForeColor;
                dv.BackgroundColor = BackColor;
                
                dv.AllowUserToAddRows = false;
                
                
                dv.AllowUserToDeleteRows = false;
                dv.AllowUserToOrderColumns = true;
                dv.AllowUserToResizeColumns = true;
                
                dv.RowHeadersVisible = false;
                dv.ColumnHeadersVisible = true;
                dv.Capture = true;
                dv.EnableHeadersVisualStyles = false;
                dv.CellBorderStyle = DataGridViewCellBorderStyle.None;
                dv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dv.ColumnHeadersDefaultCellStyle.BackColor = BackColor;
                dv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dv.BackgroundColor = BackColor;
                gridstyle = new DataGridViewCellStyle();
                gridstyle.BackColor = BackColor;
                gridstyle.ForeColor = ForeColor;
                dv.DefaultCellStyle = gridstyle;
                dv.RowHeadersDefaultCellStyle.BackColor = BackColor;
                dv.RowHeadersDefaultCellStyle.ForeColor = ForeColor;
                dv.AlternatingRowsDefaultCellStyle.BackColor = dv.BackgroundColor;
                dv.AlternatingRowsDefaultCellStyle.ForeColor = dv.ForeColor;
                dv.GridColor = BackColor;
                dv.MultiSelect = true;
                dv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dv.ReadOnly = true;
                dv.ColumnHeadersHeight = dv.ColumnHeadersDefaultCellStyle.Font.Height * 2;
                dv.CellFormatting += new DataGridViewCellFormattingEventHandler(dv_CellFormatting);
                dv.DataError += new DataGridViewDataErrorEventHandler(dv_DataError);
                dv.CellDoubleClick += new DataGridViewCellEventHandler(dv_CellDoubleClick);

                negativestyle = new DataGridViewCellStyle();
                negativestyle.BackColor = Color.DeepPink;
                positivestyle = new DataGridViewCellStyle();
                positivestyle.BackColor = Color.LightGreen;

                
                // bind everything together
                bs.DataSource = dt;
                dv.DataSource = bs;
                dv.Parent = this;
                dv.Dock = DockStyle.Fill;
                
                
                // initilize column formats (if possible at this time)
                initformat(modformatcols, formats);
                // set grid size
                setdgsize();

                // freeze requested columns
                int ocf = ColFreeze;
                ColFreeze = ocf;


            }


        }

        protected virtual void grid_doubleclick(int r, int c)
        {
        }

        void dv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var r = e.RowIndex;
            var c = e.ColumnIndex;
            if ((r < 0) || (r >= models.Count))
                return;
            if ((c < 0) || (c >= formats.Length))
                return;
            grid_doubleclick(r, c);
        }



        void dv_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            
        }

        void dv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (isColoringEnabled)
            {
                int r = e.RowIndex;
                int c = e.ColumnIndex;
                dvupdategridcell(r, c);
            }
        }




        protected virtual void dvupdategridcell(int r, int c)
        {

            bool setbg = false;
            DataGridViewCellStyle cur = null;
            try 
            {
                if (r < 0)
                    return;
                if (c < 0)
                    return;
                if (c>=formats.Length)
                    return;

                // get table index
                int idx = gridrow2tablerow(r);
            

                bool exists = (idx>=0) && (idx<dt.Rows.Count) && (dt.Rows[idx].RowState != DataRowState.Deleted);
                //int max = _tests.Count;

                // get colum type
                Type t = coltypes[c];
                // get format
                var fmt = formats[c];
                
                cur = dv[c, r].Style;

                if (exists && (t==typeof(decimal)) || (t==typeof(long)) || (t==typeof(int)) || (t==typeof(double)))
                {
                    // get value
                    decimal v = 0;
                    if (decimal.TryParse(dt.Rows[idx][c].ToString(), out v))
                    {
                        try
                        {
                            if (v < 0)
                            {
                                if (cur.BackColor != negativestyle.BackColor)
                                {
                                    setbg = true;

                                    cur = negativestyle;
                                }
                            }
                            else if ((v > 0) && ispctfmt[c])
                            {
                                if (cur.BackColor != positivestyle.BackColor)
                                {
                                    setbg = true;
                                    cur = positivestyle;
                                }
                            }
                            else
                            {
                                if (cur.BackColor != gridstyle.BackColor)
                                {
                                    cur = gridstyle;
                                    setbg = true;
                                }
                            }
                        }
                        catch { setbg = false; }
                    }

                }
            }
            catch (Exception ex)
            {
                debug("error on row: " + r + " c: " + c + " total: " + dt.Rows.Count + " err: " + ex.Message + ex.StackTrace);
                setbg = false;
            }
            if (setbg)
            {
                dv[c, r].Style = cur;
            }

        }

        protected virtual int[] selectedrows()
        {
            return selectedrows(this.dv);
        }

        public List<string> getallsymbols()
        {
            List<string> symbols = new List<string>();
            models.ForEach(m => symbols.Add(m.symbol));
            return symbols;
        }

        public virtual List<string> getselectedsymbols() { return selectedsymbols(this.dv, models); }

        public static List<string> selectedsymbols(DataGridView dg, List<T> noms)
        {
            List<string> syms = new List<string>();

            var rows = selectedrows(dg);

            foreach (var rowidx in rows)
            {
                if ((rowidx < 0) || (rowidx >= noms.Count))
                    continue;
                var mod =noms[rowidx];
                if ((mod!=null) && mod.isValid)
                    syms.Add(mod.symbol);
            }
            return syms;
        }

        protected List<T> getselectedmods()
        {
            List<T> syms = new List<T>();


            foreach (var rowidx in selectedrows())
            {
                var mod = models[rowidx];
                if (mod.isValid)
                    syms.Add(mod);
            }
            return syms;
        }

        internal static int[] selectedrows(DataGridView dg)
        {
            List<int> rows = new List<int>();
            foreach (DataGridViewRow row in dg.SelectedRows)
                rows.Add(row.Index);
                


            return rows.ToArray();

        }

        protected virtual int gridrow2tablerow(int disprow) 
        { 
            return gridrow2tablerow(dv,dt, disprow); 
        }

        internal static int gridrow2tablerow(DataGridView dg, DataTable tab, int disprow)
        {
            try
            {
                var tabrow = ((DataRowView)dg.Rows[disprow].DataBoundItem).Row;
                return tab.Rows.IndexOf(tabrow);
                
            }
            catch { }
            return -1;

        }

        protected  virtual int[] selectedrowsdisp()
        {
            return selectedrowsdisp(this.dv);
        }

        internal static int[] selectedrowsdisp(DataGridView dg)
        {
            List<int> rows = new List<int>();
            foreach (DataGridViewRow dr in dg.SelectedRows)
                rows.Add(dr.Index);

            return rows.ToArray();

        }


        delegate void datagriddel(DataGridView dg);
        protected virtual void setdgsize()
        {
            if (dv.InvokeRequired)
                dv.Invoke(new VoidDelegate(setdgsize));
            else
            {
                dv.AutoResizeColumns( DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }


    }

    /// <summary>
    /// used to simplify mapping models to views
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericViewItem<T> where T : Model
    {
        public GenericViewItem() : this(null) { }
        public GenericViewItem(DebugDelegate deb)
        {
            SendDebugEvent = deb;

        }
        /// <summary>
        /// names of each dimension
        /// </summary>
        public List<string> DimNames { get { return itemnames; } }
        /// <summary>
        /// type of each dimension
        /// </summary>
        public List<Type> DimTypes { get { return itemtypes; } }
        /// <summary>
        /// format of each dimension when displayed as string/exportable
        /// </summary>
        public List<string> DimFormats { get { return gridfmts; } }
        /// <summary>
        /// type of this item
        /// </summary>
        public Type ItemType { get { return typeof(T); } }
        /// <summary>
        /// formats for importing and exporting
        /// </summary>
        public List<string> itemformats = new List<string>();
        /// <summary>
        /// formats for display
        /// </summary>
        public List<string> gridfmts = new List<string>();
        /// <summary>
        /// object field mapping
        /// </summary>
        public List<string> fieldnames = new List<string>();
        Dictionary<string, bool> fieldnameisfield = new Dictionary<string, bool>();


        /// <summary>
        /// column/display name
        /// </summary>
        public List<string> itemnames = new List<string>();
        /// <summary>
        /// type of the object field (autodetected)
        /// </summary>
        public List<Type> itemtypes = new List<Type>();
        /// <summary>
        /// clear all viewitem information
        /// </summary>
        public void Clear()
        {
            itemformats.Clear();
            gridfmts.Clear();
            itemnames.Clear();
            fieldnames.Clear();
            itemtypes.Clear();
        }
        /// <summary>
        /// count of items in this view definition
        /// </summary>
        public int Count { get { return itemnames.Count; } }

        /// <summary>
        /// listen to errors and debugs from this view item
        /// </summary>
        public event DebugDelegate SendDebugEvent;

        /// <summary>
        /// send debugging for view item
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);
        }

        /// <summary>
        /// listen to status events
        /// </summary>
        public event DebugDelegate SendStatusEvent;
        /// <summary>
        /// send status info
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void status(string msg)
        {
            if (SendStatusEvent != null)
                SendStatusEvent(msg);
        }

        

        /// <summary>
        /// whether extended debugging is heard
        /// </summary>
        public bool VerboseDebugging = true;
        /// <summary>
        /// send extended debugging
        /// </summary>
        /// <param name="msg"></param>
        void v(string msg)
        {
            if (VerboseDebugging)
            {
                debug(msg);
            }
        }

        object getobj_field(Type t, T it, string field, bool update)
        {
            bool r = false;
            try
            {
                
                var mi = t.GetField(field);
                if (update)
                {
                    if (!fieldnameisfield.TryGetValue(field, out r))
                        fieldnameisfield.Add(field, true);
                }
                return mi.GetValue(it);
            }
            catch (Exception)
            {
                if (update)
                {
                    if (!fieldnameisfield.TryGetValue(field, out r))
                        fieldnameisfield.Add(field, false);
                }
            }
            return getobj_property(t,it,field);
        }

        object getobj_property(Type t, T it, string property)
        {
            var pi = t.GetProperty(property);
            if (pi == null)
            {
                v("unable to find property: " + property + " on: " + t.FullName + " in: " + it.ToString());
            }
            else
            {
                try
                {
                    var obj = pi.GetValue(it, null);
                    return obj;
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    string exs = ex.Message + ex.StackTrace;
                    if (ex.InnerException != null)
                        exs = ex.InnerException.Message + ex.InnerException.StackTrace + "    " + exs;
                    v("error getting: " + property + " from: " + it.symbol + " err: " + exs);
                }
            }
            return default(T);
        }

        GenericTracker<int> dynvalidx = new GenericTracker<int>();
        bool setdyn = true;
        int dynsymidx = -1;
        string getobj_dyn(T it, string name)
        {
            if (setdyn)
            {
                dynsymidx = -1;
                for (int i = 0; i < DimNames.Count; i++)
                {
                    if (DimNames[i] == "symbol")
                        dynsymidx = i;
                    dynvalidx.addindex(DimNames[i], i);
                }
                setdyn = false;
            }
            var idx = dynvalidx.getindex(name);
            if (idx < 0)
                return string.Empty; // should not happen
            if (idx == dynsymidx)
                return it.symbol;
            if (idx >= it.Value.Length)
                return string.Empty; // should not happen after indicators populated
            var v = it.Value[idx];
            return v;

        }

        object getobj(T it, string fieldorpropertyname)
        {
            if (it.isDynamic)
                return getobj_dyn(it, fieldorpropertyname);
            var t = it.GetType();

            bool isfield = true;
            if (fieldnameisfield.TryGetValue(fieldorpropertyname, out isfield))
            {
                if (isfield)
                    return getobj_field(t, it, fieldorpropertyname,false);
                return getobj_property(t, it, fieldorpropertyname);
            }

            return getobj_field(t, it, fieldorpropertyname,true);
            
        }


        Type gettype(string fieldorpropertyname)
        {
            var v = Activator.CreateInstance<T>();
            try
            {
                var mi = v.GetType().GetField(fieldorpropertyname);
                return mi.FieldType;
            }
            catch { }

            var pi = v.GetType().GetProperty(fieldorpropertyname);
            return pi.PropertyType;

            
        }

        string getval(T it, string fieldorpropertyname, string fmt)
        {
            var obj = getobj(it,fieldorpropertyname);
            if (fmt!=string.Empty)
                return string.Format("{0:"+fmt+"}",obj);
            return obj.ToString();
        }

        public List<string> ToItems(T it) 
        {
            List<string> its = new List<string>(itemnames.Count);
            for (int i = 0; i<itemnames.Count; i++)
                its.Add(getval(it, fieldnames[i], itemformats[i]));
            return its;
        }

        public List<object> ToData(T it)
        {
            List<object> its = new List<object>(itemnames.Count);
            for (int i = 0; i<fieldnames.Count; i++)
                its.Add(getobj(it, fieldnames[i]));
            return its;
        }



        /// <summary>
        /// defaults to a numeric grid format (eg N2)
        /// </summary>
        /// <param name="name"></param>
        public void AddFormat(string name)
        {
            string gridformat = config.DecimalFormatDisplay;
            AddFormat(name, gridformat, gridfmt2itemfmt(gridformat));
        }

        public void AddFormat(string name, string gridformat)
        {

            AddFormat(name, gridformat.ToUpper(), gridfmt2itemfmt(gridformat.ToUpper()));
        }

        static string gridfmt2itemfmt(string gridformat)
        {
            if (string.IsNullOrWhiteSpace(gridformat))
                return gridformat;
            string itmfmt = gridformat.ToUpper();
            // replace percents
            itmfmt = itmfmt.Replace("P0", "F2");
            itmfmt = itmfmt.Replace("P1", "F3");
            itmfmt = itmfmt.Replace("P2", "F4");
            // replace commas/numerics
            itmfmt = itmfmt.Replace("N", "F");
            return itmfmt;
        }

        public void AddFormat(string name, string gridformat, string itemfmt)
        {
            Add(name, name, gridformat, itemfmt);
        }

        /// <summary>
        /// add column with field of same name and no formatting
        /// </summary>
        /// <param name="name"></param>
        public void Add(string name) { Add(name,name, string.Empty); }
        /// <summary>
        /// add column with field of different name, and no formatting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldname"></param>
        public void Add(string name, string fieldname) { Add(name, fieldname, string.Empty); }
        /// <summary>
        /// specify grid formatting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldname"></param>
        /// <param name="gridformat"></param>
        public void Add(string name, string fieldname, string gridformat) 
        { 
            Add(name, fieldname, gridformat, gridfmt2itemfmt(gridformat)); 
        }
        /// <summary>
        /// specify item (aka export) formatting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldname"></param>
        /// <param name="gridformat"></param>
        /// <param name="itemfmt"></param>
        public void Add(string name, string fieldname, string gridformat, string itemfmt)
        {
            Add(name, fieldname, gridformat.ToUpper(), itemfmt, gettype(fieldname));
        }
        /// <summary>
        /// specify type explictly rather than autodetect
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldname"></param>
        /// <param name="gridformat"></param>
        /// <param name="itemfmt"></param>
        /// <param name="t"></param>
        public void Add(string name, string fieldname, string gridformat, string itemfmt, Type t)
        {
            fieldnames.Add(fieldname);
            
            itemnames.Add(name);
            itemformats.Add(itemfmt.ToUpper());
            gridfmts.Add(gridformat.ToUpper());
            itemtypes.Add(t);
        }
    }
}
