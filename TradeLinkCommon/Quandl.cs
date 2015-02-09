using System;
using System.Collections.Generic;
using TradeLink.API;
using TradeLink.Common;
using QuandlCS.Requests;
using QuandlCS.Types;
using QuandlCS.Connection;
using QuandlCS.Interfaces;
using System.Xml.Serialization;
using System.Xml;


namespace TradeLink.Common
{
    public delegate RootObject GetQdlRootEvent(string name);
    public class Quandl
    {
        // also see: https://github.com/HubertJ/QuandlCS/blob/master/README.md
        public const string PROGRAM = "Quandl";

        private const string pp_apik = "";

        /// <summary>
        /// set the quandl api key
        /// </summary>
        public static string APIK = pp_apik;

        public const string DATASOURCE_STOCKPRICE_NA = "WIKI";
        public const string DATASOURCE_SEC_USA = "SEC";
        public const string DATASOURCE_SEC_HARMONIZED = "RAYMOND";
        public const string DATASOURCE_FUNDAMENTAL_NYU = "DMDRN";
        public const string DATASOURCE_ZACKS_EARNEST = "ZEE";
        public const string DATASOURCE_ZACKS_EARNSUP = "ZES";

        public const string DATASOURCE_FREQ_QTR = "Q";
        public const string DATASOURCE_FREQ_YEAR = "A";


        static string GetQdlFreq(qdl_freq freq)
        {
            switch (freq)
            {
                case qdl_freq.None:
                    return string.Empty;
                case qdl_freq.Annual:
                    return DATASOURCE_FREQ_YEAR;
                case qdl_freq.Quarterly:
                    return DATASOURCE_FREQ_QTR;
                default:
                    throw new Exception("Data set frequency: " + freq + " not yet implemented.");
            }

        }


        public static string BuildQdlCol(params string[] cols)
        {
            return string.Join("_", cols);
        }
        /// <summary>
        /// gets quandl datacode given source and column/symbol
        /// </summary>
        /// <param name="source"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static Datacode BuildQdlCode(string source, params string[] columns)
        {
            if (columns.Length==1)
                return new Datacode(source,columns[0]);
            return new Datacode(source, BuildQdlCol(columns));

        }

        /// <summary>
        /// gets quandl datacode given source and symbol
        /// </summary>
        /// <param name="source"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static Datacode BuildQdlCode(qdl_ds source, qdl_freq freq,  params string[] datasetids)
        {
            if (freq== qdl_freq.None)
                return BuildQdlCode(GetQdlDataCode(source),datasetids);
            List<string> ids = new List<string>(datasetids);
            ids.Add(GetQdlFreq(freq));
            return BuildQdlCode(GetQdlDataCode(source), ids.ToArray());
        }


        /// <summary>
        /// get string-ified version of qdl data set code
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static string GetQdlDataCode(qdl_ds source)
        {
            var code = string.Empty;
            switch (source)
            {
                case  qdl_ds.SecHarm:
                    code = DATASOURCE_SEC_HARMONIZED;
                    break;
                case qdl_ds.SECRaw:
                    code = DATASOURCE_SEC_USA;
                    break;
                case qdl_ds.StockPriceNA:
                    code = DATASOURCE_STOCKPRICE_NA;
                    break;
                case qdl_ds.ZacksEarnings:
                    code = DATASOURCE_ZACKS_EARNEST;
                    break;
                case qdl_ds.ZacksSurprises:
                    code = DATASOURCE_ZACKS_EARNSUP;
                    break;
                case qdl_ds.NYUProfessorDamodaran:
                    code = DATASOURCE_FUNDAMENTAL_NYU;
                    break;
                default: // should never happen
                    throw new NotImplementedException("Unknown quandl data source: " + source.ToString());
            }
            return code;
        }


        /// <summary>
        /// get a quandl data set for particular symbol/column
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static RootObject Get(qdl_ds ds, string symbol)
        {
            return Get(ds, symbol, string.Empty, null);
        }

