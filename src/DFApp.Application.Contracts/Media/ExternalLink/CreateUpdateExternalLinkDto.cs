using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Media.ExternalLink
{
    public class CreateUpdateExternalLinkDto
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string TimeConsumed { get; set; }
        public bool IsRemove { get; set; }
        public string LinkContent { get; set; }
        public string MediaIds { get; set; }
    }
}
