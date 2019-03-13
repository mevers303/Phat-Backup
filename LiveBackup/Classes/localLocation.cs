using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;

namespace LiveBackup
{
    public class localLocation
    {

        public const string registryPath = "Software\\LiveBackup\\";

        protected bool _ready = true;
        protected bool ready
        {
            get
            {
                return _ready;
            }
            set
            {
                _ready = value;
                if (!value)
                    stopWatcher();
            }
        }

        protected string _status;
        protected string overallStatus;
        public string status
        {
            get
            {
                return _status;
            }
            set
            {
                Console.WriteLine(keyName + ": " + value);
                _status = value;
                parent.updateListView();
            }
        }

        protected string _keyName;
        public string keyName
        {
            get
            {
                return _keyName;
            }
            set
            {
                _keyName = value;
                ready = loadSettings();
                parent.updateListView();
            }
        }

        protected string _localPath;
        public string localPath
        {
            get
            {
                return _localPath;
            }
            set
            {
                _localPath = value;
                ready = checkLocalPath();
                startWatcher();
                parent.updateListView();
            }
        }

        protected string _remotePath;
        public string remotePath
        {
            get
            {
                return _remotePath;
            }
            set
            {
                _remotePath = value;
                ready = checkRemotePath();
                parent.updateListView();
            }
        }

        protected FileSystemWatcher watcher = new FileSystemWatcher();

        protected Queue<queueItem> fileQ = new Queue<queueItem>();

        public int queueCount
        {
            get
            {
                return fileQ.Count;
            }
        }

        protected formMain parent;



        protected enum fileAction
        {
            copy,
            delete,
            rename
        }

        protected struct queueItem
        {
            public string relativePath;
            public fileAction action;
            public string newName; //for renames
        }



        //accepts the name of the registry key to load the settings from
        public localLocation(string name, formMain p)
        {
            parent = p;
            keyName = name;
        }

        //loads settings from the specified registry key
        protected bool loadSettings()
        {

            status = "Loading settings...";



            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath + keyName);

            if (key == null)
            {
                status = "Could not find registry key " + registryPath + keyName + ".";
                return false;
            }



            string local = key.GetValue("localPath", null).ToString();

            if (local == null)
            {
                status = "Could not find localPath key in " + registryPath + keyName + ".";
                return false;
            }

            //add trailing slash
            if (!local.EndsWith("\\"))
                local += "\\";

            localPath = local;



            string remote = key.GetValue("remotePath", null).ToString();

            if (remote == null)
            {
                status = "Could not find remotePath key in " + registryPath + keyName + ".";
                return false;
            }

            //add trailing slash
            if (!remote.EndsWith("\\"))
                remote += "\\";

            remotePath = remote;



            return ready;

        }

        //checks if localPath is valid
        protected bool checkLocalPath()
        {

            if (!ready)
                return false;



            status = "Checking local path...";



            DirectoryInfo localDir = new DirectoryInfo(localPath);

            if (!localDir.Exists)
            {
                status = "Local path not found!";
                return false;
            }



            return true;

        }

        //checks if remotePath is valid
        protected bool checkRemotePath()
        {

            if (!ready)
                return false;



            status = "Checking remote path...";



            DirectoryInfo remoteDir = new DirectoryInfo(remotePath);

            if (!remoteDir.Root.Exists)
            {
                status = "Remote drive " + remoteDir.Root.FullName + " not found!";
                return false;
            }



            //create it if it doesn't exist
            if (!remoteDir.Exists)
            {
                try
                {
                    remoteDir.Create();
                }
                catch
                {
                    status = "Could not create directory \"" + remoteDir.FullName + "\"!";
                    return false;
                }
            }



            return true;
            
        }

        //starts the file system watcher
        protected void startWatcher()
        {

            if (!ready)
                return;



            status = "Starting file system watcher...";



            watcher = new FileSystemWatcher();



            watcher.Path = localPath;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName;



            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.Created += new FileSystemEventHandler(watcher_Changed);
            watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
            watcher.Renamed +=new RenamedEventHandler(watcher_Renamed);



            watcher.EnableRaisingEvents = true;

        }

