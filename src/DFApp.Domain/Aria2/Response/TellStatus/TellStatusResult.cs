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

        public string? Bitfield { get; set; }

        public long? CompletedLength { get; set; }

        public long? Connections { get; set; }

        public string? Dir { get; set; }

        public long? DownloadSpeed { get; set; }

        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public List<FilesItem>? Files { get; set; }

        public string? GID { get; set; }

        public long? NumPieces { get; set; }

        public long? PieceLength { get; set; }

        public string? Status { get; set; }

        public long? TotalLength { get; set; }

        public long? UploadLength { get; set; }

        public long? UploadSpeed { get; set; }

    }
}
