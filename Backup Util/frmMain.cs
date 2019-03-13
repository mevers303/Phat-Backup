using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Backup_Util.Classes;

namespace Backup_Util
{
    public partial class frmMain : Form
    {

        private List<LocalLocation> localLocations = new List<LocalLocation>();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, System.EventArgs e)
        {
            Backup_Util.Classes.Location.form = this;
            localLocations = LocalLocation.loadLocalLocations();
            Activate();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Backup_Util.Classes.Location.form = null;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                trayIcon.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            trayIcon.Visible = false;
            Show();
            WindowState = FormWindowState.Normal;
        }

    }
}
