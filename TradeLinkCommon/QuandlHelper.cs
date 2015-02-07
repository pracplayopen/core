using System;
using System.Collections.Generic;
using TradeLink.API;
using TradeLink.Common;
using QuandlCS.Requests;
using QuandlCS.Types;
using QuandlCS.Connection;
using QuandlCS.Interfaces;


namespace TradeLink.Common
{
    public class QuandlHelper
    {


        /// <summary>
        /// converts a quandl object into a gt (w/default date,val columns)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static GenericTracker<decimal> Qdl2GT(RootObject ro, DebugDelegate d)
        {
            return Qdl2GT(ro, DefaultDateColumnName, DefaultValueColumnName, d);
        }


        /// <summary>
        /// converts a quandl object into a gt (w/default date column)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="valcol"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static GenericTracker<decimal> Qdl2GT(RootObject ro, string valcol, DebugDelegate d)
        {
            return Qdl2GT(ro, DefaultDateColumnName, valcol, d);
        }

        /// <summary>
        /// convert quandl object to gt
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="datecol"></param>
        /// <param name="valcol"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static GenericTracker<decimal> Qdl2GT(RootObject ro, string datecol, string valcol, DebugDelegate d)
        {
            return Qdl2GT(ro, datecol, valcol, 0, ro.LastRowIndex, d);
        }

        /// <summary>
        /// takes all or part of quandl object and converts it into GT
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="datecol"></param>
        /// <param name="valcol"></param>
        /// <param name="startrow"></param>
        /// <param name="endrow"></param>
        /// <returns></returns>
        public static GenericTracker<decimal> Qdl2GT(RootObject ro, string datecol, string valcol, int startrow, int endrow, DebugDelegate d)
        {
            // get columns
            var datecolidx = GetQdlColidx(ro,datecol);
            var valcolidx = GetQdlColidx(ro,valcol);
            // slice out the data
            var subset = GetAllRows(ro, startrow, endrow, d);
            // get date column
            var dates = QdlCol2Dates( GetColumn(subset, datecolidx));
            var vals =QdlCol2Vals( GetColumn(subset, valcolidx));
            // populate GT
            GenericTracker<decimal> gt = new GenericTracker<decimal>(dates.Count);
            for (int i = 0; i < dates.Count; i++)
            {
                var val = vals[i];
                var dt = dates[i];
                if (val == qh.ERROR_VALUE)
                    continue;
                if (dt == qh.ERROR_DATE)
                    continue;
                var tldate = Util.ToTLDate(dt);
                gt.addindex(tldate.ToString("F0"), val);
                

            }
            return gt;

            

            
        }

        

        public static List<decimal> QdlCol2Vals(List<object> qdlvals)
        {
            List<decimal> vs = new List<decimal>();
            for (int i = 0; i < qdlvals.Count; i++)
            {
                var qdt = qdlvals[i];
                if ((qdt== null) || string.IsNullOrWhiteSpace(qdt.ToString()))
                    vs.Add(qh.ERROR_VALUE);
                else
                {
                    var dt = qh.ERROR_VALUE;
                    if (decimal.TryParse(qdt.ToString(), out dt))
                        vs.Add(dt);
                    else
                        vs.Add(qh.ERROR_VALUE);

                }
            }


            return vs;
        }

        public static List<DateTime> QdlCol2Dates(List<object> qdldates)
        {
            List<DateTime> dates = new List<DateTime>(qdldates.Count);
            for (int i = 0; i < qdldates.Count; i++)
            {
                var qdt = qdldates[i];
                var dts = qdt.ToString();
                if ((dts == null) || string.IsNullOrWhiteSpace(dts))
                    dates.Add(qh.ERROR_DATE);
                else
                {
                    var dt = qh.ERROR_DATE;
                    if (DateTime.TryParse(dts, out dt))
                        dates.Add(dt);
                    else
                        dates.Add(qh.ERROR_DATE);

                }
            }


            return dates;
        }

        /// <summary>
        /// gets pretty quandl code
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static string dc2str(Datacode dc) { return DataCode2String(dc); }
        /// <summary>
        /// gets pretty quandl code
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static string DataCode2String(Datacode dc)
        {
            return dc.ToDatacodeString('.');
        }

