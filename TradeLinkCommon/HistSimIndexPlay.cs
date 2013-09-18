using System;
using System.Collections.Generic;
using System.Text;
using TradeLink.API;
using System.ComponentModel;
using System.IO;

namespace TradeLink.Common
{

    /// <summary>
    /// playback a historical simulation from an index
    /// </summary>
    public class HistSimIndexPlay : HistSim
    {
        protected HistSimIndex myhsi = new HistSimIndex();
        BackgroundWorker _reader = new BackgroundWorker();
        public event DebugDelegate GotDebug;
        public event TickDelegate GotTick;
        int _readwait = 0;
        /// <summary>
        ///  wait between reads
        /// </summary>
        public int WaitRead { get { return _readwait; } set { _readwait = value; } }
        int _writewait = 0;
        /// <summary>
        ///  wait between writes
        /// </summary>
        public int WaitWrite { get { return _writewait; } set { _writewait = value; } }
        /// <summary>
        /// play simulation until specified day/time is hit
        /// </summary>
        /// <param name="datetime"></param>
        virtual public void PlayTo(long datetime)
        {
            // check for event
            if (GotTick == null)
            {
                debug("Not handling ticks, quitting...");
                return;
            }

            // handle non binary indicies
            if (!myhsi.isBinaryIndex)
            {
                // make sure inited
                if (!hsipinited)
                {
                    Initialize();
                    // wait a moment
                    System.Threading.Thread.Sleep(WaitRead * 2);
                }
                // ensure reader is done
                while (_reader.IsBusy)
                {
                    _reader.CancelAsync();
                }

                // start reading
                _reader.RunWorkerAsync(datetime);

                stopwatch();
                // start writing
                while (hsipplay && (hsip_nexttime <= datetime))
                {
                    if (_buf.hasItems)
                    {
                        Tick k = _buf.Read();
                        gotnewtick(k);
                    }
                    else if (!_reader.IsBusy)
                        break;
                    System.Threading.Thread.Sleep(_writewait);
                }
                stopwatch();
            }
            else
            {
                debug("Playing index containing " + myhsi.TicksPresent.ToString("N0") + " ticks.");
                TikReader tw = new TikReader(myhsi.BinaryIndex);
                // reset counters
                br = 0;
                tw.gotTick += new TickDelegate(tw_gotTick);
                // get symbols for every toc
                string[] syms = new string[myhsi.TOCTicks.Length];
                for (int i = 0; i < syms.Length; i++)
                    syms[i] = SecurityImpl.SecurityFromFileName(myhsi.TOCTicks[i]).symbol;
                // get symbol for every tick
                symidx = new string[TicksPresent];
                for (int i = 0; i < symidx.Length; i++)
                    symidx[i] = syms[myhsi.Playindex[i]];
                // playback ticks
                stopwatch();
                while (hsipplay && (hsip_nexttime <= datetime) && tw.NextTick())
                {
                    
                }
                stopwatch();
            }
        }
        int br = 0;
        string[] symidx;
        void tw_gotTick(Tick t)
        {
            if (br > symidx.Length) 
                return;
            t.symbol = symidx[br++];
            hsip_nexttime = t.datetime;
            gotnewtick(t);
        }

        virtual protected void gotnewtick(Tick k)
        {
            hsip_ticks++;
            GotTick(k);
        }
        protected bool hsipplay = true;
        protected bool hsipinited = false;
        List<TikReader> readers = new List<TikReader>();
        RingBuffer<Tick> _buf = new RingBuffer<Tick>();
        /// <summary>
        /// initialize simulation (will be initilized automatically when calling playto)
        /// </summary>
        virtual public void Initialize()
        {
            // setup the readers
            for (int i = 0; i < myhsi.TOC.GetLength(0); i++)
            {
                readers.Add(new tagged(i, Folder+myhsi.TOC[i, 0]));
                readers[readers.Count - 1].gotTick += new TickDelegate(HistSimIndexPlay_gotTick);
                hsip_avail += readers[readers.Count - 1].ApproxTicks;
            }
            debug("Initialized " + readers.Count + " files containing ~" + hsip_avail + " ticks.");
            int buffsize = (int)((double)hsip_avail / 10);
            _buf = new RingBuffer<Tick>(buffsize);
            hsipinited = true;
            if (myhsi.Playtimes.Length>0)
                hsip_nexttime = myhsi.Playtimes[0];
        }

