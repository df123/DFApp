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
        public required string Name { get; set; }
        public long Size { get; set; }
        public long TimeConsumed { get; set; }
        public bool IsRemove { get; set; }
        public required string LinkContent { get; set; }    
        public required ICollection<MediaExternalLinkMediaIds> MediaIds { get; set; }
    }
}
