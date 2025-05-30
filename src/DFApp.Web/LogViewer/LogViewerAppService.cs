using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DFApp.LogViewer;
using DFApp.LogViewer.Dtos;
using Microsoft.AspNetCore.Hosting;
using Volo.Abp;

namespace DFApp.Web.LogViewer
{
    public class LogViewerAppService : DFAppAppService, ILogViewerAppService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string LogFolder = "Logs";
        private const int DefaultTailLines = 1000; // 默认读取最后1000行

        public LogViewerAppService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

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

        public async Task<string> GetLogContentAsync(string fileName, bool isTail = true)
        {
            Check.NotNullOrWhiteSpace(fileName, nameof(fileName));
            
            var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, LogFolder);
            var filePath = Path.Combine(logPath, fileName);

            if (!File.Exists(filePath))
            {
                throw new UserFriendlyException($"Log file {fileName} not found");
            }

            if (isTail)
            {
                return await ReadLastLinesAsync(filePath, DefaultTailLines);
            }
            
            return await File.ReadAllTextAsync(filePath);
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
    }
}
