using System;

namespace DFApp.LogViewer.Dtos
{
    public class LogFileDto
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }
}
