using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LiveBackup
{
    public partial class formAddBackup : Form
    {

        protected string keyName;
        protected formMain parent;

        public formAddBackup(formMain p)
        {
            InitializeComponent();
            parent = p;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnLocal_Click(object sender, EventArgs e)
        {
            if (localBrowser.ShowDialog() == DialogResult.OK)
                txtLocal.Text = localBrowser.SelectedPath;
        }

        private void btnRemote_Click(object sender, EventArgs e)
        {
            if (remoteBrowser.ShowDialog() == DialogResult.OK)
                txtRemote.Text = remoteBrowser.SelectedPath;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            string name = txtName.Text;
            string localPath = txtLocal.Text;
            string remotePath = txtRemote.Text;

            if (!localPath.EndsWith("\\"))
                localPath += "\\";
            if (!remotePath.EndsWith("\\"))
                remotePath += "\\";


            if (name == "")
            {
                MessageBox.Show("Backup name was empty!", "Invalid name!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            if (localPath == "\\")
            {
                MessageBox.Show("Local path was empty!", "Invalid path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLocal.Focus();
                return;
            }

            if (remotePath == "\\")
            {
                MessageBox.Show("Remote path was empty!", "Invalid path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRemote.Focus();
                return;
            }



            if (Registry.LocalMachine.OpenSubKey(localLocation.registryPath + name) != null)
            {
                MessageBox.Show("There is already a backup named \"" + name + "\"!  Please choose a unique name.", "Name already exists!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }


            RegistryKey key = Registry.LocalMachine.CreateSubKey(localLocation.registryPath + name);
            key.SetValue("localPath", localPath);
            key.SetValue("remotePath", remotePath);



            keyName = name;
            initialWorker.RunWorkerAsync();
            Hide();

        }

        private void initialWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            localLocation local = new localLocation(keyName, parent);
            parent.localLocations.Add(local);
            local.removeMissing();
            local.initialSync();

        }

        private void initialWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

    }
}
