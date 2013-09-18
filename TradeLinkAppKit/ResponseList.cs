using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TradeLink.API;

namespace TradeLink.AppKit
{
    /// <summary>
    /// lets users choose from responses in a dll
    /// </summary>
    public partial class ResponseList : Form
    {
        public ResponseList()
        {
            InitializeComponent();
            BackColor = Color.White;
            ForeColor = Color.Black;
        }

        public ResponseList(List<string> responses)
        {
            InitializeComponent();
            _list.Items.Clear();
            foreach (string r in responses)
                _list.Items.Add(r);
            _list.Sorted = true;
            _list.Invalidate(true);
        }

        public static string GetUserResponseName(string dll, DebugDelegate debs)
        {
            rname = string.Empty;

            // get all responses
            var all = TradeLink.Common.ResponseLoader.GetResponseList(dll,debs);

            // prompt user
            ResponseList rl = new ResponseList(all);
            rl.ResponseSelected += new DebugDelegate(rl_ResponseSelected);
            if (rl.ShowDialog() != DialogResult.OK)
            {
                rname = string.Empty;
                if (debs != null)
                    debs("User canceled response name selection.");
            }
            rl = null;

            // return result
            return rname;
        }

        static string rname = string.Empty;

        static void rl_ResponseSelected(string msg)
        {
            rname = msg;
        }

        public event DebugDelegate ResponseSelected;


        private void _choose_Click(object sender, EventArgs e)
        {

        }

        private void _list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((_list.SelectedIndex < 0)) return;
            string r = _list.Items[_list.SelectedIndex].ToString();
            if (ResponseSelected != null)
                ResponseSelected(r);
            this.DialogResult = DialogResult.OK;
            Visible = false;
            Invalidate(true);
        }
    }
}
