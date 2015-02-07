using System;
using System.Collections.Generic;
using TradeLink.API;
using TradeLink.Common;

namespace TradeLink.Common
{
    public class json
    {
        public static string NormalizeJsonForParsing(string inputjson)
        {
            string final = Util.rxr(inputjson, "([a-z_]+):", "\"$1\":");
            return final;
        }

        public static RootObject Deserialize2Root(string jsondata) { return Deserialize2Root(jsondata, true, null); }
        public static RootObject Deserialize2Root(string jsondata, DebugDelegate debs) { return Deserialize2Root(jsondata, true, debs); }
        public static RootObject Deserialize2Root(string jsondata, bool autonormalize, DebugDelegate debs)
        {
            return Deserialize<RootObject>(jsondata, autonormalize, debs);
        }

        public static T Deserialize<T>(string jsondata, DebugDelegate debs) { return Deserialize<T>(jsondata, true, debs); }
        public static T Deserialize<T>(string jsondata, bool autonormalize, DebugDelegate debs)
        {
#if DEBUG
#else
            try
            {
#endif
                //RootObject oc = (RootObject)fastJSON.JSON.Instance.Parse(json);
            if (autonormalize)
                jsondata = NormalizeJsonForParsing(jsondata);
            
            
                T oc = fastJSON.JSON.ToObject<T>(jsondata);
                return oc;
#if DEBUG
#else
            }
            catch (Exception ex)
            {
            if (debs!=null)
                debs("error deserializing: " + jsondata + " err: " + ex.Message + ex.StackTrace);
                return default(T);
            }
#endif
        }
    }
}
