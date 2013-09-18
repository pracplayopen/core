using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TradeLink.API;
using TradeLink.Common;

namespace TradeLink.AppKit
{
    /// <summary>
    /// Used to pass changes to barlists
    /// </summary>
    public delegate void BarListUpdated(BarListImpl newbl);

    /// <summary>
    /// A generic charting form that plots BarList objects
    /// </summary>
    public partial class ChartControl : UserControl
    {

        //public event SecurityDelegate FetchStock;
        BarList bl = null;
        public BarList Bars 
        { 
            get { return bl; } 
            set { NewBarList(value); } 
        }
        bool _alwaysupdate = false;
        /// <summary>
        /// if set, control will autorefresh with each tick.
        /// otherwise, refresh must be called manually.
        /// manual is recommended during rapid updates, as the chart may flash otherwise.
        /// </summary>
        public bool AutoUpdate { get { return _alwaysupdate; } set { _alwaysupdate = value; } }
        /// <summary>
        /// create bars from ticks
        /// </summary>
        /// <param name="k"></param>
        public void newTick(Tick k)
        {
            if (bl == null)
            {
                Symbol = k.symbol;
                bl = new BarListImpl(k.symbol);
            }
            bl.newTick(k);
            if (k.isTrade)
            {
                if (k.trade > highesth)
                    highesth = k.trade;
                if (k.trade < lowestl)
                    lowestl = k.trade;
            }
            if (_alwaysupdate)
                redraw();
        }
        /// <summary>
        /// create bars from points
        /// </summary>
        /// <param name="p"></param>
        /// <param name="time"></param>
        /// <param name="date"></param>
        /// <param name="size"></param>
        public void newPoint(string symbol, decimal p, int time, int date, int size)
        {
            if (bl == null)
            {
                Symbol = symbol;
                highesth = SMALLVAL;
                bl = new BarListImpl(symbol);
            }
            bl.newPoint(symbol,p, time, date, size);
            if (p!=0)
            {
                if (p > highesth)
                    highesth = p;
                if (p < lowestl)
                    lowestl = p;
            }
            if (_alwaysupdate)
                redraw();
        }

        int barc { get { return bl == null ? 0 : bl.Count; } }

        /// <summary>
        /// force a manual refresh of the chart
        /// </summary>
        public void redraw()
        {
            if (InvokeRequired)
                Invoke(new VoidDelegate(redraw));
            else
            {
                Invalidate(true);
                Update();
            }
        }

        /// <summary>
        /// controls whether right click menu can be selected
        /// </summary>
        public bool DisplayRightClick { get { return chartContextMenu.Enabled; } set { chartContextMenu.Enabled = !chartContextMenu.Enabled; redraw(); } }

        /// <summary>
        /// reset the chart and underlying data structures
        /// </summary>
        public void Reset()
        {
            if (InvokeRequired)
                Invoke(new VoidDelegate(Reset));
            else
            {
                bl = null;
                sym = string.Empty;
                highesth = 0;
                lowestl = BIGVAL;
                hasosctextlabels = false;
                hastextlabels = false;
                _collineend.Clear();
                _collineend_osc.Clear();
                _colpoints.Clear();
                _colpoints_osc.Clear();
                redraw();
            }
        }
        string sym = string.Empty;
        public string Symbol { get { return sym; } set { sym = value; settitle(Title); } }

        void settitle(string tit)
        {
            if (InvokeRequired)
                Invoke(new DebugDelegate(settitle), new object[] { tit });
            else
            {
                Text = tit;
            }
        }
        Graphics g = null;
        string mlabel = null;
        decimal highesth = 0;
        const decimal SMALLVAL = -100000000000000;
        const decimal BIGVAL = 10000000000000000000;
        decimal lowestl = BIGVAL;

        Rectangle r;
        int border = 60;
        int hborder = 60;
        decimal pixperbar = 0;
        decimal  pixperdollar = 0;
        

