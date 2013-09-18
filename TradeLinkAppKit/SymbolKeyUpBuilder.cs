using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TradeLink.API;
using TradeLink.Common;

namespace TradeLink.AppKit
{
    public class SymbolKeyUpBuilder
    {

        public SymbolKeyUpBuilder()
        {
            mb.SendDebugEvent+=new DebugDelegate(debug);
        }

        public int id = 0;
        public event DebugDelegate SendDebugEvent;
        public event DebugDelegate SendStatusEvent;
        public event BasketDelegate SendNewBasketEvent;
        public event DebugDelegate SendNewRawSymbolsEvent;

        Basket mb = new BasketImpl();

        public bool isAutoUpperCase = true;

        
        bool addsymbol(string symtext)
        {
            mb.Clear();
            if (isAutoUpperCase)
                symtext = symtext.ToUpper();
            if (symtext.Contains(","))
            {
                
                mb = BasketImpl.FromString(symtext);
                debug("Used explicit parsing: " + symtext+" -> "+Util.join(mb.ToSymArrayFull()));
            }
            else
                mb = BasketImpl.parsedata(symtext,false,false,debug);
            status("Added " + symtext);
            if (SendNewBasketEvent != null)
                SendNewBasketEvent(mb, id);
            if (SendNewRawSymbolsEvent != null)
                SendNewRawSymbolsEvent(symtext);
            return true;
        }

        public void Clear()
        {
            newsymbol = string.Empty;
            mb.Clear();
        }

        void status(string msg)
        {
            if (SendStatusEvent != null)
                SendStatusEvent(msg);
        }

        void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);
        }
        
        string newsymbol = "";
        public void KeyUp(object sender, KeyEventArgs e)
        {
            string preface = "Adding symbol: ";
            if (e.KeyCode == Keys.Enter)
            {
                Security sec = SecurityImpl.Parse(newsymbol);
                if (sec.isValid)
                {
                    if (addsymbol(newsymbol))
                    {
                        newsymbol = "";
                    }
                }
                else
                {
                    status("Invalid Security " + newsymbol);
                    newsymbol = "";
                }
            }
            else if (e.Shift && (e.KeyCode == Keys.OemPipe))
            {
                newsymbol += "|";
                status(preface + newsymbol);
            }
            else if (e.Shift && (e.KeyCode == Keys.OemQuestion))
            {
                newsymbol += "?";
                status(preface + newsymbol);
            }
            else if ((e.KeyCode == Keys.OemPipe))
            {
                newsymbol += "\\";
                status(preface + newsymbol);
            }
            else if (e.KeyCode == Keys.OemQuestion)
            {
                newsymbol += "/";
                status(preface + newsymbol);
            }
            else if (e.KeyCode == Keys.OemPeriod)
            {
                newsymbol += ".";
                status(preface + newsymbol);
            }
            else if ((e.KeyCode == Keys.OemMinus) && e.Shift)
            {
                newsymbol += "_";
                status(preface + newsymbol);
            }
            else if ((e.KeyCode == Keys.D4) && e.Shift)
            {
                newsymbol = "$";
                status(preface + newsymbol);
            }
            else if ((e.KeyCode == Keys.Escape) || (e.KeyCode == Keys.Delete))
            {
                newsymbol = "";
                status("Symbol add canceled...");
            }
            else if ((e.KeyCode == Keys.Back) && (newsymbol.Length > 0))
            {
                newsymbol = newsymbol.Substring(0, newsymbol.Length - 1);
                status(preface + newsymbol);
            }
            else if (((e.KeyValue >= (int)Keys.A) && (e.KeyValue <= (int)Keys.Z))
                || ((e.KeyValue >= (int)Keys.D0) && (e.KeyValue <= (int)Keys.D9)) || e.Shift || (e.KeyData == Keys.Space))
            {
                string val = "";
                if (e.Shift)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.D3: val = "#";
                            break;
                    }
                }
                else
                {
                    char v = (char)e.KeyValue;
                    val += v;
                }
                newsymbol += val;
                status("Adding symbol: " + newsymbol);
            }
            else if (e.KeyData == Keys.OemPeriod)
            {
                newsymbol += ".";
                status("Adding symbol: " + newsymbol);
            }
            else if (e.KeyData == Keys.Space)
            {
                newsymbol += " ";
                status("Adding symbol: " + newsymbol);
            }
            else if (e.KeyData == Keys.OemMinus)
            {
                newsymbol += "-";
                status("Adding symbol: " + newsymbol);
            }
        }
    }
}
