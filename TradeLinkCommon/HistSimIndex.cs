using System;
using System.Collections.Generic;
using System.Text;
using TradeLink.API;
using TradeLink.Common;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

namespace TradeLink.Common
{
    /// <summary>
    /// builds an index for faster playback of historical simulation
    /// </summary>
    [XmlRoot("HistSimIndexInstance")]
    public class HistSimIndex
    {
        /// <summary>
        /// serialize index
        /// </summary>
        /// <param name="hsi"></param>
        /// <returns></returns>
        public static string Serialize(HistSimIndex hsi) { return Serialize(hsi, null); }
        public static string Serialize(HistSimIndex hsi, DebugDelegate debug)
        {
            try
            {
                hsi.packTOC();
                XmlSerializer xs = new XmlSerializer(typeof(HistSimIndex));
                StringWriter fs = new StringWriter();
                xs.Serialize(fs, hsi);
                fs.Close();
                string msg = fs.GetStringBuilder().ToString();
                return GZip.Compress(msg);
            }
            catch (Exception ex)
            {
                if (debug != null)
                    debug(ex.Message + ex.StackTrace);
            }
            return string.Empty;
        }
        internal void packTOC()
        {
            int count = TOC.GetLength(0);
            List<string> tiks = new List<string>(count);
            List<string> sizes = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                tiks.Add(TOC[i, 0]);
                sizes.Add(TOC[i, 1]);
            }
            _tiks = tiks.ToArray();
            _size = sizes.ToArray();

        }
        void unpackTOC()
        {
            _contents = new string[_tiks.Length, 2];
            for (int i = 0; i < _tiks.Length; i++)
            {
                _contents[i, 0] = _tiks[i];
                _contents[i, 1] = _size[i];
            }
                
        }
        /// <summary>
        /// deserialize an index
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static HistSimIndex Deserialize(string msg)
        {
            HistSimIndex cw = null;
            try
            {
                string xmsg = GZip.Uncompress(msg);
                // prepare serializer
                XmlSerializer xs = new XmlSerializer(typeof(HistSimIndex));
                // read in message
                StringReader fs = new StringReader(xmsg);
                // deserialize message
                cw = (HistSimIndex)xs.Deserialize(fs);
                // close serializer
                fs.Close();
                // unpack toc
                cw.unpackTOC();
            }
#if DEBUG
            catch (FileNotFoundException ex) 
            { 
                Console.WriteLine("fill not found error deserializing histsimindex: "+msg+" err: "+ex.Message+ex.StackTrace);
            }
#else
            catch (FileNotFoundException)
            {
            }
#endif
            return cw;
        }
        /// <summary>
        /// get index from a set of tick files given a filter
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tff"></param>
        /// <param name="hsi"></param>
        /// <returns></returns>
        public static bool BuildIndex(string folder, TickFileFilter tff, out HistSimIndex hsi, bool start) { return BuildIndex(folder, tff, out hsi, start, true, 0,null); }
        public static bool BuildIndex(string folder, TickFileFilter tff, out HistSimIndex hsi, bool start,bool saveidx,int interval, DebugDelegate debug)
        {
            if (debug != null)
                debug("getting tickfiles available, please wait...");
            string[,] allows = Util.TickFileIndex(folder, TikConst.WILDCARD_EXT);
            if (debug != null)
                debug("found " + allows.Length + " tickfiles.");
            return BuildIndex(folder,allows, tff, out hsi,start,saveidx,interval,debug);
        }
        /// <summary>
        /// get index from a set of tick files given a filter and starts indexing
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tff"></param>
        /// <param name="hsi"></param>
        /// <returns></returns>
        public static bool BuildIndex(string folder, TickFileFilter tff, out HistSimIndex hsi) { return BuildIndex(folder, tff, out hsi, true, true, 0,null); }
        public static bool BuildIndex(string folder, TickFileFilter tff, out HistSimIndex hsi,int interval,DebugDelegate debug)
        {
            if (debug != null)
                debug("getting tickfiles available, please wait...");
            string[,] allows = Util.TickFileIndex(folder, TikConst.WILDCARD_EXT);
            if (debug != null)
                debug("found " + allows.Length + " tickfiles.");

            return BuildIndex(folder,allows, tff,out hsi, true,true,interval,debug);
        }

