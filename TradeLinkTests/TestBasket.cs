using System;
using NUnit.Framework;
using TradeLink.Common;
using TradeLink.API;
using System.IO;

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
    public class TestBasket
    {
        public TestBasket()
        {
        }

        [Test]
        public void SymbolTrimming()
        {
            string[] t1 = new string[] { " IBM", "NYMEX FUT NYMEX ", ",WAG", "LVS,", " ,FRX,","" };

            var r = BasketImpl.TrimSymbols(t1);
            int i = 0;
            Assert.AreEqual(5, r.Length,string.Join(".",r));
            Assert.AreEqual("IBM", r[i++]);
            Assert.AreEqual("NYMEX FUT NYMEX", r[i++]);
            Assert.AreEqual("WAG", r[i++]);
            Assert.AreEqual("LVS", r[i++]);
            Assert.AreEqual("FRX", r[i++]);
            //Assert.AreEqual(string.Empty, r[i++]);
        }

        [Test]
        public void BasketBasics()
        {
            BasketImpl mb = new BasketImpl();
            
            Assert.That(mb != null);
            SecurityImpl i = new SecurityImpl("IBM");
            mb = new BasketImpl(i);
            mb.SendDebugEvent += new DebugDelegate(rt.d);
            Assert.That(mb.isNotEmpty);
            Assert.That(mb.isSecurityPresent(i), "missing ibm security");
            Assert.That(mb.isSymbolPresent("IBM"), "missing ibm symbol");
            Assert.IsFalse(mb.isSymbolPresent("LVS"), "had lvs before added");
            mb.Remove(i);
            Assert.That(!mb.isNotEmpty);
            mb.Add(new SecurityImpl("LVS"));
            Assert.That(mb[0].symbol=="LVS",mb[0].ToString());
            mb.Add(new SecurityImpl("IBM"));
            Assert.That(mb[1].symbol=="IBM");
            mb.Add("CLV8 FUT NYMEX");
            Assert.AreEqual(3, mb.Count,"missing futures symbol");
            Assert.IsTrue(mb[1].Type == SecurityType.STK, "not a equities type:" + mb[1].Type);
            Assert.IsTrue(mb[2].Type == SecurityType.FUT, "not a futures type:"+mb[2].Type);
            Security ts;
            Assert.IsTrue(mb.TryGetSecurityAnySymbol("IBM", out ts), "ibm fetch failed");
            Assert.IsTrue(mb.TryGetSecurityAnySymbol("LVS", out ts), "lvs fetch failed");
            Assert.IsTrue(mb.TryGetSecurityAnySymbol("CLV8", out ts), "CLV8 short fetch failed");
            Assert.IsTrue(mb.TryGetSecurityAnySymbol("CLV8 NYMEX FUT", out ts), "CLV8 short fetch failed");
            BasketImpl newbasket = new BasketImpl(new SecurityImpl("FDX"));
            newbasket.Add(mb);
            var orgcount = mb.Count;
            mb.Clear();
            Assert.That(mb.Count==0,"basket clear did not work");
            Assert.AreEqual(orgcount+1,newbasket.Count,"new basket missing symbols");
        }

        [Test]
        public void Multiple()
        {
            // setup some symbols
            string[] ab = new string[] { "IBM", "LVS", "T", "GS", "MHS" };
            string[] bb = new string[] { "LVS", "MHS" };
            // create baskets from our symbols
            Basket mb = new BasketImpl( ab);
            Basket rem = new BasketImpl(bb);
            // verify symbol counts of our baskets
            Assert.That(mb.Count == ab.Length);
            Assert.That(rem.Count == bb.Length);
            // remove one basket from another
            mb.Remove(rem);
            // verify count matches
            Assert.That(mb.Count == 3,mb.Count.ToString());

            // add single symbol
            Basket cb = new BasketImpl("GM");
            // add another symbol
            cb.Add("GOOG");
            // verify we have two
            Assert.AreEqual(2, cb.Count);
            // attempt to add dupplicate
            cb.Add("GM");
            // verify we have two
            Assert.AreEqual(2, cb.Count);


        }

        [Test]
        public void Serialization()
        {
            BasketImpl mb = new BasketImpl();
            mb.Add(new SecurityImpl("IBM"));
            BasketImpl compare = BasketImpl.Deserialize(mb.ToString());
            Assert.That(compare.Count == 1);
            mb.Clear();
            compare = BasketImpl.Deserialize(mb.ToString());
            Assert.That(compare.Count==0);

            mb.Clear();
            SecurityImpl longform = SecurityImpl.Parse("CLZ8 FUT NYMEX");
            mb.Add(longform);
            compare = BasketImpl.Deserialize(mb.ToString());
            Assert.AreEqual(longform.ToString(),compare[0].ToString());



        }

        [Test]
        public void Enumeration()
        {
            BasketImpl mb = new BasketImpl(new string[] { "IBM", "MHS", "LVS", "GM" });
            string[] l = new string[4];
            int i = 0;
            foreach (SecurityImpl s in mb)
                l[i++] = s.symbol;
            Assert.AreEqual(4, i);

        }

        [Test]
        public void Files()
        {
            // create basket
            BasketImpl mb = new BasketImpl(new string[] { "IBM", "MHS", "LVS", "GM" });
            // save it to a file
            const string file = "test.txt";
            BasketImpl.ToFile(mb, file);
            // restore it
            Basket nb = BasketImpl.FromFile(file);
            // verify it has same number of symbols
            Assert.AreEqual(mb.Count, nb.Count);
            // remove original contents from restored copy
            nb.Remove(mb);
            // verify nothing is left
            Assert.AreEqual(0, nb.Count);
        }


        [Test]
        public void FromFile_EnhancedDelim()
        {
            const string origFilePath = "BasketImplTest_orig.txt";
            const string newFilePath = "BasketImplTest_new.txt";
            const string comboFilePath = "BasketImplTest_combo.txt";
            string[] symbols = { "A", "B", "C", "D" };
            BasketImpl baseCase = new BasketImpl(symbols);

            StreamWriter origFile = new StreamWriter(origFilePath); origFile.Write("A\nB\rC\r\nD"); origFile.Close();
            StreamWriter newFile = new StreamWriter(newFilePath); newFile.Write("A,B,C,D"); newFile.Close();
            StreamWriter comboFile = new StreamWriter(comboFilePath); comboFile.Write("A\nB\r\nC,D"); comboFile.Close();

            Assert.AreEqual(baseCase.ToSymArray(), BasketImpl.FromFile(origFilePath).ToSymArray(), "At original test");
            Assert.AreEqual(baseCase.ToSymArray(), BasketImpl.FromFile(newFilePath).ToSymArray(), "At new test");
            Assert.AreEqual(baseCase.ToSymArray(), BasketImpl.FromFile(comboFilePath).ToSymArray(), "At combo test");

        }
    }
}
