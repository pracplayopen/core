using System;
using System.Collections.Generic;
using TradeLink.API;
using TradeLink.Common;
using TradeLink.AppKit;
using MBTCOMLib;
using MBTORDERSLib;
using MBTQUOTELib;
using MBTHISTLib;

namespace ServerMB
{
    public class ServerMB : IMbtQuotesNotify
    {
        TLServer tl;
        public MbtComMgr m_ComMgr;
        public MbtOrderClient m_OrderClient;
        public MbtQuotes m_Quotes;
        public MbtOpenOrders m_OpenOrders; //pmh - 9/18/12 - was m_Orders
        public MbtHistMgr m_HistMgr;       //pmh - 10/1/12 - was m_Hist
        PositionTracker pt = new PositionTracker();

        Dictionary<long, string> tl2broker = new Dictionary<long, string>();
        Dictionary<string, long> broker2tl = new Dictionary<string, long>();
        List<string> sentNewOrders = new List<string>();
        Dictionary<string, MbtOpenOrder> orders = new Dictionary<string, MbtOpenOrder>();
        Dictionary<string, bool> cancelids = new Dictionary<string, bool>();
        Dictionary<long, bool> canceledids = new Dictionary<long, bool>(); long _lasttime = 0;
        //TODO: make this configurable from the app.config
        bool DisableOldTicks;

        bool _noverb = true;
		  public bool VerboseDebugging { get { return !_noverb; } set { _noverb = !value; } }
		  
		 public ServerMB(TLServer tls)
        {
			  v("VerboseDebugging: " + VerboseDebugging);
            tl = tls;
            m_ComMgr = null;
            try
            {
					m_ComMgr = new MbtComMgr();  //pmh - Preferred. Was MbtComMgrClass(). Either should work.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Process.Start(@"http://code.google.com/p/tradelink/wiki/ComFactoryErrors");
					 debug("m_ComMgr = new MbtComMgr() failed: " + ex.Message + ex.StackTrace); //pmh
					 return;
            }
//            m_ComMgr.SilentMode = true; //pmh - 9/18/12 - display login dialog if blank UID/Pwd (this allows me/pmh to connect to special servers for advanced testing)
            m_ComMgr.EnableSplash(false);
            m_OrderClient = m_ComMgr.OrderClient;
            m_OrderClient.SilentMode = true;
/* //pmh - OnDemandMode has been deprecated 
	if (!m_ComMgr.IsPreviousInstanceDetected("tradelink"))
	{
		m_OrderClient.OnDemandMode = false;                                             
		v("order client on demand mode disabled as previous tradelink was running.");   
	}
*/
            m_Quotes = m_ComMgr.Quotes;
            m_OpenOrders = m_OrderClient.OpenOrders;
            m_HistMgr = m_ComMgr.HistMgr;

            // tradelink bindings
            tl.newProviderName = Providers.MBTrading;
            tl.newFeatureRequest += new MessageArrayDelegate(tl_newFeatureRequest);
            tl.newSendOrderRequest += new OrderDelegateStatus(tl_newSendOrderRequest);
            tl.newRegisterSymbols += new SymbolRegisterDel(tl_newRegisterSymbols);
            tl.newOrderCancelRequest += new LongDelegate(tl_newOrderCancelRequest);
            tl.newAcctRequest += new StringDelegate(tl_newAcctRequest);
            tl.newPosList += new PositionAccountArrayDelegate(tl_newPosList);
            tl.newUnknownRequest += new UnknownMessageDelegate(tl_newUnknownRequest);

				//pmh - 8/29/12 - reordered alphabetically and added almost all remaining events

				// MB Trading MbtComMgr bindings
				m_ComMgr.OnAlertAdded += new IMbtComMgrEvents_OnAlertAddedEventHandler(m_ComMgr_OnAlertAdded); //pmh
				m_ComMgr.OnAlertsRefresh += new IMbtComMgrEvents_OnAlertsRefreshEventHandler(m_ComMgr_OnAlertsRefresh); //pmh
				m_ComMgr.OnCriticalShutdown += new IMbtComMgrEvents_OnCriticalShutdownEventHandler(m_ComMgr_OnCriticalShutdown); //pmh
				m_ComMgr.OnHealthUpdate += new IMbtComMgrEvents_OnHealthUpdateEventHandler(m_ComMgr_OnHealthUpdate);
				m_ComMgr.OnLogonDeny += new IMbtComMgrEvents_OnLogonDenyEventHandler(m_ComMgr_OnLogonDeny);
				m_ComMgr.OnLogonSucceed += new IMbtComMgrEvents_OnLogonSucceedEventHandler(m_ComMgr_OnLogonSucceed);
			  /*
			   * pmh - add later if desired
			   * 
				m_ComMgr.OnMATAccountPercentSet += new IMbtComMgrEvents_OnMATAccountPercentSetEventHandler(m_ComMgr_OnMATAccountPercentSet); //pmh
				m_ComMgr.OnMATGroupAccountAdded += new IMbtComMgrEvents_OnMATGroupAccountAddedEventHandler(m_ComMgr_OnMATGroupAccountAdded); //pmh
				m_ComMgr.OnMATGroupAccountRemoved += new IMbtComMgrEvents_OnMATGroupAccountRemovedEventHandler(m_ComMgr_OnMATGroupAccountRemoved); //pmh
				m_ComMgr.OnMATGroupsGroupAdded += new IMbtComMgrEvents_OnMATGroupsGroupAddedEventHandler(m_ComMgr_OnMATGroupsGroupAdded); //pmh
				m_ComMgr.OnMATGroupsGroupRemoved += new IMbtComMgrEvents_OnMATGroupsGroupRemovedEventHandler(m_ComMgr_OnMATGroupsGroupRemoved); //pmh
				m_ComMgr.OnMATGroupsGroupRenamed += new IMbtComMgrEvents_OnMATGroupsGroupRenamedEventHandler(m_ComMgr_OnMATGroupsGroupRenamed); //pmh
			   */
				
			  // MB Trading MbtOrderClient bindings
				m_OrderClient.OnAccountLoaded += new _IMbtOrderClientEvents_OnAccountLoadedEventHandler(m_OrderClient_OnAccountLoaded);
				m_OrderClient.OnAccountLoading += new _IMbtOrderClientEvents_OnAccountLoadingEventHandler(m_OrderClient_OnAccountLoading);
				m_OrderClient.OnAccountUnavailable += new _IMbtOrderClientEvents_OnAccountUnavailableEventHandler(m_OrderClient_OnAccountUnavailable); //pmh
				m_OrderClient.OnAcknowledge += new _IMbtOrderClientEvents_OnAcknowledgeEventHandler(m_OrderClient_OnAcknowledge); //pmh
				m_OrderClient.OnBalanceUpdate += new _IMbtOrderClientEvents_OnBalanceUpdateEventHandler(m_OrderClient_OnBalanceUpdate); //pmh
				m_OrderClient.OnCancelPlaced += new _IMbtOrderClientEvents_OnCancelPlacedEventHandler(m_OrderClient_OnCancelPlaced);
				m_OrderClient.OnCancelRejected += new _IMbtOrderClientEvents_OnCancelRejectedEventHandler(m_OrderClient_OnCancelRejected);
				m_OrderClient.OnClose += new _IMbtOrderClientEvents_OnCloseEventHandler(m_OrderClient_OnClose);
				m_OrderClient.OnConnect += new _IMbtOrderClientEvents_OnConnectEventHandler(m_OrderClient_OnConnect);
				//pmh - 9/21/12 - See note on DefaultAccount at end of document!
				//m_OrderClient.OnDefaultAccountChanged += new _IMbtOrderClientEvents_OnDefaultAccountChangedEventHandler(m_OrderClient_OnDefaultAccountChanged); //pmh
				m_OrderClient.OnExecute += new _IMbtOrderClientEvents_OnExecuteEventHandler(m_OrderClient_OnExecute); //pmh
				m_OrderClient.OnHistoryAdded += new _IMbtOrderClientEvents_OnHistoryAddedEventHandler(m_OrderClient_OnHistoryAdded);
				m_OrderClient.OnLogonSucceed += new _IMbtOrderClientEvents_OnLogonSucceedEventHandler(m_OrderClient_OnLogonSucceed);
				m_OrderClient.OnPositionAdded += new _IMbtOrderClientEvents_OnPositionAddedEventHandler(m_OrderClient_OnPositionAdded);
				m_OrderClient.OnPositionStrategyGroupAdded += new _IMbtOrderClientEvents_OnPositionStrategyGroupAddedEventHandler(m_OrderClient_OnPositionStrategyGroupAdded); //pmh
				m_OrderClient.OnPositionStrategyGroupRemoved += new _IMbtOrderClientEvents_OnPositionStrategyGroupRemovedEventHandler(m_OrderClient_OnPositionStrategyGroupRemoved); //pmh
				m_OrderClient.OnPositionStrategyGroupUpdated += new _IMbtOrderClientEvents_OnPositionStrategyGroupUpdatedEventHandler(m_OrderClient_OnPositionStrategyGroupUpdated); //pmh
				m_OrderClient.OnPositionUpdated += new _IMbtOrderClientEvents_OnPositionUpdatedEventHandler(m_OrderClient_OnPositionUpdated);
				m_OrderClient.OnRemove += new _IMbtOrderClientEvents_OnRemoveEventHandler(m_OrderClient_OnRemove); //pmh
				m_OrderClient.OnReplacePlaced += new _IMbtOrderClientEvents_OnReplacePlacedEventHandler(m_OrderClient_OnReplacePlaced); //pmh
				m_OrderClient.OnReplaceRejected += new _IMbtOrderClientEvents_OnReplaceRejectedEventHandler(m_OrderClient_OnReplaceRejected); //pmh
				m_OrderClient.OnSubmit += new _IMbtOrderClientEvents_OnSubmitEventHandler(m_OrderClient_OnSubmit);

				// MB Trading MbtQuote bindings
				m_Quotes.OnClose += new _IMbtQuotesEvents_OnCloseEventHandler(m_Quotes_OnClose);
            m_Quotes.OnConnect += new _IMbtQuotesEvents_OnConnectEventHandler(m_Quotes_OnConnect);
            m_Quotes.OnLogonSucceed += new _IMbtQuotesEvents_OnLogonSucceedEventHandler(m_Quotes_OnLogonSucceed);

				// MB Trading MbtHist bindings
				m_HistMgr.OnDataEvent += new _IMbtHistEvents_OnDataEventEventHandler(m_HistMgr_OnDataEvent);
				m_HistMgr.OnError += new _IMbtHistEvents_OnErrorEventHandler(m_HistMgr_OnError);

				//disable old ticks
            //DisableOldTicks = Convert.ToBoolean(ConfigurationSettings.AppSettings["DisableOldTicks"]);
            DisableOldTicks = true;
            debug("TL binding and MBT Desktop event mapping complete.");
        }

