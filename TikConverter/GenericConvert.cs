using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TradeLink.API;
using TradeLink.Common;
using System.IO;

namespace TikConverter
{
    public partial class GenericConvert : Form
    {
        public GenericConvert()
        {
            InitializeComponent();
            importtype.Items.AddRange(ConvertMap.GetConvertTypes());
        }

        public List<string> files = new List<string>();

        string exfile { get { return files.Count == 0 ? string.Empty : files[0]; } }

        List<string> headers = new List<string>();
        List<int> headercol = new List<int>();
        List<List<string>> data = new List<List<string>>();

        void setupexample()
        {
            // parse csv
            var csv = Util.getfile(exfile,debug);
            if (string.IsNullOrWhiteSpace(csv))
            {
                status("File could not be read, see main window for details.");
                return;
            }
            try
            {
                headers = Util.ParseCsvHeaderData(csv, debug);
                for (int i = 0; i < headers.Count; i++)
                {
                    headercol.Add(i);
                }
                inputfields.Items.Clear();
                inputfields.Items.AddRange(headers.ToArray());
                var headsimple = string.Join(" ", headers).ToUpper();
                if (headsimple.Contains("OPEN") || headsimple.Contains("CLOSE") || headsimple.Contains("HIGH"))
                    importtype.Text = ConvertMapType.Bar.ToString();
                else
                    importtype.Text = ConvertMapType.Level1Full.ToString();

                data = Util.ParseCsvData(csv, debug);

                isvalidconvert();
            }
            catch (Exception ex)
            {
                debug("unexpected error parsing file: " + exfile + ", it must be comma seperated and contain a header. err: " + ex.Message + ex.StackTrace);
                status("An error occured with your file, see main window for more info.");
                return;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // get some files
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All Files (*.*)|*.*";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                files.Clear();
                files.AddRange(ofd.FileNames);
                status("Selected " + files.Count + " files of the same format.");
                if (files.Count > 5)
                    filelist.Text = Path.GetFileName(files[0]) + " and " + (files.Count - 1) + " other files.";
                else
                {
                    var fis = string.Empty;
                    foreach (var fi in files)
                        fis += Path.GetFileName(fi) + " ";
                    filelist.Text = fis;
                    // setup example for user to control conversion
                    setupexample();
                }
            }
            else
                status("Selection canceled.");
        }

        public event DebugDelegate SendDebugEvent;

        void debug(string msg)
        {
            if (SendDebugEvent != null)
                SendDebugEvent(msg);
        }

        void status(string msg)
        {
            if (InvokeRequired)
                Invoke(new DebugDelegate(status), new object[] { msg });
            else
            {
                _stat.Text = msg;
                _stat.Invalidate();
            }
        }

        int selcsv { get { return inputfields.SelectedIndex; } }
        int selcount { get { return inputfields.Items.Count; } }
        int tlsel { get { return outputfields.SelectedIndex; } }

        private void moveup_Click(object sender, EventArgs e)
        {
            var idx = selcsv;
            if ((idx < 0) || (idx >= selcount))
            {
                status("select a column");
                return;
            }
            if (idx == 0)
                return;
            // get current
            var col = inputfields.Items[idx].ToString();
            var colidx = headercol[idx];
            // remove it
            inputfields.Items.Insert(idx - 1, col);
            inputfields.Items.RemoveAt(idx+1);
            headercol.Insert(idx - 1, colidx);
            headercol.RemoveAt(idx+1);
            inputfields.SelectedIndex = idx-1;
            isvalidconvert();
            Invalidate();

        }

        private void ignore_Click(object sender, EventArgs e)
        {
            var idx = selcsv;
            if ((idx < 0) || (idx >= selcount))
            {
                status("select a column");
                return;
            }
            if (idx == 0)
                return;
            // get current
            // remove it
            inputfields.Items.RemoveAt(idx);
            headercol.RemoveAt(idx);
            isvalidconvert();

            Invalidate();
        }

        private void movedown_Click(object sender, EventArgs e)
        {
            var idx = selcsv;
            if ((idx < 0) || (idx >= selcount))
            {
                status("select a column");
                return;
            }
            if (idx ==selcount-1)
                return;
            // get current
            var col = inputfields.Items[idx].ToString();
            var colidx = headercol[idx];
            // remove it
            inputfields.Items.Insert(idx + 2, col);
            
            inputfields.Items.RemoveAt(idx);
            headercol.Insert(idx + 2, colidx);
            
            headercol.RemoveAt(idx);
            inputfields.SelectedIndex = idx + 1;
            isvalidconvert();
            
            Invalidate();
        }

        List<string> custom = new List<string>();

        private void addfield_Click(object sender, EventArgs e)
        {
            var idx = selcsv;
            if ((idx < 0) || (idx >= selcount))
            {
                status("select a column");
                return;
            }
            // get custom idx (1 indexed)
            var cidx = custom.Count +1;
            // prompt user
            var val = TradeLink.AppKit.TextPrompt.Prompt("Fill a new column", "Enter value this column will have");
            custom.Add(val);
            inputfields.Items.Insert(idx, val);
            headercol.Insert(idx, -cidx);
            inputfields.SelectedIndex = idx;
            isvalidconvert();
        }

        private void dupecol_Click(object sender, EventArgs e)
        {
            var idx = selcsv;
            if ((idx < 0) || (idx >= selcount))
            {
                status("select a column");
                return;
            }
            // get current
            var col = inputfields.Items[idx].ToString();
            var colidx = headercol[idx];
            // add it again
            inputfields.Items.Insert(idx + 1, col);
            headercol.Insert(idx + 1, colidx);
            inputfields.SelectedIndex = idx+1;
            isvalidconvert();
        }


        public ConvertMap GetMap()
        {
            var map = new ConvertMap(headercol, custom, selmaptype, ignoreinvalid.Checked, debug, status);
            map.Files = files.ToArray();
            return map;
        }

        public ConvertMap CurrentMap = null;

        public bool isConvertOk = false;


        bool isvalidconvert()
        {
            convertexample.Items.Clear();
            if (inputfields.Items.Count != outputfields.Items.Count)
            {
                status("Must have same # of csv and tradelink fields.");
                return false;
            }
            CurrentMap = GetMap();
            var ks = CurrentMap.convert(data);
            var ec = CurrentMap.expectcount(data);
            isConvertOk = ks.Count == ec;
            status("Sample import : Got " + ks.Count + " records and expected " + ec);

            
            convertexample.BeginUpdate();
            for (int i = 0; (i < 20) && (i<ks.Count); i++)
            {
                convertexample.Items.Add(ks[i]);
            }
            convertexample.EndUpdate();

            return isConvertOk;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (!isvalidconvert())
            {
                return;
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        ConvertMapType selmaptype
        {
            get
            {
                ConvertMapType mt = ConvertMapType.None;
                if (Enum.TryParse<ConvertMapType>(importtype.Text, out mt)) return mt; return ConvertMapType.None;
            }
        }

        private void importtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            outputfields.Items.Clear();
            outputfields.Items.AddRange(ConvertMap.GetFields(ConvertMap.GetMapFields(selmaptype)));
            isvalidconvert();
        }

        private void ignoreinvalid_CheckedChanged(object sender, EventArgs e)
        {
            isvalidconvert();
        }


    }



   
}
