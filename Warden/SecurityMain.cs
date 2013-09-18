using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TradeLink.API;
using TradeLink.AppKit;
using TradeLink.Common;
using Affirma.ThreeSharp.Wrapper;
using System.IO;

namespace Warden
{
    public partial class SecurityMain : Form
    {
        const string PROGRAM = "Warden";
        Log log = new Log(PROGRAM);
        DebugWindow dw = new DebugWindow();

        public SecurityMain()
        {
            InitializeComponent();
            Text += " " + Util.TLVersion();
            var tmp = Properties.Settings.Default.programlist.Split(',');
            if ((tmp.Length>0))
                _program.AutoCompleteCustomSource.AddRange(tmp);
            tsw = new ThreeSharpWrapper(awsid, awskey);
            FormClosing += new FormClosingEventHandler(SecurityMain_FormClosing);
            Resize += new EventHandler(SecurityMain_Resize);
            if (Width > 0)
                dw.Width = Width;
            debug("For setup help: " + HELPURL);
        }

        void SecurityMain_Resize(object sender, EventArgs e)
        {
            if (Width > 0)
                dw.Width = Width;
        }

        void SecurityMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Stop();
            string[] tmp = new string[_program.AutoCompleteCustomSource.Count];
            _program.AutoCompleteCustomSource.CopyTo(tmp, 0);
            if ((tmp.Length > 0) && !string.IsNullOrWhiteSpace(tmp[0]))
            {
                
                Properties.Settings.Default.programlist = string.Join(",", tmp);
            }
            Properties.Settings.Default.Save();
        }

        void status(string msg)
        {
            if (stat.InvokeRequired)
                stat.Invoke(new DebugDelegate(status), new object[] { msg });
            else
            {
                stat.Text = msg;
                stat.Invalidate(true);
            }
        }
        void debug(string msg)
        {
            log.GotDebug(msg);
            dw.GotDebug(msg);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        string program { get { var v = _program.Text; if (v.Contains(".txt")) return v; return v + ".txt"; } }

        string localfile_program { get 
        {
            string p = Util.ProgramData(PROGRAM)+"\\";
            string f =Util.rxr(program,@".*?[/]([a-z_0-9.]+)", "$1");
            return p + f;
        } 
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (!_program.AutoCompleteCustomSource.Contains(program))
                _program.AutoCompleteCustomSource.Add(program);

            gets3(localfile_program);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dw.Toggle();
        }

        private void addbut_Click(object sender, EventArgs e)
        {
            _user.Clear();
            _regcode.Clear();
        }

        private void modbut_Click(object sender, EventArgs e)
        {
            string acl = selectedacl;
            if (acl==string.Empty)
                return;
            string[] r = System.Text.RegularExpressions.Regex.Split(acl,"[ ]+");
            if (r.Length>1)
                _user.Text = r[1];
            if (r.Length>0)
                _regcode.Text = r[0];
        }


        string selectedacl { get { int idx = acllist.SelectedIndex; if (idx<0) return string.Empty; return acllist.Items[idx].ToString(); } }

        private void delbut_Click(object sender, EventArgs e)
        {
            string acl = selectedacl ;
            if (MessageBox.Show("Sure you want to delete: " + acl+ "?", "Delete acl", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                acllist.Items.RemoveAt(acllist.SelectedIndex);
                status("removed " + acl);
            }
        }

        void open(string fn)
        {
            StreamReader sr = new StreamReader(fn);
            string data = sr.ReadToEnd();
            sr.Close();
            var lines = data.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            acllist.Items.Clear();
            acllist.Items.AddRange(lines);
            status("opened: " + program);
        }


        void saves3(string fn)
        {
            try
            {
                tsw.AddFileObject(bucket, program, fn);
            }
            catch (Exception ex)
            {
                debug("error saving: " + program + " to bucket: "+bucket+" err: " + ex.Message + ex.StackTrace);
                debug("ensure you have created your s3 account and bucket via: http://aws.amazon.com/console/");
                status("Error, please click (!) for details.");
            }
        }

        string awsid { get { return _awsid.Text; } } 
        string awskey { get { return _awskey.Text; } }

        string bucket { get { return _s3bucket.Text; } }

        private ThreeSharpWrapper tsw;

        bool backup(string file)
        {
            string bf = file + ".backup." + Util.ToTLDate()+""+Util.ToTLTime() + ".txt";
            try {
                if (File.Exists(bf))
                    File.Delete(bf);
            } catch {}
            try
            {
                File.Copy(file, bf);
                return true;
            }
            catch (Exception ex) {
                debug("error backup: " + file + " err: " + ex.Message + ex.StackTrace);
            }
            return false;
        }

        void gets3(string fn)
        {
            try
            {
                tsw.GetFileObject(bucket, program, fn);
                backup(fn);
                open(fn);
            }
            catch (Exception ex)
            {
                debug("error getting: " + program + " from: "+bucket+" err: " + ex.Message + ex.StackTrace);
                debug("ensure you have created your s3 account and bucket via: http://aws.amazon.com/console/");
                status("Error, please click (!) for details.");
            }
        }

        private void import_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                open(ofd.FileName);
            }
        }

