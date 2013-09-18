using System;
using System.Collections.Generic;
using System.Collections;
using TradeLink.API;
using System.IO;
using System.Text.RegularExpressions;

namespace TradeLink.Common
{
    /// <summary>
    /// Holds collections of securities.
    /// </summary>
    [Serializable]
    public class BasketImpl : TradeLink.API.Basket
    {
        /// <summary>
        /// gets symbols removed from new list of symbols, given original list
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newb"></param>
        /// <returns></returns>
        public static Basket Subtract(string[] old, string[] newb)
        {
            return Subtract(new BasketImpl(old), new BasketImpl(newb));
        }
        /// <summary>
        /// gets symbols removed from newbasket, given original basket
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newb"></param>
        /// <returns></returns>
        public static Basket Subtract(Basket old, Basket newb)
        {
            if (old.Count == 0) return new BasketImpl();
            Basket rem = new BasketImpl();
            foreach (Security sec in old)
            {
                if (!newb.ToString().Contains(sec.symbol))
                    rem.Add(sec);
            }
            return rem;
        }

        public static string[] TrimSymbols(string[] syms)
        {
            List<string> trimmed = new List<string>();
            for (int i = 0; i < syms.Length; i++)
            {
                syms[i] = syms[i].TrimStart(' ', ',');
                syms[i] = syms[i].TrimEnd(' ', ',');
                // ensure we still have a symbol
                if (string.IsNullOrWhiteSpace(syms[i]))
                    continue;
                trimmed.Add(syms[i]);
            }
            return trimmed.ToArray();
        }

        /// <summary>
        /// Create a basket of securities
        /// </summary>
        /// <param name="onesymbol">first symbol</param>
        public BasketImpl(string onesymbol) : this(new string[] { onesymbol }) { }
        /// <summary>
        /// Create a basket of securities
        /// </summary>
        /// <param name="symbolist">symbols</param>
        public BasketImpl(string[] symbolist)
        {
            foreach (string s in symbolist)
                Add(new SecurityImpl(s));
        }
        /// <summary>
        /// clone a basket
        /// </summary>
        /// <param name="copy"></param>
        public BasketImpl(Basket copy)
        {
            foreach (Security s in copy)
                Add(new SecurityImpl(s));
            Name = copy.Name;
        }
        /// <summary>
        /// Create a basket of securities
        /// </summary>
        /// <param name="firstsec">security</param>
        public BasketImpl(SecurityImpl firstsec)
        {
            Add(firstsec);
        }
        /// <summary>
        /// Create a basket of securities
        /// </summary>
        /// <param name="securities"></param>
        public BasketImpl(SecurityImpl[] securities)
        {
            foreach (SecurityImpl s in securities)
                Add(s);
        }
        public BasketImpl() { }
        public Security this [int index] { get { return securities[index]; } set { securities[index] = value; } }
        List<Security> securities = new List<Security>();
        string _name = "";
        /// <summary>
        /// name of basket
        /// </summary>
        public string Name { get { return _name; } set { _name = value; } }
        public int Count { get { return securities.Count; } }
        public bool isNotEmpty { get { return securities.Count > 0; } }
        /// <summary>
        /// adds a security if not already present
        /// </summary>
        /// <param name="sym"></param>
        public void Add(string sym) 
        {
            var sec = SecurityImpl.Parse(sym);
            Add(sec); 

        }




        public event DebugDelegate SendDebugEvent;

