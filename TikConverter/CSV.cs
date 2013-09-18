using System;
using System.IO;
using TradeLink.API;
using TradeLink.Common;

public struct CSV
{
    // fields of tradestation files
    const int DATE = 0;
    const int TIME = 1;
    const int OPEN = 2;
    const int HIGH = 3;
    const int LOW = 4;
    const int CLOSE = 5;
    const int VOLUME = 6;

    // here is where a line is converted
    public static Tick[] parseline(string line, string sym)
    {
        // split line
        string[] r = line.Split(',');
        // create tick for this symbol
        Tick[] tickArray = new Tick[4];
        Tick o = new TickImpl(sym);
        Tick h = new TickImpl(sym);
        Tick l = new TickImpl(sym);
        Tick c = new TickImpl(sym);
        // setup temp vars
        int iv = 0;
        decimal dv = 0;
        DateTime date;

        // we want to structure an array of ticks: OHLC, with volume evenly divided by each
        // these three fields will be common across all four ticks ------------------------
        // parse date
        if (DateTime.TryParse(r[DATE], out date))
        {
            o.date = Util.ToTLDate(date);
            h.date = Util.ToTLDate(date);
            l.date = Util.ToTLDate(date);
            c.date = Util.ToTLDate(date);
        }
        // parse time
        if (int.TryParse(r[TIME], out iv))
        {
            o.time = iv * 100;
            h.time = iv * 100;
            l.time = iv * 100;
            c.time = iv * 100;
        }
        // parse volume
        int volume = 0;
        if (int.TryParse(r[VOLUME], out volume))
        {
            o.size = volume / 4;
            h.size = volume / 4;
            l.size = volume / 4;
            c.size = volume / 4;
        }
        // --------------------------------------------------------------------------------

        // OPEN
        if (decimal.TryParse(r[OPEN], out dv))
        {
            o.trade = dv;
            tickArray[0] = o;
        }

        // HIGH
        if (decimal.TryParse(r[HIGH], out dv))
        {
            h.trade = dv;
            tickArray[1] = h;
        }

        // LOW
        if (decimal.TryParse(r[LOW], out dv))
        {
            l.trade = dv;
            tickArray[2] = l;
        }

        // CLOSE
        if (decimal.TryParse(r[CLOSE], out dv))
        {
            c.trade = dv;
            tickArray[3] = c;
        }

        // return tick
        return tickArray;
    }
}