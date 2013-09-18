using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeLink.API
{
    public interface Result
    {
        /// <summary>
        /// date time in ticks
        /// </summary>
        long ResultsDateTime { get; set; }
        /// <summary>
        /// symbols traded in result
        /// </summary>
        string Symbols { get; set; }
        /// <summary>
        /// gross pl of result
        /// </summary>
        decimal GrossPL { get; set; }
        /// <summary>
        /// net pl of result
        /// </summary>
        decimal NetPL { get;  }
        /// <summary>
        /// long trade pl
        /// </summary>
        decimal BuyPL { get; set; }
        /// <summary>
        /// short trade pl
        /// </summary>
        decimal SellPL { get; set; }
        /// <summary>
        /// number of winning trades
        /// </summary>
         int Winners { get; set; }
        /// <summary>
        /// number of long winners
        /// </summary>
         int BuyWins { get; set; }
        /// <summary>
        /// number of short winners
        /// </summary>
         int SellWins { get; set; }
        /// <summary>
        /// number of short losers
        /// </summary>
         int SellLosers { get; set; }
        /// <summary>
        /// number of buy losers
        /// </summary>
         int BuyLosers { get; set; }
        /// <summary>
        /// number of total losers
        /// </summary>
         int Losers { get; set; }
        /// <summary>
        /// number of break even trades
        /// </summary>
         int Flats { get; set; }
        /// <summary>
        /// average gross pl per trade 
        /// </summary>
         decimal AvgPerTrade { get; set; }
        /// <summary>
        /// average gross per win trade
        /// </summary>
         decimal AvgWin { get; set; }
        /// <summary>
        /// average gross per losing trade
        /// </summary>
         decimal AvgLoser { get; set; }
        /// <summary>
        /// total/max money used to acheive result
        /// </summary>
         decimal MoneyInUse { get; set; }
        /// <summary>
        /// highest gross pl to acheive final result
        /// </summary>
         decimal MaxPL { get; set; }
        /// <summary>
        /// lowest gross pl to acheive final result
        /// </summary>
         decimal MinPL { get; set; }
        /// <summary>
        /// comissions
        /// </summary>
         decimal ComPerShare { get; set; }
        /// <summary>
        /// biggest winner gross
        /// </summary>
         decimal MaxWin { get; set; }
        /// <summary>
        /// biggest loser gross
        /// </summary>
         decimal MaxLoss { get; set; }
        /// <summary>
        /// max unclosed winning gross
        /// </summary>
         decimal MaxOpenWin { get; set; }
        /// <summary>
        /// max unclosed losing gross
        /// </summary>
         decimal MaxOpenLoss { get; set; }
        /// <summary>
        /// shares/contracts traded during result
        /// </summary>
         int SharesTraded { get; set; }
        /// <summary>
        /// round turns
        /// </summary>
         int RoundTurns { get; set; }
         int RoundWinners { get; set; }
         int RoundLosers { get; set; }
         int Trades { get; set; }
         int SymbolCount { get; set; }
         int DaysTraded { get; set; }
         decimal GrossPerDay { get; set; }
         decimal GrossPerSymbol { get; set; }
         decimal SharpeRatio { get; set; }
         decimal SortinoRatio { get; set; }
         int ConsecWin { get; set; }
         int ConsecLose { get; set; }
         List<string> PerSymbolStats { get; set; }
         string SimParameters { get; set; }
         string ResultsId { get; set; }
    }
}
