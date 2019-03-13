using System.IO;

namespace Backup_Util.Classes
{
    abstract class Location
    {

        public static frmMain form = null;
        private static DriveDetector driveDetector = new DriveDetector();

        public string configName;
        public string path;
        private bool _ready = true;
        public virtual bool ready
        {
            get
            {
                return _ready;
            }
            set
            {
                _ready = value;
            }
        }
        protected string proofFile;

        public virtual string driveName
        {
            get
            {
                return (new DriveInfo(Path.GetPathRoot(path))).Name;
            }
        }
        public virtual long driveSpace
        {
            get
            {
                if (!ready)
                    return 0;
                else
                    return (new DriveInfo(Path.GetPathRoot(path))).AvailableFreeSpace;
            }
        }
        public virtual string driveFormat
        {
            get
            {
                if (!ready)
                    return "Unknown";
                else
                    return (new DriveInfo(Path.GetPathRoot(path))).DriveFormat;
            }
        }
        public virtual DriveType driveType
        {
            get
            {
                if (!ready)
                    return DriveType.Unknown;
                else
                    return (new DriveInfo(Path.GetPathRoot(path))).DriveType;
            }
        }

        protected abstract void loadGUI();
        public abstract void loadConfig();
        public abstract void verifyPath();
        public abstract void syncFiles();

        protected abstract void driveRemoved();
        protected abstract void driveArrived();

        public abstract void deleteFile(string relativePath);
        public abstract void deleteDir(string relativePath);
        public abstract void copyFile(string relativePath);
        public abstract void copyDir(string relativePath);
        public abstract void renameFile(string oldRelativePath, string newRelativePath);
        public abstract void renameDir(string oldRelativePath, string newRelativePath);

        protected virtual void load()
        {
            loadGUI();
            reload();
            loadDriveDetector();
        }

        protected virtual void reload()
        {

            ready = true;

            loadConfig();
            verifyPath();
            enqueueSync();

        }

        protected void loadDriveDetector()
        {
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(driveDetector_DeviceArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(driveDetector_DeviceRemoved);
        }

        public static string deroot(string relativePath)
        {

            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()) || relativePath.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                relativePath = relativePath.Substring(1);

            return relativePath;

        }

        void driveDetector_DeviceRemoved(object sender, DriveDetectorEventArgs e)
        {
            if (e.Drive == driveName)
            {
                ready = false;
                driveRemoved();
            }
        }

        void driveDetector_DeviceArrived(object sender, DriveDetectorEventArgs e)
        {
            if (e.Drive == driveName)
            {
                driveArrived();
                reload();
            }
        }

        public virtual void enqueueSync()
        {
        }

    }

}
