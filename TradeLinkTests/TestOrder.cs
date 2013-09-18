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
    public class TestOrder : AssertionHelper
    {
        public TestOrder() { }

        [Test]
        public void Defaults()
        {
            // assert that a default order is:
            // not valid, not filled
            OrderImpl o = new OrderImpl();
            Assert.That(!o.isValid);
            Assert.That(!o.isFilled);
        }

        [Test]
        public void FillBidAskPartial()
        {

            PapertradeTracker ptt = new PapertradeTracker();
            PositionTracker pt = new PositionTracker();


            ptt.UseBidAskFills = true;
            ptt.GotFillEvent += new FillDelegate(pt.GotFill);

            foreach (bool side in new bool[] { true, false })
            {
                pt.Clear();
                Order o = new MarketOrder("IBM", side, 1000);
                int size = o.size;
                o.id = 1;
                ptt.sendorder(o);
                Tick k = TickImpl.NewQuote("IBM", 100, 101, 400, 400, "", "");
                ptt.newTick(k); // partial fill
                ptt.newTick(k); // partial fill
                ptt.newTick(k); // partial fill, completed
                Expect(pt["IBM"].Size, Is.EqualTo(size));
            }
        }

        [Test]
        public void MarketOrder()
        {
            const string s = "SYM";
            OrderImpl o = new OrderImpl(s,100);
            Assert.That(o.isValid);
            Assert.That(o.isMarket);
            Assert.That(!o.isLimit);
            Assert.That(!o.isStop);
            Assert.That(!o.isFilled);
            Assert.That(o.symbol == s);
        }

        [Test]
        public void Fill()
        {
            const string s = "TST";
            // market should fill on trade but not on quote
            OrderImpl o = new BuyMarket(s, 100);
            Assert.That(o.Fill(TickImpl.NewTrade(s, 9, 100)));
            Assert.That(!o.Fill(TickImpl.NewBid(s, 8, 100)));

            // buy limit

            // limit should fill if order price is inside market
            o = new BuyLimit(s, 100, 10m);
            Assert.That(o.Fill(TickImpl.NewTrade(s, 9, 100)));
            // shouldn't fill outside market
            o = new BuyLimit(s, 100, 10m);
            Assert.That(!o.Fill(TickImpl.NewTrade(s, 11, 100)));

            // sell limit

            // limit should fill if order price is inside market
            o = new SellLimit(s, 100, 10m);
            Assert.That(o.Fill(TickImpl.NewTrade(s, 11, 100)));
            // shouldn't fill outside market
            o = new SellLimit(s, 100, 10m);
            Assert.That(!o.Fill(TickImpl.NewTrade(s, 9, 100)));

            // buy stop

            o = new BuyStop(s, 100, 10m);
            Assert.That(o.Fill(TickImpl.NewTrade(s, 11, 100)));
            // shouldn't fill outside market
            o = new BuyStop(s, 100, 10m);
            Assert.That(!o.Fill(TickImpl.NewTrade(s, 9, 100)));

            // sell stop

            o = new SellStop(s, 100, 10m);
            Assert.That(o.Fill(TickImpl.NewTrade(s, 9, 100)));
            // shouldn't fill outside market
            o = new SellStop(s, 100, 10m);
            Assert.That(!o.Fill(TickImpl.NewTrade(s, 11, 100)));

            // always fail filling an invalid tick
            o = new BuyMarket(s, 100);
            Assert.IsFalse(o.Fill(TickImpl.NewTrade(s, 0, 0)));

            // always fail filling invalid order
            o = new BuyLimit(s, 100, 10);
            OrderImpl x = new OrderImpl();
            Assert.IsFalse(o.Fill(x));

            // always fail filling an order that doesn't cross market
            x = new BuyMarket(s, 100);
            Assert.IsFalse(o.Fill(x));

            const string t2 = "trader2";
            // suceed on crossing market
            x = new SellMarket(s,100);
            x.Account = t2;
            Assert.IsTrue(o.Fill(x));

            // fail when accounts are the same
            x = new SellMarket(s, 100);
            x.Account = o.Account;
            Assert.IsFalse(o.Fill(x));


            // fail on match outside of market
            x = new SellLimit(s, 100, 11);
            x.Account = t2;
            Assert.IsFalse(o.Fill(x));

            // succeed on limit cross
            o = new BuyLimit(s, 100, 10);
            x = new SellLimit(s, 100, 10);
            x.Account = t2;
            Assert.IsTrue(o.Fill(x));

            // make sure we can stop cross
            o = new SellStop(s, 100, 10);
            x = new BuyMarket(s, 100);
            x.Account = t2;
            Assert.IsTrue(o.Fill(x));

        }

        [Test]
        public void FillBidAsk()
        {
            const string s = "TST";
            // market should fill on trade but not on quote
            OrderImpl o = new BuyMarket(s, 100);
            Assert.That(o.FillBidAsk(TickImpl.NewAsk(s, 9, 100)));
            Assert.That(!o.FillBidAsk(TickImpl.NewTrade(s, 9, 100)));
            Assert.That(!o.FillBidAsk(TickImpl.NewBid(s, 8, 100)));

            // buy limit

            // limit should fill if order price is inside market
            o = new BuyLimit(s, 100, 10m);
            Assert.That(o.FillBidAsk(TickImpl.NewAsk(s, 9, 100)));
            // shouldn't fill outside market
            o = new BuyLimit(s, 100, 10m);
            Assert.That(!o.FillBidAsk(TickImpl.NewTrade(s, 11, 100)));
            Assert.That(!o.FillBidAsk(TickImpl.NewAsk(s, 11, 100)));
            Assert.That(!o.FillBidAsk(TickImpl.NewBid(s, 10, 100)));

            // sell limit

            // limit should fill if order price is inside market
            o = new SellLimit(s, 100, 10m);
            Assert.That(o.FillBidAsk(TickImpl.NewBid(s, 11, 100)));
            // shouldn't fill outside market
            o = new SellLimit(s, 100, 10m);
            Assert.That(!o.FillBidAsk(TickImpl.NewTrade(s, 9, 100)));

            // buy stop

            o = new BuyStop(s, 100, 10m);
            Assert.That(o.FillBidAsk(TickImpl.NewAsk(s, 11, 100)));
            // shouldn't fill outside market
            o = new BuyStop(s, 100, 10m);
            Assert.That(!o.FillBidAsk(TickImpl.NewTrade(s, 9, 100)));

            // sell stop

            o = new SellStop(s, 100, 10m);
            Assert.That(o.FillBidAsk(TickImpl.NewBid(s, 9, 100)));
            // shouldn't fill outside market
            o = new SellStop(s, 100, 10m);
            Assert.That(!o.FillBidAsk(TickImpl.NewTrade(s, 11, 100)));

            // always fail filling an invalid tick
            o = new BuyMarket(s, 100);
            Assert.IsFalse(o.FillBidAsk(TickImpl.NewTrade(s, 0, 0)));
        }


        [Test]
        public void SerializationAndDeserialization()
        {
            // create an order
            const string s = "TST";
            const string x = "NYSE";
            const string a = "ACCOUNT";
            const string u = "COMMENT";
            
            const decimal p = 10;
            const int z = 100;
            const CurrencyType c = CurrencyType.USD;
            const SecurityType t = SecurityType.STK;
            Order o = new OrderImpl(s, z);
            o.date = 20080718;
            o.time = 94800;
            o.price = p;
            o.Account = a;
            o.ex = x;
            o.Currency = c;
            o.Security = t;
            o.comment = u;
            var inst = OrderInstructionType.GTC;
            o.ValidInstruct = inst;
            //o.TIF = ot;
            // convert it to a message
            string msg = OrderImpl.Serialize(o);

            // convert it back to an object and validate nothing was lost
            string exception=null;
            Order n = new OrderImpl();
            try
            {
                n = OrderImpl.Deserialize(msg);
            }
            catch (Exception ex) { exception = ex.ToString(); }
            Assert.That(exception==null, msg+" "+exception);
            Assert.That(n.Account == a,n.Account);
            Assert.That(n.symbol == s,n.symbol);
            Assert.That(n.size == z,n.size.ToString());
            Assert.That(n.price == p,n.price.ToString());
            Assert.That(n.Exchange == x,n.Exchange);
            
            Assert.That(n.Security == t,n.Security.ToString());
            Assert.That(n.Currency == c,n.Currency.ToString());
            Assert.That(n.ValidInstruct== inst, n.ValidInstruct.ToString(),"unexpected instruction: "+n.ValidInstruct.ToString());
            Assert.That(n.date == o.date, n.date.ToString());
            Assert.That(n.time == o.time, n.time.ToString());
        }

        [Test]
        public void IdentityStops()
        {
            string sym = "SPY";
            bool side = false;
            int size = -256;
            decimal stop = 134.40m;
            string comment = "Hello, World!";   // not checked for
            long id = 8675309;                  // not checked for

            Order orig = new StopOrder(sym, side, size, stop);
            Order comp;
            
            comp = new StopOrder(sym, size, stop, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SignedStop");
            Assert.AreEqual(orig.side, comp.side, "Side, SignedStop");
            Assert.AreEqual(orig.size, comp.size, "Size, SignedStop");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, SignedStop");
            Assert.AreEqual(orig.price, comp.price, "Price, SignedStop");

            comp = new StopOrder(sym, side, size, stop, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, StopID");
            Assert.AreEqual(orig.side, comp.side, "Side, StopID");
            Assert.AreEqual(orig.size, comp.size, "Size, StopID");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, StopID");
            Assert.AreEqual(orig.price, comp.price, "Price, StopID");

            comp = new StopOrder(sym, side, size, stop, comment);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, StopComment");
            Assert.AreEqual(orig.side, comp.side, "Side, StopComment");
            Assert.AreEqual(orig.size, comp.size, "Size, StopComment");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, StopComment");
            Assert.AreEqual(orig.price, comp.price, "Price, StopComment");

            comp = new SellStop(sym, size, stop);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SellStop");
            Assert.AreEqual(orig.side, comp.side, "Side, SellStop");
            Assert.AreEqual(orig.size, comp.size, "Size, SellStop");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, SellStop");
            Assert.AreEqual(orig.price, comp.price, "Price, SellStop");

            comp = new SellStop(sym, size, stop, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SellStopID");
            Assert.AreEqual(orig.side, comp.side, "Side, SellStopID");
            Assert.AreEqual(orig.size, comp.size, "Size, SellStopID");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, SellStopID");
            Assert.AreEqual(orig.price, comp.price, "Price, SellStopID");

            comp = new SellStop(sym, size, stop, comment);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SellStopComment");
            Assert.AreEqual(orig.side, comp.side, "Side, SellStopComment");
            Assert.AreEqual(orig.size, comp.size, "Size, SellStopComment");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, SellStopComment");
            Assert.AreEqual(orig.price, comp.price, "Price, SellStopComment");

            side = true;
            orig = new StopOrder(sym, side, size, stop);

            comp = new BuyStop(sym, size, stop);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, BuyStop");
            Assert.AreEqual(orig.side, comp.side, "Side, BuyStop");
            Assert.AreEqual(orig.size, comp.size, "Size, BuyStop");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, BuyStop");
            Assert.AreEqual(orig.price, comp.price, "Price, BuyStop");

            comp = new BuyStop(sym, size, stop, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, BuyStopID");
            Assert.AreEqual(orig.side, comp.side, "Side, BuyStopID");
            Assert.AreEqual(orig.size, comp.size, "Size, BuyStopID");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, BuyStopID");
            Assert.AreEqual(orig.price, comp.price, "Price, BuyStopID");

            comp = new BuyStop(sym, size, stop, comment);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, BuyStopComment");
            Assert.AreEqual(orig.side, comp.side, "Side, BuyStopComment");
            Assert.AreEqual(orig.size, comp.size, "Size, BuyStopComment");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop, BuyStopComment");
            Assert.AreEqual(orig.price, comp.price, "Price, BuyStopComment");
        }

        [Test]
        public void IdentityLimits()
        {
            string sym = "SPY";
            bool side = false;
            int size = -256;
            decimal Limit = 134.40m;
            string comment = "Hello, World!";   // not checked for
            long id = 8675309;                  // not checked for

            Order orig = new LimitOrder(sym, side, size, Limit);
            Order comp;

            comp = new LimitOrder(sym, size, Limit, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SignedLimit");
            Assert.AreEqual(orig.side, comp.side, "Side, SignedLimit");
            Assert.AreEqual(orig.size, comp.size, "Size, SignedLimit");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop SignedLimit");
            Assert.AreEqual(orig.price, comp.price, "Price, SignedLimit");

            comp = new LimitOrder(sym, side, size, Limit, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, LimitID");
            Assert.AreEqual(orig.side, comp.side, "Side, LimitID");
            Assert.AreEqual(orig.size, comp.size, "Size, LimitID");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop LimitID");
            Assert.AreEqual(orig.price, comp.price, "Price, LimitID");

            comp = new LimitOrder(sym, side, size, Limit, comment);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, LimitComment");
            Assert.AreEqual(orig.side, comp.side, "Side, LimitComment");
            Assert.AreEqual(orig.size, comp.size, "Size, LimitComment");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop LimitComment");
            Assert.AreEqual(orig.price, comp.price, "Price, LimitComment");

            comp = new SellLimit(sym, size, Limit);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SellLimit");
            Assert.AreEqual(orig.side, comp.side, "Side, SellLimit");
            Assert.AreEqual(orig.size, comp.size, "Size, SellLimit");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop SellLimit");
            Assert.AreEqual(orig.price, comp.price, "Price, SellLimit");

            comp = new SellLimit(sym, size, Limit, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SellLimitID");
            Assert.AreEqual(orig.side, comp.side, "Side, SellLimitID");
            Assert.AreEqual(orig.size, comp.size, "Size, SellLimitID");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop SellLimitID");
            Assert.AreEqual(orig.price, comp.price, "Price, SellLimitID");

            comp = new SellLimit(sym, size, Limit, comment);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, SellLimitComment");
            Assert.AreEqual(orig.side, comp.side, "Side, SellLimitComment");
            Assert.AreEqual(orig.size, comp.size, "Size, SellLimitComment");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop SellLimitComment");
            Assert.AreEqual(orig.price, comp.price, "Price, SellLimitComment");

            side = true;
            orig = new LimitOrder(sym, side, size, Limit);

            comp = new BuyLimit(sym, size, Limit);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, BuyLimit");
            Assert.AreEqual(orig.side, comp.side, "Side, BuyLimit");
            Assert.AreEqual(orig.size, comp.size, "Size, BuyLimit");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop BuyLimit");
            Assert.AreEqual(orig.price, comp.price, "Price, BuyLimit");

            comp = new BuyLimit(sym, size, Limit, id);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, BuyLimitID");
            Assert.AreEqual(orig.side, comp.side, "Side, BuyLimitID");
            Assert.AreEqual(orig.size, comp.size, "Size, BuyLimitID");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop BuyLimitID");
            Assert.AreEqual(orig.price, comp.price, "Price, BuyLimitID");

            comp = new BuyLimit(sym, size, Limit, comment);
            Assert.AreEqual(orig.symbol, comp.symbol, "Symbol, BuyLimitComment");
            Assert.AreEqual(orig.side, comp.side, "Side, BuyLimitComment");
            Assert.AreEqual(orig.size, comp.size, "Size, BuyLimitComment");
            Assert.AreEqual(orig.stopp, comp.stopp, "Stop BuyLimitComment");
            Assert.AreEqual(orig.price, comp.price, "Price, BuyLimitComment");
        }
    }
}
