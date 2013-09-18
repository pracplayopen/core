using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeLink.API
{
    public interface GenericViewI
    {
        // properties

        /// <summary>
        /// name of view
        /// </summary>
        string ViewName { get; set; }
        /// <summary>
        /// how much to sleep between model additions
        /// </summary>
        int Sleep { get; set; }
        /// <summary>
        /// number of view columsn frozen
        /// </summary>
        int ColFreeze { get; set; }
        /// <summary>
        /// id of view
        /// </summary>
        int id { get; set; }
        /// <summary>
        /// whether cache should be used
        /// </summary>
        bool TryCache { get; set; }
        /// <summary>
        /// who owns this view
        /// </summary>
        string Owner { get; set; }
        /// <summary>
        /// whether owner is defined
        /// </summary>
        bool isOwnerValid { get; }
        /// <summary>
        /// number of models in view
        /// </summary>
        int Count { get; }
        /// <summary>
        /// whether view uses extra debugging
        /// </summary>
        bool VerboseDebugging { get; set; }
        /// <summary>
        /// whether view colors negative numbers and percents (slower)
        /// </summary>
        bool isColoringEnabled { get; set; }



        /// <summary>
        /// get current positions
        /// </summary>
        event PositionArrayDelegate GetPositionsEvent;
        /// <summary>
        /// allows sending of orders from views
        /// </summary>
        event OrderDelegate SendOrderEvent;

        // events
        /// <summary>
        /// listen to view debug info
        /// </summary>
        event DebugDelegate SendDebugEvent;
        /// <summary>
        /// listen to status updates
        /// </summary>
        event DebugDelegate SendStatusEvent;
        /// <summary>
        /// be notified after models are created/added
        /// </summary>
        event VoidDelegate SendPostCreationEvent;
        /// <summary>
        /// request debug window be hidden/shown
        /// </summary>
        event VoidDelegate SendDebugVisibleToggleEvent;
        /// <summary>
        /// request another (next) view
        /// </summary>
        event VoidDelegate SendNextViewRequestEvent;
        /// <summary>
        /// request another (prev) view
        /// </summary>
        event VoidDelegate SendPreviousViewRequestEvent;
        /// <summary>
        /// request another specific view
        /// </summary>
        event DebugDelegate SendViewRequestEvent;
        /// <summary>
        /// request all views available
        /// </summary>
        event GetAvailableGenericViewNamesDel GetAvailViewsEvent;
        


        // methods
        /// <summary>
        /// get symbols from selected models
        /// </summary>
        /// <returns></returns>
        List<string> getselectedsymbols();
        /// <summary>
        /// get symbols for all models
        /// </summary>
        /// <returns></returns>
        List<string> getallsymbols();
        /// <summary>
        /// clear all models from view
        /// </summary>
        void Clear();
        /// <summary>
        /// hide view
        /// </summary>
        void Hide();
        /// <summary>
        /// show view
        /// </summary>
        void Show();
        /// <summary>
        /// toggle view between hidden/shown
        /// </summary>
        void Toggle();
        /// <summary>
        /// reset the grid
        /// </summary>
        void Reset();
        /// <summary>
        /// refresh grid now
        /// </summary>
        void refreshnow();
        /// <summary>
        /// recreate right click menu
        /// </summary>
        void CreateRightClick();

        /// <summary>
        /// add models for symbols
        /// </summary>
        /// <param name="symbols"></param>
        void addsymbols(params string[] symbols);
    }
}
