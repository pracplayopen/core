using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeLink.Common;
using TradeLink.API;
using TradeLink.Research;

namespace TestTradeLink
{
#if DEBUG
    // for speeding up debugger-based unit test runs
    // all tests marked as explicit only temporarily unmark for test being debugged
    [TestFixture, Explicit]
    //[TestFixture]
#else
    [TestFixture]
#endif
    public class TestRandomSymbol
    {
        public TestRandomSymbol() { }

        [Test]
        public void Basics()
        {
            string [] syms = RandomSymbol.GetSymbols((int)DateTime.Now.Ticks, 4, 100);
            bool v = true;

            foreach (string sym in syms)
            {
                bool bv = v;
                v &= (sym.Length > 0) && (System.Text.RegularExpressions.Regex.Replace(sym, "[a-z]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Length == 0);
            }
            Assert.IsTrue(v);
        }
    }
}
