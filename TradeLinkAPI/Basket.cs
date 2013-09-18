using System;
using System.Collections;

namespace TradeLink.API
{
    public interface Basket
    {
        /// <summary>
        /// name of basket (optional)
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// fetch specific security in basket
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Security this[int index] { get; set; }
        /// <summary>
        /// number of securities contained in basket
        /// </summary>
        int Count { get; }
        /// <summary>
        /// add symbol to basket
        /// </summary>
        /// <param name="symbol"></param>
        void Add(string symbol);
        /// <summary>
        /// add symbols to basket
        /// </summary>
        /// <param name="symbols"></param>
        void Add(string[] symbols);
        /// <summary>
        /// add security to basket
        /// </summary>
        /// <param name="newsecurity"></param>
        void Add(Security newsecurity);
        /// <summary>
        /// add existing basket to this one
        /// </summary>
        /// <param name="newbasket"></param>
        void Add(Basket newbasket);
        /// <summary>
        /// remove any contents of another basket from this one
        /// </summary>
        /// <param name="subtractbasket"></param>
        void Remove(Basket subtractbasket);
        /// <summary>
        /// remove a specific security
        /// </summary>
        /// <param name="i"></param>
        void Remove(int i);
        /// <summary>
        /// remove a specific security
        /// </summary>
        /// <param name="s"></param>
        void Remove(Security s);
        /// <summary>
        /// remove a specific security by it's symbol
        /// </summary>
        /// <param name="symbol"></param>
        void Remove(string symbol);
        /// <summary>
        /// clear basket
        /// </summary>
        void Clear();
        /// <summary>
        /// get securities
        /// </summary>
        /// <returns></returns>
        Security[] ToArray();
        /// <summary>
        /// get short symbols
        /// </summary>
        /// <returns></returns>
        string[] ToSymArray();
        /// <summary>
        /// get full symbols
        /// </summary>
        /// <returns></returns>
        string[] ToSymArrayFull();
        IEnumerator GetEnumerator();

        event DebugDelegate SendDebugEvent;

        /// <summary>
        /// whether security is present
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool isSecurityPresent(Security sec);
        /// <summary>
        /// whether short form of security is present
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        bool isSymbolPresent(string sym);
        /// <summary>
        /// whether long form of security is present
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        bool isFullSymbolPresent(string sym);
        /// <summary>
        /// whether either long or short form of security is present
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        bool isAnySymbolPresent(string sym);
        /// <summary>
        /// attempts to fetch security by long symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool TryGetSecurityFullSymbol(string sym, out Security sec);
        /// <summary>
        /// attempts to fetch security by short symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool TryGetSecurityShortSymbol(string sym, out Security sec);
        /// <summary>
        /// attempts to fetch security first by long, then by short symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool TryGetSecurityAnySymbol(string sym, out Security sec);
    }
}