        /// <summary>
        /// get index from set of tick files given a filter
        /// </summary>
        /// <param name="tickfiles"></param>
        /// <param name="tff"></param>
        /// <param name="hsi"></param>
        /// <returns></returns>
        public static bool BuildIndex(string folder, string[,] tickfiles, TickFileFilter tff, out HistSimIndex hsi, bool start) { return BuildIndex(folder, tickfiles, out hsi, start, true, 0,null); }
        public static bool BuildIndex(string folder, string[,] tickfiles, TickFileFilter tff, out HistSimIndex hsi, bool start,bool saveidx, int interval, DebugDelegate debug)
        {
            if (debug != null)
                debug("filtering tickfiles...");
            string[,] allows = tff.AllowsIndexAndSize(tickfiles);
            if (debug != null)
                debug("using " + allows.Length + " tickfiles post-filter.");
            return BuildIndex(folder,allows, out hsi,start,saveidx,interval,debug);
        }
        /// <summary>
        /// gets index from set of tick files and starts indexing
        /// </summary>
        /// <param name="tickfiles"></param>
        /// <param name="tff"></param>
        /// <param name="hsi"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static bool BuildIndex(string folder, string[,] tickfiles, TickFileFilter tff, out HistSimIndex hsi) { return BuildIndex(folder, tickfiles, tff,out hsi,0, null); }
        public static bool BuildIndex(string folder, string[,] tickfiles, TickFileFilter tff, out HistSimIndex hsi, int interval, DebugDelegate debug)
        {
            if (debug != null)
                debug("filtering tickfiles...");
            string[,] allows = tff.AllowsIndexAndSize(tickfiles);
            if (debug != null)
                debug("using " + allows.Length + " tickfiles post-filter.");

            return BuildIndex(folder,allows, out hsi, true,true,interval,debug);
        }
        /// <summary>
        /// gets index for given set of tick files
        /// </summary>
        /// <param name="tickfiles"></param>
        /// <param name="hsi"></param>
        /// <returns></returns>
        public static bool BuildIndex(string folder, string[,] tickfiles, out HistSimIndex hsi) { return BuildIndex(folder, tickfiles, out hsi, true); }
        /// <summary>
        /// gets index for set of tick files
        /// </summary>
        /// <param name="tickfiles"></param>
        /// <param name="hsi"></param>
        /// <param name="start">whether to build the index or just the TOC</param>
        /// <returns></returns>
        public static bool BuildIndex(string folder, string[,] tickfiles, out HistSimIndex hsi, bool start) { return BuildIndex(folder, tickfiles, out hsi, start, true, 0,null); }
        public static bool BuildIndex(string folder, string[,] tickfiles, out HistSimIndex hsi, bool start, bool saveindex) { return BuildIndex(folder, tickfiles, out hsi, start, saveindex, 0,null); }
        public static bool BuildIndex(string folder, string[,] tickfiles, out HistSimIndex hsi, bool start, bool saveindex, int interval, DebugDelegate debug)
        {
            hsi = new HistSimIndex(folder,tickfiles,interval);
            if (debug!=null)
                hsi.GotDebug+=new DebugDelegate(debug);
            bool ok = true;
            if (start)
                hsi.Start();
            if (start && saveindex)
            {
                bool save = ToFile(hsi, idxfold(folder), debug);
                if (debug != null)
                    debug(save ? ("Saved index: " + folder) : "Saving index failed.");
                ok &= save;
            }
            ok &= start ? hsi.isComplete : hsi.TOC.Length > 0;
            return ok;
        }

