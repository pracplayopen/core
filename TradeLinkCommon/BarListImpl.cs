using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using TradeLink.API;

namespace TradeLink.Common
{
    /// <summary>
    /// Holds a succession of bars.  Will acceptt ticks and automatically create new bars as needed.
    /// </summary>
    public class BarListImpl : TradeLink.API.BarList
    {
        /// <summary>
        /// converts integer array of intervals to BarIntervals... supplying custom interval for any unrecognized interval types.
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static BarInterval[] Int2BarInterval(int[] intervals) { List<BarInterval> o = new List<BarInterval>(); foreach (int i in intervals) { try { BarInterval bi = (BarInterval)i; o.Add(bi); } catch (Exception) { o.Add(BarInterval.CustomTime); } } return o.ToArray(); }
        /// <summary>
        /// converts array of BarIntervals to integer intervals.
        /// </summary>
        /// <param name="ints"></param>
        /// <returns></returns>
        public static int[] BarInterval2Int(BarInterval[] ints) { List<int> o = new List<int>(); foreach (BarInterval bi in ints) o.Add((int)bi); return o.ToArray(); }
        /// <summary>
        /// gets array of all possible non custom bar intevals
        /// </summary>
        public static BarInterval[] ALLINTERVALS { get { return new BarInterval[] { BarInterval.FiveMin, BarInterval.Minute, BarInterval.Hour, BarInterval.ThirtyMin, BarInterval.FifteenMin, BarInterval.Day }; } }
        // holds available intervals
        int[] _intervaltypes = new int[0];
        // holds all raw data
        IntervalData[] _intdata = new IntervalData[0];
        
        
        /// <summary>
        /// creates barlist with defined symbol and requests all intervals
        /// </summary>
        /// <param name="symbol"></param>
        public BarListImpl(string symbol) : this(symbol, ALLINTERVALS) { }
        /// <summary>
        /// creates a barlist with requested interval and defined symbol
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="symbol"></param>
        public BarListImpl(BarInterval interval, string symbol) : this(symbol, new BarInterval[] { interval }) { }
        /// <summary>
        /// creates a barlist with requested custom interval and defined symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        public BarListImpl(string symbol, int interval) : this(symbol, interval, BarInterval.CustomTime) { }
        /// <summary>
        /// creates a barlist with custom interval and a custom type (tick/vol)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="type"></param>
        public BarListImpl(string symbol, int interval, BarInterval type) : this(symbol, new int[] { interval }, new BarInterval[] { type }) { }
        /// <summary>
        /// creates a barlist with requested interval.  symbol will be defined by first tick received
        /// </summary>
        /// <param name="interval"></param>
        public BarListImpl(BarInterval interval) : this(string.Empty, new BarInterval[] { interval }) { }
        /// <summary>
        /// creates barlist with no symbol defined and requests 5min bars
        /// </summary>
        public BarListImpl() : this(string.Empty,new BarInterval[] { BarInterval.FiveMin,BarInterval.Minute,BarInterval.Hour,BarInterval.ThirtyMin,BarInterval.FifteenMin, BarInterval.Day }) { }
        /// <summary>
        /// creates barlist with specified symbol and requested intervals
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="intervals"></param>
        public BarListImpl(string symbol, BarInterval[] intervals) : this(symbol,BarInterval2Int(intervals), intervals) {}
        /// <summary>
        /// make copy of a barlist.  remember you must re-setup GotNewBar events after using this.
        /// </summary>
        /// <param name="original"></param>
        public BarListImpl(BarList original) : this(original.symbol,original.CustomIntervals,original.Intervals) 
        {
            for (int j = 0; j<original.Intervals.Length; j++)
            {
                original.SetDefaultInterval(original.Intervals[j], original.CustomIntervals[j]);
                for (int i = 0; i < original.Count; i++)
                {
                    addbar(this, original[i, original.Intervals[j],original.CustomIntervals[j]], j);
                }
            }
        }
        /// <summary>
        /// fill bars with arbitrary price data for a symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="prices"></param>
        /// <param name="startdate"></param>
        /// <param name="blt"></param>
        /// <param name="interval"></param>
        /// <param name="debugs"></param>
        /// <returns></returns>
        public static bool backfillbars(string sym, decimal[] prices, int startdate, int starttime, ref BarListTracker blt, int interval, DebugDelegate debugs)
        {
            // ensure we have closing data
            if (prices.Length == 0)
            {
                if (debugs != null)
                    debugs(sym + " no price data provided/available, will have to wait until bars are created from market.");
                return false;
            }
            // make desired numbers of ticks
            DateTime n = Util.ToDateTime(startdate, starttime);
            bool ok = true;
            List<string> bfd = new List<string>();
            for (int i = prices.Length - 1; i >= 0; i--)
            {
                // get time now - exitlen*60
                var ndt = n.Subtract(new TimeSpan(0, i * interval, 0));
                int nt = Util.ToTLTime(ndt);
                int nd = Util.ToTLDate(ndt);
                Tick k = TickImpl.NewTrade(sym, nd, nt, prices[i], 100, string.Empty);
                ok &= k.isValid && k.isTrade;
                bfd.Add(nd.ToString() + nt.ToString() + "=" + prices[i]);
                if (i <= 2)
                    debugs(nd + " " + nt);
                blt.newTick(k);
            }
            if (ok && (debugs != null))
                debugs(sym + " bars backfilled using: " + Util.join(bfd));
            return ok;
        }

