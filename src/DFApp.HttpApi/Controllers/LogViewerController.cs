using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Controllers;
using DFApp.LogViewer.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace DFApp.Web.LogViewer
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogViewerController : DFAppController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string LogFolder = "Logs";
        private const int DefaultTailLines = 1000; // 默认读取最后1000行

        public LogViewerController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("log-files")]
        public async Task<List<LogFileDto>> GetLogFilesAsync()
        {
            var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, LogFolder);

            if (!Directory.Exists(logPath))
            {
                return new List<LogFileDto>();
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

            return await Task.FromResult(logFiles);
        }

        [HttpGet("log-content")]
        public async Task<string> GetLogContentAsync(string fileName, bool isTail = true)
        {
            Check.NotNullOrWhiteSpace(fileName, nameof(fileName));

            var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, LogFolder);
            var filePath = Path.Combine(logPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                throw new UserFriendlyException($"Log file {fileName} not found");
            }

            if (isTail)
            {
                return await ReadLastLinesAsync(filePath, DefaultTailLines);
            }

            return await System.IO.File.ReadAllTextAsync(filePath);
        }

        private async Task<string> ReadLastLinesAsync(string filePath, int lines)
        {
            var buffer = new char[4096];
            var lineCount = 0;
            var contentBuilder = new List<string>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
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
            }

            return string.Join(Environment.NewLine, contentBuilder);
        }
        [HttpGet("download")]
        public IActionResult DownloadLog(string fileName)
        {
            Check.NotNullOrWhiteSpace(fileName, nameof(fileName));

            var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, LogFolder);
            var filePath = Path.Combine(logPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                throw new UserFriendlyException($"Log file {fileName} not found");
            }

            var contentType = "text/plain";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = fileName
            };
        }
    }
}
