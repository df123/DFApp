using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Media.ExternalLink
{
    public class ExternalLinkDto:AuditedEntityDto<long>
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public string TimeConsumed { get; set; }
        public bool IsRemove { get; set; }
        public string LinkContent { get; set; }
        public string MediaIds { get; set; }
    }
}
