using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeLink.API;
using TradeLink.Common;

namespace TradeLink.Research
{
    public struct Screener : SymbolI
    {
        const int FV_NUM = 0;
        const int FV_TICKER = 1;
        const int FV_COMPANY = 2;
        const int FV_SECTOR = 3;
        const int FV_INDUSTRY = 4;
        const int FV_COUNTRY = 5;
        const int FV_MARKETCAP = 6;
        const int FV_PE = 7;
        const int FV_PRICE = 8;
        const int FV_CHANGE = 9;
        const int FV_VOLUME = 10;

        const int QL_TICKER = 0;
        const int QL_DATASETCODE = 1;
        const int QL_COMPANY = 2;
        const int QL_INDUSTRY = 3;
        const int QL_EXCHANGE = 4;
        const int QL_ID = 5;

        const decimal ERROR_VAL = decimal.MinValue;

        

        public int id;
        string _sym;
        public string symbol { get { return _sym; } set { _sym = value; } }
        public bool isValid { get { return !string.IsNullOrWhiteSpace(symbol); } }
        public bool isCompanyPresent { get { return !string.IsNullOrWhiteSpace(company); } }
        public bool isSectorPresent { get { return !string.IsNullOrWhiteSpace(sector); } }
        public bool isIndustryPresent { get { return !string.IsNullOrWhiteSpace(industry); } }
        public bool isCountryPresent { get { return !string.IsNullOrWhiteSpace(country); } }
        public bool isMarketCapPresent { get { return marketcap > 0; } }
        public bool isPERatioPresent { get { return (peratio != ERROR_VAL); } }
        public bool isPricePresent { get { return (price != ERROR_VAL); } }
        public bool isPctChangePresent { get { return pctchange != ERROR_VAL; } }
        public bool isVolumePresent { get { return volume > 0; } }
        public bool isSourcePresent { get { return !string.IsNullOrWhiteSpace(source); } }
        public string company;
        public string sector;
        public string industry;
        public string country;
        public decimal marketcap;
        public decimal peratio;
        public decimal price;
        public decimal pctchange;
        public Int64 volume;
        public string source;


        public Screener(Screener copy)
        {
            _sym = string.Empty;
            industry = copy.industry;
            company = copy.company;
            marketcap = copy.marketcap;
            sector = copy.sector;
            country = copy.country;
            peratio = copy.peratio;
            price = copy.price;
            pctchange = copy.pctchange;
            volume = copy.volume;
            id = copy.id;
            source = copy.source;
            symbol = copy.symbol;
            

        }

        public override string ToString()
        {
            return symbol + " screen";
        }

        static DebugDelegate d = null;
        static void debug(string msg)
        {
            if (d != null)
                d(msg);
        }
        static bool isstring(int pos, string line)
        {
            return line[pos]=='\"';
        }
        static int geteos(int start, string line)
        {
            return line.IndexOf('\"',start+1);
        }
        static int geteod(int start, string line)
        {
            int eod = line.IndexOf(',',start);
            if (eod==-1)
                eod = line.Length;
            return eod;
        }
        static string getdata(int start, int eod, string line)
        {
            return line.Substring(start, eod - start).Replace("%", string.Empty).Replace(",",string.Empty);
        }
        static string getstring(int start, int eos, string line)
        {
            return line.Substring(start,eos-start).Replace("\"",string.Empty).Replace(","," ");
        }
        static string[] getrec(string line)
        {
            // get record number
            int rn = 0;
            int lastrn = -1;
            List<string> r = new List<string>();
            // process every character
            for (int c = 0; c<line.Length; c++)
            {
                bool newrec = rn!=lastrn;
                lastrn = rn;
                if (newrec && isstring(c,line))
                {
                    int eos = geteos(c,line);
                    r.Add(getstring(c,eos,line));
                    rn++;
                    c = eos+1;
                }
                else if (newrec)
                {
                    int eod = geteod(c,line);
                    string data = getdata(c, eod, line);
                    if (data == string.Empty)
                        data = "0";
                    r.Add(data);
                    rn++;
                    c = eod;
                }

            }

            return r.ToArray();

            

        }

