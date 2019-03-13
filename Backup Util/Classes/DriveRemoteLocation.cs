using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Drawing;

namespace Backup_Util.Classes
{
    class DriveRemoteLocation: RemoteLocation
    {

        public DriveRemoteLocation(string name, LocalLocation local)
        {
            load(name, local);
        }

        public override void loadConfig()
        {

            setAction("Loading configuration...");

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Phat Backup\Local Locations\" + localLocation.configName + @"\Remote Locations\" + configName);
            if (key == null)
            {
                setStatus("Registry key not found!?!", Color.Red);
                setAction();
                ready = false;
                return;
            }

            path = (string)key.GetValue("path", "C:\backup");
            setPath(path);

            proofFile = (string)key.GetValue("proofFile", "phat_backup");

        }

        public override void verifyPath()
        {

            if (!ready)
                return;


            setAction("Verifying paths...");


            DirectoryInfo remoteDir = new DirectoryInfo(path);

            if (!remoteDir.Root.Exists)
            {
                setStatus("Could not find " + remoteDir.Root.FullName + "!", Color.Red);
                setAction();
                ready = false;
                return;
            }
            else if (!remoteDir.Exists)
            {

                try
                {
                    remoteDir.Create();
                }
                catch
                {
                    setStatus(remoteDir.FullName + " does not exist, and it could not be created!", Color.Red);
                    setAction();
                    ready = false;
                    return;
                }

            }


            FileInfo file = new FileInfo(Path.Combine(driveName, proofFile));

            if (!file.Exists)
            {

                if (System.Windows.Forms.MessageBox.Show(driveName + " may be a different drive from last time, would you like to use it anyway?  ALL files in the destination selected on the drive will be replaced with the backup files.", "New drive detected!", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.No)
                {
                    setStatus("Stopped!  New drive detected", Color.Red);
                    setAction();
                    ready = false;
                    return;
                }


                try
                {
                    file.Create();
                    file.Attributes |= FileAttributes.System & FileAttributes.Hidden;
                }
                catch
                {
                }

            }

        }

        public override void syncFiles()
        {

            if (!ready)
                return;


            setStatus("Removing deleted files");
            setAction();


            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();
            dirQ.Enqueue(new DirectoryInfo(path));


            do
            {

                DirectoryInfo remoteDir = dirQ.Dequeue();

                string relativePath = deroot(remoteDir.FullName.Substring(path.Length));

                DirectoryInfo localDir = new DirectoryInfo(Path.Combine(localLocation.path, relativePath));

                if (!localDir.Exists)
                {
                    enqueue(deleteDir, relativePath);
                    return;
                }


                FileInfo[] files;
                try
                {
                    files = remoteDir.GetFiles();
                }
                catch
                {
                    enqueue(deleteDir, relativePath);
                    continue;
                }


                foreach (FileInfo remoteFile in files)
                {

                    relativePath = deroot(remoteFile.FullName.Substring(path.Length));

                    FileInfo localFile = new FileInfo(Path.Combine(localLocation.path, relativePath));

                    if (!localFile.Exists)
                        enqueue(deleteFile, relativePath);

                }

                foreach (DirectoryInfo subDir in remoteDir.GetDirectories())
                    dirQ.Enqueue(subDir);

            } while (dirQ.Count > 0);

        }

        public override void deleteFile(string relativePath)
        {

            if (!ready)
                return;


            relativePath = deroot(relativePath);


            FileInfo remoteFile = new FileInfo(Path.Combine(path, relativePath));

            if (!remoteFile.Exists)
                return;


            if (remoteFile.Name == proofFile)
                return;


            setAction("Deleting " + remoteFile.FullName + "...");


            if (remoteFile.IsReadOnly)
            {
                try
                {
                    remoteFile.IsReadOnly = false;
                }
                catch
                {
                }

            }


            try
            {
                remoteFile.Delete();
            }
            catch
            {
            }

        }

        public override void deleteDir(string relativePath)
        {

            if (!ready)
                return;


            relativePath = deroot(relativePath);


            DirectoryInfo remoteDir = new DirectoryInfo(Path.Combine(path, relativePath));

            if (!remoteDir.Exists)
                return;


            setAction("Deleting " + remoteDir.FullName + "...");


            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();
            dirQ.Enqueue(remoteDir);


            do
            {

                DirectoryInfo dir = dirQ.Dequeue();

                foreach (FileInfo file in dir.GetFiles())
                {

                    if (file.IsReadOnly)
                    {
                        try
                        {
                            file.IsReadOnly = false;
                        }
                        catch
                        {
                        }
                    }

                }

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                    dirQ.Enqueue(subDir);

            } while (dirQ.Count > 0);


            try
            {
                remoteDir.Delete(true);
            }
            catch
            {
            }

        }

        public override bool compareFile(string relativePath)
        {

            if (!ready)
                return true;


            relativePath = deroot(relativePath);


            FileInfo remoteFile = new FileInfo(Path.Combine(path, relativePath));
            FileInfo localFile = new FileInfo(Path.Combine(localLocation.path, relativePath));


            if (!localFile.Exists)
            {
                deleteFile(relativePath);
                return true;
            }

            if (!remoteFile.Exists)
                return false;


            int comparison = compareFileTimes(relativePath);

            if (comparison != 0)
            {
                if (comparison < 0)
                    return false;
                else if ((comparison > 0) && (!localLocation.reverseSyncNewer))
                    return false;
            }


            if (remoteFile.Length != localFile.Length)
                return false;


            return true;

        }

        public override bool compareDir(string relativePath)
        {

            if (!ready)
                return true;


            relativePath = deroot(relativePath);


            DirectoryInfo remoteDir = new DirectoryInfo(Path.Combine(path, relativePath));
            DirectoryInfo localDir = new DirectoryInfo(Path.Combine(localLocation.path, relativePath));


            if (!localDir.Exists)
            {
                deleteDir(relativePath);
                return true;
            }

            if (!remoteDir.Exists)
                return false;


            return true;

        }

        public override void copyFile(string relativePath)
        {

            if (!ready)
                return;


            if (compareFile(relativePath))
                return;


            relativePath = deroot(relativePath);


            FileInfo localFile = new FileInfo(Path.Combine(localLocation.path, relativePath));
            FileInfo remoteFile = new FileInfo(Path.Combine(path, relativePath));


            setAction("Copying " + localFile.FullName + "...");


            if (! remoteFile.Directory.Exists)
                copyDir(Path.GetFullPath(relativePath));


            try
            {
                localFile.CopyTo(remoteFile.FullName, true);
            }
            catch
            {
            }

        }

        public override void copyDir(string relativePath)
        {

            if (!ready)
                return;


            if (compareDir(relativePath))
                return;


            relativePath = deroot(relativePath);


            DirectoryInfo localDir = new DirectoryInfo(Path.Combine(localLocation.path, relativePath));
            DirectoryInfo remoteDir = new DirectoryInfo(Path.Combine(path, relativePath));


            setAction("Copying " + localDir.FullName + "...");


            try
            {
                remoteDir.Create();
            }
            catch
            {
            }

        }

        public override void renameFile(string oldRelativePath, string newRelativePath)
        {

            if (!ready)
                return;


            oldRelativePath = deroot(oldRelativePath);
            newRelativePath = deroot(newRelativePath);


            FileInfo remoteFile = new FileInfo(Path.Combine(path, oldRelativePath));
            string newRemotePath = Path.Combine(path, newRelativePath);


            if (!remoteFile.Exists)
            {
                copyFile(newRelativePath);
                return;
            }


            setAction("Renaming " + remoteFile.FullName + " to " + newRemotePath + "...");


            bool readOnly = remoteFile.IsReadOnly;

            if (readOnly)
            {
                try
                {
                    remoteFile.IsReadOnly = false;
                }
                catch
                {
                }
            }


            try
            {
                remoteFile.MoveTo(newRemotePath);
            }
            catch
            {
            }


            if (readOnly)
                remoteFile.IsReadOnly = true;

        }

        public override void renameDir(string oldRelativePath, string newRelativePath)
        {

            if (!ready)
                return;


            oldRelativePath = deroot(oldRelativePath);
            newRelativePath = deroot(newRelativePath);


            DirectoryInfo remoteDir = new DirectoryInfo(Path.Combine(path, oldRelativePath));
            string newRemotePath = Path.Combine(path, newRelativePath);


            setAction("Renaming " + remoteDir.FullName + " to " + newRemotePath + "...");


            if (!remoteDir.Exists)
            {
                copyDir(newRelativePath);
                return;
            }


            try
            {
                remoteDir.MoveTo(newRemotePath);
            }
            catch
            {
            }

        }

    }
}
