using System;
using System.Collections.Generic;
using TradeLink.Common;
using TradeLink.API;
using TradeLink.AppKit;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.ComponentModel;

namespace WinGauntlet
{
    
    public class Optimize : GauntletEngine
    {
        public string Dll = string.Empty;
        public string ResponseName = string.Empty;
        public Optimize() : base(new InvalidResponse(), new SingleSimImpl()) { }
        public Optimize(string dll, string response, HistSim sim) : base(ResponseLoader.FromDLL(response,dll), sim) 
        {
            Dll = dll;
            ResponseName = response;
        }
        public string OptimizeName = string.Empty;
        public decimal StartAt = 0;
        public decimal StopAt = 0;
        public decimal Advance = 1;
        private decimal Current = 0;
        private int id = -1;
        private bool countup = true;
        public bool isNextAvail { get { return countup ? Current + Advance < StopAt : Current + Advance > StopAt; } }
        public int OptimizeRemain { get { return OptimizeCount - id - 1; } }
        public int OptimizeCount { get { return (int)(Math.Abs(StartAt - StopAt) / Advance); } }
        public decimal NextParam { get 
        { 
            if (id<0) 
            { 
                Current = StartAt; 
                if (StartAt>StopAt)
                {
                    countup = false;
                    Advance = Math.Abs(Advance)*-1;
                }
                else
                    Advance = Math.Abs(Advance);
            } 
            else
                Current += Advance;
            id++;
            return Current;
        }
        }
        public string OptimizeDecisionsName = string.Empty;
        public bool isHigherDecisionValuesBetter = true;
        public bool isValid { get { return !string.IsNullOrWhiteSpace(OptimizeName) && !string.IsNullOrWhiteSpace(OptimizeDecisionsName)
            && !string.IsNullOrWhiteSpace(Dll) && !string.IsNullOrWhiteSpace(ResponseName) 
            && (StartAt != StopAt) && (Advance < (Math.Abs(StartAt - StopAt))); } }

        
        void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);
        }
        void status(string msg)
        {
            if (SendStatusEvent != null)
                SendStatusEvent(msg);
        }

        public event DebugDelegate SendDebugEvent;
        public event DebugDelegate SendStatusEvent;

        string DLL = string.Empty;
        string RNAME = string.Empty;
        HistSim SIM = null;

        public bool Start()
        {
            //ensure everything is ready to start the optimization
            if (!isValid)
            {
                debug("Invalid optimization, Must configure this optimization completely before re-starting.");
                status("Optimization not configured.");
                return false;
            }
            SIM = myhistsim;
            DLL = Dll;
            RNAME = ResponseName;

            debug("Starting optimization, Queueing up all "+this.OptimizeCount+" combinations...");

            //queue up all the possibilities
            /*
            List<List<string[]>> paramListList = new List<List<string[]>>();
            foreach (OptimizationParam op in varListControl.Items)
            {
                List<string[]> paramList = new List<string[]>();
                string testClass = (String)reslist.SelectedItem;
                string testName = op.name.ToString();
                decimal val = decimal.MinValue;
                decimal max = decimal.MaxValue;
                while (val < max)
                {
                    if (val == decimal.MinValue) { val = op.low; max = op.high; }
                    else val += op.step;
                    paramList.Add(new string[] { testClass, testName, val.ToString() });
                }
                paramListList.Add(paramList);
            }

            //now we have all the possibilities in a list
            //we need to combine them into all possible combos

            //keep all possible combinations in a list
            //each string[] is a parameter value
            //each List<string[]> is a parameter set
            List<List<string[]>> comboList = new List<List<string[]>>();

            foreach (List<string[]> paramList in paramListList)
            {
                comboList = appendList(comboList, paramList);
            }*/

            debug("All combinations queued, Starting Gauntlet Threads");
            

            var rh = RunHelper.run(runopt, null, debug, "runopt: " + this.ToString());
            status("Optimizaton started with " + OptimizeCount + " combinations.");
            return rh.isStarted;
        }

        public bool isRunning = false;

        void runopt()
        {
            isRunning = true;
            while (isNextAvail)
            {
                gauntletArgs ga = new gauntletArgs(DLL, RNAME, SIM);
                ga.value = NextParam;
                ga.id = id;
                Response r = ResponseLoader.FromDLL(ga.response, ga.dll);
                if (changeVars(ref r, ga.value))
                {
                    //GauntletEngine ge = new GauntletEngine(r, ga.hsim);
                    //Thread.SpinWait(1000);
                    //debugControl1.GotDebug("Starting a Gauntlet instance");
                    debug("Started Gauntlet Engine: " + ga.id);
                    Go();

                    List<Trade> trades = SimBroker.GetTradeList();
                    Results results = Results.ResultsFromTradeList(trades, 0, .01m, new DebugDelegate(debug));

                    GauntletOptimizationResults gor = new GauntletOptimizationResults();
                    gor.parameter = ga.value;
                    gor.results = results;
                    gor.resultsCalculated = true;
                    allResults.Add(gor);
                    decimal rv = 0;
                    getresult(results, out rv);
                    debug("optimize "+ga.id+" finished.  Used: "+gor.parameter+" -> "+OptimizeDecisionsName+": "+rv+" trades: " + gor.results.Trades + " SimsRemaining: " + OptimizeRemain);
                    progress(id, OptimizeCount);
                }
                else
                    debug("Unable to start gauntlet engine: " + ga.id + " with: " + ga.value);

            }
            debug("All optimization runs complete, computing optimum...");
            var sorted = calculateBest(allResults);
            // unbind
            UnbindEvents();
            if (SendOptimizationCompleteEvent != null)
                SendOptimizationCompleteEvent(sorted);
            isRunning = false;

        }

        List<Result> gor2results(List<GauntletOptimizationResults> gors)
        {
            List<Result> f = new List<Result>(gors.Count);
            foreach (var gor in gors)
            {
                var res = gor.results;
                res.SimParameters = gor.parameter.ToString();
                f.Add(gor.results);
            }
            return f;
        }

        public event ResultListDel SendOptimizationCompleteEvent;

        public event IntDelegate SendOptimizationProgressEvent;

        int lastpct = -1;
        void progress(int cur, int max)
        {
            int pct = (int)(((double)cur*100) / max);
            progress(pct);
        }
        void progress(int pct)
        {
            if (pct <= lastpct)
                return;
            if (pct > 100)
                pct = 100;
            lastpct = pct;
            if (SendOptimizationProgressEvent != null)
                SendOptimizationProgressEvent(pct);
        }

        List<GauntletOptimizationResults> allResults = new List<GauntletOptimizationResults>();


        PropertyInfo resultproperty = null;


        bool getresult(Result r, out decimal result)
        {
            result = 0;
            if (resultproperty == null)
            {
                foreach (var pr in r.GetType().GetProperties())
                {
                    if (pr.Name == OptimizeDecisionsName)
                    {
                        resultproperty = pr;
                        break;
                    }
                }
            }
            try
            {
                var o = resultproperty.GetValue(r, null);
                result = (decimal)o;
                return true;
            }
            catch (Exception ex)
            {
                debug("error getting result from OptimizeDecision name: " + OptimizeDecisionsName + " err: " + ex.Message + ex.StackTrace);
            }
            return false;

        }



        List<Result> calculateBest(List<GauntletOptimizationResults> resultSet)
        {
            var results = gor2results(resultSet);
            var decides = new List<decimal>(results.Count);
            foreach (var res in results)
            {
                decimal v;
                if (getresult(res, out v))
                {
                    decides.Add(v);
                }
            }
            var rtmp = results.ToArray();
            var dtmp = decides.ToArray();
            Array.Sort(dtmp, rtmp);
            if (isHigherDecisionValuesBetter)
                Array.Reverse(rtmp);
            if (rtmp.Length==0)
                debug("There were no results calculated from any run.");
            else
            {
                // get result
                decimal bestresult;
                var best = rtmp[0];
                if (getresult(best, out bestresult))
                {
                    debug(OptimizeDecisionsName + " optimal at: " + bestresult.ToString() + " with: " + OptimizeName + " = " + best.SimParameters);
                    status(OptimizeDecisionsName + " optimal at: " + bestresult.ToString() + " with: " + OptimizeName + " = " + best.SimParameters);
                }
            }
            return new List<Result>(rtmp);
        }




        bool changeVars(ref Response r, decimal v)
        {
            try
            {
                var t = r.GetType();
                foreach (var pi in t.GetProperties())
                    if (pi.Name == OptimizeName)
                    {
                        if (pi.PropertyType == typeof(int))
                        {
                            pi.SetValue(r, (int)v, null);
                        }
                        else
                            pi.SetValue(r, v, null);
                        return true;
                    }
            }
            catch (Exception ex)
            {
                debug("error setting parameter " + OptimizeName + " on response: " + r.FullName + " to: " + v+" err: "+ex.Message+ex.StackTrace);
            }
            return false;
        }


 


        public static object DeepClone(object obj)
        {
            object objResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);

                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }

        public static bool isNameInt(string dll, string rn, string name)
        {
            // load repsonse
            var r = ResponseLoader.FromDLL(rn, dll);
            // ensure valid
            if (!r.isValid)
                return false;
            // get all the public global properties
            foreach (var pi in r.GetType().GetProperties())
            {
                if (pi.Name != name)
                    continue;
                var pt = pi.PropertyType;
                if (pt == typeof(int))
                    return true;
                return false;
            }
            return false;
        }

        public static decimal GetMinAdvance(string dll, string rn, string name)
        {
            
            // load repsonse
            var r = ResponseLoader.FromDLL(rn, dll);
            // ensure valid
            if (!r.isValid)
                return 0;
            // get all the public global properties
            foreach (var pi in r.GetType().GetProperties())
            {
                if (pi.Name != name)
                    continue;
                var pt = pi.PropertyType;
                if (pt == typeof(int))
                    return 1;
                else return .01m;
            }
            return 0;
        }

        public static List<string> GetDecideable()
        {
            Results r = new Results();
            List<string> dr = new List<string>();
            foreach (var pi in r.GetType().GetProperties())
            {
                if (!pi.GetAccessors()[0].IsPublic)
                    continue;
                var pt = pi.PropertyType;
                if ((pt == typeof(decimal)) || (pt == typeof(int)))
                    dr.Add(pi.Name);
            }
            foreach (var pi in r.GetType().GetFields())
            {
                if (!pi.IsPublic)
                    continue;
                var pt = pi.FieldType;
                if ((pt == typeof(decimal)) || (pt == typeof(int)))
                    dr.Add(pi.Name);
            }
            var tmp = dr.ToArray();
            Array.Sort(tmp);
            return new List<string>(tmp);
        }

        public static List<string> GetOptimizeable(string dll, string rn)
        {
            List<string> op = new List<string>();
            // load repsonse
            var r = ResponseLoader.FromDLL(rn, dll);
            // ensure valid
            if (!r.isValid)
                return op;
            // get all the public global properties
            foreach (var pi in r.GetType().GetProperties())
            {
                var pt = pi.PropertyType;
                if ((pt == typeof(int)) || (pt == typeof(decimal)))
                {
                    if (pi.CanWrite && pi.GetAccessors(false)[0].IsPublic)
                    {
                        op.Add(pi.Name);
                    }
                }
            }
            var tmp = op.ToArray();
            Array.Sort(tmp);
            return new List<string>(tmp);

        }
    }

    public class GauntletOptimizationResults
    {
        public GauntletOptimizationResults() { }
        public long id = 0;
        public Results results;
        public bool resultsCalculated = false;
        public decimal parameter = 0;
    }

    public class gauntletArgs
    {
        public gauntletArgs(string _dll, string _response, HistSim h)
        {
            dll = _dll;
            response = _response;
            hsim = h;
        }

        public string dll = "";
        public string response = "";
        public HistSim hsim;
        public decimal value = 0;
        public int id = 0;
        
        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", dll, response,id,value);
        }
    }   
}
