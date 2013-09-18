using System;
using System.Collections.Generic;
using System.Text;
using TradeLink.API;
using TradeLink.Common;

namespace TikConverter
{
    public struct CQG
    {
        // data format specification is at
        // https://www.cqgdatafactory.com/docs/CQGDataFactory_FAQ.pdf

        public const string SOURCE = "CQG";
        // fields of CQG files
        const int SYM = 0;
        const int DATE = 1;
        const int TIME = 3;
        const int PRICE = 4;
        const int TYPE = 5;

        // here is where a line is converted
        public static Tick parseline(string line, int defaultsize, int decimalplaces)
        {
            // split line
            string[] r;
            if (line.Contains(","))
                r = line.Split(','); // optional CQG format
            else
                r = line.Split(' '); // standard CQG format

            // create tick for this symbol
            string symbol = r[SYM];

            Tick k = new TickImpl(symbol);
            // setup temp vars
            int iv = 0;
            decimal dv = 0;
            // parse date
            if (int.TryParse(r[DATE], out iv))
                k.date = iv;
            // parse time
            if (int.TryParse(r[TIME], out iv))
                k.time = iv * 100;

            // parse price
            if (decimal.TryParse(r[PRICE], out dv))
            {
                decimal divisor = (decimal)(Math.Pow(10, decimalplaces));
                dv = (decimal)dv / divisor;
                string type = r[TYPE];
                if (type == "T") // trade
                {
                    k.trade = dv;
                    k.size = defaultsize;
                }
                else if (type == "B") // bid
                {
                    k.bid = dv;
                    k.bs = defaultsize;
                }
                else if (type == "A") // ask
                {
                    k.ask = dv;
                    k.os = defaultsize;
                }
            }
            // return tick
            return k;
        }
    }
}
