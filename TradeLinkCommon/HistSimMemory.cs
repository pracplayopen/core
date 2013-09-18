using System;
using System.Collections.Generic;
using System.Text;
using TradeLink.API;

namespace TradeLink.Common
{
    /// <summary>
    /// loads and plays back simulations in memory for faster playback
    /// </summary>
    public class HistSimMemory : HistSimIndexPlay
    {
        bool _greedy = true;
        /// <summary>
        /// whether simulation is run in memory.
        /// if disabled runs as a histsimindexplay
        /// </summary>
        public bool inMemory { get { return _greedy; } set { _greedy = value; } }
        public HistSimMemory(string folder) : this(folder, 0) { }
        public HistSimMemory(string folder,int interval)
            : base(folder,interval)
        {
        }

        public HistSimMemory(HistSimIndex hsi)
            : base(hsi,null)
        {
        }
        public HistSimMemory(HistSimIndex hsi,DebugDelegate debug)
            : base(hsi,debug)
        {
        }


        public HistSimMemory(string folder, TickFileFilter tff)
            : base(folder, tff,0, null)
        { }

        public HistSimMemory(string folder, TickFileFilter tff,DebugDelegate deb)
            : base(folder, tff,0,deb)
        {
        }

        public HistSimMemory(string[] set,string folder) : this(set, folder,0,null) { }
        public HistSimMemory(string[] set, string folder,DebugDelegate deb) : this(set, folder, 0, deb) { }
        public HistSimMemory(string[] set, string folder, int interval,DebugDelegate deb)
            : base(set,folder,interval,deb)
        {
        }

        
        int _ntc = 0;
        public int NullTickCount { get { return _ntc; } set { _ntc = value; } }
        protected override void gotnewtick(Tick k)
        {

                // save in memory
                if (_greedy)
                    _mem.Add(k);
                // notify user
                base.gotnewtick(k);

        }
        private List<Tick> _mem;
        public override void Initialize()
        {
            


            // size tick array
            if (_greedy && (_mem == null))
            {
                _mem = new List<Tick>(hsip_avail);
                hsipinited = true;
                _pc = 0;
            }
            else if (!_greedy)
                base.Initialize();
            
            
        }

        int _pc = 0;
        long _lastplayto = MultiSimImpl.STARTSIM;

        public override void PlayTo(long datetime)
        {
            if (!hsipinited)
                Initialize();
            // see if sim has to be restarted
            if (_lastplayto == MultiSimImpl.ENDSIM)
                _lastplayto = MultiSimImpl.STARTSIM;
            // test whether we've already played this sim
            if (_greedy && (datetime <= _lastplayto))
            {
                // if so, play from memory
                debug("Playing memory containing " + _mem.Count.ToString("N0") + " ticks.");
                // should have in memory
                int end = _mem.Count-1;
                stopwatch();
                while (_pc<_mem.Count)
                {
                    Tick k = _mem[_pc++];
                    base.gotnewtick(k);
                    if (_pc >= end)
                    {
                        break;
                    }

                    hsip_nexttime = _mem[_pc].datetime;
                    if (datetime < hsip_nexttime)
                        break;
                    
                }
                stopwatch();
            }
            else // play from disk
            {
                base.PlayTo(datetime);
                _lastplayto = datetime;
            }
        }

        public override void Reset() { Reset(false); }
        public  void Reset(bool reloaddata)
        {
            base.Reset();

            if (_mem.Count > 0)
            {
                if (_pc >= _mem.Count)
                {
                    _pc = 0;
                    hsip_nexttime = MultiSimImpl.STARTSIM;
                }
                else
                    hsip_nexttime = _mem[_pc].datetime;
            }
                
            else if (myhsi.Playtimes.Length > 0)
                hsip_nexttime = myhsi.Playtimes[0];
            else
                hsip_nexttime = MultiSimImpl.STARTSIM;
            if (reloaddata)
            {
                _pc = 0;
                _lastplayto = 0;

                _mem = new List<Tick>(hsip_avail);
            }
        }
    }
}