        int estsize()
        {
            int size = 0;
            for (int i = 0; i < TOC.GetLength(0); i++)
            {
                int s = 0;
                if (int.TryParse(TOC[i,1],out s))
                    size += s;
            }
            return size;
        }
        /// <summary>
        /// prepares to index a set of tickfiles
        /// </summary>
        /// <param name="tickfiles"></param>
        public HistSimIndex(string folder, string[,] tickfiles,int interval)
        {
            Interval = interval;
            _folder = folder;
            _contents = tickfiles;
            readers = new List<tickreader>(tickfiles.Length);
        }
        string _folder = string.Empty;
        /// <summary>
        /// folder where tick files are located
        /// </summary>
        public string Folder { get { return _folder; } set { _folder = value; } }
        const string IDXFOLDER = "\\index\\";
        /// <summary>
        /// folder where index files are located
        /// </summary>
        public string IndexFolder { get { return _folder + IDXFOLDER; } }
        /// <summary>
        /// create empty index
        /// </summary>
        public HistSimIndex() : this (string.Empty,new string[0,0],0) 
        {
            
        }
        List<tickreader> readers;
        bool _running = false;
        bool _finished = false;
        /// <summary>
        /// whether index is presently running/building
        /// </summary>
        public bool isRunning { get { return _running; } }
        /// <summary>
        /// whether all files in index were read successfully
        /// </summary>
        public bool isComplete { get { return _finished; } set { _finished = value; } }

        bool _binaryindex = true;
        /// <summary>
        /// whether binary index is used to complement text index
        /// </summary>
        public bool isBinaryIndex { get { return _binaryindex; } set { _binaryindex = value; } }
        string _binaryidx = string.Empty;
        /// <summary>
        /// name of binary index file if used
        /// </summary>
        public string BinaryIndex { get { return _binaryidx; } set { _binaryidx = value; } }
        /// <summary>
        /// build the index
        /// </summary>
        public void Start()
        {
            const int RATE = 30000;
            int esttick = estsize() / 40;
            double est =  esttick/ RATE;
            TimeSpan ts = new TimeSpan(0, 0, (int)est);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            debug("Starting creation of index... estimated: "+ts.ToString());
            // start the workers
            for (int i = 0; i < TOC.GetLength(0); i++)
            {
                // try provided path
                string fn = TOC[i, 0];
                // ensure file exists
                if (File.Exists(fn))
                    readers.Add(new tickreader(i, fn, _binaryindex, Interval));
                else
                {
                    // try default path
                    fn = Folder + "\\" + Path.GetFileName(TOC[i, 0]);
                    if (File.Exists(fn))
                        readers.Add(new tickreader(i, fn, _binaryindex, Interval));
                    else
                        debug(fn + " not found.");
                }
            }
            debug("Started "+readers.Count+" readers, waiting for completition");
            // wait till they're done
            _running = true;
            while (_running)
            {
                System.Threading.Thread.Sleep(SLEEP);
                bool busy = false;
                for (int i = 0; i < readers.Count; i++)
                    busy |= readers[i].IsBusy;
                _running = busy;
            }
            debug("Readers completed, assembling master index.");
            _running = false;
            // estimate size
            int size = 0;
            bool finished = true;
            foreach (tickreader tr in readers)
            {
                finished &= tr.finished;
                size += tr.count;
            }
            // append to single arrays
            List<long> times = new List<long>(size);
            List<int> index = new List<int>(size);
            foreach (tickreader tr in readers)
            {
                times.AddRange(tr.datetimes);
                index.AddRange(tr.index);
            }
            long[] ltimes = times.ToArray();
            int[] iindex = index.ToArray();
            // sort master array
            Array.Sort(ltimes, iindex);
            // save play index
            _idx = iindex;
            // save times
            _times = ltimes;
            // see if we're saving binary index
            if (_binaryindex)
            {
                // fake symbol name
                string sym = MD5(indextoc(this));
                // ensure binary index folder exists
                if (!Directory.Exists(IndexFolder))
                    Directory.CreateDirectory(IndexFolder);
                // create file
                TikWriter tw = new TikWriter(IndexFolder, sym, 0);
                // get a filename
                _binaryidx = tw.Filepath;
                debug("Creating binary index: "+BinaryIndex);
                // master tick list
                Tick[] ktiks = new Tick[times.Count];
                ltimes = times.ToArray();
                int next = 0;
                for (int i = 0; i < readers.Count; i++)
                {
                    int len = readers[i].ticks.Count;
                    readers[i].ticks.CopyTo(0, ktiks, next, len);
                    next += len;
                    readers[i].ticks.Clear();
                }
                // sort ticks
                Array.Sort(ltimes, ktiks);
                // save ticks to binary file
                int count = 0;
                for (int i = 0; i < iindex.Length; i++)
                {
                    // get tick
                    Tick k = ktiks[count++];
                    // save
                    tw.newTick(k);
                }
                tw.Close();
                debug("binary index completed containing " + count + " ticks.");

            }
            // save finish state
            _finished = finished;
            // clean up readers
            readers.Clear();
            sw.Stop();
            double actual = sw.Elapsed.TotalSeconds;
            double rate =  actual == 0 ? 0 : ltimes.Length / actual;
            debug("Master index complete.   actual: "+sw.Elapsed.ToString()+"("+rate.ToString("N0")+" ticks/sec)");
            
        }



