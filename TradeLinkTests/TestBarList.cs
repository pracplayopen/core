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
    //[TestFixture, Explicit]
    [TestFixture]
#else
    [TestFixture]
#endif
    public class TestBarList
    {
        int newbars = 0;

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void ReadAndOverwrite()
        {
            const string tf = @"SPX20070926.TIK";

            // read a barlist
            var bl = BarListImpl.FromTIK(tf);

            // get new bars
            var newbl = BarListImpl.DayFromGoogle("FTI");

            // append
            for (int i = 0; i < newbl.Count; i++)
                foreach (var k in BarImpl.ToTick(newbl[i]))
                    bl.newTick(k);

            

            // write the barlist
            Assert
                .IsTrue(BarListImpl.ChartSave(bl, Environment.CurrentDirectory, 20070926), "error saving new data");


        }


#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void FiveMin()
        {
            // get some sample data to fill barlist
            Tick[] ticklist = SampleData();
            // prepare barlist
            BarListImpl bl = new BarListImpl(BarInterval.FiveMin);
            bl.GotNewBar+=new SymBarIntervalDelegate(bl_GotNewBar);
            // reset count
            newbars = 0;
            // create bars from all ticks available
            foreach (TickImpl k in ticklist)
            {
                /// add tick to bar
                bl.newTick(k);
            }

            // verify we had expected number of bars
            Assert.AreEqual(3,newbars);
            // verify symbol was set
            Assert.AreEqual(sym, bl.symbol);
            // verify each bar symbol matches barlist
            foreach (Bar b in bl)
                Assert.AreEqual(bl.symbol, b.Symbol);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void PointFiveMin()
        {
            // get some sample data to fill barlist
            Tick[] ticklist = SampleData();
            // prepare barlist
            BarListImpl bl = new BarListImpl(BarInterval.FiveMin);
            bl.GotNewBar += new SymBarIntervalDelegate(bl_GotNewBar);
            // reset count
            newbars = 0;
            // create bars from all ticks available
            foreach (TickImpl k in ticklist)
            {
                /// add tick to bar
                bl.newPoint(k.symbol,k.trade,k.time,k.date,k.size);
            }

            // verify we had expected number of bars
            Assert.AreEqual(3, bl.Count);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void OneMinute()
        {
            // prepare barlist
            BarList bl = new BarListImpl(BarInterval.Minute);
            // reset count
            int newbars = 0;
            // build bars from ticks available
            foreach (TickImpl k in SampleData())
            {
                // add tick to bar
                bl.newTick(k);
                // count if it's a new bar
                if (bl.RecentBar.isNew)
                    newbars++;
            }
            // verify expected # of bars are present
            Assert.AreEqual(9, newbars);
            // verify barcount is same as newbars
            Assert.AreEqual(newbars, bl.Count);

        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void PointMinute()
        {
            // prepare barlist
            BarList bl = new BarListImpl(BarInterval.Minute);
            // reset count
            int newbars = 0;
            // build bars from ticks available
            foreach (TickImpl k in SampleData())
            {
                // add tick to bar
                bl.newPoint(k.symbol,k.trade,k.time,k.date,k.size);
                // count if it's a new bar
                if (bl.RecentBar.isNew)
                    newbars++;
            }
            // verify expected # of bars are present
            Assert.AreEqual(9, newbars);
            // verify barcount is same as newbars
            Assert.AreEqual(newbars, bl.Count);
        }


        const string sym = "TST";

        public static Tick[] SampleData()
        {

            const int d = 20070517;
            const int t = 93500;
            const string x = "NYSE";
            Tick[] tape = new Tick[] { 
                TickImpl.NewTrade(sym,d,t,10,100,x), // new on all intervals
                TickImpl.NewTrade(sym,d,t+100,10,100,x), // new on 1min
                TickImpl.NewTrade(sym,d,t+200,10,100,x),
                TickImpl.NewTrade(sym,d,t+300,10,100,x),
                TickImpl.NewTrade(sym,d,t+400,15,100,x), 
                TickImpl.NewTrade(sym,d,t+500,16,100,x), //new on 5min
                TickImpl.NewTrade(sym,d,t+600,16,100,x),
                TickImpl.NewTrade(sym,d,t+700,10,100,x), 
                TickImpl.NewTrade(sym,d,t+710,10,100,x), 
                TickImpl.NewTrade(sym,d,t+10000,10,100,x), // new on hour interval
            };
            return tape;
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void BarMath()
        {
            // get tickdata
            Tick[] tape = SampleData();
            // create bar
            BarList bl = new BarListImpl(BarInterval.Minute);
            // pass ticks to bar
            foreach (Tick k in tape)
                bl.newTick(k);
            // verify HH
            Assert.AreEqual(16, Calc.HH(bl));
            // verify LL
            Assert.AreEqual(10, Calc.LL(bl));
            // verify average
            Assert.AreEqual(11.888888888888888888888888889m, Calc.Avg(bl.Open()));
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void HourTest()
        {
            // get data
            Tick[] tape = SampleData();
            // count new hour bars
            newbars = 0;
            // setup hour bar barlist
            BarListImpl bl = new BarListImpl(BarInterval.Hour, sym);
            // handle new bar events
            bl.GotNewBar+=new SymBarIntervalDelegate(bl_GotNewBar);
            // add ticks to bar
            foreach (Tick k in tape)
            {
                // add ticks
                bl.newTick(k);
            }
            // make sure we have at least 1 bars
            Assert.IsTrue(bl.Has(1));
            // make sure we actually have two bars
            Assert.AreEqual(2, newbars);
            Assert.AreEqual(bl.Count, newbars);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void PointHour()
        {
            // get data
            Tick[] tape = SampleData();
            // count new hour bars
            newbars = 0;
            // setup hour bar barlist
            BarListImpl bl = new BarListImpl(BarInterval.Hour, sym);
            // handle new bar events
            bl.GotNewBar += new SymBarIntervalDelegate(bl_GotNewBar);
            // add ticks to bar
            foreach (Tick k in tape)
            {
                // add ticks
                bl.newPoint(k.symbol,k.trade,k.time,k.date,k.size);
            }
            // make sure we have at least 1 bars
            Assert.IsTrue(bl.Has(1));
            // make sure we actually have two bars
            Assert.AreEqual(2, bl.Count);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void DefaultIntervalAndReset()
        {
            // get some data
            Tick[] tape = SampleData();
            // setup an hour barlist
            BarList bl = new BarListImpl();
            bl.DefaultInterval = BarInterval.Hour;
            // build the barlist
            foreach (Tick k in tape)
                bl.newTick(k);
            // make sure we have 2 hour bars
            Assert.AreEqual(2, bl.Count);
            // switch default
            bl.DefaultInterval = BarInterval.FiveMin;
            // make sure we have 3 5min bars
            Assert.AreEqual(3, bl.Count);
            // reset it
            bl.Reset();
            // verify we have no data
            Assert.AreEqual(0, bl.Count);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void InsertBar_NoexistingBars()
        {
            string sym = "FTI";
            int d = 20070926;
            // historical bar filename
            string filename = sym+d+TikConst.DOT_EXT;
            
            // test for the parameter's prescence
            Assert.IsNotEmpty(filename,"forgot to assign insert bar filename");

            // unit test case 1 no existing bars aka (empty or brand-new insertion)
            BarList org = new BarListImpl(BarInterval.FiveMin,sym);
            Assert.IsTrue(org.isValid, "your original barlist is not valid 1");
            int orgcount = org.Count;
            Assert.AreEqual(0,orgcount);
            // make up a bar here  (eg at 755 in FTI there are no ticks so this should add a new bar in most scenarios)
            Bar insert = new BarImpl(30,30,30,30,10000,d,75500,sym,(int)BarInterval.FiveMin);
            Assert.IsTrue(insert.isValid,"your bar to insert is not valid 1");
            BarList inserted = BarListImpl.InsertBar(org,insert,org.Count);
            Assert.AreEqual(inserted.Count,orgcount+1);
            Bar actualinsert = inserted.RecentBar;
            Assert.IsTrue(actualinsert.isValid);
            Assert.AreEqual(insert.Close,actualinsert.Close);
            Assert.AreEqual(insert.Open,actualinsert.Open);
            Assert.AreEqual(insert.High,actualinsert.High);
            Assert.AreEqual(insert.Low,actualinsert.Low);
            Assert.AreEqual(insert.Symbol,actualinsert.Symbol);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void InsertBar_MultipleInsert()
        {
            string sym = "FTI";
            int d = 20070926;
            var bint = BarInterval.FiveMin;
            var bsize = (int)bint;
            BarList org = new BarListImpl(bint, sym);
            Assert.IsTrue(org.isValid, "your original barlist is not valid 1");
            int orgcount = org.Count;
            Assert.AreEqual(0, orgcount);

            int h = 7;
            int m = 55;
            for (int i = 0; i < 10; i++)
            {
                int t = h*10000+m*100;
                Bar insert = new BarImpl(30, 30, 30, 30, 10000, d, t, sym, bsize);
                Assert.IsTrue(insert.isValid, "your bar to insert is not valid #" + i);
                int insertpos = BarListImpl.GetBarIndexPreceeding(org, insert.Bardate, insert.Bartime);
                Assert.AreEqual(i - 1, insertpos, "insertion position#" + i);
                BarList inserted = BarListImpl.InsertBar(org, insert, insertpos);
                Assert.IsTrue(g.ta(i + 1 == inserted.Count, BarListImpl.Bars2String(org)+Environment.NewLine+ BarListImpl.Bars2String(inserted)), "element count after insertion #" + i + " pos: " + insertpos);
                m += 5;
                if (m >= 60)
                {
                    h += m / 60;
                    m = m % 60;
                }
                org = inserted;
            }
            Assert.AreEqual(orgcount+10, org.Count, "total element count after insertion");
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void InsertBar_OutOfOrderTicks()
        {
            string sym = "TPX";
            int d = 20120724;
            // historical bar filename
            string filename = sym + d + TikConst.DOT_EXT;
            //string filename = @"TPX20120724.TIK";

            // unit test case 2 existing bars with front insertion (aka historical bar insert)
            int[] intervals = { (int)BarInterval.Minute };
            BarInterval[] types = { BarInterval.Minute };

            BarList org = BarListImpl.FromTIK(filename,true,false, intervals, types);
            
            Assert.IsTrue(org.isValid, "your original bar is not valid 2");
            var orgcount = org.Count;
            Assert.AreEqual(2, orgcount, "Number of bars constructed doesn't match");
        }





#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void NewBarEvent()
        {
            // get tickdata
            Tick[] tape = SampleData();
            // reset bar count
            newbars = 0;
            // request hour interval
            BarList bl = new BarListImpl(BarInterval.Hour, sym);
            // handle new bars
            bl.GotNewBar += new SymBarIntervalDelegate(bl_GotNewBar);


            foreach (TickImpl k in tape)
                bl.newTick(k);

            Assert.AreEqual(2, newbars);
        }

        void bl_GotNewBar(string symbol, int interval)
        {
            newbars++;
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void FromEPF()
        {
            // get sample tick data
            BarList bl = BarListImpl.FromEPF("FTI20070926.EPF");
            // verify expected number of 5min bars exist (78 in 9:30-4p)
            Assert.AreEqual(83, bl.Count);

        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void FromTIK()
        {
            const string tf = "FTI20070926.TIK";
            // get sample tick data
            BarList bl = BarListImpl.FromTIK(tf);
            // verify expected number of 5min bars exist (78 in 9:30-4p)
            Assert.Greater(bl.Count,82,"not enough bars from: "+tf);

        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void FromGoogle()
        {
            string[] testsyms = new string[] { "IBM", "AAPL" };
            foreach (var sym in testsyms)
            {
                // get a year chart
                BarList bl = BarListImpl.DayFromGoogle(sym,false, g.d);
                // make sure it's there
                Assert.IsTrue(bl.isValid,"invalid barlist: "+sym);
                // verify we have at least a year of bar data
                Assert.GreaterOrEqual(bl.Count, 199,"not enough data in barlist: "+sym);

                // show bars
                //g.d(sym+" "+bl.Count.ToString());
            }

        }
#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void NegativeBars()
        {
            // get sample tick data
            BarList bl = BarListImpl.FromEPF("FTI20070926.EPF");
            // verify expected number of 5min bars exist (78 in 9:30-4p)
            Assert.AreEqual(83, bl.Count);
            // verify that 5th bar from end is same as 77th bar
            Assert.AreEqual(bl[-5].High, bl[77].High);
            Assert.AreEqual(bl[-5].Open, bl[77].Open);
            Assert.AreEqual(bl[-5].Low, bl[77].Low);
            Assert.AreEqual(bl[-5].Close, bl[77].Close);
            Assert.AreEqual(bl[-5].Bardate, bl[77].Bardate);
            Assert.AreEqual(bl[-5].Bartime, bl[77].Bartime);
        }
#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void CustomInterval()
        {
            // request 5 second bars
            const int MYINTERVAL = 5;
            BarList bl = new BarListImpl(sym, MYINTERVAL);
            // verify custom interval
            Assert.AreEqual(BarInterval.CustomTime, bl.DefaultInterval, "bad default interval");
            Assert.AreEqual(BarInterval.CustomTime, bl.Intervals[0], "bad interval");
            Assert.AreEqual(MYINTERVAL, bl.DefaultCustomInterval,"bad default custom interval");
            Assert.AreEqual(MYINTERVAL, bl.CustomIntervals[0],"bad custom interval");
            // iterate ticks
            foreach (Tick k in SampleData())
                bl.newTick(k);
            // count em
            Assert.AreEqual(10, bl.Count);

        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void TickInterval()
        {
            // request 2 tick bars
            const int MYINTERVAL = 2;
            BarList bl = new BarListImpl(sym, MYINTERVAL, BarInterval.CustomTicks);
            // verify custom interval
            Assert.AreEqual(MYINTERVAL, bl.DefaultCustomInterval);
            Assert.AreEqual(MYINTERVAL, bl.CustomIntervals[0]);
            // iterate ticks
            foreach (Tick k in SampleData())
                bl.newTick(k);
            // count em
            Assert.AreEqual(5, bl.Count);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void VolInterval()
        {            // request 300 volume bars
            const int MYINTERVAL = 300;
            BarList bl = new BarListImpl(sym, MYINTERVAL, BarInterval.CustomVol);
            // verify custom interval
            Assert.AreEqual(MYINTERVAL, bl.DefaultCustomInterval);
            Assert.AreEqual(MYINTERVAL, bl.CustomIntervals[0]);
            // iterate ticks
            foreach (Tick k in SampleData())
                bl.newTick(k);
            // count em
            Assert.AreEqual(4, bl.Count);
        }


        BarListTracker dailyBarTracker;
        BarListTracker intraBarTracker;

        [SetUp]
        public void Initialize()
        {
            dailyBarTracker = new BarListTracker(BarInterval.Day);
            intraBarTracker = new BarListTracker(BarInterval.FiveMin);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void SpoolDailyBars()
        {
            BarList bl = dailyBarTracker["SPY", (int)BarInterval.Day];
            for (int x = 0; x < SPY_Daily.Length; x++)
            {
                Bar b = BarImpl.Deserialize(SPY_Daily[x]);

                bl.newPoint(b.Symbol, b.Open, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);
            }

            Console.WriteLine("Count: " + bl.Count);
            Assert.AreEqual(SPY_Daily.Length, bl.Count);
            for (int x = 0; x < SPY_Daily.Length; x++)
            {
                Bar t = BarImpl.Deserialize(SPY_Daily[x]);
                Assert.AreEqual(bl[x].Bardate, t.Bardate);
                Assert.AreEqual(bl[x].Close, t.Close);
            }

            DisplayTrackerContent(dailyBarTracker["SPY"]);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void SpoolIntraBars()
        {
            BarList bl = intraBarTracker["SPY", (int)BarInterval.FiveMin];
            for (int x = 0; x < SPY_Intra.Length; x++)
            {
                Bar b = BarImpl.Deserialize(SPY_Intra[x]);

                bl.newPoint(b.Symbol, b.Open, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);
            }

            Console.WriteLine("Count: " + bl.Count);

            Assert.AreEqual(SPY_Intra.Length, bl.Count);
            for (int x = 0; x < SPY_Intra.Length; x++)
            {
                Bar t = BarImpl.Deserialize(SPY_Intra[x]);
                Assert.AreEqual(bl[x].Bardate, t.Bardate);
                Assert.AreEqual(bl[x].Close, t.Close);
            }

            DisplayTrackerContent(intraBarTracker["SPY"]);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void AddDailyTicks()
        {
            SpoolDailyBars();

            Tick k = new TickImpl("SPY");
            k.ask = 101;
            k.bid = 100;
            k.date = 20110208;
            k.trade = 100.5m;
            k.size = 5000;
            int baseTime = 144500;

            for (int x = 0; x < 20; x++)
            {
                k.time = baseTime + x;
                dailyBarTracker.newTick(k);
            }

            Assert.AreEqual(dailyBarTracker["SPY"].RecentBar.Close, k.trade);
            DisplayTrackerContent(dailyBarTracker["SPY"]);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void AddIntraTicks()
        {
            SpoolIntraBars();

            Tick k = new TickImpl("SPY");
            k.ask = 101;
            k.bid = 100;
            k.date = 20110208;
            k.trade = 100.5m;
            k.size = 5000;
            int baseTime = 144500;

            for (int x = 0; x < 20; x++)
            {
                k.time = baseTime + x;
                intraBarTracker.newTick(k);
            }

            Assert.AreEqual(intraBarTracker["SPY"].RecentBar.Close, k.trade);
            DisplayTrackerContent(intraBarTracker["SPY"]);
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void IntraMixedTickAndBars()
        {
            BarList bl = intraBarTracker["SPY", (int)BarInterval.FiveMin];
            for (int x = 0; x < SPY_Intra.Length; x++)
            {
                Bar b = BarImpl.Deserialize(SPY_Intra[x]);

                bl.newPoint(b.Symbol, b.Open, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);

                // interleave ticks with the bars to simulate datafeed asynchronous-ness
                int baseTime = 144500;
                Tick k = new TickImpl("SPY");
                k.ask = 101;
                k.bid = 100;
                k.date = 20110208;
                k.trade = 100.5m;
                k.size = 5000;
                k.time = baseTime + x;
                intraBarTracker.newTick(k);
            }

            Console.WriteLine("Count: " + bl.Count);
            DisplayTrackerContent(intraBarTracker["SPY"]);
            Assert.AreEqual(SPY_Intra.Length, bl.Count);
            for (int x = 0; x < SPY_Intra.Length - 1; x++)
            {
                Bar t = BarImpl.Deserialize(SPY_Intra[x]);
                Assert.AreEqual(bl[x].Bardate, t.Bardate);
                Assert.AreEqual(bl[x].Close, t.Close);
            }
        }

#if DEBUG
        [Test, Explicit]
        //[Test]
#else
        [Test]
#endif
        public void DailyMixedTickAndBars()
        {
            BarList bl = dailyBarTracker["SPY", (int)BarInterval.Day];
            for (int x = 0; x < SPY_Daily.Length; x++)
            {
                Bar b = BarImpl.Deserialize(SPY_Daily[x]);
                bl.newPoint(b.Symbol,b.Open, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.newPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);

                // interleave ticks with the bars to simulate datafeed asynchronous-ness
                int baseTime = 144500;
                Tick k = new TickImpl("SPY");
                k.ask = 101;
                k.bid = 100;
                k.date = 20110208;
                k.trade = 100.5m;
                k.size = 5000;
                k.time = baseTime + x;
                dailyBarTracker.newTick(k);
            }

            Console.WriteLine("Count: " + bl.Count);
            DisplayTrackerContent(dailyBarTracker["SPY"]);
            Assert.AreEqual(SPY_Daily.Length, bl.Count);
            for (int x = 0; x < SPY_Daily.Length - 1; x++)
            {
                Bar t = BarImpl.Deserialize(SPY_Daily[x]);
                Assert.AreEqual(bl[x].Bardate, t.Bardate);
                Assert.AreEqual(bl[x].Close, t.Close);
            }
        }


        private void DisplayTrackerContent(BarList bl)
        {
            Console.WriteLine("Displaying Content.");

            foreach (Bar b in bl)
                Console.WriteLine("d: " + b.Bardate + " t: " + b.Bartime
                    + " o: " + b.Open
                    + " h: " + b.High
                    + " l: " + b.Low
                    + " c: " + b.Close);
        }

        #region Data
        string[] SPY_Daily = {  // verified from Yahoo! Finance
            "127.440000,127.740000,126.950000,127.430000,110183900,20110111,93000,SPY,86400",
            "128.210000,128.720000,127.460000,128.580000,107436100,20110112,93000,SPY,86400",
            "128.630000,128.690000,128.050000,128.370000,129048400,20110113,93000,SPY,86400",
            "128.190000,129.330000,128.100000,129.300000,117611100,20110114,93000,SPY,86400",
            "129.180000,129.640000,129.030000,129.520000,114249600,20110118,93000,SPY,86400",

            "129.410000,129.540000,127.910000,128.250000,151709000,20110119,93000,SPY,86400",
            "127.960000,128.400000,127.130000,128.080000,175511200,20110120,93000,SPY,86400",
            "128.880000,129.170000,128.230000,128.370000,151377200,20110121,93000,SPY,86400",
            "128.290000,129.250000,128.260000,129.100000,113647600,20110124,93000,SPY,86400",
            "128.760000,129.280000,128.110000,129.170000,167388000,20110125,93000,SPY,86400",

            "129.490000,130.050000,129.230000,129.670000,141139500,20110126,93000,SPY,86400",
            "129.700000,130.210000,129.470000,129.990000,123206300,20110127,93000,SPY,86400",
            "130.140000,130.350000,127.510000,127.720000,295569200,20110128,93000,SPY,86400",
            "128.070000,128.780000,127.750000,128.680000,149126600,20110131,93000,SPY,86400",
            "129.460000,130.970000,129.380000,130.740000,166962200,20110201,93000,SPY,86400",

            "130.400000,130.840000,130.330000,130.490000,118163000,20110202,93000,SPY,86400",
            "130.260000,130.980000,129.570000,130.780000,145726000,20110203,93000,SPY,86400",
            "130.830000,131.200000,130.230000,131.150000,134584900,20110204,93000,SPY,86400",
            "131.440000,132.400000,131.430000,131.970000,112330500,20110207,93000,SPY,86400",
            "132.090000,132.640000,131.730000,132.570000, 98858300,20110208,93000,SPY,86400"
        };

        string[] SPY_Intra = {
            "132.200000,132.220000,132.200000,132.210000,108263,20110208,131000,SPY,300",
            "132.205000,132.220000,132.140000,132.180000,596368,20110208,131500,SPY,300",
            "132.180000,132.240000,132.160600,132.220000,504343,20110208,132000,SPY,300",
            "132.229900,132.260000,132.220000,132.230000,301859,20110208,132500,SPY,300",
            "132.240000,132.250000,132.210000,132.230000,173798,20110208,133000,SPY,300",

            "132.225000,132.230000,132.200000,132.210000,251372,20110208,133500,SPY,300",
            "132.205000,132.270000,132.205000,132.265000,151243,20110208,134000,SPY,300",
            "132.260000,132.310000,132.250000,132.300000,338762,20110208,134500,SPY,300",
            "132.290000,132.340000,132.270000,132.319000,570296,20110208,135000,SPY,300",
            "132.300000,132.320000,132.290000,132.320000,183190,20110208,135500,SPY,300",

            "132.320000,132.330000,132.290000,132.310000,235016,20110208,140000,SPY,300",
            "132.310000,132.320000,132.290000,132.299900,117468,20110208,140500,SPY,300",
            "132.300000,132.310000,132.280000,132.308000,199589,20110208,141000,SPY,300",
            "132.300000,132.320000,132.290000,132.310000,319106,20110208,141500,SPY,300",
            "132.300000,132.370000,132.300000,132.340000,576937,20110208,142000,SPY,300",

            "132.330000,132.390000,132.330000,132.380000,320253,20110208,142500,SPY,300",
            "132.385000,132.450000,132.380000,132.440000,881651,20110208,143000,SPY,300",
            "132.430200,132.440000,132.390000,132.400000,281805,20110208,143500,SPY,300",
            "132.400000,132.480000,132.400000,132.470000,624035,20110208,144000,SPY,300",
            "132.462000,132.490000,132.430000,132.450000,785918,20110208,144500,SPY,300"
        };
        #endregion

  

        BarList blt = null;
        
        void testbar(BarList bars)
        {
            blt = bars;
        }


    }
}
