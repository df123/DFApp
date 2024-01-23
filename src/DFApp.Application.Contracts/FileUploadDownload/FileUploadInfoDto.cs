using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.FileUploadDownload
{
    public class FileUploadInfoDto:AuditedEntityDto<long>
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Sha1 { get; set; }
        public long FileSize { get; set; }

    }
}