        public event DebugDelegate GotDebug;
        void debug(string msg)
        {
            if (GotDebug != null)
                GotDebug(msg);
        }
        /// <summary>
        /// resets everything
        /// </summary>
        public void Reset()
        {
            _contents = new string[0, 0];
            _idx = new int[0];
            _finished = false;
        }
        /// <summary>
        /// stops indexing if running
        /// </summary>
        public void Stop()
        {
            for (int i = 0; i < readers.Count; i++)
                readers[i].CancelAsync();
        }
        /// <summary>
        /// polling delay in ms, how often index data load is checked for completion
        /// </summary>
        public int SLEEP = 50;

        long[] _times = new long[0];
        /// <summary>
        /// playtimes corresponding with playindicies
        /// </summary>
        public long[] Playtimes { get { return _times; } set { _times = value; } }

        int[] _idx = new int[0];
        /// <summary>
        /// playorder of tickfiles in index
        /// </summary>
        public int[] Playindex { get { return _idx; } set { _idx = value; } } 
        string[,] _contents = new string[0,0];
        /// <summary>
        /// tickfiles making up index and their sizes
        /// </summary>
        public string[,] TOC { get { return _contents; } }
        string[] _tiks = new string[0];
        /// <summary>
        /// used by serializer, do not use.
        /// </summary>
        public string[] TOCTicks { get { return _tiks; } set { _tiks = value; } }
        string[] _size = new string[0];
        /// <summary>
        /// used by serializer, do not use
        /// </summary>
        public string[] TOCSizes { get { return _size; } set { _size = value; } }

        /// <summary>
        /// whether two indicies are equivalent (index same files)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public bool Equals(HistSimIndex idx)
        {
            if (_contents.Length != idx.TOC.Length) return false;
            bool equal = true;
            for (int i = 0; i < idx.TOC.GetLength(0); i++)
            {
                // look for our match
                bool match = false;
                for (int j = 0; j<_contents.GetLength(0); j++)
                    if ((idx.TOC[i,0] == _contents[j,0]) && (idx.TOC[i,1]==_contents[j,1]))
                    {
                        match = true;
                        break;
                    }
                equal &= match;
            }
            return equal;
        }
        /// <summary>
        /// returns true if it has some index data ready for playback
        /// </summary>
        public bool isReady { get { return _idx.Length > 0; } }
        public override bool Equals(object obj)
        {
            try
            {
                HistSimIndex hsi = (HistSimIndex)obj;
                return Equals(hsi);
            }
            catch { }
            return false;
        }

        public override int GetHashCode()
        {
            int size = 0;
            for (int i = 0; i < _size.GetLength(0); i++)
            {
                int n = 0;
                if (int.TryParse(_size[i], out n))
                    size += n;
            }
            return _contents.Length+size;
        }

        /// <summary>
        /// ticks present in index
        /// </summary>
        public int TicksPresent { get { return Playindex.Length; } }


