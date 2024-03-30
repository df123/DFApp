using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Aria2.Response.TellStatus
{
    public class TellStatusResult : CreationAuditedAggregateRoot<long>
    {
        public string Bitfield { get; set; }

        public string CompletedLength { get; set; }

        public string Connections { get; set; }

        public string Dir { get; set; }

        public string DownloadSpeed { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public List<FilesItem> Files { get; set; }

        public string GID { get; set; }

        public string NumPieces { get; set; }

        public string PieceLength { get; set; }

        public string Status { get; set; }

        public string TotalLength { get; set; }

        public string UploadLength { get; set; }

        public string UploadSpeed { get; set; }
    }
}
