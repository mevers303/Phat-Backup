using Microsoft.Win32;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace Backup_Util.Classes
{
    class LocalLocation : Location
    {

        public ListViewGroup lvgGroup = null;
        public List<RemoteLocation> remoteLocations = new List<RemoteLocation>();
        FileSystemWatcher fileWatcher = new FileSystemWatcher();
        public bool reverseSyncNewer = false;

        /* These are virtual members of Location, now it isn't repeated here and in DriveRemoteLocation
        public override long driveSpace
        {
            get
            {
                if (!ready)
                    return 0;
                else
                    return (new DriveInfo(Path.GetPathRoot(path))).AvailableFreeSpace;
            }
        }
        public override string driveFormat
        {
            get
            {
                if (!ready)
                    return "Unknown";
                else
                    return (new DriveInfo(Path.GetPathRoot(path))).DriveFormat;
            }
        }
        public override DriveType driveType
        {
            get
            {
                if (!ready)
                    return DriveType.Unknown;
                else
                    return (new DriveInfo(Path.GetPathRoot(path))).DriveType;
            }
        }
        */
        
        public static List<LocalLocation> loadLocalLocations()
        {
            
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Phat Backup\Local Locations");
            if (key == null)
                Registry.LocalMachine.CreateSubKey(@"Software\Phat Backup\Local Locations");

            List<LocalLocation> locations = new List<LocalLocation>();

            foreach (string name in key.GetSubKeyNames())
                locations.Add(new LocalLocation(name));

            return locations;

        }

        public LocalLocation(string name)
        {

            configName = name;

            load();
            
        }

        protected override void load()
        {

            base.load();

            loadFileWatcher();

        }

        protected override void reload()
        {

            fileWatcher.EnableRaisingEvents = false;

            base.reload();

            startWatcher();

        }

        protected void startWatcher()
        {

            if (!ready)
                return;

            fileWatcher.Path = path;
            fileWatcher.EnableRaisingEvents = true;

        }

        protected void loadFileWatcher()
        {

            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;

            fileWatcher.Changed += new FileSystemEventHandler(fileWatcher_Changed);
            fileWatcher.Created += new FileSystemEventHandler(fileWatcher_Changed);
            fileWatcher.Deleted += new FileSystemEventHandler(fileWatcher_Deleted);
            fileWatcher.Renamed += new RenamedEventHandler(fileWatcher_Renamed);

        }

        void  fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {

            if (!ready)
                return;


            string oldRelativePath = e.OldFullPath.Substring(path.Length);
            string newRelativePath = e.FullPath.Substring(path.Length);


            if (File.Exists(e.FullPath))
                renameFile(oldRelativePath, newRelativePath);
            else if (Directory.Exists(e.FullPath))
                renameDir(oldRelativePath, newRelativePath);

        }

        void  fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {

            if (!ready)
                return;

            string relativePath = e.FullPath.Substring(path.Length);

            deleteFile(relativePath);
            deleteDir(relativePath);

        }

        void  fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {

            if (!ready)
                return;
            
            string relativePath = e.FullPath.Substring(path.Length);

            if (File.Exists(e.FullPath))
                copyFile(relativePath);
            else if (Directory.Exists(e.FullPath))
                copyDir(relativePath);

        }

        protected override void loadGUI()
        {

            lvgGroup = new ListViewGroup();
            form.listView1.Groups.Add(lvgGroup);

        }

        public override void loadConfig()
        {

            remoteLocations.Clear();
            fileWatcher.EnableRaisingEvents = false;

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Phat Backup\Local Locations\" + configName);
            if (key == null)
            {
                lvgGroup.Header = "!!! Registry error! !!! " + configName;
                ready = false;
                return;
            }

            path = (string)key.GetValue("path");
            lvgGroup.Header = path;



            RegistryKey key2 = key.OpenSubKey("Remote Locations");
            if (key2 == null)
            {
                lvgGroup.Header = "!!! Registry error! !!! " + configName;
                ready = false;
                return;
            }

            foreach (string name in key2.GetSubKeyNames())
                remoteLocations.Add(new DriveRemoteLocation(name, this));

        }

        public override void verifyPath()
        {

            if (!ready)
                return;

            DirectoryInfo localDir = new DirectoryInfo(path);
            if (!localDir.Exists)
            {
                lvgGroup.Header = "!!! Does not exist! !!!! " + path;
                ready = false;
                return;
            }


            foreach (RemoteLocation remoteLocation in remoteLocations)
                remoteLocation.verifyPath();

        }

        public override void syncFiles()
        {

            if (!ready)
                return;


            foreach (RemoteLocation remoteLocation in remoteLocations)
                remoteLocation.syncFiles();


            foreach (RemoteLocation remoteLocation in remoteLocations)
            {
                if (remoteLocation.ready)
                    remoteLocation.setStatus("Searching for updated files");
            }


            DirectoryInfo localDir = new DirectoryInfo(path);

            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();
            dirQ.Enqueue(localDir);


            do
            {

                DirectoryInfo dir = dirQ.Dequeue();


                string dirRelativePath = deroot(dir.FullName.Substring(path.Length));


                FileInfo[] files;
                try
                {
                    files = dir.GetFiles();
                }
                catch
                {
                    continue;
                }


                copyDir(dirRelativePath);


                foreach (FileInfo file in files)
                {

                    string fileRelativePath = deroot(file.FullName.Substring(path.Length));

                    copyFile(fileRelativePath);

                }


                foreach (DirectoryInfo subDir in dir.GetDirectories())
                    dirQ.Enqueue(subDir);

            } while (dirQ.Count > 0);

        }

        public override void deleteFile(string relativePath)
        {

            if (!ready)
                return;

            foreach (RemoteLocation remoteLocation in remoteLocations)
                remoteLocation.enqueue(remoteLocation.deleteFile, relativePath);

        }

        public override void deleteDir(string relativePath)
        {

            if (!ready)
                return;

            foreach (RemoteLocation remoteLocation in remoteLocations)
                remoteLocation.enqueue(remoteLocation.deleteDir, relativePath);

        }

        public override void copyFile(string relativePath)
        {

            if (!ready)
                return;

            foreach (RemoteLocation remoteLocation in remoteLocations)
            {
                if (!remoteLocation.compareFile(relativePath))
                    remoteLocation.enqueue(remoteLocation.copyFile, relativePath);
            }

        }

        public override void copyDir(string relativePath)
        {

            if (!ready)
                return;

            foreach (RemoteLocation remoteLocation in remoteLocations)
            {
                if (!remoteLocation.compareDir(relativePath))
                    remoteLocation.enqueue(remoteLocation.copyDir, relativePath);
            }

        }

        public override void renameFile(string oldRelativePath, string newRelativePath)
        {

            foreach (RemoteLocation remoteLocation in remoteLocations)
                remoteLocation.enqueue(remoteLocation.renameFile, oldRelativePath, newRelativePath);

        }

        public override void renameDir(string oldRelativePath, string newRelativePath)
        {

            foreach (RemoteLocation remoteLocation in remoteLocations)
                remoteLocation.enqueue(remoteLocation.renameDir, oldRelativePath, newRelativePath);

        }

        protected override void driveArrived()
        {
            lvgGroup.Header = path;
        }

        protected override void driveRemoved()
        {
            lvgGroup.Header = "!!! Drive removed! !!!! " + path;
        }

        public override void enqueueSync()
        {

            if (!ready)
                return;

            queueItem.queue.Enqueue(new queueItem(this, syncFiles));

        }

    }
}
