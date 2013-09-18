using System;
using System.Text;
using System.Globalization;

using TradeLink.API;
using TradeLink.Common;

namespace TikConverter
{
    public struct TrueFX
    {
        public const string SOURCE = "TrueFX";

        const int SYM = 0;
        const int DATETIME = 1;
        const int BID = 2;
        const int ASK = 3;

        public static Tick parseline(string line, string sym)
        {
            int decimalplaces = 5;

            string[] r = line.Split(',');            

            var t = new TickImpl(sym);

            if(sym.Contains("JPY"))  
                decimalplaces = 3;
            
            DateTime dt;
            if (DateTime.TryParseExact(r[DATETIME], "yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                t.date = Util.ToTLDate(dt.Date);
                t.time = Util.ToTLTime(dt);
            }

            decimal b, a;
            if (decimal.TryParse(r[BID], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out b))
                t.bid = b;
            if (decimal.TryParse(r[ASK], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out a))
                t.ask = a;
            
            //simulate bid/offer midpoint trade-tick
            decimal px = Math.Round((a + b) / 2, decimalplaces); 
            t.trade = px;

            //there is no size information in the data
            //assume bid/offer size of 500K basecurrency units
            //simulated midpoint trade of 1 BCU, for compatibility only
            t.bs = 500000; t.os = 500000; t.size = 1;  

            return t;
        }
    }
}