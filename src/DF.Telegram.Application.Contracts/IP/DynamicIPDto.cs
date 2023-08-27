using System;
using Volo.Abp.Application.Dtos;

namespace DF.Telegram.IP
{
    public class DynamicIPDto : AuditedEntityDto<Guid>
    {
        public string? IP { get; set; }
        public string? Port { get; set; }
    }
}
