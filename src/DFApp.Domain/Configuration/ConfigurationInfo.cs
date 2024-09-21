using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Configuration
{
    public class ConfigurationInfo : AuditedAggregateRoot<long>, ISoftDelete
    {

        public required string ModuleName { get; set; } 
        public required string ConfigurationName { get; set; } 
        public required string ConfigurationValue { get; set; } 
        public required string Remark { get; set; } 
        public bool IsDeleted { get; set; }
    }
}