        public static string MD5(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            System.Security.Cryptography.MD5 cryptp = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            data = cryptp.ComputeHash(data);
            string md5 = Convert.ToBase64String(data);
            string safe = string.Empty;
            char[] md5c = md5.ToCharArray();
            char[] inv = Path.GetInvalidFileNameChars();
            for (int i = 0; i < md5c.Length; i++)
            {
                bool isinvalid = false;
                for (int j = 0; j < inv.Length; j++)
                    isinvalid |= inv[j] == md5c[i];
                if (!isinvalid)
                    safe+=md5c[i];
            }
            return safe;
        }
        /// <summary>
        /// gets a serialized index from a file
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static HistSimIndex FromFile(string filepath) { return FromFile(filepath, null); }
        public static HistSimIndex FromFile(string filepath,DebugDelegate debug)
        {
            try
            {
                HistSimIndex hsi;
                // prepare serializer
                XmlSerializer xs = new XmlSerializer(typeof(HistSimIndex));
                // read in message
                Stream fs = new FileStream(filepath, FileMode.Open);
                // uncompress
                System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress);
                // deserialize message
                hsi = (HistSimIndex)xs.Deserialize(gs);
                // close everything
                gs.Close();
                // close serializer
                fs.Close();

                // unpack toc
                hsi.unpackTOC();
                return hsi;
            }
            catch (Exception ex)
            {
                if (debug != null)
                    debug(ex.Message + ex.StackTrace);
                return null;
            }

        }
        int _int = 0;
        /// <summary>
        /// bar interval in seconds
        /// </summary>
        public int Interval { get { return _int; } set { _int = value; } }
        bool hasInterval { get { return _int != 0; } }
        static string indextoc(HistSimIndex hsi)
        {
            string data = hsi.Interval == 0 ? string.Empty : hsi.Interval.ToString();
            for (int i = 0; i < hsi.TOC.GetLength(0); i++)
            {
                data += hsi.TOC[i,0]+"="+hsi.TOC[i,1] + " ";
            }
            return data;
        }
        /// <summary>
        /// serialize idnex as a file
        /// </summary>
        /// <param name="hsi"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ToFile(HistSimIndex hsi, string path) { return ToFile(hsi, path, null); }
        public static bool ToFile(HistSimIndex hsi, string path,DebugDelegate debug)
        {
            try
            {
                // get checksum of data
                string md5 = MD5(indextoc(hsi));
                // see if it's complete
                if (!hsi.isComplete)
                {
                    if (debug!=null)
                        debug("Index not completed "+md5);
                    return false;
                }
                // get index directory
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                // get filepath
                string filepath = dir + "\\" + md5 + ".txt";
                // pack contents so serializable
                hsi.packTOC();
                XmlSerializer xs = new XmlSerializer(typeof(HistSimIndex));
                Stream fs = new FileStream(filepath, FileMode.Create);
                System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress);
                xs.Serialize(gs, hsi);
                gs.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                if (debug != null)
                    debug(ex.Message + ex.StackTrace);
                return false;
            }

            return true;

        }
        /// <summary>
        /// checks whether we already have an index built for a given pre-filtered data set
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tff"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool HaveIndex(string folder, TickFileFilter tff, out string file) { return HaveIndex(folder, tff, out file, null); }
        public static bool HaveIndex(string folder, TickFileFilter tff, out string file, DebugDelegate debug)
        {
            if (debug != null)
                debug("getting tickfiles present: " + folder);
            string[,] files = Util.TickFileIndex(folder, TikConst.WILDCARD_EXT);
            if (debug!=null)
                debug("got "+files.Length.ToString("N0")+" tickfiles");
            string[,] filtered = tff.AllowsIndexAndSize(files);
            if (debug != null)
                debug("found " + filtered.GetLength(0).ToString("N0") + " files post-filter.");
            return HaveIndex(folder, filtered, out file, debug);

        }
        /// <summary>
        /// checks whether we already have an index for a given (post-filtered) data set
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tickfiles"></param>
        /// <param name="file"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static bool HaveIndex(string folder, string[,] tickfiles, out string file, DebugDelegate debug)
        {
            return HaveIndex(folder, tickfiles, out file, 0, debug);
        }
        public static bool HaveIndex(string folder, string[,] tickfiles, out string file,int interval,DebugDelegate debug)
        {
            HistSimIndex hsi;
            if (BuildIndex(folder,tickfiles,out hsi,false,false,interval,debug))
            {
                string checksum = MD5(indextoc(hsi));
                string fn = idxfold(folder)+ checksum + ".txt";
                if (File.Exists(fn))
                {
                    file = fn;
                    return true;
                }
                    
            }
            file = string.Empty;
            return false;
        }

        static string idxfold(string fold)
        {
            return fold + IDXFOLDER;
        }

        /// <summary>
        /// checks whether we already have an index built for a given data set.
        /// if index is empty, deletes index.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tff"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool HaveIndex(string folder,string[,] tickindex, int interval,out string file)
        {
            HistSimIndex hsi;
            if (BuildIndex(folder,tickindex,out hsi, false,false,interval,null))
            {
                string checksum = MD5(indextoc(hsi));
                string fn = idxfold(folder) + checksum + ".txt";
                if (File.Exists(fn))
                {
                    file = fn;
                    hsi = HistSimIndex.FromFile(fn);
                    bool complete = hsi.isComplete && (hsi.Playindex.Length > 0);
                    if (!complete)
                    {
                        File.Delete(fn);
                        return false;
                    }
                    return true;
                }

            }
            file = string.Empty;
            return false;
        }

                         /*sorting rectangular array
                                      int[] tags = genidx(hsi.TickFiles.GetLength(0)*2);
            Array.Sort(tags, (t1, t2) => { return hsi.TickFiles[t1, 0].CompareTo(hsi.TickFiles[t2, 0]); });
            var sorted = from x in tags orderby hsi.TickFiles[x,1] select x;
                          */



        



        internal class tickreader 
        {
            const int EXPECTTICKS = 20000;
            internal List<int> index;
            internal List<long> datetimes ;
            internal int idx ;
            bool _saveticks ;
            internal List<Tick> ticks;
            TikReader tr;
            string file;
            int _int = 0;
            BarList bl;
            internal tickreader(int Index, string filepath, bool saveticks, int interval)
            {
                _int = interval;
                file = filepath;
                finished = false;
                count = 0;
                Cancel = false;
                IsBusy = false;
                _saveticks = false;
                datetimes = new List<long>(EXPECTTICKS);
                index = new List<int>(EXPECTTICKS);
                tr = new TikReader(filepath);
                if (_saveticks)
                    ticks = new List<Tick>(tr.ApproxTicks);
                else
                    ticks = new List<Tick>();
                _saveticks = saveticks;
                idx = Index;
                if (interval == 0)
                    tr.gotTick += new TickDelegate(tr_gotTick);
                else
                {
                    bl = new BarListImpl(tr.Symbol, interval, BarInterval.CustomTime);
                    bl.GotNewBar += new SymBarIntervalDelegate(bl_GotNewBar);
                    tr.gotTick += new TickDelegate(tr_gotTick2);
                }

                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolGo), this);
                IsBusy = true;
            }

            void bl_GotNewBar(string symbol, int interval)
            {
                
                Bar b = bl[-1];
                Tick[] ks = BarImpl.ToTick(b);
                foreach (Tick t in ks)
                {
                    if (_saveticks)
                        ticks.Add(t);
                    datetimes.Add(t.datetime);
                    index.Add(idx);
                }
            }

            void tr_gotTick2(Tick t)
            {
                bl.newTick(t);
            }
            internal bool finished;
            internal int count;
            internal bool Cancel;
            internal bool IsBusy;

            internal static void ThreadPoolGo(object info)
            {
                tickreader reader = (tickreader)info;
                reader.IsBusy = true;
                TikReader tr = reader.tr;

                //int i = 0;
                while (!reader.Cancel && tr.NextTick()) ;
                    //if (i++ % 10 == 0) 
                      //  Console.WriteLine("i:" + reader.idx + " c:" + i + " f:"+reader.file) ;
                reader.count = tr.Count;
                reader.finished = !reader.Cancel;
                tr.Close();
                reader.IsBusy = false;
            }

            void tr_gotTick(Tick t)
            {
                if (_saveticks)
                    ticks.Add(t);
                datetimes.Add(t.datetime);
                index.Add(idx);
            }

            internal void CancelAsync() { Cancel = true; }

        }

        internal class ArrayComparer : System.Collections.IComparer
        {
            int ix;
            public ArrayComparer(int SortFieldIndex)
            {
                ix = SortFieldIndex;
            }

            public int Compare(object x, object y)
            {
                IComparable cx = (IComparable)((Array)x).GetValue(ix);
                IComparable cy = (IComparable)((Array)y).GetValue(ix);
                return cx.CompareTo(cy);
            }
        }




    }
}
