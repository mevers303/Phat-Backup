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
    public partial class formMain : Form
    {

        public List<localLocation> localLocations = new List<localLocation>();

        protected delegate void voidFunc();



        //constructor
        public formMain()
        {
            InitializeComponent();
        }

        private void formMain_Load(object sender, EventArgs e)
        {

            queueWorker.RunWorkerAsync();
            initialWorker.RunWorkerAsync();

        }

        private void queueWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < localLocations.Count; i++)
                localLocations[i].popQ();
        }

        private void initialWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            string[] keys = Registry.LocalMachine.OpenSubKey(localLocation.registryPath).GetSubKeyNames();



            for (int i = 0; i < keys.Length; i++)
                localLocations.Add(new localLocation(keys[i], this));

            for (int i = 0; i < localLocations.Count; i++)
            {
                localLocations[i].removeMissing();
                localLocations[i].initialSync();
            }

        }

        private void formMain_SizeChanged(object sender, EventArgs e)
        {

            Size s = this.Size;

            s.Width -= 15;
            s.Height -= 35;

            this.listView1.Size = s;
            
        }

        private void addBackupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formAddBackup f = new formAddBackup(this);
            f.Show();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {

            for (int i = listView1.Items.Count - 1; i >= 0; i--)
            {
                if (listView1.Items[i].Selected)
                {

                    RegistryKey key = Registry.LocalMachine.OpenSubKey(localLocation.registryPath, true);

                    if (key != null)
                    {
                        key.DeleteSubKeyTree(localLocations[i].keyName);
                    }

                    localLocations.RemoveAt(i);

                }
            }

            updateListView();

        }

        private void queueWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            for (int i = 0; i < localLocations.Count; i++)
            {
                if (localLocations[i].queueCount > 0)
                {
                    lock (this)
                    {
                        if (!queueWorker.IsBusy)
                            queueWorker.RunWorkerAsync();
                    }
                    return;
                }
            }
        }

        public void updateListView()
        {

            if (InvokeRequired)
            {
                Invoke(new voidFunc(updateListView));
                return;
            }



            for (int i = 0; i < localLocations.Count; i++)
            {

                if (i >= listView1.Items.Count)
                {

                    ListViewItem item = new ListViewItem(localLocations[i].keyName);

                    item.SubItems.Add(localLocations[i].localPath);
                    item.SubItems.Add(localLocations[i].remotePath);
                    item.SubItems.Add(localLocations[i].status);

                    listView1.Items.Add(item);

                }
                else
                {

                    if (listView1.Items[i].Text != localLocations[i].keyName)
                        listView1.Items[i].Text = localLocations[i].keyName;

                    if (listView1.Items[i].SubItems[1].Text != localLocations[i].localPath)
                        listView1.Items[i].SubItems[1].Text = localLocations[i].localPath;

                    if (listView1.Items[i].SubItems[2].Text != localLocations[i].remotePath)
                        listView1.Items[i].SubItems[2].Text = localLocations[i].remotePath;

                    if (listView1.Items[i].SubItems[3].Text != localLocations[i].status)
                        listView1.Items[i].SubItems[3].Text = localLocations[i].status;

                }

            }



            while (listView1.Items.Count > localLocations.Count)
                listView1.Items.RemoveAt(listView1.Items.Count - 1);

        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void rescanFilesForChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            for (int i = listView1.Items.Count - 1; i >= 0; i--)
            {
                if (listView1.Items[i].Selected)
                    localLocations[i].initialSync();
            }

            updateListView();

        }

        private void removeDeletedFilesFromBackupToolStripMenuItem_Click(object sender, EventArgs e)
        {

            for (int i = listView1.Items.Count - 1; i >= 0; i--)
            {
                if (listView1.Items[i].Selected)
                    localLocations[i].removeMissing();
            }

            updateListView();

        }

    }
}