        //stops the file system watcher
        protected void stopWatcher()
        {
            watcher.EnableRaisingEvents = false;
        }

        //checks if the specified file is equal to the local one
        protected bool checkFile(string relativePath)
        {

            if (!ready)
                return true;



            FileInfo localFile = new FileInfo(localPath + relativePath);
            FileInfo remoteFile = new FileInfo(remotePath + relativePath);



            if (!localFile.Exists)
            {
                if (remoteFile.Exists)
                    addQ(relativePath, fileAction.delete);
                return true;
            }



            if (!remoteFile.Exists)
                return false;

            if (remoteFile.Length != localFile.Length)
                return false;

            if ((remoteFile.LastWriteTime != localFile.LastWriteTime))
                return false;

            /*if (remoteFile.CreationTime != localFile.CreationTime)
                return false;*/

            if (remoteFile.IsReadOnly != localFile.IsReadOnly)
                remoteFile.IsReadOnly = localFile.IsReadOnly;

            if ((remoteFile.Attributes & FileAttributes.Hidden) != (localFile.Attributes & FileAttributes.Hidden))
                remoteFile.Attributes ^= FileAttributes.Hidden;

            if ((remoteFile.Attributes & FileAttributes.System) != (localFile.Attributes & FileAttributes.System))
                remoteFile.Attributes ^= FileAttributes.System;



            return true;

        }

        //checks if the specified directory is equal to the local one
        protected bool checkDir(string relativePath)
        {

            if (!ready)
                return true;



            DirectoryInfo localDir = new DirectoryInfo(localPath + relativePath);
            DirectoryInfo remoteDir = new DirectoryInfo(remotePath + relativePath);



            if (!localDir.Exists)
            {
                if (remoteDir.Exists)
                    addQ(relativePath, fileAction.delete);
                return true;
            }



            if (!remoteDir.Exists)
                return false;

            //if (remoteDir.LastWriteTime != localDir.LastWriteTime)
            //    return false;

            //if (remoteDir.CreationTime != localDir.CreationTime)
            //    return false;

            if ((remoteDir.Attributes & FileAttributes.Hidden) != (localDir.Attributes & FileAttributes.Hidden))
                return false;

            if ((remoteDir.Attributes & FileAttributes.System) != (localDir.Attributes & FileAttributes.System))
                return false;



            return true;

        }