        public static string Bars2String(BarList bl) { return Bars2String(bl, bl.DefaultInterval, bl.DefaultCustomInterval, Environment.NewLine); }
        public static string Bars2String(BarList bl, BarInterval type) { return Bars2String(bl, type, (int)type, Environment.NewLine); }
        public static string Bars2String(BarList bl, BarInterval type, int size) { return Bars2String(bl, type, size, Environment.NewLine); }
        public static string Bars2String(BarList bl, BarInterval type, int size, string delim)
        {
            if (((bl.DefaultInterval==type) && (bl.DefaultCustomInterval==size)) || bl.SetDefaultInterval(type, size))
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < bl.Count; i++)
                {
                    sb.Append(bl[i].ToString());
                    sb.Append(delim);
                }
                return sb.ToString();
            }
            else
                return string.Empty;
        }

        

        /// <summary>
        /// insert a bar at particular place in the list.
        /// REMEMBER YOU MUST REHANDLE GOTNEWBAR EVENT AFTER CALLING THIS.
        /// </summary>
        /// <param name="bl"></param>
        /// <param name="b"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static BarListImpl InsertBar(BarList bl, Bar b, int position)
        {
            BarListImpl copy = new BarListImpl(bl.symbol,bl.CustomIntervals,bl.Intervals);
            // verify bar is valid
            if (!b.isValid)
            {
                return copy;
            }
            for (int j = 0; j < bl.CustomIntervals.Length; j++)
            {
                if ((bl.CustomIntervals[j] != b.CustomInterval) && (bl.Intervals[j] != b.BarInterval))
                    continue;
                int count = bl.IntervalCount(b.BarInterval,b.CustomInterval);
                if (count < 0)
                    throw new Exception("bar has interval: " + b.BarInterval + "/" + b.CustomInterval + " not defined in your barlist for: " + bl.symbol);
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (i == position)
                        {
                            addbar(copy, b, j);
                        }
                        addbar(copy, bl[i, b.BarInterval,b.CustomInterval], j);
                    }
                }
                else
                    addbar(copy, b, j);
            }
            return copy;
        }
        /// <summary>
        /// insert one barlist into another barlist
        /// REMEMBER: You must re-handle the GotNewBar event after calling this method.
        /// You should also ensure that inserted barlist has same intervals/types as original barlist.
        /// </summary>
        /// <param name="bl"></param>
        /// <param name="insert"></param>
        /// <returns></returns>
        public static BarListImpl InsertBarList(BarList bl, BarList insert)
        {
            BarListImpl copy = new BarListImpl(bl);
            for (int j = 0; j < bl.CustomIntervals.Length; j++)
            {
                for (int k = 0; k < insert.CustomIntervals.Length; k++)
                {
                    if (bl.CustomIntervals[j] != insert.CustomIntervals[k])
                        continue;
                    for (int l = 0; l < insert.Count; l++)
                    {
                        for (int m = 0; m < bl.Count; m++)
                        {
                            if (l == m)
                            {
                                addbar(copy, insert[l, insert.Intervals[k],insert.CustomIntervals[k]], j);
                            }
                            addbar(copy, bl[m], j);
                        }
                    }
                }
            }
            return copy;
        }

        int curintervalidx = 0;
        /// <summary>
        /// index to current default interval pair (type/size)
        /// </summary>
        public int DefaultIntervalIndex
        {
            get { return curintervalidx; }
            set
            {
                var nv = value;
                if ((nv >= 0) && (nv < _custintervals.Length))
                    curintervalidx = nv;
            }
        }

        public bool SetDefaultInterval(BarInterval type, int size)
        {
            bool found = false;
            for (int i = 0; i < _intervaltypes.Length; i++)
            {
                var it = (BarInterval)_intervaltypes[i];
                var ints = _custintervals[i];
                if ((it == type) && (size == ints))
                {
                    curintervalidx = i;
                    found = true;
                    break;
                }
            }
            return found;
        }

        GenericTracker<int> typesize2idx = new GenericTracker<int>();

        /// <summary>
        /// creates a barlist with array of custom intervals
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="custintervals"></param>
        public BarListImpl(string symbol, int[] custintervals, BarInterval[] types)
        {            // set symbol
            _sym = symbol;
            _custintervals = custintervals;
            // set intervals requested
            _intervaltypes = BarInterval2Int(types);
            // size length of interval data to # of requested intervals
            _intdata = new IntervalData[_intervaltypes.Length];
            // index the pairs
            typesize2idx.Clear();
            // create interval data object for each interval
            for (int i = 0; i < _intervaltypes.Length; i++)
            {
                // set default interval to first one
                if (i == 0)
                {
                    _defaultint = _intervaltypes[0];
                    _defaultcustint = _custintervals[0];
                    curintervalidx = 0;
                }

                // save index to this size for the interval
                switch (types[i])
                {
                    case BarInterval.CustomTicks:
                        _intdata[i] = new TickIntervalData(_custintervals[i]);
                        break;
                    case BarInterval.CustomVol:
                        _intdata[i] = new VolIntervalData(_custintervals[i]);
                        break;
                    case BarInterval.CustomTime:
                        _intdata[i] = new TimeIntervalData(_custintervals[i]);
                        break;
                    default:
                        _intdata[i] = new TimeIntervalData(_intervaltypes[i]);
                        break;
                }

            
                // subscribe to bar events
                _intdata[i].NewBar += new SymBarIntervalDelegate(BarListImpl_NewBar);

                // index the pair
                typesize2idx.addindex(_intervaltypes[i].ToString() + _custintervals[i].ToString(), i);
            }
        }

        int _defaultint = (int)BarInterval.FiveMin;
        // array functions
        public decimal[] Open() { return _intdata[curintervalidx].open().ToArray(); }
        public decimal[] High() { return _intdata[curintervalidx].high().ToArray(); }
        public decimal[] Low() { return _intdata[curintervalidx].low().ToArray(); }
        public decimal[] Close() { return _intdata[curintervalidx].close().ToArray(); }
        public long[] Vol() { return _intdata[curintervalidx].vol().ToArray(); }
        public int[] Date() { return _intdata[curintervalidx].date().ToArray(); }
        public int[] Time() { return _intdata[curintervalidx].time().ToArray(); }
        public decimal[] Open(int intervalidx) { return _intdata[intervalidx].open().ToArray(); }
        public decimal[] High(int intervalidx) { return _intdata[intervalidx].high().ToArray(); }
        public decimal[] Low(int intervalidx) { return _intdata[intervalidx].low().ToArray(); }
        public decimal[] Close(int intervalidx) { return _intdata[intervalidx].close().ToArray(); }
        public int[] Date(int intervalidx) { return _intdata[intervalidx].date().ToArray(); }
        public int[] Time(int intervalidx) { return _intdata[intervalidx].time().ToArray(); }


        /// <summary>
        /// gets intervals available/requested by this barlist when it was created
        /// </summary>
        public BarInterval[] Intervals { get { return  Int2BarInterval(_intervaltypes); } }

        int[] _custintervals = new int[0];
        /// <summary>
        /// gets all available/requested intervals as a custom array of integers
        /// </summary>
        public int[] CustomIntervals { get { return _custintervals; } }

        /// <summary>
        /// set true for new bar.  don't use this, use GotNewBar event as it's faster.
        /// </summary>
        [Obsolete("this is deprecated method.  use GotNewBar event")]
        public bool NewBar { get { return _intdata[curintervalidx].isRecentNew(); } }

        // standard accessors
        /// <summary>
        /// symbol for bar
        /// </summary>
        public string symbol { get { return _sym; } set { _sym = value.Replace(AMEX,""); } }
        /// <summary>
        /// returns true if bar has symbol and has requested intervals
        /// </summary>
        public bool isValid { get { return (_sym != string.Empty) && (_intdata.Length>0); } }
        public IEnumerator GetEnumerator() 
        { 
            var data = _intdata[curintervalidx];
            int max = data.Count(); 
            for (int i = 0; i < max; i++) 
                yield return data.GetBar(i,symbol); 
        }
        /// <summary>
        /// gets first bar in any interval
        /// </summary>
        public int First { get { return 0; } }
        /// <summary>
        /// gets or sets the default interval in seconds
        /// </summary>
        int _defaultcustint = 0;
        public int DefaultCustomInterval 
        { 
            get 
            { 
                return _defaultcustint; 
            } 
            set 
            { 
                var nv = (int)value;
                // verify present
                int foundidx = -1;
                for (int i = 0; i < _custintervals.Length; i++)
                {
                    if (nv == _custintervals[i])
                    {
                        foundidx = i;
                    }
                }
                if (foundidx >= 0)
                {
                    curintervalidx = foundidx;
                    _defaultcustint = value;

                }
            } 
        }
        /// <summary>
        /// gets or sets the default interval in bar intervals
        /// </summary>
        public BarInterval DefaultInterval 
        { 
            get 
            { 
                return Int2BarInterval(new int[] { _defaultint })[0]; } 
            set 
            {
                var nv = (int)value;
                // verify present
                int foundidx = -1;
                for (int i = 0; i < _intervaltypes.Length; i++)
                {
                    if (nv == _intervaltypes[i])
                    {
                        foundidx = i;
                    }
                }
                if (foundidx >= 0)
                {
                    curintervalidx = foundidx;
                    _defaultint = nv;
                }
            } 
        }
        /// <summary>
        /// gets specific bar in specified interval
        /// </summary>
        /// <param name="barnumber"></param>
        /// <returns></returns>
        public Bar this[int barnumber] 
        { 
            get 
            { 
                return _intdata[curintervalidx].GetBar(barnumber,symbol); 
            } 
        }
        /// <summary>
        /// gets a specific bar in specified interval
        /// </summary>
        /// <param name="barnumber"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Bar this[int barnumber, BarInterval interval, int size] { get { var idx = gettypesizeidx(interval, size); if (idx < 0) return new BarImpl(); return _intdata[idx].GetBar(barnumber,symbol); } }
        /// <summary>
        /// gets count for given bar interval
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public int IntervalCount(BarInterval interval, int size) 
        { 
            var idx = gettypesizeidx(interval, size); 
            // interval not in this tracker
            if (idx < 0) 
                return -1; 
            return _intdata[idx].Count();  
        }
        public int IntervalCount(BarInterval interval) 
        { 
            var idx = gettypesizeidx(interval, (int)interval); 
            if (idx < 0) 
                return -1; 
            return _intdata[idx].Count(); }

        int gettypesizeidx(BarInterval type, int size)
        {
            var types = ((int)type).ToString();
            return typesize2idx.getindex(types+ size.ToString());
        }

        /// <summary>
        /// gets a specific bar in specified seconds interval
        /// </summary>
        /// <param name="barnumber"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Bar this[int barnumber, int intervalidx] { get { return _intdata[intervalidx].GetBar(barnumber, symbol); } }
        /// <summary>
        /// gets the last bar in default interval
        /// </summary>
        public int Last { get { return _intdata[curintervalidx].Last(); } }
        /// <summary>
        /// gets the # of bars in default interval
        /// </summary>
        public int Count { get { return _intdata[curintervalidx].Count(); } }
       

        /// <summary>
        /// gets most recent bar from default interval
        /// </summary>
        public Bar RecentBar { get { return this[Last]; } }

        /// <summary>
        /// returns true if barslist has at least minimum # of bars for specified interval
        /// </summary>
        /// <param name="minBars"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool Has(int minBars, BarInterval interval, int size) 
        { 
            var idx = gettypesizeidx(interval, size); 
            if (idx < 0) 
                return false; 
            return _intdata[idx].Count()>=minBars; }

        /// <summary>
        /// returns true if barlist has at least minimum # of bars for default interval
        /// </summary>
        /// <param name="minBars"></param>
        /// <returns></returns>
        public bool Has(int minBars) { return _intdata[curintervalidx].Count() >= minBars; }
        
        /// <summary>
        /// this event is thrown when a new bar arrives
        /// </summary>
        public event SymBarIntervalDelegate GotNewBar;
        void BarListImpl_NewBar(string symbol, int interval)
        {
            // if event is handled by user, pass the event
            if (GotNewBar != null)
                GotNewBar(symbol, interval);
        }
        /// <summary>
        /// erases all bar data
        /// </summary>
        public void Reset()
        {
            foreach (IntervalData id in _intdata)
            {
                id.Reset();
            }
        }

        string _sym = string.Empty;
        int _symh = 0;
        bool _valid = false;
        public void newTick(Tick k)
        {
            // only pay attention to trades and indicies
            if (k.trade == 0) return;
            // make sure we have a symbol defined 
            if (!_valid)
            {
                _symh = k.symbol.GetHashCode();
                _sym = k.symbol;
                _valid = true;
            }
            // make sure tick is from our symbol
            if (_symh != k.symbol.GetHashCode()) return;
            // add tick to every requested bar interval
            for (int i = 0; i < _intdata.Length; i++)
                _intdata[i].newTick(k);
        }

        public void newPoint(string symbol, decimal p, int time, int date, int size)
        {
            // add tick to every requested bar interval
            for (int i = 0; i < _intdata.Length; i++)
                _intdata[i].newPoint(symbol,p,time,date,size);
        }

        /// <summary>
        /// Create a barlist from a succession of bar records provided as comma-delimited OHLC+volume data.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="filedata">The file containing the CSV records.</param>
        /// <returns></returns>
        public static BarListImpl FromCSV(string symbol, string filedata, int barinterval)
        {
            BarListImpl b = new BarListImpl(BarInterval.Day, symbol);
            string[] line = filedata.Split(Environment.NewLine.ToCharArray());
            for (int i = line.Length - 1; i > 0; i--)
            {
                if (line[i] != string.Empty)
                    addbar(b,BarImpl.FromCSV(line[i],symbol, barinterval),0);
            }
            return b;
        }

        /// <summary>
        /// find the bar # that matches a given time
        /// </summary>
        /// <param name="bl"></param>
        /// <param name="time"></param>
        /// <param name="bint"></param>
        /// <returns></returns>
        public static int GetNearestIntraBar(BarList bl, int time, BarInterval bint) { return GetNearestIntraBar(bl, time, bint, null); }
        public static int GetNearestIntraBar(BarList bl, int time, BarInterval bint, DebugDelegate debug) { return GetNearestIntraBar(bl, time, bint, 0,debug); }
        public static int GetNearestIntraBar(BarList bl, int time, BarInterval bint, int size, DebugDelegate debug)
        {
            try
            {
                long barid = TimeIntervalData.getbarid(time, bl.RecentBar.Bardate, (int)bint);
                BarListImpl bli = (BarListImpl)bl;
                if (size==0)
                    size = (int)bint;
                TimeIntervalData tid = (TimeIntervalData)bli._intdata[bli.gettypesizeidx(bint,size)];
                for (int i = 0; i < tid.Count(); i++)
                    if (tid.ids[i] == barid)
                        return i;
            }
            catch (Exception ex)
            {
                if (debug != null)
                    debug("error getting nearest bar from: " + bl.symbol + " at: " + time + " for: " + bint + " error: " + ex.Message + ex.StackTrace);
            }
            return -1;
        }

        internal static void addbar(BarListImpl b, Bar mybar, int instdataidx)
        {
            b._intdata[instdataidx].addbar(mybar);
        }
        private const string AMEX = ":AMEX";
        /// <summary>
        /// Populate the day-interval barlist of this instance from a URL, where the results are returned as a CSV file.  URL should accept requests in the form of http://url/get.py?sym=IBM
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        
        private static BarList DayFromURL(string url,string Symbol,bool appendAMEXonfail, bool failonmissingdata, DebugDelegate d)
        {
            debs = d;
            BarListImpl bl = new BarListImpl(BarInterval.Day,Symbol);
            if (Symbol == "")
            {
                debug("No symbol specified.");
                return bl;
            }
            string res = Util.geturl(url + Symbol,d);
            if (string.IsNullOrWhiteSpace(res) && appendAMEXonfail)
            {
                debug(Symbol + " daychart pull failed, attempting amex pull.");
                return DayFromURL(url, Symbol + AMEX, false, failonmissingdata, d);
            }
            string[] line = res.Split(Environment.NewLine.ToCharArray());
            for (int i = line.Length - 1; i > 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(line[i] )) 
                    addbar(bl,BarImpl.FromCSV(line[i],Symbol,(int)BarInterval.Day,failonmissingdata,d),0);
            }
            return bl;
        }



        /// <summary>
        /// attempts to get year worth of daily data from google, if fails tries yahoo.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static BarList DayFromAny(string symbol, DebugDelegate d)
        {
            BarList bl = BarListImpl.DayFromGoogle(symbol, 0, Util.ToTLDate(), true, d);
            if (bl.Count == 0)
                bl = BarListImpl.DayFromYahoo(symbol, d);
            return bl;
        }
        public static BarList DayFromAny(string symbol, bool skipbarsmissingdata, DebugDelegate d)
        {
            BarList bl = BarListImpl.DayFromGoogle(symbol,0, Util.ToTLDate(),skipbarsmissingdata,d);
            if (bl.Count == 0)
                bl = BarListImpl.DayFromYahoo(symbol,d);
            return bl;
        }

        public static BarList DayFromGoogle(string symbol, DebugDelegate d)
        {
            return
                DayFromGoogle(symbol, 0, Util.ToTLDate(),false,d);
        }

        public static BarList DayFromGoogle(string symbol, bool skipbarsmissingdata, DebugDelegate d)
        {
            return
                DayFromGoogle(symbol, 0, Util.ToTLDate(), skipbarsmissingdata, d);
        }

        public static BarList DayFromGoogle(string symbol, int startdate)
        {
            return
                DayFromGoogle(symbol, startdate, Util.ToTLDate());
        }
        /// <summary>
        /// gets specific date range of bars from google
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public static BarList DayFromGoogle(string symbol, int startdate, int enddate) { return DayFromGoogle(symbol, startdate, enddate, false, null); }
        public static BarList DayFromGoogle(string symbol, int startdate, int enddate, bool skipbarsmissingdata, DebugDelegate d)
        {
            debs = d;
            const string AMEX = ":AMEX";
            if ((symbol == null) || (symbol == string.Empty)) return new
BarListImpl();
            string url = @"http://finance.google.com/finance/historical?
histperiod=daily&startdate=" + startdate + "&enddate=" + enddate + "&output=csv&q=" + symbol;
            BarListImpl bl = new BarListImpl(BarInterval.Day, symbol);

            string res = Util.geturl(url, d);
            if (string.IsNullOrWhiteSpace(res) && !symbol.Contains(AMEX))
                    DayFromGoogle(symbol + AMEX, startdate, enddate,skipbarsmissingdata,d);
            string[] line = res.Split(Environment.NewLine.ToCharArray());
            for (int i = line.Length - 1; i > 0; i--)
            {
                if (string.IsNullOrWhiteSpace(line[i]))
                    continue;
                Bar b = BarImpl.FromCSV(line[i],symbol,(int)BarInterval.Day,skipbarsmissingdata,d);
                if (b != null && (b.isValid))
                {
                    foreach (Tick k in BarImpl.ToTick(b))
                        bl.newTick(k);
                }
                else
                    debug(symbol + " invalid bar#" + i + " on data: " + line[i] + ", ignored on import.");
            }
            return bl;
        }






        /// <summary>
        /// Populate the day-interval barlist using google finance as the source.
        /// </summary>
        /// <returns></returns>
        public static BarList DayFromGoogle(string symbol) { return DayFromGoogle(symbol, null); }

        const string GOOGURL = @"http://finance.google.com/finance/historical?histperiod=daily&start=250&num=25&output=csv&q=";

        /// <summary>
        /// Build a barlist using an EPF file as the source
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>barlist</returns>
        public static BarList FromEPF(string filename)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(filename);
            SecurityImpl s = eSigTick.InitEpf(sr);
            BarList b = new BarListImpl(s.symbol);
            while (!sr.EndOfStream)
                b.newTick(eSigTick.FromStream(s.symbol, sr));
            return b;
        }

        private static BarListImpl _fromepf;
        private static bool _uselast = true;
        private static bool _usebid = true;
        /// <summary>
        /// get a barlist from tick data
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static BarList FromTIK(string filename) { return FromTIK(filename, true, true); }
        /// <summary>
        /// get a barlist from tick data and optionally use bid/ask data to construct bars
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="uselast"></param>
        /// <param name="usebid"></param>
        /// <returns></returns>
        public static BarList FromTIK(string filename, bool uselast, bool usebid)
        {
            _uselast = uselast;
            _usebid = usebid;
            SecurityImpl s = SecurityImpl.FromTIK(filename);
            s.HistSource.gotTick += new TickDelegate(HistSource_gotTick);
            _fromepf = new BarListImpl(s.symbol);
            while (s.HistSource.NextTick()) ;
            s.HistSource.Close();
            return _fromepf;
        }
        /// <summary>
        /// create barlist from a tik file using given intervals/types
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="uselast"></param>
        /// <param name="usebid"></param>
        /// <param name="intervals"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static BarList FromTIK(string filename, bool uselast, bool usebid, int[] intervals, BarInterval[] types)
        {
            _uselast = uselast;
            _usebid = usebid;
            SecurityImpl s = SecurityImpl.FromTIK(filename);
            s.HistSource.gotTick += new TickDelegate(HistSource_gotTick);
            _fromepf = new BarListImpl(s.symbol,intervals,types);
            while (s.HistSource.NextTick()) ;
            return _fromepf;
        }

        static void HistSource_gotTick(Tick t)
        {
            if (_uselast)
                _fromepf.newTick(t);
            else
            {
                if (t.hasAsk && !_usebid)
                    _fromepf.newPoint(t.symbol,t.ask, t.time, t.date, t.AskSize);
                else if (t.hasBid && _usebid)
                    _fromepf.newPoint(t.symbol, t.bid, t.time, t.date, t.BidSize);
            }
        }
        /// <summary>
        /// gets index of bar that preceeds given date
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetBarIndexPreceeding(BarList chart, int date)
        {
            // look for previous day's close
            for (int j = chart.Last; (j >= chart.First); j--)
            {
                if (chart.Date()[j] < date)
                {
                    return j;
                }
            }
            // first bar
            return -1;
        }
        /// <summary>
        /// gets preceeding bar by time (assumes same day)
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int GetBarIndexPreceeding(BarList chart, int date, int time)
        {
            // look for previous day's close
            for (int j = chart.Last; (j >= chart.First); j--)
            {
                if (chart.Date()[j] > date) continue;
                if (chart.Time()[j] < time || date > chart.Date()[j])
                {
                    return j;
                }
            }
            // first bar
            return -1;
        }
        /// <summary>
        /// gets bar that preceeds a given date (invalid if no preceeding bar)
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static Bar GetBarPreceeding(BarList chart, int date)
        {
            // look for previous day's close
            for (int j = chart.Last; (j >= chart.First); j--)
            {
                if (chart.Date()[j] < date)
                {
                    return chart[j];
                }
            }
            return new BarImpl();
        }

        public static Bar GetBar(BarList chart, int date)
        {
            // look for previous day's close
            for (int j = chart.Last; (j >= chart.First); j--)
            {
                if (chart.Date()[j] == date)
                {
                    return chart[j];
                }
            }
            return new BarImpl();
        }

        /// <summary>
        /// Populate the day-interval barlist using google finance as the source.
        /// </summary>
        /// <returns></returns>
        ///
        public static BarList DayFromYahoo(string symbol) { return DayFromYahoo(symbol, null, null,false,null); }
        
        public static BarList DayFromYahoo(string symbol, DebugDelegate d) { return DayFromYahoo(symbol, null, null,false, d); }
        public static BarList DayFromYahoo(string symbol, bool skipbarsmissingdata, DebugDelegate d) { return DayFromYahoo(symbol, null, null, skipbarsmissingdata, d); }
        public static BarList DayFromYahoo(string symbol, DateTime? startDate, DateTime? endDate, bool skipbarsmissingdata, DebugDelegate d)
        {
            debs = d;
            string urlTemplate =
           @"http://ichart.finance.yahoo.com/table.csv?s=[symbol]&a=" +
             "[startMonth]&b=[startDay]&c=[startYear]&d=[endMonth]&e=" +
                "[endDay]&f=[endYear]&g=d&ignore=.csv";

            if (!endDate.HasValue) endDate = DateTime.Now;
            if (!startDate.HasValue) startDate = DateTime.Now.AddYears(-5);
            if (symbol == null || symbol.Length < 1)
                throw new ArgumentException("Symbol invalid: " + symbol);
            // NOTE: Yahoo's scheme uses a month number 1 less than actual e.g. Jan. ="0"
            int strtMo = startDate.Value.Month - 1;
            string startMonth = strtMo.ToString();
            string startDay = startDate.Value.Day.ToString();
            string startYear = startDate.Value.Year.ToString();

            int endMo = endDate.Value.Month - 1;
            string endMonth = endMo.ToString();
            string endDay = endDate.Value.Day.ToString();
            string endYear = endDate.Value.Year.ToString();

            urlTemplate = urlTemplate.Replace("[symbol]", symbol);

            urlTemplate = urlTemplate.Replace("[startMonth]", startMonth);
            urlTemplate = urlTemplate.Replace("[startDay]", startDay);
            urlTemplate = urlTemplate.Replace("[startYear]", startYear);

            urlTemplate = urlTemplate.Replace("[endMonth]", endMonth);
            urlTemplate = urlTemplate.Replace("[endDay]", endDay);
            urlTemplate = urlTemplate.Replace("[endYear]", endYear);
            return DayFromURL(urlTemplate, symbol,false,skipbarsmissingdata,d);
        }


        /// <summary>
        /// Populate the day-interval barlist using Euronext.com as the source.
        /// </summary>
        /// <param name="isin">The ISIN (mnemonics not accepted)</param>
        /// <returns></returns>
        ///
        public static BarList DayFromEuronext(string isin) { return DayFromEuronext(isin, null, null); }
        public static BarList DayFromEuronext(string isin, DateTime? startDate, DateTime? endDate)
        {
            string market;
            string urlTemplate =
                @"http://www.euronext.com/tools/datacentre/dataCentreDownloadExcell.jcsv?cha=2593&lan=EN&fileFormat=txt&separator=.&dateFormat=dd/MM/yy" + 
                "&isinCode=[symbol]&selectedMep=[market]&indexCompo=&opening=on&high=on&low=on&closing=on&volume=on&dateFrom=[startDay]/[startMonth]/[startYear]&" +
                "dateTo=[endDay]/[endMonth]/[endYear]&typeDownload=2";

            if (!endDate.HasValue) endDate = DateTime.Now;
            if (!startDate.HasValue) startDate = DateTime.Now.AddYears(-5);
            if (isin == null || !Regex.IsMatch(isin, "[A-Za-z0-9]{12}"))
                throw new ArgumentException("Invalid ISIN: " + isin);

            /* ugly hack to get the market number from the isin (not always valid..) */
            CompareInfo myComp = CultureInfo.InvariantCulture.CompareInfo;
            if (myComp.IsPrefix(isin, "BE")) market = "3";
            else if (myComp.IsPrefix(isin, "FR")) market = "1";
            else if (myComp.IsPrefix(isin, "NL")) market = "2";
            else if (myComp.IsPrefix(isin, "PT")) market = "5";
            else market = "1";
            
            string startMonth = startDate.Value.Month.ToString();
            string startDay = startDate.Value.Day.ToString();
            string startYear = startDate.Value.Year.ToString();

            string endMonth = endDate.Value.Month.ToString();
            string endDay = endDate.Value.Day.ToString();
            string endYear = endDate.Value.Year.ToString();

            urlTemplate = urlTemplate.Replace("[symbol]", isin);
            urlTemplate = urlTemplate.Replace("[market]", market);
            urlTemplate = urlTemplate.Replace("[startMonth]", startMonth);
            urlTemplate = urlTemplate.Replace("[startDay]", startDay);
            urlTemplate = urlTemplate.Replace("[startYear]", startYear);

            urlTemplate = urlTemplate.Replace("[endMonth]", endMonth);
            urlTemplate = urlTemplate.Replace("[endDay]", endDay);
            urlTemplate = urlTemplate.Replace("[endYear]", endYear);

            BarListImpl bl = new BarListImpl(BarInterval.Day, isin);
            System.Net.WebClient wc = new System.Net.WebClient();
            StreamReader res;
            try
            {
                res = new StreamReader(wc.OpenRead(urlTemplate));
                int skipCount = 0;
                string tmp = null;
                do
                {
                    tmp = res.ReadLine();
                    if (skipCount++ < 7) 
                        continue;
                    tmp = tmp.Replace(";", ",");
                    Bar b = BarImpl.FromCSV(tmp, isin, (int)BarInterval.Day);
                    foreach (Tick k in BarImpl.ToTick(b))
                        bl.newTick(k);
                } while (tmp != null);
            }
            catch (Exception)
            {
                return bl;
            }
            
            return bl;
        }



        public static bool TryCache_Chart = true;
        public static int DelayMs_Chart = 0;
        public static int MaxCacheDays_Chart = 1;
        public static Providers DefaultChartSource = Providers.Google;

        public const string CHARTDBDEFAULTNAME = "ChartDB";
        static string GetChartDb() { return GetChartDb(TradeLink.Common.Util.ProgramData(CHARTDBDEFAULTNAME)); }
        static string GetChartDb(string path)
        {
            string db = path;
            if (!System.IO.Directory.Exists(db))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(db);
                }
                catch { }
            }
            return db;
        }

        public static BarList GetChartFrom(Providers p, string sym, DebugDelegate d)
        {
            debs = d;
            switch (p)
            {
                case Providers.Google:
                    return BarListImpl.DayFromGoogle(sym, d);
                case Providers.Yahoo:
                    return BarListImpl.DayFromYahoo(sym, d);
                case Providers.Euronext:
                    return BarListImpl.DayFromEuronext(sym);

            }
            debug(sym + " No defined chart source for provider: " + p);
            return new BarListImpl();
        }

        public static BarList GetChart(string sym, bool trycache, int delayms, DebugDelegate deb) { return GetChart(sym, TradeLink.Common.Util.ToTLDate(), MaxCacheDays_Chart, trycache, delayms, deb); }
        public static BarList GetChart(string sym, Providers chartsource, DebugDelegate deb) { return GetChart(sym, TradeLink.Common.Util.ToTLDate(), MaxCacheDays_Chart, TryCache_Chart, DelayMs_Chart, chartsource,deb); }
        public static BarList GetChart(string sym, Providers chartsource) { return GetChart(sym, TradeLink.Common.Util.ToTLDate(), MaxCacheDays_Chart, TryCache_Chart, DelayMs_Chart, chartsource,null); }

        public static BarList GetChart(string sym, DebugDelegate deb) { return GetChart(sym, TradeLink.Common.Util.ToTLDate(), MaxCacheDays_Chart, TryCache_Chart, DelayMs_Chart, deb); }
        public static BarList GetChart(string sym) { return GetChart(sym, TradeLink.Common.Util.ToTLDate(), MaxCacheDays_Chart, TryCache_Chart, DelayMs_Chart, null); }
        public static BarList GetChart(string sym, int maxcacheagedays, bool trycache) { return GetChart(sym, Util.ToTLDate(), maxcacheagedays, trycache, DelayMs_Chart, null); }
        public static BarList GetChart(string sym, int maxcacheagedays, bool trycache, Providers chartsource) { return GetChart(sym, Util.ToTLDate(), maxcacheagedays, trycache, DelayMs_Chart, chartsource,null); }
        public static BarList GetChart(string sym, int maxcacheagedays, bool trycache, Providers chartsource, DebugDelegate deb) { return GetChart(sym, Util.ToTLDate(), maxcacheagedays, trycache, DelayMs_Chart, chartsource, deb); }
        public static BarList GetChart(string sym, int maxcacheagedays) { return GetChart(sym, Util.ToTLDate(), maxcacheagedays, TryCache_Chart, DelayMs_Chart, null); }

        public static BarList GetChart(string sym, int maxcacheagedays, bool trycache, int delayms, DebugDelegate deb) { return GetChart(sym, Util.ToTLDate(), maxcacheagedays, trycache, delayms, deb); }
        public static BarList GetChart(string sym, int maxcacheagedays, DebugDelegate deb) { return GetChart(sym, Util.ToTLDate(), maxcacheagedays, TryCache_Chart, DelayMs_Chart, deb); }
        public static BarList GetChart(string sym, int date, int maxcacheagedays, DebugDelegate deb) { return GetChart(sym, date, maxcacheagedays, TryCache_Chart, DelayMs_Chart, deb); }
        public static BarList GetChart(string sym, int date, int maxcacheagedays, bool trycache, int delayms, DebugDelegate deb) { return GetChart(sym, date, maxcacheagedays, trycache, delayms, DefaultChartSource, deb); }
        public static BarList GetChart(string sym, int date, int maxcacheagedays, bool trycache, int delayms, Providers chartsource, DebugDelegate deb)
        {
            debs = deb;
            // expect 
            string chart = GetDBLoc(CHARTDBDEFAULTNAME, sym, maxcacheagedays, Util.ToDateTime(date, Util.ToTLTime()), TikConst.DOT_EXT);
            string task = CHARTDBDEFAULTNAME + sym;
            // see if it exists
            if (trycache && System.IO.File.Exists(chart))
            {
                try
                {
                    var bl = TradeLink.Common.BarListImpl.FromTIK(chart);
                    bl.DefaultInterval = BarInterval.Day;
                    debug(sym + " chart existed for: " + date + " bars: " + bl.Count);
                    return bl;
                }
                catch (Exception ex)
                {
                    debug(sym + " error reading cached chart, will re-download.   (loc: " + chart + " err: " + ex.Message + ex.StackTrace);
                }
            }
            else if (TaskStatus.hasMaxFails(task))
            {
                debug(sym + " hit max failures with no success, skipping...");
                return new BarListImpl(BarInterval.Day, sym);
            }
            else
                debug(sym + " chart not found for " + date + ", downloading...");
            // grab it
            var newbl = GetChartFrom(chartsource, sym, deb);
            if (ChartSave(newbl, Path.GetDirectoryName(chart), date))
            {
                if (delayms != 0)
                    Util.sleep(delayms);
                debug(sym + " saved chart for " + date + " with bars: " + newbl.Count + " to: " + chart);
                TaskStatus.CountSuccess(task);
            }
            else
            {
                debug(sym + " error saving chart for " + date + " and bars: " + newbl.Count + " to: " + chart);
                TaskStatus.CountFail(task);
            }
            return newbl;
        }

        public static bool ChartSave(BarList bl, string path, int date)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch { return false; }
            try
            {
                if (date == 0)
                    date = bl.RecentBar.Bardate;
                string fn = TikWriter.SafeFilename(bl.symbol, path, date);
                if (System.IO.File.Exists(fn))
                {
                    try { System.IO.File.Delete(fn); }
                    catch { return false; }
                }
                TikWriter tw = new TikWriter(path, bl.symbol, date);
                foreach (Bar b in bl)
                {
                    Tick[] ticks = BarImpl.ToTick(b);
                    foreach (Tick k in ticks)
                        tw.newTick(k);
                }
                tw.Close();
                return true;
            }
            catch { }
            return false;
        }


        static string GetDbLocBase(string path)
        {
            string db = path;
            if (!System.IO.Directory.Exists(db))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(db);
                }
                catch { }
            }
            return db;
        }

        static string BuildDBLoc(string path, string sym, int date, string fileext)
        {
            return path + "\\" + sym + date + fileext;
        }

        public static string GetDBLoc(string program, string sym, int maxcacheage_days) { return GetDBLoc(program, sym, maxcacheage_days, DateTime.Now, ".xml"); }
        public static string GetDBLoc(string program, string sym, int maxcacheage_days, DateTime currentdate) { return GetDBLoc(program, sym, maxcacheage_days, currentdate, ".xml"); }
        public static string GetDBLoc(string program, string sym, int maxcacheage_days, DateTime currentdate, string fileext)
        {
            // get our db location
            string baseloc = GetDbLocBase(Util.ProgramData(program));
            // prepare present day
            int pday = Util.ToTLDate(currentdate);
            int dbday = pday;
            // search for an existing date
            int cacheage_days = 0;
            string loc = string.Empty;
            bool found = false;
            do
            {
                currentdate = currentdate.Subtract(new TimeSpan(cacheage_days, 0, 0, 0));
                loc = BuildDBLoc(baseloc, sym, Util.ToTLDate(currentdate), fileext);
                found = System.IO.File.Exists(loc);

            } while (!found && (cacheage_days++ < maxcacheage_days));
            if (!found)
                return BuildDBLoc(baseloc, sym, pday, fileext);
            return loc;

        }


        /// <summary>
        /// load previous days bar data from tick files located in tradelink tick folder
        /// </summary>
        /// <param name="PreviousDay"></param>
        /// <param name="syms"></param>
        /// <param name="AttemptToLoadPreviousDayBars"></param>
        /// <param name="_blt"></param>
        /// <param name="NewBarEvents"></param>
        /// <param name="deb"></param>
        /// <returns></returns>
        public static bool LoadPreviousBars(int PreviousDay, string[] syms, bool AttemptToLoadPreviousDayBars, ref BarListTracker _blt, SymBarIntervalDelegate NewBarEvents, DebugDelegate deb)
        {
            if (AttemptToLoadPreviousDayBars)
            {
                bool errors = false;
                foreach (string sym in syms)
                {
                    string fn = Util.TLTickDir + "\\" + sym + PreviousDay + TikConst.DOT_EXT;
                    if (System.IO.File.Exists(fn))
                    {
                        try
                        {
                            BarList test = BarListImpl.FromTIK(fn);
                            _blt[sym] = BarListImpl.FromTIK(fn, true, false, _blt[sym].CustomIntervals, _blt[sym].Intervals);
                            _blt[sym].GotNewBar += NewBarEvents;
                            if (deb != null)
                                deb(sym + " loaded historical bars from: " + fn);
                        }
                        catch (Exception ex)
                        {
                            errors = true;
                            if (deb != null)
                            {
                                deb(sym + " error loading historical bars from: " + fn);
                                deb(ex.Message + ex.StackTrace);
                            }
                        }
                    }
                    else
                    {
                        errors = true;
                        if (deb != null)
                        {
                            deb(sym + " starting from zero, no historical bar data at: " + fn);
                        }
                    }
                }
                return !errors;
            }
            return true;
        }

        /// <summary>
        /// given some number of intervals, return a list of same intervals with duplicates removed
        /// </summary>
        /// <param name="ints"></param>
        /// <returns></returns>
        public static BarInterval[] GetUniqueIntervals(params BarInterval[] ints)
        {
            List<BarInterval> final = new List<BarInterval>(ints.Length);
            foreach (BarInterval bi in ints)
                if (!final.Contains(bi))
                    final.Add(bi);
            return final.ToArray();
        }

        /// <summary>
        /// given some number of intervals, return a list of same intervals with duplicates removed
        /// </summary>
        /// <param name="ints"></param>
        /// <returns></returns>
        public static int[] GetUniqueIntervals(params int[] ints)
        {
            List<int> final = new List<int>(ints.Length);
            foreach (int bi in ints)
                if (!final.Contains(bi))
                    final.Add(bi);
            return final.ToArray();
        }

        bool _dir = false;
        public bool DoubleIncludesRecent { get { return _dir; } set { _dir = value; } }

        public double[] OpenDouble()
        {
            return Calc.taprep(Open(),_dir);
        }

        public double[] CloseDouble()
        {
            return Calc.taprep(Close(), _dir);
        }

        public double[] HighDouble()
        {
            return Calc.taprep(High(), _dir);
        }

        public double[] LowDouble()
        {
            return Calc.taprep(Low(), _dir);
        }

        public double[] VolDouble()
        {
            return Calc.taprep(Vol(), _dir);
        }


        static DebugDelegate debs = null;
        static void debug(string msg)
        {
            if (debs != null)
                debs(msg);
        }
    }








}