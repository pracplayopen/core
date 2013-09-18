using System;
using System.Collections.Generic;
using TradeLink.API;
using TradeLink.Common;
using System.IO;

namespace TikConverter
{
    public class ConvertMap
    {
        ConvertMapType type = ConvertMapType.None;
        bool ignoreinvalidtick = false;
        List<int> inp = new List<int>();
        List<string> cust = new List<string>();
        public ConvertMap(List<int> importmap, List<string> custval, ConvertMapType t, bool ignoreinvalid, DebugDelegate deb, DebugDelegate stat)
        {
            cust = custval;
            ignoreinvalidtick = ignoreinvalid;
            inp = importmap;
            type = t;
            SendDebugEvent = deb;
            SendStatusEvent = stat;
        }

        public string[] Files = new string[0];


        public bool ConvertFiles(DoubleDel progress)
        {
            bool ok = true;
            int c = 1;
            int max = Files.Length;
            // foreach file
            foreach (string fn in Files)
            {
                // read the file
                var csv = Util.getfile(fn, debug);
                // update progress
                progress((double)c++ / max);
                ok &= !string.IsNullOrWhiteSpace(csv);
                if (!ok)
                {
                    debug("Error reading: " + fn);
                    continue;
                }
                // parse data
                var data = Util.ParseCsvData(csv, debug);
                // convert it
                var ks = convert(data);
                ok &= ks.Count > 0;
                if (!ok)
                {
                    debug("No ticks converted in: " + fn);
                    continue;
                }
                // write to ticks
                ok &= TikUtil.TicksToFile(ks.ToArray(), debug);

                // update status
                if (!ok)
                    debug("Error writing ticks to TIK file.");
            }
            return ok;
        }

        public List<Tick> convert(List<List<string>> data)
        {
            List<Tick> ks = new List<Tick>(expectcount(data));
            try
            {
                

                foreach (var row in data)
                {
                    ks.AddRange(ConvertLine(row));
                }
                return ks;
            }
            catch (Exception ex)
            {
                debug("Error converting data: " + ex.Message + ex.StackTrace);
                status("Your file type might not be supported.  Request help, try another converter or build your own.");
            }
            return ks;
        }

        public int expectcount(List<List<string>> csvdata)
        {

            var expectcount = csvdata.Count;
            if (type == ConvertMapType.Bar)
                expectcount *= 4;
            return expectcount;
        }
        

        static string gf(ConvertFields cf) { return cf.ToString(); }
        public static string GetField(ConvertFields cf)
        {
            return cf.ToString();
        }
        public static string[] GetFields(params ConvertFields[] cfs)
        {
            List<string> f = new List<string>();
            foreach (var cf in cfs)
                f.Add(gf(cf));
            return f.ToArray();
        }


        public static ConvertFields[] GetFieldList(params ConvertFields[] cfs) { return cfs; }

        public static string[] GetAllFields()
        {
            return Enum.GetNames(typeof(ConvertFields));
        }

        public static ConvertFields[] GetMapFields(ConvertMapType type)
        {
            switch (type)
            {
                case ConvertMapType.Bar:
                    return GetFieldList(ConvertFields.symbol, ConvertFields.date, ConvertFields.time,
                        ConvertFields.open, ConvertFields.high, ConvertFields.low, ConvertFields.close,
                        ConvertFields.volume, ConvertFields.barinterval);
                case ConvertMapType.TimeSales:
                    return GetFieldList(ConvertFields.symbol, ConvertFields.date, ConvertFields.time,
                        ConvertFields.trade, ConvertFields.size, ConvertFields.exchange);

                case ConvertMapType.Level1Full:
                    return GetFieldList(ConvertFields.symbol, ConvertFields.date, ConvertFields.time,
                        ConvertFields.trade, ConvertFields.size, ConvertFields.exchange,
                        ConvertFields.bid, ConvertFields.bidsize, ConvertFields.bidexchange,
                        ConvertFields.ask, ConvertFields.asksize, ConvertFields.askexchange);

                case ConvertMapType.Level2Full:
                    return GetFieldList(ConvertFields.symbol, ConvertFields.date, ConvertFields.time,
                        ConvertFields.trade, ConvertFields.size, ConvertFields.exchange,
                        ConvertFields.bid, ConvertFields.bidsize, ConvertFields.bidexchange,
                        ConvertFields.ask, ConvertFields.asksize, ConvertFields.askexchange,
                        ConvertFields.depth);



            }
            return GetFieldList();
        }

