using System;
using System.IO;
using System.Linq;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 磁盘空间和文件操作辅助类
/// </summary>
public static class SpaceHelper
{
    /// <summary>
    /// 获取指定驱动器的可用空间（GB）
    /// </summary>
    public static double GetAnyDriveAvailable(string? drivePath)
    {
        try
        {
            string? rootDir;
            if (string.IsNullOrWhiteSpace(drivePath))
            {
                rootDir = Path.GetPathRoot(Environment.CurrentDirectory);
            }
            else
            {
                rootDir = Path.GetPathRoot(drivePath);
            }

            if (rootDir != null)
            {
                var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == rootDir);
                if (driveInfo != null && driveInfo.IsReady)
                {
                    return driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                }
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 删除文件（忽略错误）
    /// </summary>
    public static void DeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // 忽略删除错误
        }
    }

    /// <summary>
    /// 删除空文件夹（递归）
    /// </summary>
    public static void DeleteEmptyFolders(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            // 先递归处理子目录
            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteEmptyFolders(directory);
            }

            // 如果目录为空，则删除
            if (!Directory.EnumerateFileSystemEntries(path).Any())
            {
                Directory.Delete(path);
            }
        }
        catch
        {
            // 忽略删除错误
        }
    }

    /// <summary>
    /// 获取指定驱动器的可用空间（MB）
    /// </summary>
    public static double GetDriveAvailableMB(string driveName)
    {
        return GetAnyDriveAvailable(driveName) * 1024;
    }

    /// <summary>
    /// 递归删除指定目录下的所有 .temp 文件
    /// </summary>
    public static void DeleteTempFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath)) return;

        foreach (string file in Directory.GetFiles(directoryPath))
        {
            if (Path.GetExtension(file).Equals(".temp", StringComparison.OrdinalIgnoreCase))
            {
                try { File.Delete(file); } catch { }
            }
        }

        foreach (string subdirectory in Directory.GetDirectories(directoryPath))
        {
            DeleteTempFiles(subdirectory);
        }
    }

    /// <summary>
    /// 清空目录内容（不删除目录本身）
    /// </summary>
    public static void ClearDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(path))
        {
            File.Delete(file);
        }

        foreach (var directory in Directory.GetDirectories(path))
        {
            Directory.Delete(directory, true);
        }
    }
}
