using System;
using TradeLink.API;
using TradeLink.Common;

namespace TikConverter
{

    public struct Drolles
    {
        const int DATE = 0;
        const int TIME = 1;
        const int OPEN = 2;
        const int HIGH = 3;
        const int LOW = 4;
        const int CLOSE = 5;
        const int VOL = 6;

        // here is where a line is converted
        public static Tick[] parseline(string line, string sym)
        {
            // split line
            line=line.Remove(8, 1);
            line=line.Insert(8, ";");
            string[] r = line.Split(';');
            // create tick for this symbol
            Tick[] result = new Tick[4];
            Tick high = new TickImpl(sym);
            Tick low = new TickImpl(sym);
            Tick open = new TickImpl(sym);
            Tick close = new TickImpl(sym);
            long dt = 0;
            int tt;
            if (long.TryParse(r[DATE], out dt))
            {
                open.date = (int)(dt);
                high.date = (int)(dt);
                low.date = (int)(dt);
                close.date = (int)(dt);
            }
            //r[TIME]=r[TIME].Substring(0, 4);
            if (int.TryParse(r[TIME], out tt))
            {
                if (tt < 040000) tt += 120000;
                open.time = tt;
                close.time = tt;
                high.time = tt;
                low.time = tt;

                open.datetime = dt * 1000000 + tt;
                high.datetime = dt * 1000000 + tt;
                low.datetime = dt * 1000000 + tt;
                close.datetime = dt * 1000000 + tt;
            }

            int size = 0;
            if (int.TryParse(r[VOL], out size))
            {
                high.size = Math.Max(1, size / 4);
                low.size = Math.Max(1, size / 4);
                open.size = Math.Max(1, size / 4);
                close.size = Math.Max(1, size/4);
            }
            decimal price = 0.0M;
            if (decimal.TryParse(r[HIGH], out price))
                high.trade = price;
            if (decimal.TryParse(r[OPEN], out price))
                open.trade = price;
            if (decimal.TryParse(r[LOW], out price))
                low.trade = price;
            if (decimal.TryParse(r[CLOSE], out price))
                close.trade = price;
            result[0] = open;
            result[1] = high;
            result[2] = low;
            result[3] = close;
            return result;
        }

    }
}