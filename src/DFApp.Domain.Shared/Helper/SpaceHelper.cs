using System;
using System.IO;
using System.Linq;
using TL;

namespace DFApp.Helper
{
    public class SpaceHelper
    {
        public static double GetHomeAvailableMB()
        {
            return StorageUnitConversionHelper.ByteToMB(GetAnyDriveAvailable("/home"));
        }

        public static double GetRootAvailableMB()
        {
            return StorageUnitConversionHelper.ByteToMB(GetAnyDriveAvailable("/"));
        }

        public static double GetDriveAvailableMB(string driveName)
        {
            return StorageUnitConversionHelper.ByteToMB(GetAnyDriveAvailable(driveName));
        }

        public static double GetAnyDriveAvailable(string name)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true && d.Name == name)
                {
                    return d.AvailableFreeSpace;
                }
            }
            return -1;
        }

        public static void DeleteFile(string path)
        {
            if ((!string.IsNullOrEmpty(path)) && (!string.IsNullOrWhiteSpace(path)))
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                if (System.IO.File.Exists(path + ".temp"))
                {
                    System.IO.File.Delete(path + ".temp");
                }
            }
        }

        public static void DeleteDirectory(string path)
        {
            if ((!string.IsNullOrEmpty(path))
                && (!string.IsNullOrWhiteSpace(path))
                && System.IO.Directory.Exists(path)
                && (!Directory.EnumerateFileSystemEntries(path).Any()))
            {
                System.IO.Directory.Delete(path);
            }
        }

        public static void DeleteTempFiles(string directoryPath)
        {
            foreach (string file in Directory.GetFiles(directoryPath))
            {
                if (Path.GetExtension(file).Equals(".temp", StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(file);
                }
            }

            foreach (string subdirectory in Directory.GetDirectories(directoryPath))
            {
                DeleteTempFiles(subdirectory);
            }
        }

        public static void DeleteEmptyFolders(string parentFolder)
        {
            string[] folders = Directory.GetDirectories(parentFolder);
            foreach (string folder in folders)
            {
                DeleteEmptyFolders(folder);
                if (Directory.GetFiles(folder).Length == 0 && Directory.GetDirectories(folder).Length == 0)
                {
                    Directory.Delete(folder);
                }
            }
        }

        public static void ClearDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrWhiteSpace(directoryPath))
            {
                return;
            }

            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            // 删除所有文件
            foreach (string file in Directory.GetFiles(directoryPath))
            {
                File.Delete(file);
            }

            // 递归删除所有子目录
            foreach (string subdirectory in Directory.GetDirectories(directoryPath))
            {
                Directory.Delete(subdirectory, true);
            }
        }

    }
}