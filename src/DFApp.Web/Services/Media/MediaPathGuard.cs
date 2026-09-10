using System;
using System.IO;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Services.Media;

/// <summary>
/// 媒体路径围栏：将保存路径解析到媒体根目录内，越界即拒绝。
/// 所有媒体物理文件的读取与删除都必须经过此围栏，防止配置被篡改或历史脏数据导致任意文件读写
/// </summary>
public static class MediaPathGuard
{
    /// <summary>
    /// 解析路径并校验包含关系：绝对路径直接使用，相对路径按根目录补全，
    /// 最终路径必须位于根目录内
    /// </summary>
    /// <param name="root">媒体根目录（绝对路径）</param>
    /// <param name="savePath">记录中的保存路径</param>
    /// <returns>围栏内的绝对路径</returns>
    public static string ResolveContainedPath(string root, string savePath)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new BusinessException("媒体存储根目录（SaveDrive）未配置");
        }

        var normalizedRoot = Path.GetFullPath(root.Trim());
        var fullPath = Path.GetFullPath(
            Path.IsPathRooted(savePath) ? savePath : Path.Combine(normalizedRoot, savePath));

        // 兼容以分隔符结尾的根（如 Linux 的 "/" 或 "D:\"）：直接用根作前缀，避免拼出 "//" 误杀全部路径
        var rootPrefix = normalizedRoot.EndsWith(Path.DirectorySeparatorChar) ||
                         normalizedRoot.EndsWith(Path.AltDirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(fullPath, normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("媒体文件路径越出存储根目录");
        }

        return fullPath;
    }
}
