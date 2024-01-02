using System;
using Volo.Abp.Application.Dtos;

namespace DFApp.IP
{
    public class DynamicIPDto : AuditedEntityDto<Guid>
    {
        public string? IP { get; set; }
        public string? Port { get; set; }
    }
}
