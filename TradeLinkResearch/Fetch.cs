using System;
using System.Collections.Generic;
using TradeLink.Common;
using System.Net;
using TradeLink.API;


namespace TradeLink.Research
{
    /// <summary>
    /// obtains lists of symbols from on internet URLs
    /// </summary>
    public class Fetch
    {
        /// <summary>
        /// gets approximate nyse symbols from a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Basket NYSEFromURL(string url)
        {
            WebClient wc = new WebClient();
            return BasketImpl.NYSE(wc.DownloadString(url));
        }
        /// <summary>
        /// gets approximate NASDAQ symbols from a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Basket NASDAQFromURL(string url)
        {
            WebClient wc = new WebClient();
            return BasketImpl.NASDAQ(wc.DownloadString(url));
        }

        /// <summary>
        /// gets approximate nyse and nasdaq symbols from url 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Basket FromURL(string url)
        {
            Basket b = NYSEFromURL(url);
            b.Add(NASDAQFromURL(url));
            return b;
        }

        /// <summary>
        /// gets any linked approximate nyse symbols from a url (approximate = unverified, any 1-3 all caps symbol)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Basket LinkedNYSEFromURL(string url)
        {
            WebClient wc = new WebClient();
            return BasketImpl.LinkedOnlyNYSE(wc.DownloadString(url));
        }

        /// <summary>
        /// gets any linked approximate nasdaq symbols from a url (approximate = unverified, any 1-3 all caps symbol)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Basket LinkedNASDAQFromURL(string url)
        {
            WebClient wc = new WebClient();
            return BasketImpl.LinkedOnlyNASDAQ(wc.DownloadString(url));
        }



        /// <summary>
        /// remove unlisted symbols, leaving only verified symbols remaining.
        /// tradelink has a list of verified nasdaq and nyse symbols, but it is not guaranteed to be all inclusive.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Basket RemoveUnlisted(Basket input)
        {
            Basket output = new BasketImpl();
            for (int i =0; i<input.Count; i++)
                if (NYSE.isListed(input[i].symbol) || NASDAQ.isListed(input[i].symbol))
                    output.Add(input[i]);
            return output;
        }

            
            


    }
}