        void HistSimIndexPlay_gotTick(Tick t)
        {
            // save time
            hsip_nexttime = t.datetime;
            // write to buffer
            _buf.Write(t);
        }
        protected void debug(string msg)
        {
            if (GotDebug != null)
                GotDebug(msg);
        }
        protected long hsip_nexttime = MultiSimImpl.STARTSIM;
        public long NextTickTime { get { return hsip_nexttime; } }
        protected int hsip_ticks = 0;
        public int TicksProcessed { get { return hsip_ticks; } }
        protected int hsip_avail = 0;
        public int TicksPresent { get { return hsip_avail; } }
        /// <summary>
        /// stop simulation if presently running
        /// </summary>
        virtual public void Stop()
        {
            hsipplay = false;
            try
            {
                foreach (TikReader tr in readers)
                    tr.Close();
            }
            catch { }
            try
            {
                _reader.CancelAsync();
            }
            catch { }
        }
        int _rc = 0;
        /// <summary>
        /// set simulation back to zero
        /// </summary>
        virtual public void Reset()
        {
            Stop();
            hsipplay = true;
            _rc = 0;
            hsip_ticks = 0;
            readers.Clear();
            if (myhsi.Playtimes.Length > 0)
                hsip_nexttime = myhsi.Playtimes[0];
            else
                hsip_nexttime = MultiSimImpl.STARTSIM;
            Initialize();
        }
        /// <summary>
        /// create histsimindex player from an index
        /// </summary>
        /// <param name="hsi"></param>
        public HistSimIndexPlay(HistSimIndex hsi) : this(hsi, null) { }
        public HistSimIndexPlay(HistSimIndex hsi, DebugDelegate deb)
        {
            _folder = hsi.Folder;
            if (deb != null)
                GotDebug += deb;
            _reader.DoWork += new DoWorkEventHandler(_reader_DoWork);
            _reader.WorkerSupportsCancellation = true;
            myhsi = hsi;
            hsip_avail = hsi.Playindex.Length;
            checkindex();
        }
        void checkindex()
        {
            if (!myhsi.isReady)
                debug("notice, empty index... nothing to playback.");
            if (!myhsi.isComplete)
                debug("warning, incomplete index... some data may be missing.");
            if (myhsi.isRunning)
            {
                edebug("notice, index is still building...");
            }
            if (hsip_avail == 0)
                debug("notice, nothing indexed for playback...");

        }
        /// <summary>
        /// create a player from some tickdata, attempt to create index if not present
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tff"></param>
        public HistSimIndexPlay(string folder, TickFileFilter tff) : this(folder, tff, 0,null) { }
        public HistSimIndexPlay(string folder, TickFileFilter tff,int interval,DebugDelegate deb) 
        {
            if (deb!=null)
                GotDebug+=deb;
            _folder = folder + "\\";
            // see if we have index
            string fn;
            HistSimIndex hsi;
            debug("getting tickfiles, please wait...");
            string[,] total = Util.TickFileIndex(folder, TikConst.WILDCARD_EXT,false);
            debug("found " + total.GetLength(0).ToString("N0") + " tickfiles");
            string[,] filtered = tff.AllowsIndexAndSize(total);
            debug("post-filter: " + filtered.GetLength(0).ToString("N0")+", checking for index...");

            if (HistSimIndex.HaveIndex(folder, filtered, out fn,deb))
            {
                debug("index found, starting load: "+fn);
                hsi = HistSimIndex.FromFile(fn,debug);
                if (hsi == null)
                {
                    debug("unable to load index.");
                    return;
                }
                hsi.GotDebug+=new DebugDelegate(debug);
            }
            else if (HistSimIndex.BuildIndex(folder, filtered, out hsi, true,true,interval,debug))
            {
                debug("Index built successfully, ready to play.");
            }
            else
            {
                debug("Error building index...");
                return;
            }
            _reader.DoWork += new DoWorkEventHandler(_reader_DoWork);
            _reader.WorkerSupportsCancellation = true;
            myhsi = hsi;
            hsip_avail = hsi.Playindex.Length;
            checkindex();

        }

        bool _extdebug = false;

        void edebug(string msg)
        {
            if (_extdebug)
            {
                debug(msg);
            }
        }

