using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.FileUploadDownload
{
    public class FileUploadInfo : AuditedAggregateRoot<long>, ISoftDelete
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Sha1 { get; set; }
        public long FileSize { get; set; }
        public bool IsDeleted { get; set; }
    }
}
