using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeLink.Common
{
    public class config
    {
        public static int DecimalPlaces = 2;
        public static string DecimalFormatDisplay { get { return "N" + DecimalPlaces; } }
        public static string DecimalFormatExport { get { return "F" + DecimalPlaces; } }
    }
}