        /// <summary>
        /// gets a quandl data object directly from url
        /// </summary>
        /// <param name="qurl"></param>
        /// <returns></returns>
        public static RootObject Get(string qurl) { return Get(qurl, isCheckCache, null); }
        /// <summary>
        /// gets a quandl data object directly from url
        /// </summary>
        /// <param name="qurl"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RootObject Get(string qurl, DebugDelegate d) { return Get(qurl, isCheckCache, d); }
        /// <summary>
        /// gets a quandl data object directly from url
        /// </summary>
        /// <param name="qurl"></param>
        /// <param name="TryCache"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RootObject Get(string qurl, bool TryCache, DebugDelegate d)
        {
            if (_d == null)
                _d = d;
            var path = BarListImpl.GetDBLoc(PROGRAM, qurl.GetHashCode().ToString(), 7, DateTime.Now, getformatext(DefaultQuadlFormat));
            if (TryCache)
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        var raw = Util.getfile(path, d);
                        var dataok = !string.IsNullOrWhiteSpace(raw) && (raw.Length > 0);
                        if (dataok)
                        {
                            var data = isCacheCompressed ? GZip.Uncompress(raw) : raw;
                            return json.Deserialize2Root(data, false, d);
                        }
                    }
                    catch (Exception ex)
                    {
                        debug("Will ignore cache and repull as error reading cache for: " + qurl + " , err: " + ex.Message + ex.StackTrace);

                    }
                }
            }
            // pull data
            var urldata = Util.geturl(qurl, d);
            var isurldataok = !string.IsNullOrWhiteSpace(urldata) && (urldata.Length > 0);
            if (isurldataok)
            {
                var compdata = isCacheCompressed ? GZip.Compress(urldata) : urldata;
                if (Util.setfile(path, compdata))
                {
                    v("Cached " + urldata.Length.ToString("N0") + " bytes of qurl: " + qurl);
                }
                else
                {
                    v("Error caching: " + qurl + ", will repull next time.");
                }
            }

            return json.Deserialize2Root(urldata, false, d);

        }

        /// <summary>
        /// get a quandl data set for particular symbol/column
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="symbol"></param>
        /// <param name="additionaldataset"></param>
        /// <returns></returns>
        public static RootObject Get(qdl_ds ds, string symbol, string additionaldataset)
        {
            return Get(ds, symbol,additionaldataset,isCheckCache, null);
        }

        /// <summary>
        /// get a quandl data set for particular symbol/column
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="symbol"></param>
        /// <param name="additionaldataset"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RootObject Get(qdl_ds ds, string symbol, string additionaldataset, DebugDelegate d)
        {
            return Get(ds, symbol, additionaldataset, isCheckCache, d);
        }

        /// <summary>
        /// get a quandl data set for particular symbol/column
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="symbol"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RootObject Get(qdl_ds ds, string symbol, DebugDelegate d)
        {
            return Get(ds, symbol, isCheckCache, d);
        }



        /// <summary>
        /// get a quandl data set for particular symbol/column
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="symbol"></param>
        /// <param name="trycache"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RootObject Get(qdl_ds ds, string symbol, bool trycache, DebugDelegate d)
        {
            return Get(ds, symbol, string.Empty, trycache, d);
        }

        /// <summary>
        /// get a quandl data set for particular symbol/column (no frequency)
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="symbol"></param>
        /// <param name="additionaldataset"></param>
        /// <param name="trycache"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RootObject Get(qdl_ds ds, string symbol, string additionaldataset, bool trycache, DebugDelegate d)
        {
            return Get(ds, symbol, additionaldataset,qdl_freq.None,  trycache, d);
        }

        /// <summary>
        /// get a quandl data set for particular symbol/column/frequency
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="symbol"></param>
        /// <param name="trycache"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RootObject Get(qdl_ds ds, string symbol, string additionaldataset, qdl_freq freq, bool trycache, DebugDelegate d)
        {
            if (_d==null)
                _d = d;
            Datacode dc = null;
            string raw = string.Empty;
            RootObject ro = null;
            try
            {
                if (string.IsNullOrWhiteSpace(additionaldataset))
                    dc = BuildQdlCode(ds,freq, symbol);
                else
                    dc = BuildQdlCode(ds, freq, symbol, additionaldataset);
            }
            catch (Exception ex)
            {
                debug("Unable to get data, error building dataset: " + qh.dc2str(dc) + " " + symbol + " err: " + ex.Message + ex.StackTrace);
                return default(RootObject);
            }
            try
            {
                raw = GetData(symbol, dc, trycache, d);
                // verify data retrieved before continuing
                if (string.IsNullOrWhiteSpace(raw))
                {
                    if (isUseFakeDataOnError)
                    {
                        ro = GetRandomData(dc, freq);
                        debug("FakeDataOnError enabled, using: " + ro.data.Count + " randomized data set.");
                        return ro;
                    }
                    else
                    {
                        debug(symbol + " " + qh.dc2str(dc) + " no data was retrieved.  Retry or contact support@pracplay.com");
                        return default(RootObject);
                    }
                }
            }
            catch (Exception ex)
            {
                debug("Unable to get data, error retrieving set: " + qh.dc2str(dc) + " " + symbol + " err: " + ex.Message + ex.StackTrace);
                return default(RootObject);
            }
            try
            {

                ro = json.Deserialize2Root(raw, false, d);
                v(symbol + " " + qh.dc2str(dc) + " got data model containing " + ro.column_names.Count.ToString("N0") + " columns and " + ro.data.Count.ToString("N0") + " records.");
            }
            catch (Exception ex)
            {

                if (isVerboseDebugging)
                {
                    debug("Unable to get data, error parsing set: " + ds + " " + symbol + " err: " + ex.Message + ex.StackTrace);
                    debug(symbol + " " + qh.dc2str(dc) + " errored data set: " + raw);
                }
                else
                    debug("Unable to get data, error parsing set: " + qh.dc2str(dc) + " " + symbol + " err: " + ex.Message + ex.StackTrace);

                return default(RootObject);

            }
            return ro;
        }