        /// <summary>
        /// tests whether quandl is under maintence
        /// </summary>
        /// <returns></returns>
        public static bool isQdlUnderMaintence() { return isQdlUnderMaintence((DebugDelegate)null); }
        /// <summary>
        /// tests whether quandl is under maintence
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool isQdlUnderMaintence(DebugDelegate d)
        {
            var data = Util.geturl("https://www.quandl.com/", d);
            return isQdlUnderMaintence(data);

        }

        /// <summary>
        /// tests whether quandl data indicates service is under maintence or down
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool isQdlUnderMaintence(string data)
        {
            if (string.IsNullOrWhiteSpace(data) || data.ToLower().Contains("quandl is under maintenance"))
                return true;
            return false;

        }


        /// <summary>
        /// tests whether quandl service is up
        /// </summary>
        /// <returns></returns>
        public static bool isQdlServiceOk() { return isQdlServiceOk(null); }
        /// <summary>
        /// tests whether quandl service is up
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool isQdlServiceOk(DebugDelegate d)
        {
            return !isQdlUnderMaintence(d);
        }

        /// <summary>
        /// tests whether a quandl result set is ok (not null and has data present)
        /// </summary>
        /// <param name="ros"></param>
        /// <returns></returns>
        public static bool isQdlSetOk(params RootObject[] ros) { return isQdlSetOk((DebugDelegate)null, ros); }


        

        /// <summary>
        /// tests whether a quandl result set is ok (not null and has data present)
        /// </summary>
        /// <param name="ro"></param>
        /// <returns></returns>
        public static bool isQdlSetOk(DebugDelegate d, params RootObject[] ros)
        {
            if (_d==null)
                _d = d;
            var ok = true;
            for (int i = 0; i < ros.Length; i++)
            {
                var ro = ros[i];
                if (ro == null)
                    return false;
                var thisrootok = (ro.column_names.Count > 0) && (ro.data.Count > 0);
                if (!thisrootok)
                    debug(ro.code + " data set not ready.");
                ok &= thisrootok;
            }
            return ok;
        }

        /// <summary>
        /// gets a column index from name (case insensitive)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public static int GetQdlColidx(RootObject ro, string colname)
        {
            return GetQdlColidx(ro, colname, false);
        }

        /// <summary>
        /// gets a column index from name (optional case sensitive)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public static int GetQdlColidx(RootObject ro, string colname, bool casesensitive)
        {
            var colnames = casesensitive ? ro.column_names : colnames_insensitive(ro);
            var compare = casesensitive ? colname : colname.ToLower();
            return colnames.IndexOf(compare);

        }

        static List<string> colnames_insensitive(RootObject ro)
        {
            List<string> cnl = new List<string>();
            for (int i = 0; i < ro.column_names.Count; i++)
                cnl.Add(ro.column_names[i].ToLower());
            return cnl;
        }

        public const string DefaultDateColumnName = "Date";
        public const string DefaultValueColumnName = "Value";

