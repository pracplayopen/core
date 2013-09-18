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
    public class TestWMUtil
    {
        public TestWMUtil() { }

        [Test]
        public void Packing()
        {
            decimal normal = 37.56m;
            long packed = WMUtil.pack(normal);
            decimal unpacked = WMUtil.unpack(packed);
            Assert.That(normal == unpacked, normal.ToString()+" -> "+unpacked.ToString());
        }
    }
}
