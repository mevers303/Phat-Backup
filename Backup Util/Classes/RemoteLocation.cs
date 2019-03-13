using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Drawing;

namespace Backup_Util.Classes
{
    abstract class RemoteLocation: Location
    {

        protected ListViewItem lviItem = null;
        protected LocalLocation localLocation = null;
        private bool _ready = true;
        public override bool ready
        {
            get
            {
                return base.ready & _ready;
            }
            set
            {
                _ready = value;
            }
        }

        private delegate void setLVSubItem(string text);
        private delegate void setLVForeColor(Color color);

        public abstract bool compareFile(string relativePath);
        public abstract bool compareDir(string relativePath);

        public void load(string name, LocalLocation local)
        {

            configName = name;
            localLocation = local;

            load();

        }

        protected override void loadGUI()
        {

            lviItem = new ListViewItem();

            lviItem.SubItems.Add("Initializing");
            lviItem.SubItems.Add("");

            form.listView1.Items.Add(lviItem);
            localLocation.lvgGroup.Items.Add(lviItem);

        }

        protected override void driveArrived()
        {
            setStatus("Drive detected");
            setAction("Reinitializing...");
            localLocation.enqueueSync();
        }

        protected override void driveRemoved()
        {
            setStatus("Drive removed!", Color.Red);
            setAction();
        }

        protected void setPath(string sPath)
        {

            if (form == null)
                return;


            if (form.InvokeRequired)
            {
                form.Invoke((setLVSubItem)setPath, sPath);
                return;
            }


            lviItem.Text = sPath;

        }

        public void setStatus(string status, Color color)
        {

            if (form == null)
                return;


            if (form.InvokeRequired)
            {
                form.Invoke((setLVSubItem)setStatus, status);
                return;
            }


            lviItem.SubItems[1].Text = status;
            lviItem.ForeColor = color;

        }

        public void setStatus(string status)
        {
            setStatus(status, Color.Black);
        }

        public void setAction(string action)
        {

            if (form == null)
                return;


            if (form.InvokeRequired)
            {
                form.Invoke((setLVSubItem)setAction, action);
                return;
            }


            lviItem.SubItems[2].Text = action;

        }

        public void setAction()
        {
            setAction("");
        }

        public void setProgress(string progress)
        {

            if (form == null)
                return;


            if (form.InvokeRequired)
            {
                form.Invoke((setLVSubItem)setProgress, progress);
                return;
            }


            lviItem.SubItems[3].Text = progress;

        }

        protected int compareFileTimes(string relativePath)
        {

            if (!ready)
                return 0;


            relativePath = deroot(relativePath);


            FileInfo remoteFile = new FileInfo(Path.Combine(path, relativePath));
            FileInfo localFile = new FileInfo(Path.Combine(localLocation.path, relativePath));


            DateTime convertedTime = localFile.LastWriteTime;


            if (driveFormat.Contains("FAT"))
            {

                long ticks = convertedTime.Ticks - (new DateTime(convertedTime.Year, convertedTime.Month, convertedTime.Day, convertedTime.Hour, convertedTime.Minute, convertedTime.Second)).Ticks;

                if (ticks > 0)
                    convertedTime = convertedTime.AddTicks(TimeSpan.TicksPerSecond - ticks);

                if ((convertedTime.Second % 2) > 0)
                    convertedTime = convertedTime.AddSeconds(1);

            }


            return remoteFile.LastWriteTime.CompareTo(convertedTime);

        }

        public void enqueue(queueItem.NormalCallback call, string relativePath)
        {
            queueItem.queue.Enqueue(new queueItem(this, call, relativePath));
        }

        public void enqueue(queueItem.RenameCallback call, string oldRelativePath, string newRelativePath)
        {
            queueItem.queue.Enqueue(new queueItem(this, call, oldRelativePath, newRelativePath));
        }

    }
}