#if DEBUG
        public static bool isUseFakeDataOnError = false;
#else
        public static bool isUseFakeDataOnError = false;
#endif


        public static RootObject GetRandomData(Datacode dc) { return GetRandomData(dc, qdl_freq.Quarterly); }
        public static RootObject GetRandomData() { return GetRandomData(qdl_freq.Quarterly); }
        public static RootObject GetRandomData(Datacode dc, qdl_freq freq) { return GetRandomData(dc, freq == qdl_freq.Quarterly ? 4 * 5 : 5); }
        public static RootObject GetRandomData(qdl_freq freq ) { return GetRandomData(null, freq== qdl_freq.Quarterly ? 4*5 : 5); }
        public static RootObject GetRandomData(int numrecords) { return GetRandomData(null, numrecords); }
        public static RootObject GetRandomData(Datacode dc, int numrecords) 
        {
            System.Random rnd = new Random();
            var basev = rnd.Next(1, 100);
            return GetRandomData(dc, numrecords, basev, 1, DateTime.Now); 
        }
        public static RootObject GetRandomData(Datacode dc, int numrecords, decimal basevalue) { return GetRandomData(dc, numrecords, basevalue, 1, DateTime.Now); }
        public static RootObject GetRandomData(Datacode dc, int numrecords, decimal basevalue, decimal mult) { return GetRandomData(dc, numrecords, basevalue, mult, DateTime.Now); }
        public static RootObject GetRandomData(Datacode dc, int numrecords, decimal basevalue, decimal mult, DateTime startdate)
        {
            RootObject ro = new RootObject();
            if (dc!=null)
                ro.source_code = dc.ToDatacodeString('.');
            
            System.Random rnd = new Random();
            ro.id = rnd.Next(1, 1000000);
            ro.isCacheable = false;
            ro.column_names = new List<string>();
            ro.column_names.Add(qh.DefaultDateColumnName);
            ro.column_names.Add(qh.DefaultValueColumnName);
            ro.data = new List<List<object>>();
            for (int i = 0; i < numrecords; i++)
            {
                var val = ((decimal)rnd.NextDouble() * mult) + basevalue;
                var date = new DateTime(startdate.Ticks);
                ro.data.Add(new List<object>(new object[] {  date,val }));
                // next date
                startdate = startdate.AddDays(1);
            }
            return ro;
        }


        public static FileFormats DefaultQuadlFormat = FileFormats.JSON;

        public static bool isCheckCache = true;
        public static bool isCacheCompressed = true;

        static string GetData(string sym, Datacode dc, DebugDelegate d) { return GetData(sym, dc, isCheckCache, DefaultQuadlFormat,d); }
        static string GetData(string sym, Datacode dc, bool checkcache, DebugDelegate d) { return GetData(sym, dc, checkcache, DefaultQuadlFormat, d); }
        static string GetData(string sym, Datacode dc, bool checkcache, FileFormats form, DebugDelegate d)
        {
            return GetData(CreateRequest(sym, dc, form), sym, qh.dc2str(dc), form, checkcache, isCacheCompressed, d);
        }

        static XmlReader GetXmlData(string data)
        {
            System.IO.StringReader sr = new System.IO.StringReader(data);
            XmlReaderSettings set = new XmlReaderSettings();
            XmlReader read = XmlReader.Create(sr, set);
            return read;
            
        }



        static string  GetData(IQuandlRequest req, string sym, string dataset, FileFormats form, bool checkcache, bool compressedcache, DebugDelegate d)
        {
            if (_d == null)
                _d = d;
            try
            {
                var path = BarListImpl.GetDBLoc(PROGRAM , dataset+"_"+sym, 30, DateTime.Now, getformatext(form));
                if (checkcache)
                {

                    if (System.IO.File.Exists(path))
                    {
                        try
                        {
                            var cached = Util.getfile(path, null);
                            if (compressedcache)
                            {
                                var cacheddata = GZip.Uncompress(cached);
                                v(sym + " " + dataset + " found " + cacheddata.Length.ToString("N0") + " bytes of cached data.");
                                return cacheddata;
                            }
                        }
                        catch (Exception ex)
                        {
                            debug("Ignoring cache (will pull directly) after cache error for: " + dataset + " on " + sym + " err: " + ex.Message + ex.StackTrace);
                        }
                    }
                }
                var con = new QuandlConnection();
                var data = con.Request(req);
                var isdataok = !qh.isQdlUnderMaintence(data) ;
                if (isdataok)
                {
                    v(sym + " " + dataset + " retrieved " + data.Length.ToString("N0") + " bytes of " + dataset + " data.");
                    // save it for later caching
                    if (Util.setfile(path, compressedcache ? GZip.Compress(data) : data))
                    {
                        v(sym + " " + dataset + " cached " + data.Length.ToString("N0") + " bytes of " + dataset + " data.");
                    }
                    else
                        v(sym + " " + dataset + " error caching " + data.Length.ToString("N0") + " bytes of " + dataset + " data.  Will be re-pulled on next attempt.");
                }
                else
                {
                    debug(sym + " " + dataset + " can't be retrieved because quandl is down.");
                    return string.Empty;
                }
            
                return data;
            }
            catch (Exception ex)
            {
                if (isVerboseDebugging)
                {
                    debug("An error occurred getting data for: " + dataset + " on symbol: " + sym + " using url: " + req.ToRequestString());
                    debug("Error for " + dataset + " on symbol: " + sym + ": " + ex.Message + ex.StackTrace);
                }
                else
                    debug("An error occurred getting data for: " + dataset + " on symbol: " + sym + ", err: " + ex.Message + ex.StackTrace);
                return string.Empty;
            }
        }

        static IQuandlRequest CreateRequest(string sym, Datacode dc) { return CreateRequest(sym, dc, DefaultQuadlFormat); }
        static IQuandlRequest CreateRequest(string sym, Datacode dc, FileFormats form)
        {
            QuandlDownloadRequest qdr = new QuandlDownloadRequest();
            qdr.APIKey = APIK;
            qdr.Datacode = dc;
            qdr.Format = form;
            return qdr;
        }


        static string getformatext(FileFormats form)
        {
            switch (form)
            {
                case FileFormats.JSON:
                    return ".json";
                case FileFormats.XML:
                    return ".xml";
                case FileFormats.CSV:
                    return ".csv";
                case FileFormats.HTML:
                    return ".html";
            }
            return ".ext";
        }

