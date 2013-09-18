using System;
using System.Collections.Generic;
using System.Text;
using TradeLink.API;

namespace TradeLink.Common
{
    public class TimeIntervalData : IntervalData
    {
        public event SymBarIntervalDelegate NewBar;
        public List<decimal> open() { return opens; }
        public List<decimal> close() { return closes; }
        public List<decimal> high() { return highs; }
        public List<decimal> low() { return lows; }
        public List<long> vol() { return vols; }
        public List<int> date() { return dates; }
        public List<int> time() { return times; }
        public List<int> tick() { return ticks; }
        public bool isRecentNew() { return _isRecentNew; }
        public int Count() { return _Count; }
        public int Last() { return _Count - 1; }

        public TimeIntervalData(int unitsPerInterval)
        {
            intervallength = unitsPerInterval;
        }
        public void Reset()
        {
            opens.Clear();
            closes.Clear();
            highs.Clear();
            lows.Clear();
            dates.Clear();
            times.Clear();
            vols.Clear();
            _Count = 0;
        }
        void newbar(long id)
        {
            _Count++;
            opens.Add(0);
            closes.Add(0);
            highs.Add(0);
            lows.Add(decimal.MaxValue);
            vols.Add(0);
            times.Add(0);
            dates.Add(0);
            ids.Add(id);
        }
        public void addbar(Bar mybar)
        {
            _Count++;
            closes.Add(mybar.Close);
            opens.Add(mybar.Open);
            dates.Add(mybar.Bardate);
            highs.Add(mybar.High);
            lows.Add(mybar.Low);
            vols.Add(mybar.Volume);
            times.Add(mybar.Bartime);
            ids.Add(getbarid(mybar.Bartime, mybar.Bardate, intervallength));
        }
        long curr_barid = -1;
        int intervallength = 60;
        int intervaltype = (int)BarInterval.CustomTime;
        public int IntType { get { return intervaltype; } }
        public int IntSize { get { return intervallength; } }

        internal List<decimal> opens = new List<decimal>();
        internal List<decimal> closes = new List<decimal>();
        internal List<decimal> highs = new List<decimal>();
        internal List<decimal> lows = new List<decimal>();
        internal List<long> vols = new List<long>();
        internal List<int> dates = new List<int>();
        internal List<int> times = new List<int>();
        internal List<int> ticks = new List<int>();
        internal List<long> ids = new List<long>();
        internal int _Count = 0;
        internal bool _isRecentNew = false;
        public Bar GetBar(int index, string symbol)
        {
            Bar b = new BarImpl();
            if (index >= _Count) return b;
            else if (index < 0)
            {
                index = _Count - 1 + index;
                if (index < 0) return b;
            }
            b = new BarImpl(opens[index], highs[index], lows[index], closes[index], vols[index], dates[index], times[index], symbol,intervaltype,intervallength);
            if (index == Last()) b.isNew = _isRecentNew;
            return b;
        }

        public static bool isOldTickBackfillEnabled = true;

        public Bar GetBar(string symbol) { return GetBar(Last(), symbol); }
        public void newTick(Tick k)
        {
            // ignore quotes
            if (k.trade == 0) 
                return;
            newPoint(k.symbol, k.trade, k.time, k.date, k.size);
        }
        public void newPoint(string symbol, decimal p, int time, int date, int size)
        {

            appendpoint(symbol, p, time, date, size);
           

        }

        private void appendpoint(string symbol, decimal p, int time, int date, int size)
        {
            // get the barcount
            long barid = getbarid(time, date, intervallength);
            int index = 0;
            // if not current bar
            if (barid == curr_barid)
            {
                _isRecentNew = false;
                index = Last();
            }
                // if bar is a new one
            else if (barid > curr_barid)
            {
                // create a new one
                newbar(barid);
                // mark it
                _isRecentNew = true;
                // make it current
                curr_barid = barid;
                // set time
                times[times.Count - 1] = time;
                // set date
                dates[dates.Count - 1] = date;
                index = Last();
            }
            else // otherwise it's a backfill
            {
                _isRecentNew = false;

                // find the appropriate index to insert the bar (by id)
                int place = 0;
                bool found = false;
                for(int x = 0; x <= ids.Count; x++)
                {
                    // if the bar already exists
                    if (ids[x] == barid)
                    {
                        place = x;
                        found = true;
                        break;
                    }
                    // if there's a gap where the bar should exist
                    else if (ids[x] > barid)
                    {
                        place = x;
                        break;
                    }
                }
                // older than oldest
                if (!found)
                {
                    _Count++;
                    opens.Insert(place, 0);
                    closes.Insert(place, 0);
                    highs.Insert(place, 0);
                    lows.Insert(place, decimal.MaxValue);
                    vols.Insert(place, 0);
                    times.Insert(place, time);
                    dates.Insert(place, date);
                    ids.Insert(place, barid);
                }

                index = place;
            }
            // blend tick into bar
            // open
            if (opens[index] == 0) 
                opens[index] = p;
            // high
            if (p > highs[Last()]) 
                highs[Last()] = p;
            // low
            if (p < lows[index]) 
                lows[index] = p;
            // close
            closes[index] = p;
            // volume
            if (p >= 0)
                vols[index] += size;
            // notify barlist
            if (_isRecentNew)
                NewBar(symbol, intervallength);

        }

        static internal long getbarid(int time, int date, int intervallength)
        {
            // get time elapsed to this point
            int elap = Util.FT2FTS(time);
            // get number of this bar in the day for this interval
            long bcount = (int)((double)elap / intervallength);
            // add the date to the front of number to make it unique
            bcount += (long)date * 10000;
            return bcount;
        }

    }
}