        void v(string msg)
        {
            if (_noverb)
                return;
            debug(msg);
        }

        void tl_newRegisterSymbols(string client, string symbols)
        {
            test();
            v("received symbol request from: " + client + " for: " + symbols);
            string[] syms = tl.AllClientBasket.ToString().Split(',');
            m_Quotes.UnadviseAll(this);
				for (int i = 0; i < syms.Length; i++)
				{
					/* TODO
					 ********************************** 9/13/12 - pmh - I recommend replacing all of this:
					 */
					if (syms[i].Contains("."))
					{
						//we can reasonably assume this is an options request
						//pmh - 9/13/12 - ?!? - Canadian also contains dots
						m_Quotes.AdviseSymbol(this, syms[i], (int)enumQuoteServiceFlags.qsfOptions);
						//pmh - 9/13/12 - ?!? - Why request options quotes from an option symbol?
					}
					else if (syms[i].Contains("/"))
					{
						//we know (or can at least reasonably assume) this is forex
						//advise only level1 bid-ask quotes
						m_Quotes.AdviseSymbol(this, syms[i], (int)enumQuoteServiceFlags.qsfLevelOne);
						//m_Quotes.AdviseSymbol(this, syms[i], (int)enumQuoteServiceFlags.qsfLevelTwo);
					}
					else
					{
						//probably equity, advise time and sales
						m_Quotes.AdviseSymbol(this, syms[i], ((int)enumQuoteServiceFlags.qsfTimeAndSales));
					}
					/*
					 ********************************** with this:
					 *
					 * TODO: pmh - determine which qsf level(s) the user desires rather than waste an Advise on one they don't? How to do that?
					 * 
					int _qsf = (int)enumQuoteServiceFlags.qsfLevelOne + (int)enumQuoteServiceFlags.qsfTimeAndSales;  //pmh - L1 & T&S regardless.
					if (syms[i].Substring(1, 1) != "+") _qsf += (int)enumQuoteServiceFlags.qsfOptions;               //pmh - Add options chain if not options symbol
					 */
				}
		  }

        bool waitforhistorical2complete = false;
        Dictionary<int, BarRequest> _barhandle2barrequest = new Dictionary<int, BarRequest>();
        RingBuffer<BarRequest> _barrequests = new RingBuffer<BarRequest>(500);

        long tl_newUnknownRequest(MessageTypes t, string msg)
        {
            debug(String.Format("Unknown message {0}: {1}", t, msg));
            switch (t)
            {
                case MessageTypes.BARREQUEST:
                    {
                        debug("got barrequest: " + msg);
                        try
                        {
                            BarRequest br = BarRequest.Deserialize(msg);
                            if (waitforhistorical2complete) _barrequests.Write(br);
                            else submitBarRequest(br);
                        }
                        catch (Exception ex)
                        {
                            debug("error parsing bar request: " + msg);
                            debug(ex.Message + ex.StackTrace);
                        }
                        return 0;
                    }
            }
            return (long)MessageTypes.FEATURE_NOT_IMPLEMENTED;
        }

        public void submitBarRequest(BarRequest br)
        {
            waitforhistorical2complete = true;
            debug("br.Client=" + br.Client);
            string symbol = br.symbol;
            int lRequestID = (int)br.Interval % 100;
            int lPeriod = (int)br.Interval / 100;
            int lMaxRecs = br.CustomInterval / 10;
            bool bUpdate = (br.CustomInterval % 10 == 0) ? false : true;
				bool bExtSession = false;
            int tlDateS = br.StartDate;
            int tlTimeS = br.StartTime;
            DateTime dtStart = TradeLink.Common.Util.ToDateTime(tlDateS, tlTimeS);
            int tlDateE = br.EndDate;
            int tlTimeE = br.EndTime;
            DateTime dtEnd = TradeLink.Common.Util.ToDateTime(tlDateE, tlTimeE);

            if (!_barhandle2barrequest.ContainsKey(lRequestID))
                _barhandle2barrequest.Add(lRequestID, br);
            else
                debug("already had bar request: " + lRequestID + " " + _barhandle2barrequest[lRequestID].ToString());

            int lRequestType = lRequestID % 10;

            switch (lRequestType)
            {
                case 1:
                    MbtHistDayBar moDayBar = m_HistMgr.CreateHistDayBar();
                    moDayBar.Clear();
						  debug(symbol + "Day: lID:" + lRequestID + " lPd:" + lPeriod + " b:" + dtStart + " e:" + dtEnd + " lMax:" + lMaxRecs + " bUp:" + bUpdate);
                    moDayBar.SendRequest(symbol, lRequestID, lPeriod, dtStart.ToUniversalTime(), dtEnd.ToUniversalTime(), lMaxRecs, bUpdate);
                    break;
                case 2:
                    MbtHistMinBar moMinBar = m_HistMgr.CreateHistMinBar();
                    moMinBar.Clear();
						  debug(symbol + "Min: lID:" + lRequestID + " lPd:" + lPeriod + " b:" + dtStart + " e:" + dtEnd + " lMax:" + lMaxRecs + " bUp:" + bUpdate + " ext:" + bExtSession);
                    moMinBar.SendRequest(symbol, lRequestID, lPeriod, dtStart.ToUniversalTime(), dtEnd.ToUniversalTime(), lMaxRecs, bUpdate, bExtSession);
                    break;
                case 3:
                    MbtHistTick moTickBar = m_HistMgr.CreateHistTick();
                    moTickBar.Clear();
						lPeriod=0; // for all ticks
						debug(symbol + "Tick: lID:" + lRequestID + " filter:" + lPeriod + " b:" + dtStart + " e:" + dtEnd + " lMax:" + lMaxRecs + " bUp:" + bUpdate);
//						  moTickBar.SendRequest(symbol, lRequestID, lPeriod, dtStart.ToUniversalTime(), dtEnd.ToUniversalTime(), lMaxRecs, bUpdate);
						  moTickBar.SendRequest(symbol, lRequestID, (int)MBTHISTLib.enumTickRequestFilter.trfAllTicks, dtStart.ToUniversalTime(), dtEnd.ToUniversalTime(), lMaxRecs, bUpdate);
                    break;
						//pmh - 9/13/12 - PV bar
					 case 4:
						  MbtHistPVBar moMinPV = m_HistMgr.CreateHistPVBar();
						  moMinPV.Clear();
						  debug(symbol + "PV: lID:" + lRequestID + " lPd:" + lPeriod + " b:" + dtStart + " e:" + dtEnd + " lMax:" + lMaxRecs);
						  moMinPV.SendRequest(symbol, lRequestID, lPeriod, dtStart.ToUniversalTime(), dtEnd.ToUniversalTime(), lMaxRecs);
						  break;
				}
        }

