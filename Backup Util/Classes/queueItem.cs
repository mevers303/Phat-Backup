using System.Collections.Generic;
using System.ComponentModel;


namespace Backup_Util.Classes
{
    class queueItem
    {

        public static Queue<queueItem> queue = new Queue<queueItem>();
        private static BackgroundWorker worker = new BackgroundWorker();

        static queueItem()
        {
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync();
        }

        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!worker.CancellationPending)
            {
                if (queue.Count > 0)
                    queue.Dequeue().pop();
                else
                    System.Threading.Thread.Sleep(2500);
            }
        }


        public delegate void NormalCallback(string relativePath);
        public delegate void RenameCallback(string oldRelativePath, string newRelativePath);
        public delegate void SyncCallback();


        protected RemoteLocation remoteLocation = null;
        protected LocalLocation localLocation = null;
        protected string relativePath = null;
        protected string oldRelativePath = null;
        protected NormalCallback normalCallback = null;
        protected RenameCallback renameCallback = null;
        protected SyncCallback syncCallback = null;

        public queueItem(RemoteLocation remote, NormalCallback call, string path)
        {
            remoteLocation = remote;
            normalCallback = call;
            relativePath = path;
        }

        public queueItem(RemoteLocation remote, RenameCallback call, string oldPath, string path)
        {
            remoteLocation = remote;
            renameCallback = call;
            oldRelativePath = oldPath;
            relativePath = path;
        }

        public queueItem(LocalLocation local, SyncCallback call)
        {
            localLocation = local;
            syncCallback = call;
        }

        public void pop()
        {

            if (normalCallback != null)
            {
                normalCallback(relativePath);
                remoteLocation.setAction();
            }
            else if (renameCallback != null)
            {
                renameCallback(oldRelativePath, relativePath);
                remoteLocation.setAction();
            }
            else if (syncCallback != null)
            {
                syncCallback();
                foreach (RemoteLocation remoteLocation in localLocation.remoteLocations)
                {
                    if (remoteLocation.ready)
                        remoteLocation.setStatus("Watching for file updates");
                }
            }

        }

    }
}
