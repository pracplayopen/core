using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeLink.API
{
    public interface Model
    {
        string symbol { get; }
        string owner { get; set; }
        bool isValid { get; }
        bool isDynamic { get; }
        string[] Value { get; set; }
        List<string> ToItem();
        List<object> ToData();
    }
}
