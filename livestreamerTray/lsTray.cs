using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace livestreamerTray
{
    public partial class lsTray : Form
    {
        public lsTray()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.Visible = false;
        }

        private void lsTray_Load(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string url;

            if(!System.Windows.Forms.Clipboard.ContainsText())
            {
                notifyIcon1.ShowBalloonTip(250, "No Link Provided", "Copy a twitch.tv stream link before clicking here.", ToolTipIcon.None);
                return;
            }
            else
            {
                url = System.Windows.Forms.Clipboard.GetText(TextDataFormat.Text);
            }

            Regex linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = linkParser.Matches(url);

            if(matches.Count == 0)
            {
                notifyIcon1.ShowBalloonTip(250, "No Link Provided", "Copy a twitch.tv stream link before clicking here.", ToolTipIcon.None);
                return;
            }
            else
            {
                url = matches[0].Value;
            }

            url = url.Replace("http://www.", "");
            url = url.Replace("http://", "");

            var lsProcessContainer = new Process();
            lsProcessContainer.StartInfo.FileName = "cmd.exe";

            var applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var livestreamerLocation = applicationDirectory + @"\livestreamer\";

            lsProcessContainer.StartInfo.Arguments = string.Format(@"/c {0}livestreamer.exe --config {1}\livestreamer.cfg {2}", livestreamerLocation, applicationDirectory, url);

            lsProcessContainer.StartInfo.RedirectStandardOutput = true; //true
            lsProcessContainer.StartInfo.CreateNoWindow = true; // true
            lsProcessContainer.StartInfo.UseShellExecute = false; // false
            try
            {

                notifyIcon1.ShowBalloonTip(100, "Opening stream", url, ToolTipIcon.None);

                lsProcessContainer.Start();

                if(lsProcessContainer.HasExited && lsProcessContainer.ExitCode == 1)
                {
                    notifyIcon1.ShowBalloonTip(250, "Error", "Could not upen that url", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                notifyIcon1.ShowBalloonTip(250, "Error", ex.Message, ToolTipIcon.Error);
                this.Close();
            }
        }
    }
}
