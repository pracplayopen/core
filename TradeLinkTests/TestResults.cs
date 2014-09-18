using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TradeLink.Common;
using TradeLink.API;
using TradeLink.Research;
using TradeLink.AppKit;

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
    public class TestResults
    {

        Result rt = new Results();
        const string sym = "TST";
        const decimal p = 100;
        const int s = 100;
        const decimal inc = .1m;

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void KeyRatios()
        {
            // for calculation check see : https://docs.google.com/spreadsheets/d/1vYEfSleUjF4It_sB-obzRsosTXHEWrdFbhbHDNlNOCM/edit?usp=sharing
            rt = new Results();

            // get some trades
            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s), // 100 @ $100
                // increase bet
                new TradeImpl(sym,p+inc,s*2),// 300 @ $100.066666
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1), // 200 @ 100.0666 (profit = 100 * (100.20 - 100.0666) = 13.34) / maxMIU(= 300*100.06666) = .04% ret
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2), // 0 @ 0
                // go short
                new TradeImpl(sym,p,s*-2), // -200 @ 100
                // decrease bet
                new TradeImpl(sym,p,s), // -100 @100
                // exit (round turn)
                new TradeImpl(sym,p+inc,s), // 0 @ 0 (gross profit = -0.10*100 = -$10)
                // do another entry
                new TradeImpl(sym,p,s)
            });

            // compute results
            rt = Results.ResultsFromTradeList(fills, g.d);
            // check ratios
#if DEBUG
            g.d(rt.ToString());
#endif
            Assert.AreEqual(-16.413m,rt.SharpeRatio, "bad sharpe ratio");
            Assert.AreEqual(-26.909m, rt.SortinoRatio, "bad sortino ratio");
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void RoundTurnStat()
        {
            rt = new Results();

            // get some trades
            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s),
                // increase bet
                new TradeImpl(sym,p+inc,s*2),
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1),
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2),
                // go short
                new TradeImpl(sym,p,s*-2),
                // decrease bet
                new TradeImpl(sym,p,s),
                // exit (round turn)
                new TradeImpl(sym,p+inc,s),
                // do another entry
                new TradeImpl(sym,p,s)
            });

            // compute results
            rt = Results.ResultsFromTradeList(fills, g.d);
            // check trade count
            Assert.AreEqual(fills.Count, rt.Trades, "trade is missing from results");
            // check round turn count
            Assert.AreEqual(2, rt.RoundTurns, "missing round turns");

            // verify trade winners
            Assert.AreEqual(2, rt.Winners,"missing trade winner");
            // verify round turn winners
            Assert.AreEqual(1, rt.RoundWinners, "missing round turn winners");
            // verify round turn losers
            Assert.AreEqual(1, rt.RoundLosers, "missing round turn loser");
        }
    }
}
