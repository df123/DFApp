using Microsoft.AspNetCore.Mvc;

namespace DFApp.Downloader.App.Controllers;

/// <summary>
/// 日志查看接口，提供下载器运行日志的列表与内容读取
/// </summary>
[ApiController]
[Route("api")]
public class LogsController : ControllerBase
{
    private readonly string _logsDir;

    public LogsController(IWebHostEnvironment env)
    {
        // Serilog 写入 {ContentRoot}/logs，开发时 ContentRoot 为项目目录
        _logsDir = Path.Combine(env.ContentRootPath, "logs");
    }

    /// <summary>日志文件列表（按修改时间倒序）</summary>
    [HttpGet("logs")]
    public IActionResult List()
    {
        if (!Directory.Exists(_logsDir))
        {
            return Ok(new { items = Array.Empty<object>() });
        }

        var items = Directory.EnumerateFiles(_logsDir, "*.log")
            .Select(p => new FileInfo(p))
            .OrderByDescending(f => f.LastWriteTime)
            .Select(f => new
            {
                fileName = f.Name,
                sizeBytes = f.Length,
                lastWriteTime = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToArray();

        return Ok(new { items });
    }

    /// <summary>读取日志内容</summary>
    /// <param name="fileName">日志文件名</param>
    /// <param name="lines">返回行数，默认 800，上限 5000</param>
    /// <param name="order">tail（末尾，默认）/ head（开头）</param>
    [HttpGet("logs/{fileName}")]
    public IActionResult GetContent(string fileName, [FromQuery] int? lines, [FromQuery] string? order)
    {
        if (!IsValidFileName(fileName))
        {
            return BadRequest(new { message = "无效的日志文件名" });
        }

        var fullPath = Path.GetFullPath(Path.Combine(_logsDir, fileName!));
        var logsRoot = Path.GetFullPath(_logsDir) + Path.DirectorySeparatorChar;

        // 防止目录穿越：解析后的路径必须仍位于 logs 目录内
        if (!fullPath.StartsWith(logsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "无效的日志文件名" });
        }

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound(new { message = "日志文件不存在" });
        }

        var maxLines = lines is null or 0 ? 800 : Math.Min(lines.Value, 5000);
        var readTail = !"head".Equals(order, StringComparison.OrdinalIgnoreCase);

        var allLines = System.IO.File.ReadAllLines(fullPath);
        var total = allLines.Length;

        var resultLines = readTail
            ? (total > maxLines ? allLines.Skip(total - maxLines).ToArray() : allLines)
            : (total > maxLines ? allLines.Take(maxLines).ToArray() : allLines);

        return Ok(new
        {
            fileName,
            content = string.Join('\n', resultLines),
            returnedLines = resultLines.Length,
            totalLines = total,
            order = readTail ? "tail" : "head"
        });
    }

    /// <summary>
    /// 文件名合法性校验：禁止目录分隔符与上跳，仅允许 .log 扩展名
    /// </summary>
    private static bool IsValidFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        if (fileName.Contains("..", StringComparison.Ordinal)
            || fileName.Contains('/', StringComparison.Ordinal)
            || fileName.Contains('\\', StringComparison.Ordinal))
        {
            return false;
        }

        return fileName.EndsWith(".log", StringComparison.OrdinalIgnoreCase);
    }
}
