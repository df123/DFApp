using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Aria2.Response.TellStatus
{
    public class UrisItem : CreationAuditedAggregateRoot<long>
    {
        public string Status { get; set; }

        public string Uri { get; set; }

        public FilesItem FilesItem { get; set; }
        public long FilesItemId { get; set; }

    }
}
