using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Media
{
    public class MediaExternalLinkMediaIds : Entity<long>
    {
        public long MediaId { get; set; }
        public long MediaExternalLinkId { get; set; }
        public MediaExternalLink ExternalLink { get; set; } = null!;

    }
}