        protected virtual void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);
        }

        /// <summary>
        /// adds a security if not already present
        /// </summary>
        /// <param name="s"></param>
        public void Add(Security s) 
        {
            if (s.isValid &&
                !contains(s))
            {
                securities.Add(s);
                longsym2secidx.addindex(s.FullName, securities.Count);
                shortsym2secidx.addindex(s.symbol, securities.Count);
                debug(s.symbol + 
                    (s.hasType ? " added security: " + s.Type.ToString() : string.Empty) + 
                    (s.hasDest ? " for: " + s.DestEx : string.Empty) + " full symbol: " + s.FullName);
            }
        }

        GenericTracker<int> longsym2secidx = new GenericTracker<int>();
        GenericTracker<int> shortsym2secidx = new GenericTracker<int>();

        bool contains(string sym) { foreach (Security s in securities) if (s.symbol == sym) return true; return false; }
        bool contains(Security sec) { return securities.Contains(sec); }

        /// <summary>
        /// whether security is present
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public bool isSecurityPresent(Security sec)
        {
            return contains(sec);
        }
        /// <summary>
        /// whether short symbol is present
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public bool isSymbolPresent(string sym)
        {
            var idx = shortsym2secidx.getindex(sym);
            return idx >= 0;
        }
        /// <summary>
        /// whether full symbol is present
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public bool isFullSymbolPresent(string sym)
        {
            var idx = longsym2secidx.getindex(sym);
            return idx >= 0;
        }
        /// <summary>
        /// whether symbol can be found by long or short form
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public bool isAnySymbolPresent(string sym)
        {
            return isFullSymbolPresent(sym) || isSymbolPresent(sym);
        }

        /// <summary>
        /// attempt to get security by long symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        public bool TryGetSecurityFullSymbol(string sym, out Security sec)
        {
            var idx = longsym2secidx.getindex(sym);
            sec = new SecurityImpl();
            if (idx < 0)
                return false;
            sec = this[idx];
            return true;
        }
        /// <summary>
        /// attempt to get security by short symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        public bool TryGetSecurityShortSymbol(string sym, out Security sec)
        {
            var idx = shortsym2secidx.getindex(sym);
            sec = new SecurityImpl();
            if (idx < 0)
                return false;
            sec = this[idx];
            return true;
        }
        /// <summary>
        /// try to fetch security using long then short symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        public bool TryGetSecurityAnySymbol(string sym, out Security sec)
        {
            var idx = longsym2secidx.getindex(sym);
            sec = new SecurityImpl();
            if (idx < 0)
            {
                idx = shortsym2secidx.getindex(sym); 
                if (idx<0)
                    return false;
            }
            sec = this[idx];
            return true;
        }

        /// <summary>
        /// adds contents of another basket to this one.
        /// will not result in duplicate symbols
        /// </summary>
        /// <param name="mb"></param>
        public void Add(Basket mb)
        {
            for (int i = 0; i < mb.Count; i++)
                this.Add(mb[i]);
        }
        public void Add(string[] syms)
        {
            for (int i = 0; i < syms.Length; i++)
                this.Add(syms[i]);
        }
        /// <summary>
        /// removes all elements of baskets that match.
        /// unmatching elements are ignored
        /// </summary>
        /// <param name="mb"></param>
        public void Remove(Basket mb)
        {
            List<int> remove = new List<int>();
            for (int i = 0; i < securities.Count; i++)
                for (int j = 0; j < mb.Count; j++)
                    if (securities[i].symbol == mb[j].symbol)
                        remove.Add(i);
            for (int i = remove.Count - 1; i >= 0; i--)
                securities.RemoveAt(remove[i]);
        }
        /// <summary>
        /// remove single symbol from basket
        /// </summary>
        /// <param name="symbol"></param>
        public void Remove(string symbol) { int i = -1; for (int j = 0; j < securities.Count; j++) if (securities[j].symbol == symbol) i = j; if (i != -1) securities.RemoveAt(i); }
        /// <summary>
        /// remove index of a particular symbol
        /// </summary>
        /// <param name="i"></param>
        public void Remove(int i) { securities.RemoveAt(i); }
        /// <summary>
        /// remove security from basket
        /// </summary>
        /// <param name="s"></param>
        public void Remove(Security s) { securities.Remove(s); }
        /// <summary>
        /// empty basket
        /// </summary>
        public void Clear() 
        { 
            securities.Clear();
            shortsym2secidx.Clear();
            longsym2secidx.Clear();
        }
        public static string Serialize(Basket b)
        {
            List<string> s = new List<string>();
            for (int i = 0; i < b.Count; i++) s.Add(b[i].FullName);
            return string.Join(",", s.ToArray());
        }

        public static BasketImpl Deserialize(string serialBasket)
        {
            BasketImpl mb = new BasketImpl();
            if ((serialBasket == null) || (serialBasket == "")) return mb;
            string[] r = serialBasket.Split(',');
            for (int i = 0; i < r.Length; i++)
            {
                if (r[i] == "") continue;
                SecurityImpl sec = SecurityImpl.Parse(r[i]);
                if (sec.isValid)
                    mb.Add(sec);
            }
            return mb;
        }
        public static Basket FromFile(string filename)
        {
            try
            {
                StreamReader sr = new StreamReader(filename);
                string file = sr.ReadToEnd();
                sr.Close();

                char[] filters = {'\r', '\n', ','};
                string[] syms = file.Split(filters, StringSplitOptions.RemoveEmptyEntries);
                BasketImpl b = new BasketImpl(syms);
                b.Name = Path.GetFileNameWithoutExtension(filename);
                return b;
            }
            catch { }
            return new BasketImpl();
        }

        public static void ToFile(Basket b, string filename) { ToFile(b, filename, false); }
        public static void ToFile(Basket b, string filename, bool append)
        {
            StreamWriter sw = new StreamWriter(filename, append);
            for (int i = 0; i < b.Count; i++)
                sw.WriteLine(b[i].symbol);
            sw.Close();

        }
        public override string ToString() { return Serialize(this); }
        public static BasketImpl FromString(string serialbasket) { return Deserialize(serialbasket); }
        public IEnumerator GetEnumerator() { foreach (SecurityImpl s in securities) yield return s; }

        public Security[] ToArray()
        {
            return securities.ToArray();
        }

        public string[] ToSymArray()
        {
            string[] syms = new string[securities.Count];
            for (int i = 0; i < syms.Length; i++)
                syms[i] = securities[i].symbol;
            return syms;
        }

        public string[] ToSymArrayFull()
        {
            string[] syms = new string[securities.Count];
            for (int i = 0; i < syms.Length; i++)
                syms[i] = securities[i].FullName;
            return syms;
        }

        /// <summary>
        /// removes duplicate symbols
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Basket RemoveDupe(Basket input)
        {
            List<string> cache = new List<string>();
            Basket output = new BasketImpl();
            for (int i = 0; i < input.Count; i++)
                if (!cache.Contains(input[i].symbol))
                {
                    output.Add(input[i]);
                    cache.Add(input[i].symbol);
                }
            return output;
        }

        static Basket verify_equitysymbol(Basket b) { return verify_equitysymbol(b, true, null); }
        static Basket verify_equitysymbol(Basket b, DebugDelegate d) { return verify_equitysymbol(b, true, d); }
        static Basket verify_equitysymbol(Basket b, bool googleyahoo, DebugDelegate d)
        {
            debs = d;
            b = RemoveDupe(b);
            if (googleyahoo)
            {
                Basket v = new BasketImpl();
                foreach (Security s in b)
                {
                    BarList bl = BarListImpl.DayFromAny(s.symbol,sdebug);
                    if (bl.Count > 0)
                    {
                        v.Add(s.symbol);
                        sdebug("verified google/yahoo: " + s.symbol + " bars: " + bl.Count);
                    }
                    else
                        sdebug("ignoring, not verified google/yahoo: " + s.symbol + " bars: " + bl.Count);
                }
                return v;
            }
            return b;
        }

        public static Basket parsedata(string data) { return parsedata(data, true, true, null); }
        public static Basket parsedata(string data, bool onlylinked, bool verify, DebugDelegate d)
        {
            debs = d;
            Basket b = new BasketImpl();
            if (onlylinked)
            {
                Basket b2 = LinkedOnlyNASDAQ(data);
                if (b2.Count > 0)
                    b.Add(b2);
                Basket b3 = LinkedOnlyNYSE(data);
                if (b3.Count > 0)
                    b.Add(b3);
            }
            else
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"[A-Z.]{1,6}");
                foreach (System.Text.RegularExpressions.Match m in r.Matches(data))
                    b.Add(m.Value);
            }
            return verify_equitysymbol(b, verify, d);
        }

        /// <summary>
        /// gets nyse symbols
        /// </summary>
        /// <param name="ParseStocks"></param>
        /// <returns></returns>
        public static BasketImpl NYSE(string ParseStocks)
        {
            BasketImpl mb = new BasketImpl();
            MatchCollection mc = Regex.Matches(ParseStocks, @"\b[A-Z]{1,3}\b");
            for (int i = 0; i < mc.Count; i++)
                mb.Add(new SecurityImpl(mc[i].Value.ToUpper()));
            return mb;
        }
        /// <summary>
        /// gets nasdaq symbols
        /// </summary>
        /// <param name="ParseStocks"></param>
        /// <returns></returns>
        public static BasketImpl NASDAQ(string ParseStocks)
        {
            BasketImpl mb = new BasketImpl();
            string regexp = @"\b[A-Z]{4}\b";
            MatchCollection mc = Regex.Matches(ParseStocks, regexp);
            for (int i = 0; i < mc.Count; i++)
                mb.Add(new SecurityImpl(mc[i].Value.ToUpper()));
            return mb;
        }
        /// <summary>
        /// gets clickable symbols found in a string (eg html)
        /// </summary>
        /// <param name="parsestring"></param>
        /// <returns></returns>
        public static BasketImpl LinkedOnlyNYSE(string parsestring)
        {
            BasketImpl mb = new BasketImpl();
            string regexp = @">[A-Z]{1,3}</a>";
            MatchCollection mc = Regex.Matches(parsestring, regexp);
            for (int i = 0; i < mc.Count; i++)
            {
                string chunk = mc[i].Value;
                chunk = chunk.Replace("</a>", "");
                chunk = chunk.TrimStart('>');
                mb.Add(new SecurityImpl(chunk.ToUpper()));
            }
            return mb;
        }
        /// <summary>
        /// gets clickable nasdaq symbols found in a string (eg html)
        /// </summary>
        /// <param name="parsestring"></param>
        /// <returns></returns>
        public static BasketImpl LinkedOnlyNASDAQ(string parsestring)
        {
            BasketImpl mb = new BasketImpl();
            string regexp = @">[A-Z]{4}</a>";
            MatchCollection mc = Regex.Matches(parsestring, regexp);
            for (int i = 0; i < mc.Count; i++)
            {
                string chunk = mc[i].Value;
                chunk = chunk.Replace("</a>", "");
                chunk = chunk.TrimStart('>');
                mb.Add(new SecurityImpl(chunk.ToUpper()));
            }
            return mb;
        }

        static DebugDelegate debs = null;
        static void sdebug(string msg)
        {
            if (debs != null)
                debs(msg);
        }

    }
}