#if DEBUG
        public static bool isVerboseDebugging = true;
#else
        public static bool isVerboseDebugging = false;
#endif

        static void v(string msg)
        {
            if (isVerboseDebugging)
                debug(msg);
        }


        static DebugDelegate _d = null;
        static void debug(string msg)
        {
            if (_d != null)
                _d(msg);
        }

        
    }

    public class qdl : Quandl
    {
    }

    public enum qdl_ds
    {
        /// <summary>
        /// no data source
        /// </summary>
        None,
        /// <summary>
        /// north american stock prices
        /// </summary>
        StockPriceNA,
        /// <summary>
        /// sec database raw format
        /// </summary>
        SECRaw,
        /// <summary>
        /// sec database in easily comparable format
        /// </summary>
        SecHarm,
        /// <summary>
        /// zacks equities earnings 
        /// </summary>
        ZacksEarnings,
        /// <summary>
        /// zacks equities earnings estimates and misses
        /// </summary>
        ZacksSurprises,
        /// <summary>
        /// fundamentals and ratios
        /// </summary>
        NYUProfessorDamodaran,
    }


    public enum qdl_freq
    {
        /// <summary>
        /// none or N/A
        /// </summary>
        None,
        Daily,
        Quarterly,
        Annual,
    }
  
}