        string data
        {
            get
            {
                string d = string.Empty;
                foreach (var it in acllist.Items)
                    d += it.ToString() + Environment.NewLine;
                return d;
            }
        }

        bool savefile(string fn)
        {
            try
            {
                StreamWriter sw = new StreamWriter(fn, false);
                sw.Write(data);
                sw.Close();
                return true;
            }
            catch (Exception ex)
            {
                debug("error saving: " + fn + " err: " + ex.Message + ex.StackTrace);
                return false;
            }

        }

        private void export_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (savefile(sfd.FileName))
                    status("saved file: " + sfd.FileName);
                else
                    status("unable to save file.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            savefile(localfile_program);
            saves3(localfile_program);
            if (verifytest())
                status("warden changes verified");
            else
                status("warden changes NOT verifiable.  see details (!)");
        }

        private void okbut_Click(object sender, EventArgs e)
        {
            string l = _regcode.Text + "    " + _user.Text;
            acllist.Items.Add(l);
            status("warden added user, remember to remove last user.");
        }

        private void sync_Click(object sender, EventArgs e)
        {
            gets3(localfile_program);
        }

        private void acllist_SelectedIndexChanged(object sender, EventArgs e)
        {
            modbut_Click(null, null);
        }

        const string HELPURL = @"https://code.google.com/p/tradelink/wiki/WardenConfig";

        private void help_Click(object sender, EventArgs e)
        {
            Util.openurl(HELPURL,debug);
            HelpReportCommunity.Help(PROGRAM);
        }

        bool verifytest()
        {
            var dr = data.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            // get url data
            var wardenlist = Util.geturl(curtesturl, true, debug);
            if (string.IsNullOrWhiteSpace(wardenlist))
            {
                status("Error verifying warden changes.  See details (!)");
                debug("No warden data was obtainable, did you forget to make your warden page public?");
                debug("Double check instructions here: " + HELPURL);
                return false;
            }
            bool allpresent = true;
            foreach (var r in dr)
            {
                var rv = wardenlist.Contains(r);
                if (!rv)
                    debug("unable to verify warden line: " + r);
                allpresent &= rv;
            }
            return allpresent;
        }

        string curtesturl { get { return "https://s3.amazonaws.com/" + bucket + "/" + program; } }

        private void testurl_Click(object sender, EventArgs e)
        {
            debug("suggested test url of: " + curtesturl);
            debug("This URL's contents should be visible in browser, otherwise you likely forgot setup instructions: " + HELPURL);
            Util.openurl(curtesturl, debug);
            status("Verify opened page matches warden list.");
        }
    }
}