        public int MbtDayInt2CustInt(int lPeriod)
        {
				switch (lPeriod)
				{
						case -3:
							return 365 * (int)BarInterval.Day; 
						case -2:
							return 30 * (int)BarInterval.Day; 
						case -1:
							return 7 * (int)BarInterval.Day; 
						case 0: return (int)BarInterval.Day; 
						default: return lPeriod * (int)BarInterval.Day; 
				}
        }

        public int MbtMinInt2CustInt(int lPeriod)
        {

            switch (lPeriod)
            {
                case 0: return (int)BarInterval.Minute; 
                default: return lPeriod; 
            }
        }

		  //pmh #####################################################################################################
		  //pmh #####################################################################################################

		  //pmh MbtComMgr events ##################################################################################
		  void m_ComMgr_OnAlertAdded(MbtAlert pAlert)
		  {
			  debug(String.Format("m_ComMgr_OnAlertAdded: {0} {1}", pAlert.Event, pAlert.Message));
		  }
		  void m_ComMgr_OnAlertsRefresh()
		  {
			  debug("m_ComMgr_OnAlertsRefresh.");
		  }
		  void m_ComMgr_OnCriticalShutdown()
		  {
			  debug("m_ComMgr__OnCriticalShutdown.");
		  }
		  void m_ComMgr_OnHealthUpdate(enumServerIndex index, enumConnectionState state)
		  {
			  debug(String.Format("m_ComMgr_OnHealthUpdate: {0} {1}", index, state));
		  }
		  void m_ComMgr_OnLogonDeny(string bstrReason)
        {
			  debug("m_ComMgr_OnLogonDeny: " + bstrReason);
			  isValid = false;  //pmh - ?!?
        }
        void m_ComMgr_OnLogonSucceed()
        {
			  debug("m_ComMgr_OnLogonSucceed.");
			  isValid = true;  //pmh - ?!?
        }


		  //pmh MbtOrderClient events #############################################################################
		  void m_OrderClient_OnAccountLoaded(MbtAccount pAcct)
		  {
			  debug("m_OrderClient_OnAccountLoaded: " + pAcct.Account + ";" + pAcct.Customer);
				  //pmh - 9/21/12 - See note on DefaultAccount at end of document!
				  // + " DefaultAccount is: " + m_OrderClient.Accounts.DefaultAccount.Account + ";" + m_OrderClient.Accounts.DefaultAccount.Customer);
			  debug(DisplayAccount(pAcct));
		  }
		  void m_OrderClient_OnAccountLoading(MbtAccount pAcct)
        {
			  debug("m_OrderClient_OnAccountLoading: " + pAcct.Account);
        }
		  void m_OrderClient_OnAccountUnavailable(MbtAccount pAcct)
        {
			  debug("m_OrderClient_OnAccountUnavailable: " + pAcct.Account);
        }
        void m_OrderClient_OnAcknowledge(MbtOpenOrder pOrd)
        {
            debug("m_OrderClient_OnAcknowledge: " + pOrd.OrderNumber);
        }
		  void m_OrderClient_OnBalanceUpdate(MbtAccount pAcct)
        {
			  debug("m_OrderClient_OnBalanceUpdate: " + pAcct.Account);
        }
		  void m_OrderClient_OnCancelPlaced(MbtOpenOrder pOrd)
		  {
			  debug("m_OrderClient_OnCancelPlaced: " + pOrd.OrderNumber);
		  }
		  void m_OrderClient_OnCancelRejected(MbtOpenOrder pOrd)
		  {
			  debug("m_OrderClient_OnCancelRejected: " + pOrd.OrderNumber);
		  }
		  void m_OrderClient_OnClose(int ErrorCode)
		  {
			  debug("m_OrderClient_OnClose: " + ErrorCode);
		  }
		  void m_OrderClient_OnConnect(int ErrorCode)
		  {
			  debug("m_OrderClient_OnConnect: " + ErrorCode);
			  //pmh - 9/21/12 - See note on DefaultAccount at end of document!
			  // debug("DefaultAccount: " + m_OrderClient.Accounts.DefaultAccount.Account);
		  }
		  /* pmh - 9/21/12 - See note on DefaultAccount at end of document!
		  void m_OrderClient_OnDefaultAccountChanged(MbtAccount pAcct)
		  {
			  debug("m_OrderClient_OnDefaultAccountChanged: " + pAcct.Account);
		  }
		   */
		  void m_OrderClient_OnExecute(MbtOpenOrder pOrd)
		  {
			  //add successfully executed orders to the orders dictionary
			  //done in the history now
			  //if (!orders.ContainsKey(pOrd.OrderNumber)) orders.Add(pOrd.OrderNumber, pOrd);
			  debug("m_OrderClient_OnExecute: " + pOrd.OrderNumber); //pmh - go ahead and log the event anyway.
		  }

