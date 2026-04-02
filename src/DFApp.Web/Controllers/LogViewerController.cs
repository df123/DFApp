using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DFApp.LogViewer.Dtos;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 日志查看控制器，提供日志文件列表、内容读取和下载功能
/// </summary>
[ApiController]
[Route("api/app/log-viewer")]
[Authorize]
public class LogViewerController : DFAppControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private const string LogFolder = "Logs";
    private const int DefaultTailLines = 1000;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="webHostEnvironment">Web 宿主环境</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public LogViewerController(
        IWebHostEnvironment webHostEnvironment,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// 获取日志文件列表
    /// </summary>
    [HttpGet("log-files")]
    [Permission(DFAppPermissions.LogViewer.Default)]
    public async Task<IActionResult> GetLogFilesAsync()
    {
        var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, LogFolder);

        if (!Directory.Exists(logPath))
        {
            return Success(new List<LogFileDto>());
        }

        var logFiles = Directory.GetFiles(logPath, "*.txt")
            .Select(f => new FileInfo(f))
            .Select(fi => new LogFileDto
            {
                Name = fi.Name,
                Size = fi.Length,
                LastModified = fi.LastWriteTime
            })
            .OrderByDescending(f => f.LastModified)
            .ToList();

        return Success(await Task.FromResult(logFiles));
    }

    /// <summary>
    /// 读取日志文件内容
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="isTail">是否只读取末尾内容（默认 true）</param>
    [HttpGet("log-content")]
    [Permission(DFAppPermissions.LogViewer.Default)]
    public async Task<IActionResult> GetLogContentAsync([FromQuery] string fileName, [FromQuery] bool isTail = true)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            ThrowBusinessException("文件名不能为空");
            return Fail("文件名不能为空");
        }

        var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, LogFolder);
        var filePath = Path.Combine(logPath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            ThrowBusinessException($"日志文件 {fileName} 不存在");
            return Fail($"日志文件 {fileName} 不存在");
        }

        string content;
        if (isTail)
        {
            content = await ReadLastLinesAsync(filePath, DefaultTailLines);
        }
        else
        {
            content = await System.IO.File.ReadAllTextAsync(filePath);
        }

        return Success(content);
    }

    /// <summary>
    /// 下载日志文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    [HttpGet("download")]
    [Permission(DFAppPermissions.LogViewer.Default)]
    public IActionResult DownloadLog([FromQuery] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            ThrowBusinessException("文件名不能为空");
            return Fail("文件名不能为空");
        }

        var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, LogFolder);
        var filePath = Path.Combine(logPath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            ThrowBusinessException($"日志文件 {fileName} 不存在");
            return Fail($"日志文件 {fileName} 不存在");
        }

        var contentType = "text/plain";
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        return new FileStreamResult(fileStream, contentType)
        {
            FileDownloadName = fileName
        };
    }

    /// <summary>
    /// 读取文件末尾指定行数的内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="lines">行数</param>
    private async Task<string> ReadLastLinesAsync(string filePath, int lines)
    {
        var contentBuilder = new List<string>();

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line != null)
            {
                contentBuilder.Add(line);
                if (contentBuilder.Count > lines)
                {
                    contentBuilder.RemoveAt(0);
                }
            }
        }

        return string.Join(System.Environment.NewLine, contentBuilder);
    }
}
