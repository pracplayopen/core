using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeLink.AppKit
{
    /// <summary>
    /// request help from community
    /// </summary>
    public class HelpReportCommunity
    {
        const string CommunityEmail = "tradelink-users@googlegroups.com";
        /// <summary>
        /// starts a help email
        /// </summary>
        /// <param name="program"></param>
        public static void Help(string program) { Help(program, string.Empty); }
        /// <summary>
        /// starts a help email
        /// </summary>
        /// <param name="program"></param>
        /// <param name="err"></param>
        public static void Help(string program, string err)
        {
            try
            {
                System.Diagnostics.Process.Start(HelpLinkCommunity(program, err));
            }
            catch { }
        }
        /// <summary>
        /// gets a help email link
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public static string HelpLinkCommunity(string program) { return HelpLinkCommunity(program, string.Empty); }
        /// <summary>
        /// gets a help email link
        /// </summary>
        /// <param name="program"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static string HelpLinkCommunity(string program, string err)
        {
            return "http://community.tradelink.org/post/discussion/" ;
        }

        private static string HelpBody() { return HelpBody(string.Empty); }
        private static string HelpBody(string err)
        {
            StringBuilder sb = new StringBuilder();
            
            bool noerr = string.IsNullOrWhiteSpace(err);
            sb.AppendLine("Hello I'm attempting...");
            sb.AppendLine();
            sb.AppendLine("And my question is...");
            sb.AppendLine();
            sb.AppendLine();

            if (!noerr)
            {
                sb.AppendLine("I'm also getting this error: ");
                sb.AppendLine(err);
                sb.AppendLine();
            }

            sb.AppendLine("My relevant code is: ");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("any help appreciated.... thanks!");

            
            

            return sb.ToString();
        }

        public static string string2urlparams(string content, TradeLink.API.DebugDelegate ds)
        {
            try
            {
                return System.Web.HttpUtility.UrlEncode(content);
            }
            catch (BadImageFormatException ex)
            {
                if (ds != null)
                    ds("Error, install .net 4 from microsoft to correct. Err: " + ex.Message + ex.StackTrace);
            }
            return string.Empty;
        }
    }
}
