using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.FileUploadDownload
{
    [SugarTable("FileUploadInfos")]
    public class FileUploadInfo : AuditedEntity<long>
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Sha1 { get; set; }
        public long FileSize { get; set; }
    }
}