		  void m_OrderClient_OnHistoryAdded(MbtOrderHistory pHist)
		  {
			  long tlid = 0;
			  double price = (pHist.Price != 0) ? pHist.Price : pHist.StopPrice;
			  broker2tl.TryGetValue(pHist.OrderNumber, out tlid);
			  debug(String.Format("m_OrderClient_OnHistoryAdded {0} {1} {2} {3} {4} {5} {6}", pHist.Symbol, pHist.Event, pHist.OrderNumber, tlid, price, pHist.Quantity, pHist.Message));
			  switch (pHist.Event)
			  {
				  case "Accepted":
				  case "Live":
					  if (!sentNewOrders.Contains(pHist.OrderNumber))
					  {
						  OrderImpl o = HistToTradeLinkOrder(pHist);
						  tl.newOrder(o);
						  sentNewOrders.Add(pHist.OrderNumber);
					  }
					  break;
				  case "Executed":
					  //OrderImpl o = HistToTradeLinkOrder(pHist);
					  //tl.newOrder(o);
					  TradeImpl f = HistToTradeLinkFill(pHist);
					  pt.Adjust(f);
					  tl.newFill(f);
					  break;
				  case "Order Cancelled":
					  //TODO: have to make sure order cancellations are happening correctly, especially on repeat requests from TradeLink
					  string bid = pHist.OrderNumber;
					  //long tlid = 0;
					  if (broker2tl.TryGetValue(bid, out tlid))
					  {
						  debug(String.Format("Order {0} cancelled, matched order {1} and sending newOrderCancel to TradeLink", bid, tlid));
						  //remove the order mappings
						  if (broker2tl.ContainsKey(bid))
							  broker2tl.Remove(bid);
						  if (tl2broker.ContainsKey(tlid))
							  tl2broker.Remove(tlid);
						  cancelids.Remove(bid);
						  orders.Remove(bid);
						  tl.newCancel(tlid);
						  canceledids.Add(tlid, true);
					  }
					  else
						  debug(String.Format("Order {0} cancelled but no matching TradeLink order to cancel", bid));
					  break;
				  default:
					  break;
			  }
		  }
		  void m_OrderClient_OnLogonSucceed()
		  {
			  debug("m_OrderClient_OnLogonSucceed");
			  //pmh - 9/21/12 - See note on DefaultAccount at end of document!
			  //debug(String.Format("default account:{0}",m_OrderClient.Accounts.DefaultAccount.Account));
			  //pmh - LoadAll will be deprecated soon. Dangerous for logins with a large number of
			  // accounts mapped to it (industrial accounts?). Need to add functionality to load each account when selected.
			  m_OrderClient.Accounts.LoadAll();
		  }
		  void m_OrderClient_OnPositionAdded(MbtPosition pPos)
		  {
			  debug(String.Format("m_OrderClient_OnPositionAdded: {0} {1} {2}", pPos.Account.Account, pPos.Symbol, pPos.AggregatePosition));
			  PositionHasChanged(pPos);
		  }
		  void m_OrderClient_OnPositionStrategyGroupAdded(MbtPositionStrategyGroup pGroup)
		  {
			  debug("m_OrderClient_OnPositionStrategyGroupAdded: " + pGroup.Description);
		  }
		  void m_OrderClient_OnPositionStrategyGroupRemoved(MbtPositionStrategyGroup pGroup)
		  {
			  debug("m_OrderClient_OnPositionStrategyGroupRemoved: " + pGroup.Description);
		  }
		  void m_OrderClient_OnPositionStrategyGroupUpdated(MbtPositionStrategyGroup pGroup)
		  {
			  debug("m_OrderClient_OnPositionStrategyGroupUpdated: " + pGroup.Description);
		  }
		  void m_OrderClient_OnPositionUpdated(MbtPosition pPos)
		  {
			  debug(String.Format("m_OrderClient_OnPositionUpdated: {0} {1} {2}", pPos.Account.Account, pPos.Symbol, pPos.AggregatePosition));
			  PositionHasChanged(pPos);
		  }
		  void m_OrderClient_OnRemove(MbtOpenOrder pOrd)
			{
				debug("m_OrderClient_OnRemove: " + pOrd.OrderNumber);
				/*
				//order has been completely filled so do cleanup
				string bid = pOrd.OrderNumber;
				debug(String.Format("Cleaning up orders for {0}", bid));
				//clean up the orders list
				if( orders.ContainsKey(bid) ) orders.Remove(bid);
				else debug(String.Format("order {0} did not exist in orders", bid));
				//clean up mappings
				long tlid = 0;
				if( broker2tl.TryGetValue(bid, out tlid ) )
				{
					//got the tradelink id so clear the broker mapping
					debug(String.Format("Cleaning up broker2tl mapping {0} - {1}", bid, tlid));
					broker2tl.Remove(bid);
					//then clear the tradelink mapping
					debug(String.Format("cleaning up tl2broker mapping {0} - {1}", tlid, bid));
					if( tl2broker.ContainsKey(tlid ) ) tl2broker.Remove(tlid);
					else debug(String.Format("Order {0} did not have a matching tlid {1} to remove.", bid, tlid));
				}
				*/
			}
		  void m_OrderClient_OnReplacePlaced(MbtOpenOrder pOrd)
		  {
			  debug("m_OrderClient_OnReplacePlaced: " + pOrd.OrderNumber);
		  }
		  void m_OrderClient_OnReplaceRejected(MbtOpenOrder pOrd)
		  {
			  debug("m_OrderClient_OnReplaceRejected: " + pOrd.OrderNumber);
		  }
		  /// <summary>
		  /// Store submitted orders in a dictionary so we have access to info later on
		  /// </summary>
		  /// <param name="pOrd"></param>
		  void m_OrderClient_OnSubmit(MbtOpenOrder pOrd)
		  {
			  debug("m_OrderClient_OnSubmit: " + pOrd.OrderNumber);
			  if (!orders.ContainsKey(pOrd.OrderNumber)) orders.Add(pOrd.OrderNumber, pOrd);
		  }

		  //pmh MbtQuote events ###################################################################################
		  void m_Quotes_OnClose(int ErrorCode)
		  {
			  debug("m_Quotes_OnClose:" + ErrorCode);
		  }
		  void m_Quotes_OnConnect(int ErrorCode)
		  {
			  debug("m_Quotes_OnConnect: " + ErrorCode);
		  }
		  void m_Quotes_OnLogonSucceed()
		  {
			  debug("m_Quotes_OnLogonSucceed");
		  }

		  //pmh MbtHist events ####################################################################################
		  public void m_HistMgr_OnDataEvent(int lRequestId, object pHist, enumHistEventType evt)
		  {
			  debug(String.Format("m_HistMgr_OnDataEvent lRequestId: {0}, evt: {1}", lRequestId, evt));
			  Bar b;
			  int date, time; 
			  long vol;
			  decimal open, high, low, close;
			  string symbol;

			  // Notice we are using lRequestId in the original SendRequest()s to indicate whether we're
			  // dealing with a Day, Min or Tick bar object. Process accordingly.
			  BarRequest br;
			  int MbtCustInt;

//pmh ?!?			  debug("Unknown barrequest handle: ");

			  if (!_barhandle2barrequest.TryGetValue(lRequestId, out br))
			  {
				  debug("Unknown barrequest handle, lRequestId: " + lRequestId);
				  return;
			  }

			  int lRequestType = lRequestId % 10;

			  switch (lRequestType)
			  {
				  case 1:
					  //pmh ?!? - debug("number of client");
					  MbtHistDayBar barDay = pHist as MbtHistDayBar;
					  debug("Day bar, " + barDay.Count + " recs");
					  barDay.Last();
					  while (!barDay.Bof)
					  {
						  debug("number of client"); //pmh ?!?
						  symbol = barDay.Symbol; //pmh - 10/4/12 - was absent, but not sure if needed
						  open = Convert.ToDecimal(barDay.Open, System.Globalization.CultureInfo.InvariantCulture);
						  high = Convert.ToDecimal(barDay.High, System.Globalization.CultureInfo.InvariantCulture);
						  low = Convert.ToDecimal(barDay.Low, System.Globalization.CultureInfo.InvariantCulture);
						  close = Convert.ToDecimal(barDay.Close, System.Globalization.CultureInfo.InvariantCulture);
						  vol = Convert.ToInt64(barDay.TotalVolume, System.Globalization.CultureInfo.InvariantCulture);
						  date = Util.ToTLDate(barDay.CloseDate);
						  time = 0;
						  MbtCustInt = MbtDayInt2CustInt((int)br.Interval / 100);
						  b = new BarImpl(open, high, low, close, vol, date, time, barDay.Symbol, MbtCustInt);
						  debug("number of client" + tl.NumClients);
						  debug("bar" + BarImpl.Serialize(b));
						  tl.TLSend(BarImpl.Serialize(b), MessageTypes.BARRESPONSE, br.Client);
						  barDay.Previous();
					  }
					  //use this message to inform that the data for requestID is completed
					  tl.TLSend(Convert.ToString(lRequestId), MessageTypes.CUSTOM40, br.Client);
					  break;

				  case 2:
					  MbtHistMinBar barMin = pHist as MbtHistMinBar;
					  debug("Min bar, " + barMin.Count + " recs");
					  barMin.Last();
					  while (!barMin.Bof)
					  {
						  symbol = barMin.Symbol; //pmh - 10/4/12 - was absent, but not sure if needed
						  open = Convert.ToDecimal(barMin.Open, System.Globalization.CultureInfo.InvariantCulture);
						  high = Convert.ToDecimal(barMin.High, System.Globalization.CultureInfo.InvariantCulture);
						  low = Convert.ToDecimal(barMin.Low, System.Globalization.CultureInfo.InvariantCulture);
						  close = Convert.ToDecimal(barMin.Close, System.Globalization.CultureInfo.InvariantCulture);
						  vol = Convert.ToInt64(barMin.TotalVolume, System.Globalization.CultureInfo.InvariantCulture);
						  date = Util.ToTLDate(barMin.LocalDateTime);
						  time = Util.ToTLTime(barMin.LocalDateTime);
						  MbtCustInt = MbtMinInt2CustInt((int)br.Interval / 100);
						  b = new BarImpl(open, high, low, close, vol, date, time, barMin.Symbol, MbtCustInt);
						  tl.TLSend(BarImpl.Serialize(b), MessageTypes.BARRESPONSE, br.Client);
						  barMin.Previous();
					  }
					  //use this message to inform that the data for requestID is completed
					  tl.TLSend(Convert.ToString(lRequestId), MessageTypes.CUSTOM40, br.Client);
					  break;

				  case 3:
					  /* pmh
					   * 0 = All ticks
					   * 1 = Trade ticks
					   * 2 = Bid/Ask ticks
					   * 3 = Bid ticks
					   * 4 = Ask ticks
					   */
					  MbtHistTick barTick = pHist as MbtHistTick;
					  debug("Tick bar, " + barTick.Count + " recs (showing only 1 - Trade ticks)");
					  TickImpl k = new TickImpl(barTick.Symbol);
					  //MBT default tick data is trade data
					  int lTickFilter = 1;
					  barTick.First();
					  switch (lTickFilter)
					  {
						  case 1:
							  while (!barTick.Bof)
							  {
								  k.symbol = barTick.Symbol; //pmh - 10/4/12 - was absent, but not sure if needed
								  k.trade = Convert.ToDecimal(barTick.Price);
								  k.ex = barTick.Exchange;
								  k.size = barTick.Volume;
								  k.date = Util.ToTLDate(barTick.LocalDateTime);
								  k.time = Util.ToTLTime(barTick.LocalDateTime);
								  SendNewTick(k);
								  barTick.Previous();
							  };
							  break;
					  }
					  break;

				  case 4: //pmh - 9/15/12 - PV bars
					  MbtHistPVBar barPV = pHist as MbtHistPVBar;
					  debug("PV bar, " + barPV.Count + " recs");
					  barPV.Last();
					  while (!barPV.Bof)
					  {
						  open = high = low = 0;
						  symbol = barPV.Symbol; //pmh - 10/4/12 - was absent, but not sure if needed
						  close = Convert.ToDecimal(barPV.Price, System.Globalization.CultureInfo.InvariantCulture);
						  vol = Convert.ToInt64(barPV.Volume, System.Globalization.CultureInfo.InvariantCulture);
						  date = Util.ToTLDate(barPV.LocalDateTime);
						  time = Util.ToTLTime(barPV.LocalDateTime);
						  MbtCustInt = MbtMinInt2CustInt((int)br.Interval / 100);
						  b = new BarImpl(open, high, low, close, vol, date, time, barPV.Symbol, MbtCustInt);
						  tl.TLSend(BarImpl.Serialize(b), MessageTypes.BARRESPONSE, br.Client);
						  barPV.Previous();
					  }
					  //use this message to inform that the data for requestID is completed
					  tl.TLSend(Convert.ToString(lRequestId), MessageTypes.CUSTOM40, br.Client);
					  break;
			  }

			  if (_barrequests.hasItems)
			  {
				  // BarRequest br1= new BarRequest();
				  try
				  {
					  br = _barrequests.Read();
					  submitBarRequest(br);
				  }
				  catch (Exception ex)
				  {
					  debug("error on historical bar request: " + br.ToString());
					  debug(ex.Message + ex.StackTrace);
				  }
			  }
			  else waitforhistorical2complete = false;
		  }