        /// <summary>
        /// gets data index of a specific date in default column
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex(RootObject ro, DateTime dt, DebugDelegate d)
        {
            return GetQdlDataIndex(ro, dt, DefaultDateColumnName, d);
        }
        public static int GetQdlDataIndex(RootObject ro, DateTime dt, string datecol, DebugDelegate d)
        {
            var colidx = GetQdlColidx(ro, datecol);
            return GetQdlDataIndex(ro, dt, colidx, d);
        }
        /// <summary>
        /// gets date for a given date (and date column index)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="colidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex(RootObject ro, DateTime dt, int colidx, DebugDelegate d)
        {
            if (_d == null)
                _d = d;
            var date = Util.ToTLDate(dt);
            if ((colidx < 0) || (colidx >= ro.column_names.Count))
            {
                debug(ro.id + " can't get column data index for date: " + date + " because column id is invalid: " + colidx);
                return -1;
            }
            for (int i = 0; i < ro.data.Count; i++)
            {
                var thisdate= DateTime.MinValue;
                var os = ro.data[i][colidx].ToString();
                if (DateTime.TryParse(os, out thisdate))
                {
                    var tldate = Util.ToTLDate(thisdate);
                    if (tldate == date)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }


        /// <summary>
        /// gets first index after a certain date
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex_After(RootObject ro, DateTime dt, DebugDelegate d)
        {
            return GetQdlDataIndex_After(ro, dt, DefaultDateColumnName, d);
        }
        /// <summary>
        /// gets first index after a certain date
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="datecol"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex_After(RootObject ro, DateTime dt, string datecol, DebugDelegate d)
        {
            var colidx = GetQdlColidx(ro, datecol);
            return GetQdlDataIndex_After(ro, dt, colidx, d);
        }

        /// <summary>
        /// gets first index after a certain date
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="colidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex_After(RootObject ro, DateTime dt, int colidx, DebugDelegate d)
        {
            if (_d == null)
                _d = d;
            var date = Util.ToTLDate(dt);
            if ((colidx < 0) || (colidx >= ro.column_names.Count))
            {
                debug(ro.id + " can't get column data index for date: " + date + " because column id is invalid: " + colidx);
                return -1;
            }
            for (int i = 0; i < ro.data.Count; i++)
            {
                var thisdate = DateTime.MinValue;
                var os = ro.data[i][colidx].ToString();
                if (DateTime.TryParse(os, out thisdate))
                {
                    var tldate = Util.ToTLDate(thisdate);
                    if (tldate > date)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }


        /// <summary>
        /// gets first index before a certain date
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex_Before(RootObject ro, DateTime dt, DebugDelegate d)
        {
            return GetQdlDataIndex_Before(ro, dt, DefaultDateColumnName, d);
        }
        /// <summary>
        /// gets first index before a certain date
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="datecol"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex_Before(RootObject ro, DateTime dt, string datecol, DebugDelegate d)
        {
            var colidx = GetQdlColidx(ro, datecol);
            return GetQdlDataIndex_Before(ro, dt, colidx, d);
        }

        /// <summary>
        /// gets index immediately preceeding certain date
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="colidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetQdlDataIndex_Before(RootObject ro, DateTime dt, int colidx, DebugDelegate d)
        {
            if (_d == null)
                _d = d;
            var date = Util.ToTLDate(dt);
            if ((colidx < 0) || (colidx >= ro.column_names.Count))
            {
                debug(ro.id + " can't get column data index for date: " + date + " because column id is invalid: " + colidx);
                return -1;
            }
            for (int i = ro.LastRowIndex; i >= 0; i--)
            {
                var thisdate = DateTime.MinValue;
                var os = ro.data[i][colidx].ToString();
                if (DateTime.TryParse(os, out thisdate))
                {
                    var tldate = Util.ToTLDate(thisdate);
                    if (tldate < date)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// gets quandl date from a given row (using default column date name)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static DateTime GetQdlDate(RootObject ro, int dataidx, DebugDelegate d)
        {
            return GetQdlDate(ro, DefaultDateColumnName, dataidx, d);
        }

        /// <summary>
        /// gets a quandl date given row and column information
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="datacolname"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static DateTime GetQdlDate(RootObject ro, string datacolname, int dataidx, DebugDelegate d)
        {
            var tmp = GetQdlData(ro, datacolname, dataidx, d);
            if (tmp == null)
                return ERROR_DATE;
            var dt = ERROR_DATE;
            if (DateTime.TryParse(tmp.ToString(), out dt))
                return dt;
            return ERROR_DATE;

        }

        

        /// <summary>
        /// convert raw quandl data to date
        /// </summary>
        /// <param name="dateval"></param>
        /// <returns></returns>
        public static DateTime GetQdlDate(object dateval)
        {
            var tmp = dateval;
            DateTime dt = ERROR_DATE;
            if (DateTime.TryParse(tmp.ToString(), out dt))
                return dt;
            return ERROR_DATE;
        }

        /// <summary>
        /// convert raw quandl data to value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal GetQdlValue(object val)
        {
            decimal v = ERROR_VALUE;
            if (decimal.TryParse(val.ToString(), out v))
                return v;
            return ERROR_VALUE;
        }



        /// <summary>
        /// gets quandl date from partial data set
        /// </summary>
        /// <param name="data"></param>
        /// <param name="colidx"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static DateTime GetQdlDate(List<List<object>> data, int colidx, int dataidx, DebugDelegate d)
        {
            if (!isValidQdlColIdx(colidx, data))
                return ERROR_DATE;
            if (!isValidQdlRowIdx(dataidx, data))
                return ERROR_DATE;
            var tmp = data[dataidx][colidx];
            if (tmp == null)
                return ERROR_DATE;
            var dt = ERROR_DATE;
            if (DateTime.TryParse(tmp.ToString(), out dt))
                return dt;
            return ERROR_DATE;

        }




        /// <summary>
        /// returns whether a given column index is valid or not
        /// </summary>
        /// <param name="colidx"></param>
        /// <param name="ro"></param>
        /// <returns></returns>
        public static bool isValidQdlColIdx(int colidx, RootObject ro)
        {
            return isValidQdlColIdx(colidx, ro.data);
        }

        /// <summary>
        /// returns whether a given column index is valid or not
        /// </summary>
        /// <param name="colidx"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool isValidQdlColIdx(int colidx, List<List<object>> data)
        {
            if (colidx < 0)
                return false;
            if (data.Count == 0)
                return false;
            var row = data[0];
            return colidx < row.Count;
        }

        /// <summary>
        /// returns whether a given row index is valid or not
        /// </summary>
        /// <param name="rowidx"></param>
        /// <param name="ro"></param>
        /// <returns></returns>
        public static bool isValidQdlRowIdx(int rowidx, RootObject ro)
        {
            return isValidQdlRowIdx(rowidx, ro.data);
        }

        /// <summary>
        /// returns whether a given row index is valid or not
        /// </summary>
        /// <param name="rowidx"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool isValidQdlRowIdx(int rowidx, List<List<object>> data)
        {
            if (rowidx< 0)
                return false;
            if (data.Count == 0)
                return false;
            return rowidx < data.Count;
        }

        /// <summary>
        /// gets quandl value for given date (default date and value column names)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static decimal GetQdlValue(RootObject ro, DateTime dt, DebugDelegate d)
        {
            return GetQdlValue(ro, dt, DefaultDateColumnName, DefaultValueColumnName, d);
        }

        /// <summary>
        /// gets quandl value for given date (default date column)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dt"></param>
        /// <param name="valcol"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static decimal GetQdlValue(RootObject ro, DateTime dt, string valcol, DebugDelegate d)
        {
            return GetQdlValue(ro, dt, DefaultDateColumnName, valcol, d);
        }

        /// <summary>
        /// gets quandl value for a given date
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static decimal GetQdlValue(RootObject ro, DateTime dt, string datecol, string valcol, DebugDelegate d)
        {
            var dataidx = GetQdlDataIndex(ro, dt, datecol, d);
            return GetQdlValue(ro, valcol, dataidx, d);
        }


        /// <summary>
        /// gets quandl value for a given row (using default column value name)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static decimal GetQdlValue(RootObject ro, int dataidx, DebugDelegate d)
        {
            return GetQdlValue(ro, DefaultValueColumnName, dataidx, d);
        }
        /// <summary>
        /// gets decimal qdl value
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static decimal GetQdlValue(RootObject ro, string colname, int dataidx, DebugDelegate d)
        {
            if (_d == null)
                _d = d;
            var obj = GetQdlData(ro, colname, dataidx, d);
            decimal v = ERROR_VALUE;
            if ((obj != null) && decimal.TryParse(obj.ToString(), out v))
                return v;
            return ERROR_VALUE;

        }

        /// <summary>
        /// gets a quandl value from a partial data set
        /// </summary>
        /// <param name="data"></param>
        /// <param name="colidx"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static decimal GetQdlValue(List<List<object>> data, int colidx, int dataidx, DebugDelegate d)
        {
            if (!isValidQdlColIdx(colidx, data))
                return ERROR_VALUE;
            if (!isValidQdlRowIdx(dataidx,data))
                return ERROR_VALUE;
            
            if (_d==null)
                _d = d;
            
            var obj = data[dataidx][colidx];
            decimal v = ERROR_VALUE;
            if (decimal.TryParse(obj.ToString(), out v))
                return v;
            return ERROR_VALUE;

        }

        public const decimal ERROR_VALUE = decimal.MinValue;

        public static DateTime ERROR_DATE = DateTime.MinValue;


        /// <summary>
        /// get quandl data given row and column information
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="datacolname"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static object GetQdlData(RootObject ro, string datacolname, int dataidx, DebugDelegate d)
        {
            _d = d;
            // check column name
            var colidx = ro.column_names.IndexOf(datacolname);
            if (colidx < 0)
            {
                debug(ro.id + " can't get data because non existant colname: " + datacolname+" on set: "+ro.code);
                return null;
            }
            return GetQdlData(ro, colidx, dataidx, d);
        }

        /// <summary>
        /// gets quandl data given row and column information
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="datacolname"></param>
        /// <param name="dataidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static object GetQdlData(RootObject ro, int colidx, int dataidx, DebugDelegate d)
        {
            if (_d == null)
                _d = d;


            // check data index
            if ((dataidx < 0) || (dataidx >= ro.data.Count))
            {
                debug(ro.id + " can't get data because invalid data index: " + dataidx+" on set: "+ro.code);
                return null;
            }

            // get object
            return ro.data[dataidx][colidx];




        }


        /// <summary>
        /// gets all data between two rows (inclusive)
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="startidx"></param>
        /// <param name="endidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static List<List<object>> GetAllRows(RootObject ro, int startidx, int endidx, DebugDelegate d)
        {
            if (_d == null)
                _d = d;
            List<List<object>> rows = new List<List<object>>();
            for (int i = startidx; i <= endidx; i++)
            {
                if (i < 0)
                    i = 0;
                if (i >= ro.data.Count)
                    break;
                rows.Add(ro.data[i]);
            }
            return rows;
        }

        /// <summary>
        /// gets columns from end of dataset
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static List<object> GetColumn_After(RootObject ro, string colname, int start)
        {
            return GetColumn(ro, colname, start, ro.LastRowIndex);
        }

        /// <summary>
        /// gets column from start of data until a certain row
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<object> GetColumn_Until(RootObject ro, string colname, int end)
        {
            return GetColumn(ro, colname, 0, end);
        }

        /// <summary>
        /// gets column between two rows
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<object> GetColumn(RootObject ro, string colname, int start, int end)
        {
            var colidx = GetQdlColidx(ro, colname);
            var rows = GetAllRows(ro, start, end, null);
            return GetColumn(rows, colidx);
        }

        /// <summary>
        /// gets only a column from data set
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public static List<object> GetColumn(RootObject ro, string colname)
        {
            var colidx = GetQdlColidx(ro, colname);
            return GetColumn(ro.data, colidx);
        }
        /// <summary>
        /// gets only column from data set
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colidx"></param>
        /// <returns></returns>
        public static List<object> GetColumn(RootObject ro , int colidx)
        {
            return GetColumn(ro.data, colidx);
        }
        /// <summary>
        /// gets only a column from a data set
        /// </summary>
        /// <param name="data"></param>
        /// <param name="colidx"></param>
        /// <returns></returns>
        public static List<object> GetColumn(List<List<object>> data, int colidx)
        {
            if (data.Count==0)
                return new List<object>();
            if ((colidx < 0) || (colidx >= data[0].Count))
                throw new Exception("Invalid column index: " + colidx);
            List<object> col = new List<object>();
            for (int i = 0; i < data.Count; i++)
                col.Add(data[i][colidx]);
            return col;
        }


        /// <summary>
        /// pull all date with valid dates
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public static List<List<object>> GetAllDataWithDates(RootObject ro, string colname) { return GetAllDataWithDates(ro, colname, null); }

        /// <summary>
        /// pull all data with valid dates
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colname"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static List<List<object>> GetAllDataWithDates(RootObject ro, string colname, DebugDelegate d)
        {
            var cidx = GetQdlColidx(ro,colname);
            if (cidx<0)
            {
                if (d!=null)
                    d("Invalid column name: "+colname+" in: "+ro.id);
                return new List<List<object>>();
            }
            return GetAllDataWithDates(ro, cidx, d);
        }

        /// <summary>
        /// pull all data with valid dates
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="colidx"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static List<List<object>> GetAllDataWithDates(RootObject ro, int colidx, DebugDelegate d)
        {
            List<List<object>> dates = new List<List<object>>();
            for (int i = 0; i<ro.data.Count; i++)
            {
                var data = ro.data[i];
                var tmpdate = data[colidx].ToString();
                DateTime cur;
                if (DateTime.TryParse(tmpdate, out cur))
                {
                    dates.Add(data);
#if DEBUG
                    if (d != null)
                        d("got date: " + cur.ToShortDateString());
#endif

                }

            }
            return dates;
        }


        static DebugDelegate _d = null;
        static void debug(string msg)
        {
            if (_d != null)
                _d(msg);
        }

    }

    public class qh : QuandlHelper
    {
    }

   
}
