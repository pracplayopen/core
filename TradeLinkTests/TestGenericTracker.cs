using System;
using NUnit.Framework;
using TradeLink.Common;
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
    public class TestGenericTracker
    {

        [Test]
        public void TrackedTypes()
        {
            // create generic tracker for strings
            var gt = new TradeLink.Common.GenericTracker<string>();
            // verify tracked type
            Assert.IsTrue(gt.TrackedType==typeof(string),"wrong type");
            // test other common types
            var gt1 = new TradeLink.Common.GenericTracker<bool>();
            // verify tracked type
            Assert.IsTrue(gt1.TrackedType == typeof(bool), "wrong type");

            var gt2 = new TradeLink.Common.GenericTracker<decimal>();
            // verify tracked type
            Assert.IsTrue(gt2.TrackedType == typeof(decimal), "wrong type");
            var gt3 = new TradeLink.Common.GenericTracker<int>();
            // verify tracked type
            Assert.IsTrue(gt3.TrackedType == typeof(int), "wrong type");

            var gt4 = new TradeLink.Common.GenericTracker<long>();
            // verify tracked type
            Assert.IsTrue(gt4.TrackedType == typeof(long), "wrong type");

            var gt5 = new TradeLink.Common.GenericTracker<object>();
            // verify tracked type
            Assert.IsTrue(gt5.TrackedType == typeof(object), "wrong type");

            var gt6 = new TradeLink.Common.GenericTracker<Order>();
            // verify tracked type
            Assert.IsTrue(gt6.TrackedType == typeof(Order), "wrong type");



        }

        int newtxt = 0;
        [Test]
        public void Basics()
        {
            // reset count
            newtxt = 0;
            // track something
            GenericTracker<object> gt = new GenericTracker<object>();
            // count new items
            gt.NewTxt += new TextIdxDelegate(gt_NewTxt);
            // get some symbols
            string[] syms = new string[] { "IBM", "LVS", "IBM", "WAG", "GOOG" };
            // add them
            foreach (string sym in syms)
                gt.addindex(sym, sym == "IBM" ? null : new object());
            // ensure we have them
            Assert.AreEqual(4, newtxt);
            Assert.AreNotEqual(gt.Count, syms.Length);
            // test fetching by label
            Assert.IsNull(gt["IBM"]);
            Assert.IsNotNull(gt["GOOG"]);
            Assert.AreEqual(0, gt.getindex("IBM"));
            // get label from index
            Assert.AreEqual("GOOG", gt.getlabel(3));
        }

        void gt_NewTxt(string txt, int idx)
        {
            newtxt++;
        }

        // track something
        GenericTracker<bool> sym = new GenericTracker<bool>();
        GenericTracker<decimal> price1 = new GenericTracker<decimal>();
        GenericTracker<decimal> price2 = new GenericTracker<decimal>();
        GenericTracker<bool> special = new GenericTracker<bool>();

        [Test]
        public void Importing()
        {
            const string FILE = "TestGenericTracker.txt";
            // setup mappings
            sym.NewTxt += new TextIdxDelegate(gt_NewTxt2);
            // import syms
            Assert.IsTrue(GenericTracker.CSVInitGeneric<bool>(FILE, ref sym, false));
            Assert.AreEqual(4, sym.Count);
            Assert.AreEqual(4, price1.Count);
            // import other col data
            Assert.IsTrue(GenericTracker.CSVCOL2Generic<decimal>(FILE, ref price1, 1));
            Assert.IsTrue(GenericTracker.CSVCOL2Generic<decimal>(FILE, ref price2, 3));
            Assert.IsTrue(GenericTracker.CSVCOL2Generic<bool>(FILE, ref special, 4));
            // check data
            Assert.AreEqual(10, price1["IBM"]);
            Assert.AreEqual(20, price2["IBM"]);
            Assert.AreEqual(1, price2["LVS"]);
            Assert.AreEqual(false, special["LVS"]);
            Assert.AreEqual(true, special["MHS"]);
            // verify FRX left alone
            Assert.AreEqual(true, special["FRX"]);
        }

        void gt_NewTxt2(string txt, int idx)
        {
            price1.addindex(txt, 0);
            price2.addindex(txt, 0);
            special.addindex(txt, true);
        }
    }
}
