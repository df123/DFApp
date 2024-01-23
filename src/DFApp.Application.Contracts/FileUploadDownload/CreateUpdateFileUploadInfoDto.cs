using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.FileUploadDownload
{
    public class CreateUpdateFileUploadInfoDto
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Sha1 { get; set; }
        public long FileSize { get; set; }
    }
}
