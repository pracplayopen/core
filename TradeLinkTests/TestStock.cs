using System;
using System.Collections.Generic;
using System.Text;
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
    public class TestStock
    {

        public TestStock() { }

        [Test]
        public void Basics()
        {
            SecurityImpl s = new SecurityImpl("");
            Assert.That(s != null);
            Assert.That(!s.isValid);
            s = new SecurityImpl("TST");
            Assert.That(s.isValid);
        }


    }
}
