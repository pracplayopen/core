using System;
using NUnit.Framework;
using TradeLink.Common;

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
    public class TestOffsetInfo
    {
           public TestOffsetInfo()
           {
           }

           [Test]
           public void SerializeDeserialize()
           {
               OffsetInfo oi = new OffsetInfo(2, 1);
               string msg = OffsetInfo.Serialize(oi);
               OffsetInfo co = OffsetInfo.Deserialize(msg);
               Assert.AreEqual(2,oi.ProfitDist);
               Assert.AreEqual(1, oi.StopDist);
               
           }
    }
}