        /// <summary>
        /// build index for an entire folder
        /// </summary>
        /// <param name="folder"></param>
        public HistSimIndexPlay(string folder, int interval)
        {
            _folder = folder+"\\";
            string[,] tfi = Util.TickFileIndex(folder, TikConst.WILDCARD_EXT);
            // see if we have index
            string fn;
            HistSimIndex hsi;
            _extdebug = tfi.GetLength(0) > 500;
            edebug("checking for index for: "+tfi.GetLength(0)+"-sized simulation.");
            if (HistSimIndex.HaveIndex(folder,tfi,interval,out fn))
            {
                edebug("index found.");
                hsi = HistSimIndex.FromFile(fn);
                hsi.GotDebug += new DebugDelegate(debug);
            }
            else if (HistSimIndex.BuildIndex(folder,tfi,out hsi, true,true,interval,debug))
            {
                edebug("Index built successfully. ");
            }
            else
            {
                debug("Error building index.");
                return;
            }
            myhsi = hsi;
            _reader.DoWork += new DoWorkEventHandler(_reader_DoWork);
            _reader.WorkerSupportsCancellation = true;

            hsip_avail = hsi.Playindex.Length;
            checkindex();
        }

        public static string[,] GetTickIndex(string[] dataset, bool removedirectorypath, string folder)
        {
            // get index
            string[,] indx = new string[dataset.Length, 2];
            // process every element
            for (int i = 0; i < dataset.Length; i++)
            {
                string name = removedirectorypath ? folder + "\\" + Path.GetFileName(dataset[i]) : dataset[i];
                FileInfo fi = new FileInfo(name);
                if (fi.Exists)
                {
                    indx[i, 0] = dataset[i];
                    indx[i, 1] = fi.Length.ToString();
                }

            }
            return indx;

        }

        public HistSimIndexPlay(string[] dataset, string folder) : this(dataset, folder,0,null) { }
        public HistSimIndexPlay(string[] dataset,string folder, int interval, DebugDelegate deb)
        {
            if (deb != null)
                GotDebug += deb;
            if (dataset.Length < 1)
            {
                debug("Empty data set, quitting...");
                return;
            }
            _folder = folder; 
            // get index of set
            string[,] tfi = GetTickIndex(dataset,false,folder);
            _extdebug = tfi.GetLength(0) > 500;
            // see if we have index
            string fn;
            HistSimIndex hsi;
            edebug("checking for index");
            if (HistSimIndex.HaveIndex(folder, tfi,interval, out fn))
            {
                edebug("index found.");
                hsi = HistSimIndex.FromFile(fn);
                hsi.GotDebug += new DebugDelegate(debug);
            }
            else if (HistSimIndex.BuildIndex(folder, tfi, out hsi, true, true,interval, debug))
            {
                edebug("Index built successfully");
            }
            else
            {
                debug("Error building index...");
                return;
            }
            myhsi = hsi;
            _reader.DoWork += new DoWorkEventHandler(_reader_DoWork);
            _reader.WorkerSupportsCancellation = true;

            hsip_avail = hsi.Playindex.Length;
            checkindex();
        }

        System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
        int _swstart = 0;
        protected double stopwatch() { return stopwatch(!_sw.IsRunning); }
        protected double stopwatch(bool start)
        {
            if (start)
            {
                _sw.Reset();
                _sw.Start();
                _swstart = hsip_ticks;
                return 0;
            }
            _sw.Stop();
            int processed = (hsip_ticks - _swstart) ;
            double rate = processed/ _sw.Elapsed.TotalSeconds;
            debug("Played: "+processed.ToString("N0")+" ticks @ " + rate.ToString("N0") + " ticks/sec");
            return rate;

        }

        void _reader_DoWork(object sender, DoWorkEventArgs e)
        {
            long playto = (long)e.Argument;

            while (!e.Cancel && (hsip_nexttime <= playto))
            {
                // ensure we don't playtoo far
                if (_rc >= myhsi.Playindex.Length)
                    break;
                // get index file to read from
                int next = myhsi.Playindex[_rc++];
                // get tick from this file
                readers[next].NextTick();
                // wait between reads
                System.Threading.Thread.Sleep(_readwait);
            }
        }

        string _folder = string.Empty;
        /// <summary>
        /// folder where ticks are located
        /// </summary>
        public string Folder { get { return _folder; } set { _folder = value; } }

        internal class tagged : TikReader
        {
            int _idx = -1;
            internal tagged(int idx, string fullpath) : base(fullpath)
            {
                _idx = idx;
            }
        }


    }
}
