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

        public string ModuleName { get; set; }
        public string ConfigurationName { get; set; }
        public string ConfigurationValue { get; set; }
        public string Remark { get; set; }
        public bool IsDeleted { get; set; }
    }
}
