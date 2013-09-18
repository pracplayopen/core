using System;
using System.Collections.Generic;
using TradeLink.API;

namespace TradeLink.Common
{


    /// <summary>
    /// A single bar of price data, which represents OHLC and volume for an interval of time.
    /// </summary>
    public class BarImpl : GotTickIndicator, TradeLink.API.Bar
    {
        public void GotTick(Tick k) { newTick(k); }
        string _sym = "";
        public string Symbol { get { return _sym; } }
        private ulong h = ulong.MinValue;
        private ulong l = ulong.MaxValue;
        private ulong o = 0;
        private ulong c = 0;
        private long v = 0;
        private int tradesinbar = 0;
        private bool _new = false;
        private int units = 300;
        private int _time = 0;
        private int bardate = 0;
        private bool DAYEND = false;
        public int time { get { return _time; } }
        public bool DayEnd { get { return DAYEND; } }
        ulong lHigh { get { return h; } }
        ulong lLow { get { return l; } }
        ulong lOpen { get { return o; } }
        ulong lClose { get { return c; } }
        long _id = 0;
        public long id { get { return _id; } set { _id = value; } }
        public decimal High { get { return h*Const.IPRECV; } }
        public decimal Low { get { return l * Const.IPRECV; } }
        public decimal Open { get { return o * Const.IPRECV; } }
        public decimal Close { get { return c * Const.IPRECV; } }
        public long Volume { get { return v; } }
        public bool isNew { get { return _new; } set { _new = value; } }
        public bool isValid { get { return !string.IsNullOrWhiteSpace(_sym) && (h >= l) && (o != 0) && (c != 0); } }
        public int TradeCount { get { return tradesinbar; } }

        int _ci = -1;
        public int CustomInterval { get { return _ci; } set { _ci = value; } }
        public bool isCustom { get { return (_ci > 0) && (units < 0); } }

        public BarImpl() : this(BarInterval.FiveMin) { }

        public int Interval { get { return units; } }
        public BarInterval BarInterval { get { return (API.BarInterval)units; } }

        public BarImpl(decimal open, decimal high, decimal low, decimal close, long vol, int date, int time, string symbol, int interval) : this(open, high, low, close, vol, date, time, symbol, interval, interval,0) { }
        public BarImpl(decimal open, decimal high, decimal low, decimal close, long vol, int date, int time, string symbol, int interval, int custint) : this(open, high, low, close, vol, date, time, symbol, interval, custint, 0) { }
        public BarImpl(decimal open, decimal high, decimal low, decimal close, long vol, int date, int time, string symbol, int interval, int custint, long id)
        {
            if (open < 0 || high < 0 || low < 0 || close < 0)
            {
                return;
            }
            else
            {
                _id = id;
                units = interval;
                _ci = custint;
                h = (ulong)(high * Const.IPREC);
                o = (ulong)(open * Const.IPREC);
                l = (ulong)(low * Const.IPREC);
                c = (ulong)(close * Const.IPREC);
                v = vol;
                bardate = date;
                _time = time;
                _sym = symbol;
            }
        }
        public BarImpl(BarImpl b)
        {
            _id = b.id;
            units = b.units;
            _ci = b.CustomInterval;
            v = b.Volume;
            h = b.lHigh;
            l = b.lLow;
            o = b.lOpen;
            c = b.lClose;
            DAYEND = b.DAYEND;
            _time = b._time;
            bardate = b.bardate;
        }
        
        
        public BarImpl(BarInterval tu) 
        {
            units = (int)tu;
        }
        public int Bartime 
        { 
            get 
            { 
                // get num of seconds elaps
                int elap = Util.FT2FTS(_time); 
                // get remainder of dividing by interval
                int rem = elap % Interval;
                // get datetime
                DateTime dt = Util.TLD2DT(bardate);
                // add rounded down result
                dt = dt.AddSeconds(elap-rem);
                // conver back to normal time
                int bt = Util.ToTLTime(dt);
                return bt;
            } 
        }
        public int Bardate { get { return bardate; } }
        private int bt(int time) 
        {
            // get time elapsed to this point
            int elap = Util.FT2FTS(time);
            // get seconds per bar
            int secperbar = Interval;
            // get number of this bar in the day for this interval
            int bcount = (int)((double)elap / secperbar);
            return bcount;
        }

