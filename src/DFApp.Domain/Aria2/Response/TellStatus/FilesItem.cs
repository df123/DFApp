using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Aria2.Response.TellStatus
{
    public class FilesItem : CreationAuditedAggregateRoot<long>
    {
        public string CompletedLength { get; set; }

        public string Index { get; set; }

        public string Length { get; set; }

        public string Path { get; set; }

        public string Selected { get; set; }

        public List<UrisItem> Uris { get; set; }


        public TellStatusResult Result { get; set; }
        public int ResultId { get; set; }
    }
}
