using System;
using TradeLink.API;
using TradeLink.Common;

public struct Pinnacle
{
    // fields for Pinnacle files
 
    const int DATE = 0;
    const int OPEN = 1;
    const int HIGH = 2;
    const int LOW = 3;
    const int CLOSE = 4;
    const int VOLUME = 5;
    const int OPENINTREST = 6;
 
    public static Bar parseline(string line, string sym)
    {
        string[] values = line.Split(',');
 
        Bar bar = new BarImpl(sym);
 
        DateTime date;
        if(DateTime.TryParse(line[DATE], out date))
        {
            bar.Date = date;
        }
 
        decimal priceOpen;
        if(decimal.TryParse(line[OPEN], out priceOpen))
        {
            bar.Open = priceOpen;
        }
 
        decimal priceHigh;
        if(decimal.TryParse(line[HIGH], out priceHigh))
        {
            bar.High = priceHigh;
        }
 
        decimal priceLow;
        if(decimal.TryParse(line[LOW], out priceLow))
        {
            bar.Low = priceLow;
        }
 
        decimal priceClose;
        if(decimal.TryParse(line[CLOSE], out priceClose))
        {
            bar.Close = priceClose;
        }
 
        int volume;
        if(decimal.TryParse(line[VOLUME], out volume))
        {
            bar.Volume = volume;
        }
 
        int openInterest;
        if(decimal.TryParse(line[OPENINTEREST], out volume))
        {
            bar.OpenInterest = openInterest;
        }
 
        return bar;
    }
}