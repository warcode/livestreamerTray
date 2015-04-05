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
            notifyIcon1.Visible = true;

            var menu = new ContextMenu();
            menu.MenuItems.Add(0, new MenuItem("Exit", new System.EventHandler(Exit_Click)));
            notifyIcon1.ContextMenu = menu;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
                notifyIcon1.ShowBalloonTip(5, "No Link Provided", "Copy a twitch.tv stream link before clicking here.", ToolTipIcon.None);
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
            lsProcessContainer.EnableRaisingEvents = true;
            lsProcessContainer.Exited += lsProcessContainer_Exited;

            notifyIcon1.ShowBalloonTip(50, "Opening stream", url, ToolTipIcon.None);

            try
            {
                lsProcessContainer.Start();
            }
            catch (Exception ex)
            {
                notifyIcon1.ShowBalloonTip(250, "Error", ex.Message, ToolTipIcon.Error);
                this.Close();
            }
        }

        void lsProcessContainer_Exited(object sender, EventArgs e)
        {
            var process = (Process)sender;

            if (process.HasExited && process.ExitCode == 1)
            {
                notifyIcon1.ShowBalloonTip(50, "Error", "Could not upen that url", ToolTipIcon.Error);
            }

            process.Close();
        }
    }
}
