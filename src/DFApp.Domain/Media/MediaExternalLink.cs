using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Media
{
    public class MediaExternalLink : AuditedAggregateRoot<long>
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public long TimeConsumed { get; set; }
        public bool IsRemove { get; set; }
        public string LinkContent { get; set; }    
        public string MediaIds { get; set; }
    }
}