		 void m_HistMgr_OnError(int lRequestId, object pHist, enumHistErrorType err)
		  {
			  debug(String.Format("m_HistMgr_OnError: lRequestId: {0}, enumHistErrorType: {1}", lRequestId, err));
		  }

		  //pmh #####################################################################################################
		  //pmh #####################################################################################################

		  public bool isValid = false; //pmh - used for what?!?
		  public event VoidDelegate LoginEvent;

		  void PositionHasChanged(MbtPosition pPos) //pmh - Renamed from OnPositionChanged. This is not an event - don't name it so.
		  {
			  debug(String.Format("PositionHasChanged {0} {1}", pPos.Symbol, pPos.AggregatePosition));
			  string sym = pPos.Symbol;
			  //TODO: check accuracy of AveragePrice2. Currently known to not correctly include prices from further than 1 day back
			  //AveragePrice2 only available in release candidates so reverting to the calc below that
			  //decimal price = (decimal) pPos.AveragePrice2;
			  decimal price =
//pmh check each, not together   (pPos.IntradayPosition + pPos.OvernightPosition != 0)
						(pPos.IntradayPosition != 0 && pPos.OvernightPosition != 0)       //pmh - do this instead!
						?
//pmh bug!        (decimal)(((pPos.IntradayPosition * pPos.IntradayPrice) + (pPos.OvernightPosition * pPos.OvernightPrice)) / (pPos.IntradayPrice + pPos.OvernightPosition))
						(decimal)(((pPos.IntradayPosition * pPos.IntradayPrice) + (pPos.OvernightPosition * pPos.OvernightPrice)) / (pPos.IntradayPosition + pPos.OvernightPosition))
						:
						0;
			  int size = pPos.AggregatePosition; //TODO: make this pPos.RealizedPNL2 when it is available
			  decimal cpl = (decimal)pPos.RealizedPNL;
			  string account = pPos.Account.Account;
			  Position p = new PositionImpl(sym, price, size, cpl, account);
			  pt.NewPosition(p);
		  }

		 Position[] tl_newPosList(string pAcct)
        {
            test();
				v("received position list request for account: " + pAcct);
            int num = m_OrderClient.Positions.Count;
            //TODO: enable some settings in app.config, i.e. VerboseDebugging as done in ServerSterling
            //debug(String.Format("tl_newPosList called for {0} positions:{1}", account, num));
            Position[] posl = new Position[num];
            for (int i = 0; i < num; i++)
            {
					string sym = m_OrderClient.Positions[i].Symbol;
					decimal price = (decimal)m_OrderClient.Positions[i].IntradayPrice;
					int size = m_OrderClient.Positions[i].IntradayPosition;
					decimal cpl = (decimal)m_OrderClient.Positions[i].RealizedPNL;
					string acct = m_OrderClient.Positions[i].Account.Account;
					Position p = new PositionImpl(sym, price, size, cpl, acct);
					posl[i] = p;
					//debug(String.Format("tl_newPosList {4} i:{0} {1} {2} {3}", i, sym, price, size, acct));
            }
            return posl;
        }

        /// <summary>
        /// Shortcut to get an order via the order number
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        MbtOpenOrder GetOrderByOrderNumber(string OrderNumber)
        {
            MbtOpenOrder pOrd;
				orders.TryGetValue(OrderNumber, out pOrd);
            //return m_OpenOrders.FindByOrderNumber(orderNumber);
            return pOrd; //pmh - what happens if not found?!?
        }

        /// <summary>
        /// Get the order for a history object then convert that to a TradeLink order
        /// </summary>
        /// <param name="pHist"></param>
        /// <returns></returns>
        OrderImpl HistToTradeLinkOrder(MbtOrderHistory pHist)
        {
            MbtOpenOrder pOrd = GetOrderByOrderNumber(pHist.OrderNumber);
				return ToTradeLinkOrder(pOrd); //pmh - what happens if not found?!?
        }

        /// <summary>
        /// Get the order for a history object then convert that to a TradeLink fill
        /// </summary>
        /// <param name="pHist"></param>
        /// <returns></returns>
        TradeImpl HistToTradeLinkFill(MbtOrderHistory pHist)
        {
            MbtOpenOrder pOrd = GetOrderByOrderNumber(pHist.OrderNumber);
            TradeImpl fill = ToTradeLinkFill(pOrd, pHist);
				/*			  
				//clean it up if the order is completely filled
				if( pHist.Quantity == pHist.SharesFilled )
				{
					//order has been completely filled so do cleanup
					string bid = pOrd.OrderNumber;
					debug(String.Format("Cleaning up orders for {0}", bid));
					//clean up the orders list
					if( orders.ContainsKey(bid) ) orders.Remove(bid);
					else debug(String.Format("order {0} did not exist in orders", bid));
					//clean up mappings
					long tlid = 0;
					if( broker2tl.TryGetValue(bid, out tlid ) )
					{
						//got the tradelink id so clear the broker mapping
						debug(String.Format("Cleaning up broker2tl mapping {0} - {1}", bid, tlid));
						broker2tl.Remove(bid);
						//then clear the tradelink mapping
						debug(String.Format("cleaning up tl2broker mapping {0} - {1}", tlid, bid));
						if( tl2broker.ContainsKey(tlid ) ) tl2broker.Remove(tlid);
						else debug(String.Format("Order {0} did not have a matching tlid {1} to remove.", bid, tlid));
					}
					//clear the order from the orders list
					orders.Remove(pOrd.OrderNumber);
				}
				*/
            return fill;
        }

