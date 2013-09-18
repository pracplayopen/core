using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeLink.API
{
    public class MessageSETUSER
    {
        public MessageSETUSER() { }
        public MessageSETUSER(string s, string n, bool v) 
        { 
            symbol = s; 
            name = n; 
            val = v; 
        }
        public string symbol = string.Empty;
        public string name = string.Empty;
        public bool val = false;
        public bool isValid { get { return !string.IsNullOrWhiteSpace(symbol) && !string.IsNullOrWhiteSpace(name); } }
        public override string ToString()
        {
            return isValid ? "User set request for: " + name + " = " + val + " on: " + symbol : "Invalid set request.";
        }
    }
}