        //deletes a file
        protected void deleteFile(string relativePath)
        {

            if (!ready)
                return;



            FileInfo file = new FileInfo(remotePath + relativePath);

            if (!file.Exists)
                return;



            status = "Deleting " + file.Name + "...";



            if (file.IsReadOnly)
                file.IsReadOnly = false;



            try
            {
                file.Delete();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        //enqueues a directory for deletion
        protected void deleteDir(string relativePath)
        {

            if (!ready)
                return;



            DirectoryInfo dir = new DirectoryInfo(remotePath + relativePath);

            if (!dir.Exists)
                return;



            status = "Deleting " + dir.Name + "...";



            try
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    if (file.IsReadOnly)
                        file.IsReadOnly = false;
                }
            }
            catch
            {
            }



            try
            {
                dir.Delete(true);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        //backs up a file
        protected void copyFile(string relativePath)
        {

            if (!ready)
                return;



            if (checkFile(relativePath))
                return;



            FileInfo localFile = new FileInfo(localPath + relativePath);
            FileInfo remoteFile = new FileInfo(remotePath + relativePath);



            status = "Copying " + localFile.Name + "...";



            if (remoteFile.Exists)
            {
                if (remoteFile.IsReadOnly)
                    remoteFile.IsReadOnly = false;
            }


            if (!remoteFile.Directory.Exists)
                remoteFile.Directory.Create();


            try
            {
                localFile.CopyTo(remoteFile.FullName, true);
            }
            catch
            {
                return;
            }



            remoteFile = new FileInfo(remotePath + relativePath);

            /*if (remoteFile.CreationTime != localFile.CreationTime)
                localFile.CreationTime = remoteFile.CreationTime;*/

             if (remoteFile.LastWriteTime != localFile.LastWriteTime)
             {
                 try
                 {
                     localFile.LastWriteTime = remoteFile.LastWriteTime;
                 }
                 catch
                 {
                 }
             }


        }

        //creates or sets the properties of a directory
        protected void copyDir(string relativePath)
        {

            if (!ready)
                return;



            if (checkDir(relativePath))
                return;



            DirectoryInfo localDir = new DirectoryInfo(localPath + relativePath);
            DirectoryInfo remoteDir = new DirectoryInfo(remotePath + relativePath);



            status = "Copying " + localDir.Name + "...";



            if (!remoteDir.Exists)
                remoteDir.Create();



            remoteDir.Refresh();

            if ((remoteDir.Attributes & FileAttributes.Hidden) != (localDir.Attributes & FileAttributes.Hidden))
            {
                if ((remoteDir.Attributes & FileAttributes.Hidden) != 0)
                    remoteDir.Attributes ^= FileAttributes.Hidden;
                else
                    remoteDir.Attributes |= FileAttributes.Hidden;
            }

            if ((remoteDir.Attributes & FileAttributes.System) != (localDir.Attributes & FileAttributes.System))
            {
                if ((remoteDir.Attributes & FileAttributes.System) != 0)
                    remoteDir.Attributes ^= FileAttributes.System;
                else
                    remoteDir.Attributes |= FileAttributes.System;
            }

            if (remoteDir.CreationTime != localDir.CreationTime)
                remoteDir.CreationTime = localDir.CreationTime;

            if (remoteDir.LastWriteTime != localDir.LastWriteTime)
                remoteDir.LastWriteTime = localDir.LastWriteTime;

        }

        //adds an action to the queue
        protected void addQ(string relativePath, fileAction action, string newName)
        {

            queueItem item = new queueItem();



            item.relativePath = relativePath;
            item.action = action;
            item.newName = newName;



            fileQ.Enqueue(item);



            lock (this) {
            if (!parent.queueWorker.IsBusy)
                parent.queueWorker.RunWorkerAsync();
            }

        }

        //adds an action to the queue, without newName
        protected void addQ(string relativePath, fileAction action)
        {
            addQ(relativePath, action, relativePath);
        }

        //renames a file
        protected void renameFile(string relativePath, string newName)
        {

            if (!ready)
                return;



            status = "Renaming " + relativePath + " to " + newName + "...";



            FileInfo file = new FileInfo(remotePath + relativePath);



            try
            {
                file.MoveTo(remotePath + newName);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        //renames a dir
        protected void renameDir(string relativePath, string newName)
        {

            if (!ready)
                return;



            status = "Renaming " + relativePath + " to " + newName + "...";



            DirectoryInfo dir = new DirectoryInfo(remotePath + relativePath);



            try
            {
                dir.MoveTo(remotePath + newName);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        //removes files from remotePath that don't exist in localPath
        public void removeMissing()
        {

            if (!ready)
                return;



            overallStatus = "Removing deleted files...";
            if (fileQ.Count == 0)
                status = overallStatus;



            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();
            dirQ.Enqueue(new DirectoryInfo(remotePath));



            while (dirQ.Count > 0)
            {

                DirectoryInfo currentDir = dirQ.Dequeue();



                FileInfo[] files;

                try
                {
                    files = currentDir.GetFiles();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (FileInfo file in files)
                {

                    string relativePath = file.FullName.Substring(remotePath.Length);

                    if (!File.Exists(localPath + relativePath))
                        addQ(relativePath, fileAction.delete);

                }



                DirectoryInfo[] dirs;

                try
                {
                    dirs = currentDir.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (DirectoryInfo dir in dirs)
                {

                    string relativePath = dir.FullName.Substring(remotePath.Length);


                    if (!Directory.Exists(localPath + relativePath))
                        addQ(relativePath, fileAction.delete);
                    else
                        dirQ.Enqueue(dir);

                }

            }



            overallStatus = "Watching file system for changes...";

            if (fileQ.Count == 0)
                status = overallStatus;

        }

        //initially syncs local folder and all remoteLocations
        public void initialSync()
        {

            if (!ready)
                return;



            overallStatus = "Copying new and modified files...";
            if (fileQ.Count == 0)
                status = overallStatus;



            Queue<DirectoryInfo> dirQ =  new Queue<DirectoryInfo>();
            dirQ.Enqueue(new DirectoryInfo(localPath));



            while (dirQ.Count > 0)
            {

                DirectoryInfo currentDir = dirQ.Dequeue();



                FileInfo[] files;

                try
                {
                    files = currentDir.GetFiles();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (FileInfo file in files)
                {

                    string relativePath = "";

                    try
                    {
                        relativePath = file.FullName.Substring(localPath.Length);
                    }
                    catch (PathTooLongException)
                    {
                        continue;
                    }

                    if (!checkFile(relativePath))
                        addQ(relativePath, fileAction.copy);

                }



                DirectoryInfo[] dirs;

                try
                {
                    dirs = currentDir.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                
                foreach (DirectoryInfo dir in dirs)
                {

                    dirQ.Enqueue(dir);


                    string relativePath = "";

                    try
                    {
                        relativePath = dir.FullName.Substring(localPath.Length);
                    }
                    catch (PathTooLongException)
                    {
                        continue;
                    }


                    if (!checkDir(relativePath))
                        addQ(relativePath, fileAction.copy);

                }

            }


            overallStatus = "Watching file system for changes...";
            if (fileQ.Count == 0)
                status = overallStatus;

        }

        //fires when a file is created or changed
        protected void watcher_Changed(object sender, FileSystemEventArgs e)
        {

            if (!ready)
                return;



            string relativePath = e.FullPath.Substring(localPath.Length);

            addQ(relativePath, fileAction.copy);

        }

        //fires when file is deleted
        protected void watcher_Deleted(object sender, FileSystemEventArgs e)
        {

            if (!ready)
                return;



            string relativePath = e.FullPath.Substring(localPath.Length);

            addQ(relativePath, fileAction.delete);

        }

        //fires when file is renamed
        protected void watcher_Renamed(object sender, RenamedEventArgs e)
        {

            if (!ready)
                return;



            string relativePath = e.OldFullPath.Substring(localPath.Length);
            string relativeNewpath = e.FullPath.Substring(localPath.Length);


            addQ(relativePath, fileAction.rename, relativeNewpath);

        }

        //pops the fileQ
        public void popQ()
        {
            
            if (!ready)
                return;

            if (fileQ.Count == 0)
                return;



            queueItem item = fileQ.Dequeue();


            FileInfo file = new FileInfo((item.action == fileAction.delete ? remotePath : localPath) + item.newName);



            if ((file.Attributes & FileAttributes.Directory) != 0)
            {

                switch (item.action)
                {

                    case fileAction.copy:
                        copyDir(item.relativePath);
                        break;

                    case fileAction.delete:
                        deleteDir(item.relativePath);
                        break;

                    case fileAction.rename:
                        renameDir(item.relativePath, item.newName);
                        break;

                }

            }
            else
            {

                switch (item.action)
                {

                    case fileAction.copy:
                        copyFile(item.relativePath);
                        break;

                    case fileAction.delete:
                        deleteFile(item.relativePath);
                        break;

                    case fileAction.rename:
                        renameFile(item.relativePath, item.newName);
                        break;

                }

            }


            if (fileQ.Count == 0)
                status = overallStatus;
            else
                _status = overallStatus;

        }

    }
}