        IdTracker _idt = new IdTracker();
        /// <summary>
        /// Convert an MbtOpenOrder to a TradeLink OrderImpl and check if the order token
        /// is mapped to an existing TradeLink order. If so, replace the token with the OrderNumber and
        /// update the mapping for tl2broker with the order number as well. If not generate a new order number
        /// </summary>
        /// <param name="pOrd"></param>
        /// <returns></returns>
        OrderImpl ToTradeLinkOrder(MbtOpenOrder pOrd)
        {
            //get the token
            string token = pOrd.Token;
            //get the actual broker ID number now
            string bid = pOrd.OrderNumber, s = "";
            //check for a matching tradelink order
            long tlid = 0, v = 0;
            //first try to get the tlid by token
            if (!broker2tl.TryGetValue(token, out tlid))
            {
                //ok now try to get it by broker OrderNumber if it has been processed once before.
                if (!broker2tl.TryGetValue(bid, out tlid))
                {
                    //this order must be initiated by MB Trading so assign a new one
                    tlid = _idt.AssignId;
                    debug(String.Format("No matching TradeLink ID found for token {0}, generated {1}", token, tlid));
                }
                //save the mappings
                if (!broker2tl.ContainsKey(bid))
                {
                    broker2tl.Add(bid, tlid);
                }
                else
                {
                    broker2tl.TryGetValue(bid, out v);
                    debug(String.Format("WARNING! OrderNumber {0} is already mapped to {1}. Overwriting it with {2}", bid, v, tlid));
                    broker2tl[bid] = tlid;
                }
                if (!tl2broker.ContainsKey(tlid))
                {
                    tl2broker.Add(tlid, bid);
                }
                else
                {
                    tl2broker.TryGetValue(tlid, out s);
                    debug(String.Format("WARNING! TradeLinkID {0} is already mapped to {1}. Overwriting it with {2}", tlid, s, bid));
                    tl2broker[tlid] = bid;
                }
            }
            else
            {
                //we have a match so this order was generated by TradeLink
                //overwrite the Token with the OrderNumber
                debug(String.Format("Overwriting token {0} with OrderNumber {1} for TradeLink order ID {2}", token, bid, tlid));
                tl2broker[tlid] = bid;
                //remove the Token mapping
                broker2tl.Remove(token);
                //and map the OrderNumber to the TradeLink ID
                if (!broker2tl.ContainsKey(bid)) broker2tl.Add(bid, tlid);
            }
            OrderImpl o = new OrderImpl(pOrd.Symbol, pOrd.Quantity);
            o.id = tlid;
            o.Account = pOrd.Account.Account;
            o.side = (pOrd.BuySell == MBConst.VALUE_BUY);
            o.price = (decimal)pOrd.Price;
            o.stopp = (decimal)pOrd.StopPrice;
            o.ValidInstruct = pOrd.TimeInForce == MBConst.VALUE_DAY ? OrderInstructionType.DAY : OrderInstructionType.GTC;
            o.time = Util.DT2FT(pOrd.UTCDateTime);
            o.date = Util.ToTLDate(pOrd.UTCDateTime);
            o.trail = (decimal)pOrd.TrailingOffset;
            return o;
        }

        TradeImpl ToTradeLinkFill(MbtOpenOrder pOrd, MbtOrderHistory pHist)
        {
            //debug(String.Format("pOrd\n{0}\n\npHist\n{1}", Util.DumpObjectProperties(pOrd), Util.DumpObjectProperties(pHist)));
            debug(String.Format("pOrd: {0}", DisplayOrder(pOrd)));
            //TODO:Add hstory to this debug
            TradeImpl f = new TradeImpl();
            f.symbol = pOrd.Symbol;
            f.Account = pOrd.Account.Account;
            //f.xprice = (pOrd.Price > 0) ? (decimal)pOrd.Price : (decimal)pOrd.StopLimit;
            //f.xprice = Math.Abs((decimal)pOrd.Price);
            f.xprice = (decimal)pHist.Price;
            //f.xsize = pHist.Event == "Executed" ? pHist.Quantity : pHist.SharesFilled;
            f.xsize = pHist.Quantity;
            f.side = (pOrd.BuySell == MBConst.VALUE_BUY);
            f.xtime = Util.DT2FT(pOrd.UTCDateTime);
            f.xdate = Util.ToTLDate(pOrd.UTCDateTime);
            long tlid = 0;
            if (broker2tl.TryGetValue(pOrd.OrderNumber, out tlid))
                f.id = tlid;
            else
					debug(String.Format("WARNING: No order matching fill for pOrd.OrderNumber {0}", pOrd.OrderNumber));
            //debug(String.Format("New fill {1}\n is valid:{0}\ndump:{2}", f.isValid, f.ToString(), Util.DumpObjectProperties(f)));
            debug(String.Format("New fill {1} is valid? {0}", f.isValid, f.ToString()));
            return f;
        }


		 bool test() 
		 {
			 return test("");
		 }
        bool test(Order o) 
		  {
			  return test(o.Account);
		  }
        bool test(string tlAcct)
        {
            if (!m_ComMgr.IsConnected)
            {
					debug("!m_ComMgr.IsConnected! Cannot test(" + tlAcct + ").");
                return false;
            }
            if (tlAcct != null && tlAcct != "")
            {
                int num = m_OrderClient.Accounts.Count;
                for (int i = 0; i < num; i++)
                {
                    if (m_OrderClient.Accounts[i].Account != null && tlAcct == m_OrderClient.Accounts[i].Account 
                        && m_OrderClient.Accounts[i].State == enumAcctState.asLoaded)
                        return true;
                }
                debug("test(" + tlAcct + ") failed!");
                return false;
            }
				/*
				 //pmh - 9/21/12 - See note on DefaultAccount at end of document!
            else
            {
                if (m_OrderClient.Accounts.DefaultAccount != null && m_OrderClient.Accounts.DefaultAccount.State == enumAcctState.asLoaded)
                    return true;
                else
                {
						 debug("Message rejected, default account is !asLoaded");
                    return false;
                }
            }
				 */
				return true;
        }
        string tl_newAcctRequest()
        {
            test();
            v("received account request.");
            int num = 0;
            if (m_OrderClient.Accounts != null)
                num = m_OrderClient.Accounts.Count;
            string[] accts = new string[num];
            for (int i = 0; i < num; i++)
                accts[i] = m_OrderClient.Accounts[i].Account;
            //TODO:add this when verbose debugging is enabled
            //debug(String.Format("Accounts requested: {0}", string.Join(",", accts)));
            string acctmsg = string.Join(",", accts);
            v("sending accounts available as: " + acctmsg);
            return acctmsg;
        }

        /// <summary>
        /// TradeLink sent a cancel so we need to map the cancel to an existing
        /// broker OrderNumber and cancel that
        /// </summary>
        /// <param name="pRec"></param>
        void tl_newOrderCancelRequest(long tlid)
        {
            test();
            string res = null;
            string bid = null;
            if (tl2broker.TryGetValue(tlid, out bid))
            {
                if (!canceledids.ContainsKey(tlid) && !cancelids.ContainsKey(bid))
                {
                    //this order has never been canceled or already had a cancel request placed on it
                    m_OrderClient.Cancel(bid, ref res);
                    debug(String.Format("Cancel request for order {1} received from TradeLink. msg: {0}", res, tlid));
                    //mark it as having had a cancel request
                    cancelids.Add(bid, true);
                }
                else if (!canceledids.ContainsKey(tlid) && cancelids.ContainsKey(bid))
                {
                    //this order has had a cancel request but has not yet completed being canceled
                    debug(String.Format("WARNING: Repeated cancel request for order {0} received from TradeLink. MBT Order is {1}. Checking if complete...", tlid, bid));
                    if (m_OpenOrders.FindByOrderNumber(bid) == null)
                    {
                        //this order has been canceled successfully. Clean up.
                        debug(String.Format("WARNING: Repeated cancel request for order that no longer exists: {0} received from TradeLink. MBT Order was {1}. Checking if complete...", tlid, bid));
                        if (broker2tl.ContainsKey(bid))
                            broker2tl.Remove(bid);
                        if (tl2broker.ContainsKey(tlid))
                            tl2broker.Remove(tlid);
                        if (orders.ContainsKey(bid))
                            orders.Remove(bid);
                        //make sure tradelink got the message!
                        tl.newCancel(tlid);
                        canceledids.Add(tlid, true);
                    }
                }
            }
            else if (canceledids.ContainsKey(tlid))
            {
                //this order or has successfully canceled
                //m_OrderClient.Cancel(bid, ref res);
                debug(String.Format("WARNING: Repeated cancel request for completely canceled order {0} received from TradeLink. No current MBT Order exists for it", tlid));
                //make sure TradeLink really REALLY gets the message this time!
                tl.newCancel(tlid);
            }
            else
                debug(String.Format("No matching broker order found to cancel for {0}", tlid));
        }

		 void SendNewTick(TickImpl k)
        {
            if (!DisableOldTicks)
                tl.newTick(k);
            else if (k.datetime >= _lasttime)
            {
                _lasttime = k.datetime;
                tl.newTick(k);
            }
        }

