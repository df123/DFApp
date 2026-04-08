using System;

namespace DFApp.Web.DTOs.LogViewer;

/// <summary>
/// 日志文件信息 DTO
/// </summary>
public class LogFileDto
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; }
}
