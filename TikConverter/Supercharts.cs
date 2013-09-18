using System;
using TradeLink.API;
using TradeLink.Common;

namespace TikConverter
{

    public struct Supercharts
    {
        const int SYMBOL = 0;
        const int DATETIME = 1;
        const int OPEN = 2;
        const int HIGH = 3;
        const int LOW = 4;
        const int CLOSE = 5;
        const int VOL = 6;

        // here is where a line is converted
        public static Tick[] parseline(string line, string sym)
        {
            // split line
            string[] r = line.Split(',');
            sym = r[SYMBOL];
            // create tick for this symbol
            Tick[] result = new Tick[4];
            Tick high = new TickImpl(sym);
            Tick low = new TickImpl(sym);
            Tick open = new TickImpl(sym);
            Tick close = new TickImpl(sym);
            long dt=0;

            if (long.TryParse(r[DATETIME], out dt))
            {
                open.datetime = dt;
                high.datetime = dt;
                low.datetime = dt;
                close.datetime = dt;
                open.date = (int)(dt/10000);
                high.date = (int)(dt/10000);
                low.date = (int)(dt/10000);
                close.date = (int)(dt/10000);
                open.time = ((int)open.datetime - open.date * 10000)*100;
                close.time = ((int)close.datetime - close.date * 10000)*100;
                high.time = ((int)high.datetime - high.date * 10000)*100;
                low.time = ((int)low.datetime - low.date * 10000)*100;
            }
            int size = 0;
            if (int.TryParse(r[VOL], out size))
            {
                high.size = Math.Max(1, size / 4);
                low.size = Math.Max(1, size / 4);
                open.size = Math.Max(1, size / 4);
                close.size = Math.Max(1, size / 4);
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