        /// <summary>
        /// Accepts the specified tick.
        /// </summary>
        /// <param name="t">The tick you want to add to the bar.</param>
        /// <returns>true if the tick is accepted, false if it belongs to another bar.</returns>
        public bool newTick(Tick k)
        {
            TickImpl t = (TickImpl)k;
            if (_sym == "") _sym = t.symbol;
            if (_sym != t.symbol) throw new InvalidTick();
            if (_time == 0) { _time = bt(t.time); bardate = t.date;}
            if (bardate != t.date) DAYEND = true;
            else DAYEND = false;
            // check if this bar's tick
            if ((bt(t.time) != _time) || (bardate!=t.date)) return false; 
            // if tick doesn't have trade or index, ignore
            if (!t.isTrade && !t.isIndex) return true; 
            tradesinbar++; // count it
            _new = tradesinbar == 1;
            // only count volume on trades, not indicies
            if (!t.isIndex) v += t.size; // add trade size to bar volume
            if (o == 0) o = t._trade;
            if (t._trade > h) h = t._trade;
            if (t._trade < l) l = t._trade;
            c = t._trade;
            return true;
        }
        public override string ToString() { return "OHLC (" + bardate+"/"+ _time + ") " + Open.ToString("F2") + "," + High.ToString("F2") + "," + Low.ToString("F2") + "," + Close.ToString("F2"); }
        /// <summary>
        /// Create bar object from a CSV file providing OHLC+Volume data.
        /// </summary>
        /// <param name="record">The record in comma-delimited format.</param>
        /// <returns>The equivalent Bar</returns>
        public static Bar FromCSV(string record, string symbol, int interval) { return FromCSV(record, symbol, interval, false,null); }
        public static Bar FromCSV(string record,string symbol,int interval, bool failonmissing, DebugDelegate deb)
        {
            debs = deb;
            // google used as example
            string[] r = record.Split(',');
            if (r.Length < 6) return null;
            DateTime d = new DateTime();
            if (DateTime.TryParse(r[0], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d))
            {
                int date = 0;
                try
                {
                    date = Util.ToTLDate(d);
                }
                catch (Exception ex)
                {
                    debug(symbol + " unable to get bar date from: " + r[0] + " and: " + record + " err: " + ex.Message + ex.StackTrace);
                    return new BarImpl();
                }
                decimal open,high,low,close;
                long vol;
                if (!decimal.TryParse(r[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out close))
                {
                    close = 0;
                    debug(symbol + " unable to close from: " + r[4] + " and: " + record);
                    return new BarImpl();
                }
                else
                {
                    if (!decimal.TryParse(r[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out open))
                    {
                        if (failonmissing)
                            return new BarImpl();
                        debug(symbol + " error getting open, using close: " + close);
                        open = close;
                    }
                    if (!decimal.TryParse(r[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out high))
                    {
                        if (failonmissing)
                            return new BarImpl();
                        debug(symbol + " error getting high, using close: " + close);
                        high = close;
                    }
                    if (!decimal.TryParse(r[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out low))
                    {
                        if (failonmissing)
                            return new BarImpl();
                        debug(symbol + " error getting low, using close: " + close);
                        low = close;
                    }

                    if (!long.TryParse(r[5], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out vol))
                    {
                        if (failonmissing)
                            return new BarImpl();
                        vol = 0;
                        debug(symbol + " error getting vol, using: " + vol);
                    }
                    return new BarImpl(open, high, low, close, vol, date, 0, symbol, interval,interval);
                }
                
            }
            else
            {
                debug(symbol + " unable to get bar date from: " + r[0] + " and: " + record );
                
            }
            return new BarImpl();
        }

        public static string Serialize(Bar b)
        {
            const char d = ',';
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(b.Open.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.High.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Low.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Close.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Volume.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Bardate.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.time.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Symbol);
            sb.Append(d);
            sb.Append(b.Interval.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.CustomInterval.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            
            return sb.ToString();
        }

        public static Bar Deserialize(string msg)
        {
            string[] r = msg.Split(',');
            decimal open = Convert.ToDecimal(r[0], System.Globalization.CultureInfo.InvariantCulture);
            decimal high = Convert.ToDecimal(r[1], System.Globalization.CultureInfo.InvariantCulture);
            decimal low = Convert.ToDecimal(r[2], System.Globalization.CultureInfo.InvariantCulture);
            decimal close = Convert.ToDecimal(r[3], System.Globalization.CultureInfo.InvariantCulture);
            long vol = Convert.ToInt64(r[4], System.Globalization.CultureInfo.InvariantCulture);
            int date = Convert.ToInt32(r[5], System.Globalization.CultureInfo.InvariantCulture);
            int time = Convert.ToInt32(r[6], System.Globalization.CultureInfo.InvariantCulture);
            string symbol = r[7];
            int interval = Convert.ToInt32(r[8], System.Globalization.CultureInfo.InvariantCulture);
            int custint = (int)BarInterval.CustomTime;
            if (r.Length>=10)
                custint = Convert.ToInt32(r[9], System.Globalization.CultureInfo.InvariantCulture);
            long id = 0;
            if (r.Length>= 11)
            {
                try
                {
                    id = Convert.ToInt64(r[10], System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    id = 0;
                }
                catch (OverflowException)
                {
                    id = 0;
                }
            }
            return new BarImpl(open, high, low, close, vol, date, time, symbol,interval,custint,id);
        }

        /// <summary>
        /// convert a bar into an array of ticks
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        public static Tick[] ToTick(Bar bar)
        {
            if (!bar.isValid) return new Tick[0];
            List<Tick> list = new List<Tick>();
            list.Add(TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime, bar.Open,
(int)((double)bar.Volume / 4), string.Empty));
            list.Add(TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime,
bar.High, (int)((double)bar.Volume / 4), string.Empty));
            list.Add(TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime, bar.Low,
(int)((double)bar.Volume / 4), string.Empty));
            list.Add(TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime,
bar.Close, (int)((double)bar.Volume / 4), string.Empty));
            return list.ToArray();
        }

  


        public static DateTime DateFromBarsBack(int barsback, BarInterval intv) { return DateFromBarsBack(barsback, intv, DateTime.Now); }
        public static DateTime DateFromBarsBack(int barsback, BarInterval intv, DateTime enddate) { return DateFromBarsBack(barsback, (int)intv, enddate); }
        public static DateTime DateFromBarsBack(int barsback, int interval) { return DateFromBarsBack(barsback, interval, DateTime.Now); }
        public static DateTime DateFromBarsBack(int barsback, int interval, DateTime enddate)
        {
           return enddate.Subtract(new TimeSpan(0,0,interval*barsback));
        }

        public static int BarsBackFromDate(BarInterval interval, int startdate) { return BarsBackFromDate(interval, startdate, Util.ToTLDate()); }
        public static int BarsBackFromDate(BarInterval interval, int startdate, int enddate) { return BarsBackFromDate(interval, Util.ToDateTime(startdate, 0), Util.ToDateTime(enddate,Util.ToTLTime())); }
        public static int BarsBackFromDate(BarInterval interval, DateTime startdate, DateTime enddate)
        {
            double start2endseconds = enddate.Subtract(startdate).TotalSeconds;
            int bars = (int)((double)start2endseconds / (int)interval);
            return bars;
        }


        static DebugDelegate debs = null;
        static void debug(string msg)
        {
            if (debs != null)
                debs(msg);
        }

        
    }

    

}
