using System;
using System.Collections.Generic;
using System.Text;
using TradeLink.Common;
using NUnit.Framework;
using TradeLink.API;

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
    public class TestSecurity 
    {
        public TestSecurity() { }

        [Test]
        public void OptionOSIParse()
        {
            string osi = "YHOO100416C00020000";
            Security sec = new SecurityImpl();
            Assert
                .IsTrue(SecurityImpl.ParseOptionOSI(osi, ref sec,rt.d), "parsing osi had an error");
            Assert.AreEqual("YHOO", sec.symbol,"symbol incorrect");
            Assert.AreEqual(20100416, sec.Date,"date incorrect");
            Assert.AreEqual(20, sec.Strike,"strike incorrect");
            Assert.IsTrue(sec.isCall, "not a call");
        }

        [Test]
        public void OptionSec2OSI()
        {
            string osi1 = "YHOO100416C00020000";
            string osi2 = "AAPL 111022P420000";
            Security sec = new SecurityImpl();
            Assert
                .IsTrue(SecurityImpl.ParseOptionOSI(osi1, ref sec,rt.d), "parsing osi1 had an error");
            Assert.AreEqual("YHOO 100416C20000", SecurityImpl.ToOSISymbol(sec), "converting to osi1 failed");

            Assert
    .IsTrue(SecurityImpl.ParseOptionOSI(osi2, ref sec, rt.d), "parsing osi2 had an error");
            Assert.AreEqual("AAPL 111022P420000", SecurityImpl.ToOSISymbol(sec), "converting to osi2 failed");

        }

        [Test]
        public void Parsing()
        {
            // tests to parse and generate user-supplied security specifiers
            SecurityImpl nyse = new SecurityImpl("LVS");
            string p = nyse.ToString();

            SecurityImpl t = SecurityImpl.Parse(p);
            Assert.That(t.symbol == nyse.symbol, t.symbol);
            Assert.That(!t.hasDest, t.DestEx);
            Assert.That(t.Type == nyse.Type, t.Type.ToString());

            SecurityImpl crude = SecurityImpl.Parse("CLV8 FUT GLOBEX");
            Assert.That(crude.symbol == "CLV8", crude.symbol);
            Assert.That(crude.hasDest, crude.DestEx);
            Assert.That(crude.Type == SecurityType.FUT, crude.Type.ToString());
            SecurityImpl goog = SecurityImpl.Parse("GOOG");
            Assert.AreEqual("GOOG", goog.FullName);

            Security opt = SecurityImpl.Parse("IBM PUT 201004 100.00");
            Assert.That(opt.Type == SecurityType.OPT);
            Assert.AreEqual(100, opt.Strike);
            Assert.AreEqual("PUT", opt.Details);
            Assert.AreEqual(201004, opt.Date);


            
        }
    }
}
