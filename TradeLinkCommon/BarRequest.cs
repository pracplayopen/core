using System;
using System.Collections.Generic;
using TradeLink.API;
using TradeLink.Common;

namespace TradeLink.Common
{
    public struct BarRequest
    {
        /// <summary>
        /// client making request
        /// </summary>
        public string Client;
        public int StartDate;
        public int EndDate;
        public int StartTime;
        public int EndTime;
        public int CustomInterval;
        public string symbol;
        public int BarsBackExplicit;
        public int Interval;
        public long ID;
        public string Tag;
        public DateTime StartDateTime { get { return Util.ToDateTime(StartDate, StartTime); } }
        public DateTime EndDateTime { get { return Util.ToDateTime(EndDate, EndTime); } }
        public BarInterval BarInterval { get { return (BarInterval)Interval; } }
        public bool isExplictBarsBack { get { return BarsBackExplicit > 0; } }
        public bool isExplicitStart { get { return (StartDate != 0); } }
        public bool isExplicitEnd { get { return (EndDate != 0); } }
        public bool isExplicitDate { get { return isExplicitStart && isExplicitEnd; } }
        public bool isCustomInterval { get { return Interval < 0; } }
        public bool isIdValid { get { return ID != 0; } }
        public string NiceInterval { get { return isCustomInterval ? BarInterval.ToString() + "(" + CustomInterval + ") " : CustomInterval.ToString(); } }
        public int BarsBack { get { return isExplictBarsBack ? BarsBackExplicit : BarImpl.BarsBackFromDate(BarInterval, StartDateTime, EndDateTime); } }

        public BarRequest(string sym, int interval, int startdate, int starttime, int enddate, int endtime, string client)
        {
            BarsBackExplicit = -1;
            Client = client;
            symbol = sym;
            Interval = interval;
            StartDate = startdate;
            StartTime = starttime;
            EndDate = enddate;
            EndTime = endtime;
            ID = 0;
            if (interval > 0)
                CustomInterval = interval;
            else
                CustomInterval = 0;
            Tag = string.Empty;
        }

        public string ToMessage() { return isValid ? Serialize(this) : string.Empty; }
        public string Serialize() { return ToMessage(); }

        public BarRequest(string sym)
        {
            BarsBackExplicit = -1;
            Client = string.Empty;
            symbol = sym;
            Interval = 0;
            StartDate = 0;
            StartTime = 0;
            EndDate = 0;
            EndTime = 0;
            ID = 0;
            CustomInterval = 0;
            Tag = string.Empty;
        }
        public BarRequest(string sym, int interval, int custominterval, int barsback, string client)
        {
            BarsBackExplicit = barsback;
            Client = client;
            symbol = sym;
            Interval = interval;
            StartDate = 0;
            StartTime = 0;
            EndDate = 0;
            EndTime = 0;
            ID = 0;
            CustomInterval = custominterval;
            Tag = string.Empty;
        }

        public bool isValid { get { return !string.IsNullOrWhiteSpace(symbol) && (isExplictBarsBack || isExplicitDate); } }



        /// <summary>
        /// request historical data for one day back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static string BuildBarRequest(string symbol, BarInterval interval)
        {
            return Serialize(new BarRequest(symbol, (int)interval, Util.ToTLDate(), 0, Util.ToTLDate(), Util.ToTLTime(), string.Empty));
        }
        /// <summary>
        /// bar request for symbol and interval from previous date through present time
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="startdate"></param>
        /// <returns></returns>
        public static string BuildBarRequest(string symbol, BarInterval interval, int startdate)
        {
            return Serialize(new BarRequest(symbol, (int)interval, startdate, 0, Util.ToTLDate(), Util.ToTLTime(), string.Empty));
        }
        public static string BuildBarRequest(string symbol, int interval, int startdate)
        {
            return Serialize(new BarRequest(symbol, interval, startdate, 0, Util.ToTLDate(), Util.ToTLTime(), string.Empty));
        }

        /// <summary>
        /// build bar request for certain # of bars back from present
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="barsback"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static string BuildBarRequestBarsBack(string sym, int barsback, int interval) { return BuildBarRequestBarsBack(sym, barsback, interval, interval, string.Empty); }
        public static string BuildBarRequestBarsBack(string sym, int barsback, int interval, string client) { return BuildBarRequestBarsBack(sym, barsback, interval, interval, client); }
        public static string BuildBarRequestBarsBack(string sym, int barsback, int interval, int custinterval) { return BuildBarRequestBarsBack(sym, barsback, interval,custinterval, string.Empty); }
        public static string BuildBarRequestBarsBack(string sym, int barsback, int interval, int custinterval, string client)
        {

            return Serialize(new BarRequest(sym, interval,custinterval, barsback, client));
        }
        /// <summary>
        /// builds bar request
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static string Serialize(BarRequest br)
        {
            
            string[] r = new string[] 
            {
                br.symbol,
                br.Interval.ToString(System.Globalization.CultureInfo.InvariantCulture),
                br.StartDate.ToString(System.Globalization.CultureInfo.InvariantCulture),
                br.StartTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                br.EndDate.ToString(System.Globalization.CultureInfo.InvariantCulture),
                br.EndTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                br.ID.ToString(System.Globalization.CultureInfo.InvariantCulture),
                br.CustomInterval.ToString(System.Globalization.CultureInfo.InvariantCulture),
                br.Client,
                br.BarsBackExplicit.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            return string.Join(",", r);

        }

        /// <summary>
        /// parses message into a structured bar request
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static BarRequest Deserialize(string msg)
        {
            string[] r = msg.Split(',');
            BarRequest br = new BarRequest();
            try
            {
                br.symbol = r[(int)BarRequestField.Symbol];
                br.Interval = Convert.ToInt32(r[(int)BarRequestField.BarInt], System.Globalization.CultureInfo.InvariantCulture);
                br.StartDate = int.Parse(r[(int)BarRequestField.StartDate], System.Globalization.CultureInfo.InvariantCulture);
                br.StartTime = int.Parse(r[(int)BarRequestField.StartTime], System.Globalization.CultureInfo.InvariantCulture);
                br.EndDate = int.Parse(r[(int)BarRequestField.EndDate], System.Globalization.CultureInfo.InvariantCulture);
                br.EndTime = int.Parse(r[(int)BarRequestField.EndTime], System.Globalization.CultureInfo.InvariantCulture);
                br.CustomInterval = int.Parse(r[(int)BarRequestField.CustomInterval], System.Globalization.CultureInfo.InvariantCulture);
                br.ID = long.Parse(r[(int)BarRequestField.ID], System.Globalization.CultureInfo.InvariantCulture);
                br.Client = r[(int)BarRequestField.Client];
                br.BarsBackExplicit = Convert.ToInt32(r[(int)BarRequestField.BarsBackExplicit], System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (FormatException) { }
            catch (OverflowException) { }
            catch (ArgumentNullException) { }
            return br;
        }

        public override string ToString()
        {
            var r = string.Empty;
            var niceinterval = BarInterval.ToString();
            if (isCustomInterval)
                niceinterval += "_" + CustomInterval.ToString("F0");
            if (isExplictBarsBack)
            {
                if (isExplicitEnd) 
                    r = symbol + " " + niceinterval + " "+ EndDate +" -> " + BarsBackExplicit;
                else
                    r = symbol + " " + niceinterval + "  -> " + BarsBackExplicit;
            }
            else
                r = symbol + " " + Interval + " " + StartDateTime + "->" + EndDateTime;
            if (isIdValid)
                r += " " + ID;
            return r;
        }





    }

}