        void MBTQUOTELib.IMbtQuotesNotify.OnOptionsData(ref OPTIONSRECORD pRec)
        {
			  //pmh ToDo
            //not yet implemented
        }

        void MBTQUOTELib.IMbtQuotesNotify.OnTSData(ref TSRECORD pRec)
        {
            TickImpl k = new TickImpl();
            k.symbol = pRec.bstrSymbol;
            enumTickType tt = (enumTickType)pRec.lType;
            switch (tt)
            {
                case enumTickType.ttAskTick:
                    k.ask = (decimal)pRec.dPrice;
                    k.oe = pRec.bstrExchange;
                    k.os = pRec.lSize;
                    break;
                case enumTickType.ttBidTick:
                    k.bid = (decimal)pRec.dPrice;
                    k.be = pRec.bstrExchange;
                    k.bs = pRec.lSize;
                    break;
                case enumTickType.ttTradeTick:
                    k.trade = (decimal)pRec.dPrice;
                    k.ex = pRec.bstrExchange;
                    k.size = pRec.lSize;
                    break;
            }

            //tl.newTick(k);
            SendNewTick(k);
        }

        /// <summary>
        /// Provides quote data but each quote has both a bid and ask. As forex does not actually have a trade in the quotes,
        /// the Response must manage how it wants to handle the bid/ask, i.e. if you're long send....
        /// pt[symbol].isLong ? k.bid : k.ask.... and opposite if you're entering
        /// </summary>
        /// <param name="pQuote"></param>
        void MBTQUOTELib.IMbtQuotesNotify.OnQuoteData(ref QUOTERECORD pQuote)
        {
            TickImpl k = new TickImpl(pQuote.bstrSymbol);
            k.time = Util.DT2FT(pQuote.UTCDateTime);
            k.date = Util.ToTLDate(DateTime.UtcNow.Date);
            k.ask = (decimal)pQuote.dAsk;
            k.bid = (decimal)pQuote.dBid;
            k.os = k.AskSize = pQuote.lAskSize;
            k.bs = k.BidSize = pQuote.lBidSize;
            k.ex = k.be = k.oe = pQuote.bstrMarket;
            SendNewTick(k);
        }

        /// <summary>
        /// Processing Level2 data requires tracking the actual current best ask and bid. That's on the TODO
        /// Currently using Level1 data from OnQuoteData
        /// </summary>
        /// <param name="pRec"></param>
        void MBTQUOTELib.IMbtQuotesNotify.OnLevel2Data(ref LEVEL2RECORD pRec)
        {
            TickImpl k = new TickImpl(pRec.bstrSymbol);
            k.ex = pRec.bstrSource;
            k.time = Util.DT2FT(pRec.UTCTime);
            k.date = Util.ToTLDate(DateTime.UtcNow.Date);
            enumMarketSide ems = (enumMarketSide)pRec.side;
            switch (ems)
            {
                case enumMarketSide.msAsk:
                    k.ask = (decimal)pRec.dPrice;
                    k.oe = pRec.bstrSource;
                    k.os = k.AskSize = pRec.lSize;
                    k.trade = k.ask;
                    k.size = k.AskSize;
                    break;
                case enumMarketSide.msBid:
                    k.bid = (decimal)pRec.dPrice;
                    k.be = pRec.bstrSource;
                    k.bs = k.BidSize = pRec.lSize;
                    k.trade = k.bid;
                    k.size = k.BidSize;
                    break;
            }
            SendNewTick(k);
        }


        /// <summary>
        /// Submit a new order and if successful and if the o.id != 0, map the incoming TradeLink o.ID to the order Token returned in res
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        long tl_newSendOrderRequest(Order o)
        {
            test(o);
            v("received new order: " + o.ToString());
            string strType = "MBConst.VALUE_MARKET";
            int side = o.side ? MBConst.VALUE_BUY : MBConst.VALUE_SELL;
            int tif = MBConst.VALUE_GTC;
            //TODO: need to modify type depending on the type of order
            int otype = MBConst.VALUE_MARKET;
            //debug(String.Format("new order isLimit:{0} isStop:{1} isMarket:{2}", o.isLimit, o.isStop, o.isMarket));
            if (o.isMarket)
            {
                otype = MBConst.VALUE_MARKET;
                strType = "MBConst.VALUE_MARKET";
            }
            else if (o.isLimit && o.isStop)
            {
                otype = MBConst.VALUE_STOP_LIMIT;
                strType = "MBConst.VALUE_STOP_LIMIT";
            }
            else if (o.isLimit)
            {
                otype = MBConst.VALUE_LIMIT;
                strType = "MBConst.VALUE_LIMIT";
            }
            else if (o.isStop)
            {
                otype = MBConst.VALUE_STOP_MARKET;
                strType = "MBConst.VALUE_STOP_MARKET";
            }
            else if (o.isTrail)
            {
                otype = MBConst.VALUE_TRAILING_STOP;
                strType = "MBConst.VALUE_TRAILING_STOP";
            }
            else
            {
                strType = "UNKNOWN";
            }
            debug(String.Format("TradeLink order is type: {0}:{1}", otype, strType));
            if (o.comment != null && o.comment != "")
                debug(String.Format("Order has comment:{0}", o.comment));
            string route = "MBTX";
            int voltype = MBConst.VALUE_NORMAL;
            DateTime dt = new DateTime(0);
				string pbstrRetMsg = null;
            string token = null;
            MbtAccount m_account = getaccount(o.Account);
				bool bManuallyPlaced = false;         // 9/13/12 - pmh

            //bool good = m_OrderClient.Submit(side, o.UnsignedSize, o.symbol, (double)o.price, (double)o.stopp, tif, 0, otype, voltype, 0, m_account, route, "", 0, 0, dt, dt, 0, 0, 0, 0, 0, ref res);
            bool bSubmit = m_OrderClient.Submit(
                side, //lBuySell
                o.UnsignedSize,
                o.symbol,
                (double)o.price,
                (double)o.stopp,
                tif,
                MBConst.VALUE_AGENCY,					 //0,   // 9/1/12 - pmh lCapacity cannot be zero
                otype,
                voltype,
                0, //lDisplayQty
                m_account,
                route,
                "", //Deprecated - bstrPrefMMID
                0, //dPriceOther
                0, //dPriceOther2
                dt, //timeActivate - previously not used, not sure at the moment
                dt, //timeExpire - previously not used and not yet in the new UI, still checking on this
                0, //lExpMonth
                0, //lExpYear
                0, //dStrikePrice
                "", //bstrCondSymbol - just added, not sure if it's implemented yet
                0, //lCondType - just implemented
                0, //dCondPrice - just added, not sure if it's implemented yet
					 0,                 // 9/13/12 - pmh - Group Type
					 "",                // 9/13/12 - pmh - Group ID
					 bManuallyPlaced,   // 9/13/12 - pmh
               ref pbstrRetMsg);
				if (!bSubmit)
            {
			    	//debug(String.Format("The following order failed: {0}\nreason:{1} {2}", o, o.Account, pbstrRetMsg));      
					debug(String.Format("Order failed: [{o}]. Account [{1}]. pbstrRetMsg: [{2}]", o, o.Account, pbstrRetMsg)); // 9/13/12 - pmh
				}
            else
            {
					token = pbstrRetMsg;
                //debug(String.Format("Order sent to server and waiting success: {0}\ninfo: {1} {2}", o, o.Account, token));
					debug(String.Format("Order sent: [{o}]. Account [{1}]. token: [{2}]", o, o.Account, token));             // 9/13/12 - pmh
					// if the order ID is not 0, map it to the token
                if (o.id != 0)
                {
                    //get the broker Token and save the association
                    if (!tl2broker.ContainsKey(o.id))
                    {
                        debug(String.Format("Mapping TL:{0} to token:{1}", o.id, token));
                        tl2broker.Add(o.id, token);
                    }
                    else
                    {
                        //the id was there so update the token with it
                        debug(String.Format("WARNING! Updating TL:{0} with token:{1}", o.id, token));
                        tl2broker[o.id] = token;
                    }
                    if (!broker2tl.ContainsKey(token))
                    {
                        debug(String.Format("Mapping token:{0} to TL:{1}", token, o.id));
                        broker2tl.Add(token, o.id);
                    }
                    else
                    {
                        debug(String.Format("Warning: Updating token:{0} with TL:{1}", token, o.id));
                        broker2tl[token] = o.id; // this actually shouldn't be called...!
                    }
                    debug(String.Format("Mapped TL order {0} to token {1}", o.id, token));
                }
                else
                {
                    debug("WARNING! Incoming TL order does not have an id. It will be generated.");
                }
            }
            return (long)MessageTypes.OK;
        }

