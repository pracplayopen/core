using System;
using System.Collections.Generic;
using System.Text;
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
    public class TestBookTracker
    {
        public TestBookTracker()
        {
        }

        [Test]
        public void BasicTrack()
        {
        }
    }
}
