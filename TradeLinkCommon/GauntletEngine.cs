using TradeLink.API;

namespace TradeLink.Common
{
    /// <summary>
    /// lightweight backtesting class that includes binding of data and simulation components.
    /// </summary>
    public class GauntletEngine
    {
        protected TradeLink.API.Response responseengine;
        protected TickFileFilter tff;
        protected HistSim myhistsim;
        public HistSim Engine { get { return myhistsim; } }
        public TradeLink.API.Response response { get { return responseengine; } }
        public Broker SimBroker = new Broker();

        public GauntletEngine(TradeLink.API.Response r, HistSim h)
        {
            responseengine = r;
            bindresponse(ref responseengine);

            myhistsim = h;
            bindsim(ref myhistsim);
            
            SimBroker = new Broker();
            SimBroker.GotOrderCancel += new OrderCancelDelegate(SimBroker_GotOrderCancel);
            SimBroker.GotOrder += new OrderDelegate(responseengine.GotOrder);
            SimBroker.GotFill += new FillDelegate(responseengine.GotFill);
        }

        void bindsim(ref HistSim h)
        {
            h.GotTick += new TickDelegate(_h_GotTick);
        }
        void unbindsim(ref HistSim h)
        {
            h.GotTick -= new TickDelegate(_h_GotTick);
        }

        public virtual void UnbindEvents()
        {
            unbindresponse(ref responseengine);
            unbindsim(ref myhistsim);
        }


        void bindresponse(ref Response r)
        {
            r.SendOrderEvent += new OrderSourceDelegate(_r_SendOrder);
            r.SendCancelEvent += new LongSourceDelegate(_r_SendCancel);
            r.SendDebugEvent += new DebugDelegate(_r_SendDebugEvent);
            r.SendIndicatorsEvent += new ResponseStringDel(r_SendIndicatorsEvent);
            r.SendMessageEvent += new MessageDelegate(r_SendMessageEvent);
            r.SendTicketEvent += new TicketDelegate(r_SendTicketEvent);
            r.SendChartLabelEvent += new ChartLabelDelegate(r_SendChartLabelEvent);
            r.SendBasketEvent += new BasketDelegate(r_SendBasketEvent);

        }

        void r_SendBasketEvent(Basket b, int id)
        {
            
        }

        void r_SendChartLabelEvent(decimal price, int time, string label, System.Drawing.Color c)
        {
            
        }

        void r_SendTicketEvent(string space, string user, string password, string summary, string description, Priority pri, TicketStatus stat)
        {
            
        }

        void r_SendMessageEvent(MessageTypes type, long source, long dest, long msgid, string request, ref string response)
        {
            
        }

        void r_SendIndicatorsEvent(int idx, string data)
        {
            
        }

        void unbindresponse(ref Response r)
        {
            r.SendOrderEvent -= new OrderSourceDelegate(_r_SendOrder);
            r.SendCancelEvent -= new LongSourceDelegate(_r_SendCancel);
            r.SendDebugEvent -= new DebugDelegate(_r_SendDebugEvent);
            r.SendIndicatorsEvent -= new ResponseStringDel(r_SendIndicatorsEvent);
            r.SendMessageEvent -= new MessageDelegate(r_SendMessageEvent);
            r.SendTicketEvent -= new TicketDelegate(r_SendTicketEvent);
            r.SendChartLabelEvent -= new ChartLabelDelegate(r_SendChartLabelEvent);
            r.SendBasketEvent -= new BasketDelegate(r_SendBasketEvent);
        }
        
        public GauntletEngine(TradeLink.API.Response r, TickFileFilter inittff)
        {
            responseengine = r;
            responseengine.SendOrderEvent += new OrderSourceDelegate(_r_SendOrder);
            responseengine.SendCancelEvent += new LongSourceDelegate(_r_SendCancel);
            responseengine.SendDebugEvent += new DebugDelegate(_r_SendDebugEvent);
            tff = inittff;
            myhistsim = new MultiSimImpl(tff);
            myhistsim.GotTick += new TickDelegate(_h_GotTick);
            SimBroker.GotOrderCancel += new OrderCancelDelegate(SimBroker_GotOrderCancel);
            SimBroker.GotOrder += new OrderDelegate(responseengine.GotOrder);
            SimBroker.GotFill += new FillDelegate(responseengine.GotFill);

        }

        public event DebugDelegate SendEngineDebugEvent;
        void debug(string msg)
        {
            if (SendEngineDebugEvent != null)
                SendEngineDebugEvent(msg);
        }

        public void Go() 
        {
            SimBroker.Reset();
            if (responseengine!=null)
                responseengine.Reset();
            if (myhistsim != null)
            {
                myhistsim.Reset();
                myhistsim.PlayTo(MultiSimImpl.ENDSIM);
            }
            else
                debug("No simulation defined on gauntlet engine.");
                
        }

        void SimBroker_GotOrderCancel(string sym, bool side, long id)
        {
            responseengine.GotOrderCancel(id);
        }

        void SimBroker_GotOrder(Order o)
        {
            responseengine.GotOrder(o);
        }

        void _r_SendOrder(Order o, int id)
        {
            SimBroker.SendOrderStatus(o);
        }

        void _r_SendCancel(long number, int id)
        {
            SimBroker.CancelOrder(number);
        }

        void _h_GotTick(Tick t)
        {
            SimBroker.Execute(t);
            responseengine.GotTick(t);
        }

        void _r_SendDebugEvent(string msg)
        {
            
        }
    }
}