        public bool Start(int id, string user, string pw)
        {
            v("starting up MBTrading connector...");
            if (m_ComMgr == null)
            {
					debug("m_ComMgr == null. Is MBT Desktop even installed?  Check Broker config guide on TradeLink project site.");
                System.Diagnostics.Process.Start(@"http://code.google.com/p/tradelink/wiki/ComFactoryErrors");
                return false;                
            }
            v("MBT Desktop appears installed, attempting login...");
            var ok = m_ComMgr.DoLogin(id, user, pw, "");
            if (LoginEvent != null)
                LoginEvent();
            return ok;
        }

        MbtAccount getaccount(string name) 
		  {
			  foreach (MbtAccount a in m_OrderClient.Accounts)
				  if (a.Account == name) 
					  return a;
			  //pmh - 10/4/12 - return m_OrderClient.Accounts.DefaultAccount;
			  return m_OrderClient.Accounts[0];
		  }

        MessageTypes[] tl_newFeatureRequest()
        {
            v("received feature request");
            List<MessageTypes> f = new List<MessageTypes>();
            f.Add(MessageTypes.EXECUTENOTIFY);
            f.Add(MessageTypes.ORDERCANCELRESPONSE);
            f.Add(MessageTypes.FEATUREREQUEST);
            f.Add(MessageTypes.FEATURERESPONSE);
            f.Add(MessageTypes.BROKERNAME);
            f.Add(MessageTypes.SENDORDER);
            f.Add(MessageTypes.ORDERCANCELREQUEST);
            f.Add(MessageTypes.ORDERNOTIFY);
            f.Add(MessageTypes.POSITIONREQUEST);
            f.Add(MessageTypes.POSITIONRESPONSE);
            f.Add(MessageTypes.TICKNOTIFY);
            f.Add(MessageTypes.SENDORDERMARKET);
            f.Add(MessageTypes.SENDORDERLIMIT);
            f.Add(MessageTypes.SENDORDERSTOP);
            f.Add(MessageTypes.LIVEDATA);
            f.Add(MessageTypes.LIVETRADING);
            f.Add(MessageTypes.SIMTRADING);
            f.Add(MessageTypes.BARREQUEST);
            f.Add(MessageTypes.BARRESPONSE);
            return f.ToArray();
        }

        public void Stop()
        {
            v("stopping ServerMB connector.");
				pt = null;
				m_HistMgr = null;
				m_OpenOrders = null;
				m_Quotes.UnadviseAll(this); //pmh - 9/18/12 - MUST NOT FORGET THIS!
            m_Quotes.Disconnect();
            m_OrderClient.Disconnect();
				m_Quotes = null;
				m_OrderClient = null;
				m_HistMgr = null;
			  
        }

        /// <summary>
        /// Return a descriptive string of this order
        /// TODO: one for history as well
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        string DisplayOrder(MbtOpenOrder order)
        {
            //string orderType = ( order.OrderType == MBConst.VALUE_MARKET ) ? MBConst.TYPE_MARKET : MBConst.TYPE_STOP;
            string retVal = "";
				retVal += "BuySell=" + (order.BuySell == MBConst.VALUE_BUY ? "Buy" : (order.BuySell == MBConst.VALUE_SELL ? "Sell" : "undefined BuySell: " + order.BuySell));
            retVal += " Symbol=" + order.Symbol;
            retVal += " Quantity=" + order.Quantity;
            retVal += " Price=" + order.Price;
            retVal += " OrderType=" + order.OrderType; //compare with OrderType values
            retVal += " OrderNumber=" + order.OrderNumber;
            retVal += " Token=" + order.Token;
            retVal += " SharesFilled=" + order.SharesFilled;
            return retVal;
        }

        string DisplayAccount(MbtAccount acct) //pmh - spelled things out.
        {
            string retVal = "";
				retVal += "CurrentEquity=" + acct.CurrentEquity;
				retVal += " CurrentExcess=" + acct.CurrentExcess;
            retVal += " MMRUsed=" + acct.MMRUsed;
            retVal += " MMRMultiplier=" + acct.MMRMultiplier;
            retVal += " PermedForForex=" + acct.PermedForForex;
            return retVal;
        }

        public event DebugDelegate SendDebugEvent;
        void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);
        }
    }

    public static class MBConst
    {
        public const int tickDown = 1;
        public const int tickEvenUp = 0;

        // Buy/Sell action values
        public const int VALUE_BUY = 10000;
        public const int VALUE_SELL = 10001;
		  public const int VALUE_SELLSHT = 10002; //pmh - Option legs (SubmitStrategy) should always be sent as VALUE_SELL, never VALUE_SELLSHT

        // Time-in-force values
        public const int VALUE_GTC = 10008; 		// mandatory for Forex;
        public const int VALUE_DAYPLUS = 10009;	// ARCA & INET after hours;
        public const int VALUE_IOC = 10010; 		// OTC;
        public const int VALUE_DAY = 10011;

        // Capacity values
        public const int VALUE_AGENCY = 10020;
        public const int VALUE_PRINCIPAL = 10021;

        // T&S record type
        public const int VALUE_TS_NORMAL = 30030; 		// inside market hours;
        public const int VALUE_TS_FORMT = 30031; 		// outside market hours;

        // OrderType values
        public const int VALUE_DISCRETIONARY = 10043;
        public const int VALUE_LIMIT = 10030;
        public const int VALUE_LIMIT_CLOSE = 10057;
        public const int VALUE_LIMIT_OPEN = 10056;
        public const int VALUE_LIMIT_STOPMKT = 10064;
        public const int VALUE_LIMIT_TRAIL = 10054;
        public const int VALUE_LIMIT_TTO = 10050;
        public const int VALUE_MARKET = 10031;
        public const int VALUE_MARKET_CLOSE = 10039;
        public const int VALUE_MARKET_OPEN = 10038;
        public const int VALUE_MARKET_STOP = 10069;
        public const int VALUE_MARKET_TRAIL = 10055;
        public const int VALUE_MARKET_TTO = 10051;
        public const int VALUE_PEGGED = 10062;
        public const int VALUE_RESERVE = 10040;
        public const int VALUE_RSV_DISC = 10044;
        public const int VALUE_RSV_PEGGED = 10066;
        public const int VALUE_RSV_TTO = 10052;
        public const int VALUE_STOPLMT_STOP = 10072;
        public const int VALUE_STOPLMT_TRAIL = 10068;
        public const int VALUE_STOPLMT_TTO = 10067;
        public const int VALUE_STOPMKT_LIMIT = 10076;
        public const int VALUE_STOP_LIMIT = 10033;
        public const int VALUE_STOP_MARKET = 10032;
        public const int VALUE_STOP_TRAIL = 10065;
        public const int VALUE_STOP_TTO = 10053;
        public const int VALUE_TRAILING_STOP = 10034;
        public const int VALUE_TTO_ORDER = 10037;
        public const int VALUE_VWAP = 10063;

        // Alert severities
        public const int ALERT_VAL_CRIT = 30001;
        public const int ALERT_VAL_NORMAL = 30000;
        public const int ALERT_VAL_REINSTATE = 30002;
        public const int ALERT_VAL_UPDATECXN = 30003;

        // Alert types
        public const int ALERT_TYPE_ACCT = 30010;
        public const int ALERT_TYPE_INTERNAL = 30014;
        public const int ALERT_TYPE_LOGOFF = 30013;
        public const int ALERT_TYPE_ORDSRV = 30012;
        public const int ALERT_TYPE_QUOTESRV = 30015;
        public const int ALERT_TYPE_USER = 30011;

        // Customer types
        public const int VALUE_RETAIL = 30021;
        public const int VALUE_DEMO = 30022;

        // Volume types
        public const int VALUE_NORMAL = 10042;
        public const int VALUE_PART = 10046;
    }
	 /*
	  * Concerning DefaultAccount:
	  * 
	  * DefaultAccount may be deprecated in the future. It is set using MRU 
	  * and/or some random order by the Trade Server, it cannot be relied 
	  * upon for consistency, it cannot be set by the user, and it is used only to 
	  * ensure that a new instance of any given control has a valid MbtAccount 
	  * object. Your app has the responsibility to manage and set accounts.
	  * 
	  * Phil Huether - 10/4/12
	 */
 }

