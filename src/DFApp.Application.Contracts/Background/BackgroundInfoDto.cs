using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Background
{
    public class BackgroundInfoDto
    {
        public string? RunStatus { get; set; }
        public string? ModuleName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? RestartTime { get; set; }
        public int RestartCount { get; set; }
        public bool HasError { get; set; }
        public int ErrorCount { get; set; }
        public string? ErrorDescription { get; set; }
    }
}
