using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Aria2.Response.TellStatus
{
    public class FilesItem : CreationAuditedAggregateRoot<int>
    {
        public long? CompletedLength { get; set; }

        public long? Index { get; set; }

        public long? Length { get; set; }

        public string? Path { get; set; }

        public bool? Selected { get; set; }

        public List<UrisItem>? Uris { get; set; }


        public TellStatusResult Result { get; set; }
        public long ResultId { get; set; }
    }
}