        public static string[] GetConvertTypes() { return Enum.GetNames(typeof(ConvertMapType)); }

        const int SYM = 0;
        const int DATE = 1;
        const int TIME = 2;

        // tick
        const int TRADE = 3;
        const int TSIZE = 4;
        const int TEX = 5;
        const int BID = 6;
        const int BSIZE = 7;
        const int BEX = 8;
        const int ASK = 9;
        const int ASKSIZE = 10;
        const int AEX = 11;
        const int DEPTH = 12;

        const int O = 3;
        const int H = 4;
        const int L = 5;
        const int C = 6;
        const int V = 7;
        const int BI = 8;



        decimal get(string ds)
        {
            decimal v;
            if (decimal.TryParse(ds, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v))
                return v;
            return 0;
        }


        long getl(string sv)
        {
            long v;
            if (long.TryParse(sv, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v))
                return v;
            return 0;
        }

        int geti(string sv)
        {
            int v;
            if (int.TryParse(sv, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v))
                return v;
            return 0;
        }

        int getd(string sv)
        {
            DateTime dt;
            if (DateTime.TryParse(sv, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                return Util.ToTLDate(dt);
            return 0;
        }

        int gett(string sv)
        {
            DateTime dt;
            
            if (DateTime.TryParse(sv, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                return Util.ToTLTime(dt);
            return 0;
        }

        Tick[] ConvertLine(List<string> line)
        {
            if (type == ConvertMapType.Bar)
            {
                var b = GetBar(line);
                if (ignoreinvalidtick && !b.isValid)
                    return new Tick[0];
                return BarImpl.ToTick(b);
            }
            var k = GetTick(line);
            if (ignoreinvalidtick && !k.isValid)
                return new Tick[0];
            return new Tick[] { k };
        }

        string gi(List<string> r, int requestedidx)
        {
            var idx = inp[requestedidx];
            if (idx < 0)
                return cust[Math.Abs(idx)-1];
            return r[idx];
        }

        Bar GetBar(List<string> r)
        {
            if (type != ConvertMapType.Bar)
                return new BarImpl();

            Bar b = new BarImpl(get(gi(r,O)), get(gi(r,H)), get(gi(r,L)), get(gi(r,C)), getl(gi(r,V)), 
                getd(gi(r,DATE)), gett(gi(r,TIME)), gi(r,SYM), geti(gi(r,BI)));
            
            return b;
        }

        Tick GetTick(List<string> r)
        {
            Tick k = new TickImpl(gi(r,SYM));
            k.trade = get(gi(r,TRADE));
            k.bid = get(gi(r,BID));
            k.ask = get(gi(r,ASK));
            k.size = geti(gi(r,TSIZE));
            k.date = getd(gi(r,DATE));
            k.time = gett(gi(r, TIME));
            if (type > ConvertMapType.TimeSales)
            {
                k.bs = geti(gi(r,BSIZE));
                k.os = geti(gi(r,ASKSIZE));
                k.ex = gi(r,TEX);
                k.be = gi(r,BEX);
                k.oe = gi(r,AEX);
                if (type == ConvertMapType.Level2Full)
                    k.depth = geti(gi(r, DEPTH));
            }
            return k;
        }

        public event DebugDelegate SendDebugEvent;
        public event DebugDelegate SendStatusEvent;
        void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);

        }
        void status(string msg)
        {
            if (SendStatusEvent != null)
                SendStatusEvent(msg);

        }

    }

    public enum ConvertMapType
    {
        None = 0,
        /// <summary>
        /// expect: date/time/open/high/low/close/vol
        /// </summary>
        Bar,
        /// <summary>
        /// expect: date/time/trade/size/ex/bid/size/ex/ask/size/ex
        /// </summary>
        TimeSales,

        /// <summary>
        /// expect: date/time/trade/size/ex/bid/size/ex/ask/size/ex
        /// </summary>
        Level1Full,
        /// <summary>
        /// expect: date/time/trade/size/ex/bid/size/ex/ask/size/ex/depth
        /// </summary>
        Level2Full,
    }

    public enum ConvertFields
    {
        symbol,
        date,
        time,
        trade,
        size,
        exchange,
        bid,
        bidsize,
        bidexchange,
        ask,
        asksize,
        askexchange,
        depth,
        open,
        high,
        low,
        close,
        volume,
        barinterval,
    }
}