        public const string FINVIZ_SOURCE = "finviz.com";
        public const string QUANDL_SOURCE = "quandl.com";
        static Screener getscreen_fv(string[] rec)
        {
            Screener s = new Screener();
            s.id = Convert.ToInt32(rec[FV_NUM]);
            s.symbol = rec[FV_TICKER];
            s.company = rec[FV_COMPANY];
            s.sector = rec[FV_SECTOR];
            s.industry = rec[FV_INDUSTRY];
            s.country = rec[FV_COUNTRY];
            s.marketcap = Convert.ToDecimal(rec[FV_MARKETCAP]);
            s.peratio = Convert.ToDecimal(rec[FV_PE]);
            s.price = Convert.ToDecimal(rec[FV_PRICE]);
            s.pctchange = Convert.ToDecimal(rec[FV_CHANGE].Replace("%",string.Empty));
            s.volume = Convert.ToInt64(rec[FV_VOLUME]);
            s.source = FINVIZ_SOURCE;
            return s;
        }

        static Screener getscreen_qdl(string[] rec)
        {
            Screener s = new Screener();
            if ((rec.Length <= QL_ID) || (rec.Length > 1) && rec[1].ToLower().Contains("no longer trades"))
                return s;
            s.id = Convert.ToInt32(rec[QL_ID]);
            s.symbol = rec[QL_TICKER];
            s.company = rec[QL_COMPANY];
            s.sector = string.Empty;
            s.industry = rec[FV_INDUSTRY];
            s.country = "US";
            s.marketcap = 0;
            s.peratio = decimal.MinValue;
            s.price = decimal.MinValue;
            s.pctchange = 0;
            s.volume = -1;
            s.source = QUANDL_SOURCE;
            return s;
        }

        const string quandlstockinfo_url = "https://s3.amazonaws.com/quandl-static-content/Ticker+CSV%27s/Stock+Exchanges/stockinfo.csv";

        public static GenericTracker<Screener> fetchscreen() { return fetchscreen(quandlstockinfo_url, null); }
        public static GenericTracker<Screener> fetchscreen(DebugDelegate deb) { return fetchscreen(quandlstockinfo_url, deb); }
        public static GenericTracker<Screener> fetchscreen(string url, DebugDelegate deb)
        {
            // get raw list
            string[][] raw = fetchrawlist(url,deb);
            debug("beginning screen indexing of "+raw.GetLength(0)+" screens.");
            GenericTracker<Screener> ss = new GenericTracker<Screener>(raw.GetLength(0), "SCREENS", new Screener());
            int l = 0;
            foreach (string[] r in raw)
            {
                l++;
                Screener s = getscreen_qdl(r);
                if (s.isValid)
                    ss.addindex(s.symbol, s);
                else
                {
/*
                    if (r!=null)
                        debug("ignoring invalid screen line#"+l.ToString("N0")+" with data: " + Util.join(r));
                    else
                        debug("ignoring invalid screen line#" + l.ToString("N0") + " with no data.");
 */
                }
            }
            debug("completed index of "+ss.Count+" screens.");
            return ss;

        }
        const string finzurl = "http://finviz.com/export.ashx?v=111&ft=1&ta=1&p=d&r=1";
        static string[][] fetchrawlist(string url, DebugDelegate deb)
        {
            d = deb;
            
            debug("grabbing screen data from: "+url);
            string content = string.Empty;
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                content = wc.DownloadString(url);
                debug("obtained " + content.Length + " bytes of screen data.");
            }
            catch (Exception ex)
            {
                debug("error obtaining data from: " + url + " err: " + ex.Message + ex.StackTrace);
                return new string[0][];
            }
            string[] lines = content.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[][] final = new string[lines.Length - 1][];
            int err = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                // get line
                string line = lines[i];

                try
                {
                    // get records
                    string[] rec = getrec(line);
                    final[i - 1] = rec;
                }
                catch (Exception ex)
                {
                    err++;
                    debug("error parsing line: " + line+" err: "+ex.Message+ex.StackTrace);
                }
            }
            debug("retrieved " + final.GetLength(0) + " screen records with " + err + " errors.");
            return final;
        }

    }
}
