using System;
using System.Collections.Generic;
using System.Text;
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
    public class TestBarRequest
    {
        const string sym = "TST";

        [Test]
        public void BarsBackSerializeDeSerialize()
        {
            // build request
            var br = new BarRequest(sym, 300, 300, 100, string.Empty);
            // verify it
            Assert.IsTrue(br.isValid, "original not valid");
            Assert.AreEqual(300, br.Interval, "interval missing");
            Assert.AreEqual(sym, br.symbol, "symbol missing");
            Assert.AreEqual(100, br.BarsBack, "bars back missing");
            // serialize request
            var msg = br.Serialize();
            // deserialize
            var copy = BarRequest.Deserialize(msg);
            // verify valid
            Assert.IsTrue(copy.isValid, "copy not valid");
            // verify same
            Assert.AreEqual(br.symbol, copy.symbol, "symbol mismatch");
            Assert.AreEqual(br.BarsBack, copy.BarsBack, "symbol mismatch");
            Assert.AreEqual(br.Interval, copy.Interval, "symbol mismatch");
        }
    }
}
