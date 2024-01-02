using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace DFApp.IP
{
    public class DynamicIP : AuditedAggregateRoot<Guid>, ISoftDelete
    {
        public required string IP { get; set; }
        public required string Port { get; set; }
        public bool IsDeleted { get; set; }
    }
}
