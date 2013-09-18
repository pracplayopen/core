using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TradeLink.Common;
using System.Windows.Forms;
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
    public class TestTradeLink_WM
    {
        // each side of our "link"
        TLClient_WM c;
        TLClient_WM c2;
        TLServer_WM s;

        // counters used to test link events are working
        int ticks;
        int fills;
        int orders;
        int fillrequest;
        int imbalances;

        // 2nd client counters
        int copyticks;
        int copyfills;
        int copyorders;

        const string SYM = "TST";

        public TestTradeLink_WM() 
        {
            s = new TLServer_WM();
            s.newProviderName = Providers.TradeLink;
            s.newUnknownRequest += new UnknownMessageDelegate(s_newUnknownRequest);

            // make sure we select our own loopback, if other servers are running
            c = new TLClient_WM(false);
            int pi = -1;
            for (int i = 0; i < c.ProvidersAvailable.Length; i++)
            {
                long v = c.TLSend(MessageTypes.LOOPBACKSERVER, string.Empty, i);
                if (v==SPECIAL)
                    pi = i;
            }
            if (pi == -1) throw new Exception("unable to find test server");
            c.Mode(pi, false);
            
            // create a second client to verify order and fill copying work
            c2 = new TLClient_WM(false);
            c2.Mode(pi, false);

            // register server events (so server can process orders)
            s.newSendOrderRequest += new OrderDelegateStatus(tl_gotSrvFillRequest);

            // setup client events
            c.gotFill += new FillDelegate(tlclient_gotFill);
            c.gotOrder += new OrderDelegate(tlclient_gotOrder);
            c.gotTick += new TickDelegate(tlclient_gotTick);
            c.gotImbalance += new ImbalanceDelegate(c_gotImbalance);
            c.gotUnknownMessage+=new MessageDelegate(c_gotUnknownMessage);
            // setup second client events to check copying
            c2.gotFill += new FillDelegate(c2_gotFill);
            c2.gotOrder += new OrderDelegate(c2_gotOrder);
            c2.gotTick += new TickDelegate(c2_gotTick);

        }
        int barcount = 0;
        Bar bar = new BarImpl();
        void c_gotUnknownMessage(MessageTypes type, long source, long dest, long msgid, string request, ref string response)
        {
            if (type == MessageTypes.BARRESPONSE)
            {
                bar = BarImpl.Deserialize(response);
                if (bar.isValid)
                {
#if DEBUG
                    g.d("got bar: " + response);
#endif
                    barcount++;
                }
            }
        }

        
        const long SPECIAL = -717171;
        

        long s_newUnknownRequest(MessageTypes t, string msg)
        {
            if (t == MessageTypes.BARREQUEST)
            {
                var br = BarRequest.Deserialize(msg);
                var MAXBARPERREQUEST = br.BarsBack;
                if (br.isValid)
                {

                    for (int i = 0; i < MAXBARPERREQUEST; i++)
                    {
                        var bar = GetRandomBar(br.symbol, br.Interval);
                        s.TLSend(BarImpl.Serialize(bar), MessageTypes.BARRESPONSE, c.Name);
                    }
                }
            }
            if (t == MessageTypes.LOOPBACKSERVER)
            {
                return SPECIAL;
            }
            return 0;
        }

        static Random r = new Random();
        public static List<Bar> GetRandomBars(string[] syms, int interval, int barcount)
        {
            List<Bar> bars = new List<Bar>();
            bool isday = interval == (int)BarInterval.Day;
            var st = isday? 0 : 93000;
            var start = Util.ToDateTime(20070926, st);
            foreach (var sym in syms)
            {
                var dt = start;
                for (int i = 0; i < barcount; i++)
                {
                    var tld = Util.ToTLDate(dt);
                    var tlt = Util.ToTLTime(dt);
                    var randbar = GetRandomBar(sym, interval,interval, tld, tlt);
                    if (randbar.isValid)
                        bars.Add(randbar);
#if DEBUG
                    else
                        Console.WriteLine();
#endif
                    if (isday)
                        dt = dt.AddDays(1);
                    else
                        dt = dt.AddSeconds(interval);
                }
            }
            return bars;
        }
        public static Bar GetRandomBar() { return GetRandomBar(TradeLink.Research.RandomSymbol.GetSymbol(), 300,300, 20070926, 093000); }
        public static Bar GetRandomBar(string sym) { return GetRandomBar(sym, 300, 300,20070926, 093000); }
        public static Bar GetRandomBar(string sym, int interval) { return GetRandomBar(sym, interval, interval, 20070926, 093000); }
        public static Bar GetRandomBar(string sym, int intervaltype, int custinterval, int date, int time)
        {
            // send some bars back
            var b = new BarImpl();
            while (!b.isValid)
            {
                var p = (decimal)r.NextDouble() * 100;
                var d = (decimal)Math.Round(r.NextDouble(), 2);
                var v = r.Next(1000, 100000);
                var dt = Util.ToDateTime(date, time);
                b = new BarImpl(p, p + d * 3, p - d * 5, p - d, v, Util.ToTLDate(dt), Util.ToTLTime(dt), sym, intervaltype,custinterval);
            }
            return b;
        }

        [Test]
        public void BarTest()
        {
            // verify no bars
            Assert.AreEqual(0, barcount, "bars received early");
            // request bars
            c.TLSend(MessageTypes.BARREQUEST, BarRequest.BuildBarRequest(SYM, BarInterval.FiveMin));
            // verify we received it
            Assert.Greater(barcount,30, "no bar received");
        }



        [Test]
        public void Imbalance()
        {
            // make sure we have none sent
            Assert.AreEqual(0, imbalances);

            // send one
            s.newImbalance(new ImbalanceImpl(SYM, "NYSE", 100000, 154000, 0, 0,0));

            // make sure we got it
            Assert.AreEqual(1, imbalances);

            
        }



        [Test]
        public void StartupTests()
        {
            // discover our states
            Providers[] p = c.TLFound();
            Assert.Greater(p.Length, 0);
            Assert.AreEqual(Providers.TradeLink, p[c.ProviderSelected]);
        }

        [Test]
        public void TickTests()
        {
            // havent' sent any ticks, so shouldn't have any counted
            Assert.That(ticks == 0, ticks.ToString());

            // have to subscribe to a stock to get notified on fills for said stock
            c.Subscribe(new BasketImpl(new SecurityImpl(SYM)));

            //send a tick from the server
            TickImpl t = TickImpl.NewTrade(SYM, 10, 100);
            s.newTick(t);

            // make sure the client got it
            Assert.That(ticks == 1, ticks.ToString());
            // make sure other clients did not get ticks 
            // (cause we didnt' subscribe from other clients)
            Assert.AreNotEqual(copyticks, ticks);
        }

        [Test]
        public void OrderTests()
        {
            // no orders yet
            Assert.That(orders == 0, orders.ToString());
            // no fill requests yet
            Assert.That(fillrequest == 0, fillrequest.ToString());

            // client wants to buy 100 TST at market
            OrderImpl o = new OrderImpl(SYM, 100);
            // if it works it'll return zero
            int error = c.SendOrderStatus(o);
            Assert.That(error==0,error.ToString());
            // client should have received notification that an order entered his account
            Assert.That(orders == 1, orders.ToString());
            // server should have gotten a request to fill an order
            Assert.That(fillrequest == 1, fillrequest.ToString());
            // make sure order was copied to other clients
            Assert.AreEqual(copyorders, orders);
        }

        [Test]
        public void FillTests()
        {
            // no executions yet
            Assert.That(fills == 0, fills.ToString());

            // have to subscribe to a stock to get notified on fills for said stock
            c.Subscribe(new BasketImpl(new SecurityImpl(SYM)));

            // prepare and send an execution from client to server
            TradeImpl t = new TradeImpl(SYM, 100, 300, DateTime.Now);
            s.newFill(t);

            // make sure client received and counted it
            Assert.That(fills == 1, fills.ToString());

            // make sure fill was copied
            Assert.AreEqual(fills, copyfills);
        }

        [Test]
        public void ImbalancePerformance()
        {
            const int OPS = 1000;
            // reset count
            imbalances = 0;
            // get imbalances
            Imbalance[] imbs = TestImbalance.SampleImbalanceData(OPS);
            // start clock
            DateTime start = DateTime.Now;
            // send imbalances
            for (int i = 0; i < OPS; i++)
                s.newImbalance(imbs[i]);
            // stop clock
            double time = DateTime.Now.Subtract(start).TotalSeconds;
            // verify time
            Assert.LessOrEqual(time, .05);
            // verify imbalance count
            Assert.AreEqual(OPS,imbalances);
            Console.WriteLine(string.Format("Protocol performance (imbalance/sec): {0:n2}s {1:n0}", time, OPS / time));


        }
        
        [Test]
        public void TickPerformance()
        {
            // expected performance
            const decimal EXPECT = .10m;
            // get ticks for test
            const int TICKSENT = 1000;
            Tick[] tick = TradeLink.Research.RandomTicks.GenerateSymbol(SYM, TICKSENT);
            // subscribe to symbol
            c.Unsubscribe();
            c.Subscribe(new BasketImpl(SYM));
            // reset ticks
            int save = ticks;
            ticks = 0;
            // start clock
            DateTime start = DateTime.Now;

            // process ticks
            for (int i = 0; i < tick.Length; i++)
                s.newTick(tick[i]);

            // stop clock
            double time = DateTime.Now.Subtract(start).TotalSeconds;
            // make sure time exists
            Assert.Greater(time, 0);
            // make sure it's less than expected
            Assert.LessOrEqual(time, EXPECT);
            // make sure we got all the ticks
            Assert.AreEqual(TICKSENT, ticks);
            decimal ticksec = TICKSENT/(decimal)time;
            Console.WriteLine("protocol performance (tick/sec): " + ticksec.ToString("N0"));

            // restore ticks
            ticks = save;
        }

        // event handlers

        long tl_gotSrvFillRequest(Order o)
        {
            s.newOrder(o);
            fillrequest++;
            return 0;
        }

        void tlclient_gotTick(Tick t)
        {
            ticks++;

        }

        void tlclient_gotOrder(Order o)
        {
            orders++;
        }

        void tlclient_gotFill(Trade t)
        {
            fills++;
        }

        // 2nd client handlers
        void c2_gotTick(Tick t)
        {
            copyticks++;
        }

        void c2_gotOrder(Order o)
        {
            copyorders++; 
        }

        void c2_gotFill(Trade t)
        {
            copyfills++;
        }

        void c_gotImbalance(Imbalance imb)
        {
            imbalances++;
        }
    }
}