        public void InvertColors()
        {
            if (InvokeRequired)
                Invoke(new VoidDelegate(InvertColors));
            else
            {
                if (BackColor == Color.Black)
                    BackColor = Color.White;
                else
                    BackColor = Color.Black;
                redraw();
            }
        }

        
        public ChartControl() : this(null,false) { }
        public ChartControl(BarList b) : this(b, false) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Chart"/> class.
        /// </summary>
        /// <param name="b">The barlist.</param>
        /// <param name="allowtype">if set to <c>true</c> [allowtype] will allow typing/changing of new symbols on the chart window.</param>
        public ChartControl(BarList b,bool allowtype)
        {
            InitializeComponent();
            Paint += new PaintEventHandler(Chart_Paint);
            MouseDoubleClick += new MouseEventHandler(ChartControl_MouseDoubleClick);
            MouseWheel +=new MouseEventHandler(Chart_MouseUp);
            if (b != null)
            {
                bl = b;
                Symbol = b.symbol;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                if (DisplayCursor && (bl != null))
                {
                    int x = e.X;
                    int y = e.Y;

                    ChartControl f = this;
                    g = CreateGraphics();
                    float size = g.MeasureString(highesth.ToString(), f.Font).Width + g.MeasureString("255000 ", f.Font).Width + 10000;

                    //Box location
                    float boxX = intervalstringendx + 3;
                    int boxY = 3;

                    //Clean current box
                    g.FillRectangle(new SolidBrush(f.BackColor), boxX, boxY, size, f.Font.Height);
                    _curbar = getBar(x);
                    _curprice = getPrice(y);
                    size = g.MeasureString(highesth.ToString(), f.Font).Width + g.MeasureString("255000 ", f.Font).Width;
                    int time = (_curbar<0)||(_curbar>bl.Last) ? 0 : (bl.DefaultInterval== BarInterval.Day ? bl[_curbar].Bardate :  (int)((double)bl[_curbar].Bartime/100));
                    string times = time == 0 ? string.Empty : time.ToString();
                    string price = _curprice == 0 ? string.Empty : _curprice.ToString("F2");
                    string OHLC = string.Format(" Open:{0} High:{1} Low:{2} Close:{3}", bl[_curbar].Open.ToString("0.######"), bl[_curbar].High.ToString("0.######"), bl[_curbar].Low.ToString("0.######"), bl[_curbar].Close.ToString("0.######"));

                    g.DrawString("Time:" + time + " Price:" + price + OHLC, f.Font, new SolidBrush(fgcol), boxX, boxY);
                }
            }
            catch { }
            base.OnMouseMove(e);
        }

        int CurrentChartMouseTime
        {
            get
            {
                if (bl == null)
                    return 0;
                if ((_curbar < 0) || (_curbar > bl.Last))
                    return 0;
                if (bl.DefaultInterval == BarInterval.Day)
                    return bl[_curbar].Bardate; 
                // get bar time sans seconds
                var bt = (int)((double)bl[_curbar].Bartime / 100);
                return bt;
            }
        }

        public event DebugDelegate SendDebug;
        internal void debug(string msg)
        {
            if (SendDebug != null)
                SendDebug(msg);
        }

        public bool TakeScreenShot() { return TakeScreenShot(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + Symbol + bl.Date()[bl.Last] + ".png"); }
        public bool TakeScreenShot(string fn)
        {
            try
            {
                
                ScreenCapture sc = new ScreenCapture();
                sc.CaptureWindowToFile(Handle, fn, System.Drawing.Imaging.ImageFormat.Png);
                return true;
            }
            catch (Exception ex)
            {
                debug("Error writing: " + fn);
                debug(ex.Message + ex.StackTrace);
            }
            return false;
        }

        public event Int32Delegate SendChartDoubleClick;

