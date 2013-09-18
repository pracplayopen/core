using System;
using System.Collections.Generic;
using TradeLink.API;
using TradeLink.Common;
using NUnit.Framework;

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
    public class TestMessageTracker
    {
        MessageTracker _mt = new MessageTracker();
        BarListTracker _blt = new BarListTracker(BarListImpl.ALLINTERVALS); // default intervals

        public TestMessageTracker()
        {
            _mt.BLT = _blt;
            _mt.SendDebug+=new DebugDelegate(g.d);
            _mt.VerboseDebugging = true;
        }

        [Test]
        public void TestBarMix()
        {
            // get some test symbols and intervals
            var syms = new string[] { "IBM", "GOOG", "QQQ" };
            var ints = new int[] { (int)BarInterval.Day, (int)BarInterval.FiveMin, (int)BarInterval.Hour };
            var barsback = new int[] { 100, 100, 20 };
            // make some random bars
            var bars = new List<Bar>();
            for (int i = 0; i < ints.Length; i++)
                bars.AddRange(TestTradeLink_WM.GetRandomBars(syms, ints[i], barsback[i]));
            // populate the tracker
            foreach (var bar in bars)
            {
                var msg = BarImpl.Serialize(bar);
#if DEBUG
                g.d(bar.Symbol+" sending: "+msg);
#endif
                _mt.GotMessage(MessageTypes.BARRESPONSE, 0, 0, 0, string.Empty, ref msg);
            }
            // verify everything was populated
            var blt = _mt.BLT;
            foreach (var sym in syms)
            {
                for (int i = 0; i < ints.Length; i++)
                {
                    var bint = ints[i];
                    var bb = barsback[i];
                    var bl = blt[sym, bint];
                    Assert.GreaterOrEqual(bl.IntervalCount((BarInterval)bint, bint),bb, sym + " missing bars on interval: " + bint);
                }
            }
            

        }

    }
}