        void ChartControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (SendChartDoubleClick != null)
                    SendChartDoubleClick(CurrentChartMouseTime);
                debug("User double-clicked at time: " + CurrentChartMouseTime);
            }
            catch (Exception ex)
            {
                debug("Error processing chart double click: " + ex.Message + ex.StackTrace);
            }

        }

        public void NewBarList(BarList barlist)
        {
            if ((barlist != null) && (barlist.isValid))
                Symbol = barlist.symbol;
            if ((barlist == null) || (barlist.Intervals.Length==0) || (barlist.Count==0))
            {
                return;
            }
            bl = barlist;
            highesth = Calc.HH(bl);
            lowestl = Calc.LL(bl);
            redraw();
        }


        
        int getX(int bar) { return (int)(bar * pixperbar) + (border/3); } 
        
        int getY(decimal price) 
        {
            if (price > highesth)
                price = highesth;
            else if (price < lowestl)
                price = lowestl;
            return (int)(((decimal)hborder/3)+((highesth-price) * pixperdollar));
            
        }

        int getBar(int X) 
        {
            if (bl == null) return 0;
            int b = (int)((X - (border / 3)) / pixperbar);
            if (b < 0) return 0;
            if (b >= bl.Count) return bl.Last;
            return b;
        }
        decimal getPrice(int Y)
        {
            if (bl == null)
                return 0;
            if (pixperdollar == 0)
                return 0;
            // reverse of getY(price)
            decimal p = highesth + ((hborder / 3) / pixperdollar) - Y / pixperdollar;
            if (p > highesth)
                return highesth;
            else if (p < lowestl)
                return lowestl;
            return p;
        }

        Color fgcol { get { return (BackColor == Color.Black) ? Color.White : Color.Black; } }

        bool _isuseshadedclose = false;

        public bool isUsingShadedCloses { get { return _isuseshadedclose; } set { _isuseshadedclose = value; } }

        public Color UpBarColor = Color.Green;
        public Color DownBarColor = Color.Red;

        public string MissingDataMessage = "No Data.";

        /// <summary>
        /// Gets the title of this chart.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get { if (bl == null) return Symbol; return Symbol + " " + Enum.GetName(typeof(BarInterval), bl.DefaultInterval).ToString(); } }
        public void Chart_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // get form
                ChartControl f = (ChartControl)sender;
                // get graphics
                g = e.Graphics;
                if ((bl == null) || (bl.Intervals.Length == 0) || (bl.Count == 0))
                {
                    g.DrawString(MissingDataMessage, new Font(FontFamily.GenericSerif, 10, FontStyle.Bold), Brushes.Black, new PointF(r.Width / 3, r.Height / 2));
                    return;
                }
                lock (bl)
                {

                    // get window
                    r = ClientRectangle;
                    // get title
                    Text = Title;
                    border = (int)(g.MeasureString(bl.High()[0].ToString(), f.Font).Width);
                    // setup to draw

                    Pen p = new Pen(fgcol);
                    g.Clear(f.BackColor);
                    // get number of pixels available for each bar, based on screensize and barcount
                    pixperbar = (((decimal)r.Width - (decimal)border - ((decimal)border / 3)) / (decimal)barc);
                    // pixels for each time stamp
                    const int pixperbarlabel = 60;
                    // number of labels we have room to draw
                    int numbarlabels = (int)((double)(r.Width - border - ((double)border / 3)) / pixperbarlabel);
                    // draw a label every so many bars (assume every bar to star)
                    int labeleveryX = 1;
                    // if there's more bars than space
                    if (barc > numbarlabels)
                        labeleveryX = (int)Math.Round(((double)barc / numbarlabels));
                    // get dollar range for chart
                    decimal range = (highesth - lowestl);
                    // get pixels available for each dollar of movement
                    pixperdollar = range == 0 ? 0 : (((decimal)r.Height - hborder*1.27m) / range);
                    

                    // x-axis
                    g.DrawLine(new Pen(fgcol), (int)(border / 3), r.Height - hborder, r.Width - border, r.Height - hborder);
                    // y-axis
                    g.DrawLine(new Pen(fgcol), r.Width - border, r.Y + ((float)hborder/3), r.Width - border, r.Height - hborder);

                    const int minxlabelwidth = 30;


                    int lastlabelcoord = -500;
                    int lastmonthyearcoord = -500;

                    for (int i = 0; i < barc; i++)
                    {
                        // get bar color
                        Color bcolor = (bl.Close()[i] > bl.Open()[i]) ? UpBarColor : DownBarColor;
                        p = new Pen(bcolor);
                        try
                        {
                            if (isUsingShadedCloses)
                            {
                                // skip first bar
                                if (i>0)
                                {
                                    // get close of previouc
                                    var pcx = getX(i-1);
                                    var pcy = getY(bl.Close()[i-1]);
                                    var cx = getX(i);
                                    var cy = getY(bl.Close()[i]);
                                    var pts = new Point[] { 
                                        new Point(pcx,getY(lowestl)),
                                        new Point(pcx,pcy),
                                        new Point(cx,cy),
                                        new Point(cx,getY(lowestl))
                                    };

                                    g.FillPolygon(new SolidBrush(Color.CadetBlue),pts);
                                }
                            }

                            else
                            {
                                // draw high/low bar
                                g.DrawLine(p, getX(i), getY(bl.Low()[i]), getX(i), getY(bl.High()[i]));
                                // draw open bar
                                g.DrawLine(p, getX(i), getY(bl.Open()[i]), getX(i) - (int)(pixperbar / 3), getY(bl.Open()[i]));
                                // draw close bar
                                g.DrawLine(p, getX(i), getY(bl.Close()[i]), getX(i) + (int)(pixperbar / 3), getY(bl.Close()[i]));
                            }


                            // draw time labels (time @30min and date@noon)

                            // if interval is intra-day
                            if (bl.DefaultInterval != BarInterval.Day)
                            {
                                // every 6 bars draw the bartime
                                if ((i % labeleveryX) == 0) g.DrawString((bl.Time()[i] / 100).ToString(), f.Font, new SolidBrush(fgcol), getX(i), r.Height - (f.Font.GetHeight() * 3));
                                // if it's noon, draw the date
                                if (bl.Time()[i] == 120000) g.DrawString(bl.Date()[i].ToString(), f.Font, new SolidBrush(fgcol), getX(i), r.Height - (float)(f.Font.GetHeight() * 1.5));
                            }
                            else // otherwise it's daily data
                            {
                                // get date
                                int[] date = Calc.Date(bl.Date()[i]);
                                int[] lastbardate = date;
                                // get previous bar date if we have one
                                if ((i - 1) > 0)
                                    lastbardate = Calc.Date(bl.Date()[i - 1]);
                                // if we have room since last time we drew the year
                                if ((getX(lastlabelcoord) + minxlabelwidth) <= getX(i))
                                {
                                    // get coordinate for present days label
                                    lastlabelcoord = i;
                                    // draw day
                                    g.DrawString(date[2].ToString(), f.Font, new SolidBrush(fgcol), getX(i), r.Height - (f.Font.GetHeight() * 3));
                                }
                                // if it's first bar, a new month or new year
                                if ((i == 0) || (lastbardate[1] != date[1]) && ((getX(lastmonthyearcoord) + minxlabelwidth) <= getX(i)))
                                {
                                    // get coordinate for present month/year label
                                    lastmonthyearcoord = i;
                                    // get the month
                                    string ds = date[1].ToString();
                                    // if it first bar or the year has changed, add year to month
                                    if ((i == 0) || (lastbardate[0] != date[0]))
                                    {
                                        var yds = date[0].ToString();
                                        ds += '/' + yds.Substring(yds.Length - 2, 2);
                                    }
                                    // draw the month
                                    g.DrawString(ds, f.Font, new SolidBrush(fgcol), getX(i), r.Height - (float)(f.Font.GetHeight() * 1.5));
                                }
                            }
                        }
                        catch (OverflowException) { }
                    }

                    // DRAW YLABELS
                    // max number of even intervaled ylabels possible on yaxis
                    int numlabels = (int)((r.Height-((double)hborder/3)) / (f.Font.GetHeight()*2));
                    // nearest price units giving "pretty" even intervaled ylabels
                    decimal priceunits = NearestPrettyPriceUnits(highesth - lowestl, ref numlabels);
                    // starting price point from low end of range, including lowest low in barlist
                    decimal lowstart = lowestl - ((lowestl * 100) % (priceunits * 100)) / 100;
                    Pen priceline = new Pen(Color.BlueViolet);
                    priceline.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                    for (decimal i = 0; i < numlabels; i++)
                    {
                        decimal price = lowstart + (i * priceunits);
                        if (price > highesth)
                            break;
                        var labx = r.Width - border;
                        var pricey = getY(price);
                        var laby = pricey- f.Font.GetHeight();
                        g.DrawString(price.ToString("C"), f.Font, new SolidBrush(fgcol), labx,laby );
                        g.DrawLine(priceline, border / 3, getY(price), labx, pricey);
                    }
                    if (DisplayInterval && (bl != null))
                    {
                        const int dix = 3;
                        const int diy = 3;
                        var intstring = bl.DefaultInterval.ToString();
                        intervalstringendx = dix+ (int)g.MeasureString(intstring,f.Font).Width;
                        g.DrawString(intstring, f.Font, new SolidBrush(fgcol), dix, diy);
                    }
                    else
                        intervalstringendx = 0;

                    DrawLabels();
                }
            }
            catch (Exception ex)
            {
                g.DrawString("Error, submit actions to community.tradelink.org "+ex.Message+ex.StackTrace, new Font(FontFamily.GenericSerif, 8, FontStyle.Bold), Brushes.Red, new PointF(r.Width / 3, r.Height / 2));
            }
        }

        private int intervalstringendx = 0;

        /// <summary>
        /// used to control smallest possible price allowed on chart
        /// </summary>
        public decimal MinPrice = 0;

        bool hastextlabels = false;
        bool hasosctextlabels = false;

        /// <summary>
        /// draws text label on a chart.
        /// if price is less than zero (MinPrice), all labels are cleared.
        /// </summary>
        /// <param name="price"></param>
        /// <param name="bar"></param>
        /// <param name="label"></param>
        public void DrawChartLabel(decimal price, int time, string label, Color color)
        {
            // test whether this is an oscilator
            if (isosccolor(color))
            {
                DrawOscLabel(price, time, label, color);
                return;
            }
            // otherwise treat as a price
            // invalid price allows user to clear all price labels
            if (price < MinPrice)
            {
                _colpoints.Remove(color);
                _collineend.Remove(color);
                return;
            }
            List<Label> tmp;
            // setup this label if we don't have points for it already
            if (!_colpoints.TryGetValue(color, out tmp))
            {
                tmp = new List<Label>();
                _colpoints.Add(color, tmp);
                _collineend.Add(color,new List<int>());
            }
            // create new point for this label
            Label l = new Label(time, price, label, color);
            // mark whether it has text labels
            hastextlabels |= !l.isLine;
            // add it so it can be drawn with DrawLabels
            _colpoints[color].Add(l);
            // if label represents a line (rather than text), add it to label's line
            if (l.isLine)
                _collineend[color].Add(_colpoints[color].Count-1);
        }

        List<int> osccolors = new List<int>();
        /// <summary>
        /// tell chart which colors represent oscilators, so no seperate call is needed
        /// </summary>
        /// <param name="osc_colors"></param>
        public void IdentifyOscilatorColors(params Color[] osc_colors)
        {
            foreach (var c in osc_colors)
            {
                if (!isosccolor(c))
                    osccolors.Add(c.ToArgb());
            }
        }

        private bool isosccolor(Color c)
        {
            if (osccolors.Count == 0)
                return false;
            var colidx = osccolors.BinarySearch(c.ToArgb());
            return colidx >= 0;
        }

        /// <summary>
        /// maximum oscilator value
        /// </summary>
        public decimal MaxOsc = 100;
        /// <summary>
        /// minimum oscilator value
        /// </summary>
        decimal _minosc = -100;
        public decimal MinOsc { get { return _minosc; } set { _minosc = value; } }

        private decimal OscRange { get { return Math.Abs(MaxOsc - MinOsc); } }
        private decimal ZeroedMinOscAdju { get { return MinOsc * -1; } }
        private decimal ZeroedMaxOscAdju { get { return OscRange; } }

        /// <summary>
        /// draw an oscilator line/label
        /// </summary>
        /// <param name="osc"></param>
        /// <param name="time"></param>
        /// <param name="label"></param>
        /// <param name="color"></param>
        public void DrawOscLabel(decimal osc, int time, string label, Color color)
        {
            // invalid osclitor allows user to clear all labels
            if (osc < MinOsc)
            {
                _colpoints_osc.Remove(color);
                _collineend_osc.Remove(color);
                return;
            }
            if (osc > MaxOsc)
            {
                debug("Oscliator " + label + " " + osc + " at " + time + " exceeds MaxOsc: " + MaxOsc);
                return;
            }
            List<Label> tmp;
            // create new label for first time points
            if (!_colpoints_osc.TryGetValue(color, out tmp))
            {
                tmp = new List<Label>();
                _colpoints_osc.Add(color, tmp);
                _collineend_osc.Add(color, new List<int>());
                osccolors.Add(color.ToArgb());
            }
            // add a point to our label
            Label l = new Label(time, osc, label, color);
            _colpoints_osc[color].Add(l);
            hasosctextlabels |= !l.isLine;
            // if label is a line, add it to end of line
            if (l.isLine)
                _collineend_osc[color].Add(_colpoints_osc[color].Count - 1);
            // force update if necessary
            if (_alwaysupdate)
                redraw();
        }

        private decimal NearestPrettyPriceUnits(decimal pricerange,ref int maxlabels)
        {
            var absolutemax = maxlabels;
            decimal[] prettyunits = new decimal[] { 
                0.0002m,0.0005m,0.001m,.005m, 
                .01m, .02m, .04m, .05m, .1m, .2m, .25m, .4m, .5m, 
                1,2,5,10,25,50,100,
                1000,2000,4000,5000,10000 ,50000,100000,1000000
            };
            for (int i = prettyunits.Length-1; i>=0; i--)
            {
                maxlabels = (int)(pricerange / prettyunits[i]);
                if (maxlabels< absolutemax)
                    continue;
                else if (i == (prettyunits.Length - 1))
                    return prettyunits[prettyunits.Length - 1];
                return prettyunits[i+1];
            }
            return 1;
        }

        private void Chart_Resize(object sender, EventArgs e)
        {
            redraw();
        }

        struct Label
        {
            public bool isLine { get { return (Text == null) || (Text == string.Empty); } }
            public int Time;
            public decimal Price;
            public string Text;
            public Color Color;
            public Label(int bar, decimal price, string text, Color color)
            {
                Time = bar;
                Price = price;
                Text = text;
                Color = color;
            }
        }
        Dictionary<Color, List<Label>> _colpoints = new Dictionary<Color, List<Label>>();
        Dictionary<Color, List<int>> _collineend = new Dictionary<Color,List<int>>();

        private void DrawLabels()
        {
            DrawLabelsPrice();
            DrawLabelsOsc();
            // force redraw if necessary
            if (_alwaysupdate)
                redraw();
        }

        private void DrawLabelsPrice()
        {
            Graphics gd = CreateGraphics();

            if (hastextlabels)
            {
                Font font = new Font(FontFamily.GenericSerif, 8, FontStyle.Regular);
                // draw the text-only labels first
                foreach (Color c in _colpoints.Keys)
                {
                    List<Label> points = _colpoints[c];
                    for (int i = 0; i < points.Count; i++)
                    {
                        // draw labels
                        if (!points[i].isLine)
                        {
                            gd.DrawString(points[i].Text, font, new SolidBrush(c), getX(BarListImpl.GetNearestIntraBar(bl, points[i].Time, bl.DefaultInterval)), getY(points[i].Price));
                        }


                    }

                }
            }
            // draw price lines
            foreach (Color c in _collineend.Keys)
            {
                List<Label> points = _colpoints[c];
                List<int> lineidx = _collineend[c];
                for (int i = 0; i < lineidx.Count; i++)
                {
                    // get point indicies
                    // get previous point first (must be used for line)
                    var p1i = getpreceedingnonlabelindex(i, points);
                    // can't draw if we don't have two non-label points
                    if (p1i < 0)
                        continue;
                    int p2i = lineidx[i];
                    if (!points[p2i].isLine)
                        continue;
                    // get points
                    int x1 = getX(BarListImpl.GetNearestIntraBar(bl, points[p1i].Time, bl.DefaultInterval));
                    int y1 = getY(points[p1i].Price);
                    int x2 = getX(BarListImpl.GetNearestIntraBar(bl, points[p2i].Time, bl.DefaultInterval));
                    int y2 = getY(points[p2i].Price);
                    // draw from previous point
                    gd.DrawLine(new Pen(c), x1, y1, x2, y2);
                }
            }

            



        }

        Dictionary<Color, List<Label>> _colpoints_osc = new Dictionary<Color, List<Label>>();
        Dictionary<Color, List<int>> _collineend_osc = new Dictionary<Color, List<int>>();

        /// <summary>
        /// percentage of the price window the oscilator can use (.25 = 25%, 0 = disable)
        /// </summary>
        public decimal OscHeightPct = .2m;
        /// <summary>
        /// amount of transparency for oscilator lines (1 = transparent, 0 = opaque)
        /// </summary>
        public double OscTransparency = .5;

        private void DrawLabelsOsc()
        {
            // see if osc are disabled
            if (OscHeightPct <= 0)
                return;
            if (OscHeightPct > 1)
                OscHeightPct = 1;
            // compute the alpha level
            var alpha = (int)((1-OscTransparency)*255);
            Graphics gd = CreateGraphics();
            gd.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;

            if (hasosctextlabels)
            {
                Font font = new Font(FontFamily.GenericSerif, 8, FontStyle.Regular);
                // draw the text-only labels first
                foreach (Color c in _colpoints_osc.Keys)
                {
                    List<Label> points = _colpoints_osc[c];
                    for (int i = 0; i < points.Count; i++)
                    {
                        // draw labels
                        if (!points[i].isLine)
                        {
                            gd.DrawString(points[i].Text, font,
                                new SolidBrush(c),
                                getX(BarListImpl.GetNearestIntraBar(bl, points[i].Time, bl.DefaultInterval)),
                                getOscY(points[i].Price));
                        }


                    }

                }
            }

            // draw osc lines
            foreach (Color c in _collineend_osc.Keys)
            {
                List<Label> points = _colpoints_osc[c];
                List<int> lineidx = _collineend_osc[c];
                var pen = new Pen(Color.FromArgb(alpha,c));
                
                for (int i = 0; i < lineidx.Count; i++)
                {
                    // get point indicies
                    // get previous point first (must be used for line)
                    var p1i = getpreceedingnonlabelindex(i, points);
                    // can't draw if we don't have two non-label points
                    if (p1i<0) 
                        continue;
                    int p2i = lineidx[i];
                    if (!points[p2i].isLine)
                        continue;
                    if (isUsingShadedOsc)
                    {

                    }
                    else
                    {
                        // get points (x are same as price window, y are adjusted)
                        int x1 = getX(BarListImpl.GetNearestIntraBar(bl, points[p1i].Time, bl.DefaultInterval));
                        int y1 = getOscY(points[p1i].Price);
                        int x2 = getX(BarListImpl.GetNearestIntraBar(bl, points[p2i].Time, bl.DefaultInterval));
                        int y2 = getOscY(points[p2i].Price);
                        // draw from previous point
                        gd.DrawLine(pen, x1, y1, x2, y2);
                    }
                }
            }
            
        }

        bool isUsingShadedOsc = false;

        private int getOscY(decimal osc)
        {
            if (osc > MaxOsc)
                osc = MaxOsc;
            else if (osc < MinOsc)
                osc = MinOsc;
            // get relative position of this oscilator on scale
            var adjosc = osc + ZeroedMinOscAdju;
            // adjust to % of range
            var pctofoscrange = adjosc / OscRange;
            // size of osc grid in prices
            var oscgridpricerange = ((highesth - lowestl) * OscHeightPct);
            // get osc price as percent of this grid
            var oscprice = (oscgridpricerange * pctofoscrange) + lowestl;
            return getY(oscprice);
        }

        private int getpreceedingnonlabelindex(int cur, List<Label> points)
        {
            var start = cur-1;
            if (start<0)
                return -1;
            for (int i = start; i >= 0; i--)
                if (points[i].isLine)
                    return i;
            return -1;
        }

        Color _mancolor = Color.Turquoise;
        /// <summary>
        /// color used for manual chart drawings
        /// </summary>
        public Color ManualColor { get { return _mancolor; } set { _mancolor = value; } }

        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mlabel == null) return;
                DrawChartLabel(getPrice(e.Y), bl.Time()[getBar(e.X)],mlabel,ManualColor);
            }
            else if (e.Button == MouseButtons.Middle) 
            {

                DrawChartLabel(-1, 0, string.Empty, ManualColor);
            }
            redraw();
        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mlabel = "2";
        }

        private void yellowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mlabel = "1";
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mlabel = "0";
        }

        private void blackToolStripMenuItem_Click(object sender, EventArgs e)
        { //sell
            mlabel = "S";
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {// buy
            mlabel = "B";
        }

        private void offToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mlabel = null;
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DrawChartLabel(-1, 0, string.Empty, ManualColor);
            redraw();

        }
        bool _dispint = true;
        public bool DisplayInterval { get { return _dispint; } set { _dispint = value; } }

        bool _dispcur = true;
        public bool DisplayCursor { get { return _dispcur; } set { _dispcur = value; } }

        int _curbar = 0;
        decimal _curprice = 0;

        public void Chart_MouseUp(object sender, MouseEventArgs e)
        {
            if (bl == null) return;
            if (e.Delta == 0) return;
            BarInterval old = bl.DefaultInterval;
            BarInterval [] v = bl.Intervals;
            int biord = 0;
            for (int i = 0;i<v.Length;i++) if (old==v[i]) biord = i;

            if (e.Delta > 0)
            {
                bl.DefaultInterval = (biord + 1 < v.Length) ? v[biord + 1] : v[0];
            }
            else
            {
                bl.DefaultInterval = (biord - 1 < 0) ? v[v.Length - 1] : v[biord - 1];
            }
            if ((bl.DefaultInterval != old) && bl.Has(1,bl.DefaultInterval,bl.DefaultCustomInterval)) 
                NewBarList(this.bl);
        }

        private void _custom_Click(object sender, EventArgs e)
        {
            mlabel = Microsoft.VisualBasic.Interaction.InputBox("Enter label: ", "Custom chart label", "?", Location.X + 5, Location.Y + 5);
        }

        private void _point_Click(object sender, EventArgs e)
        {
            mlabel = string.Empty;
        }

        private void invertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertColors();
        }
    }
    public class TextLabel
    {
        public TextLabel(string label, int xCoord, int yCoord) { tlabel = label; x = xCoord; y = yCoord; }
        string tlabel = "";
        int x = 0;
        int y = 0;
        public string Label { get { return tlabel; } }
        public int X { get { return x; } }
        public int Y { get { return y; } }
    }
}