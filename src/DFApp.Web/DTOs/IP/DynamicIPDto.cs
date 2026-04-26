using System;

namespace DFApp.Web.DTOs.IP;

/// <summary>
/// 动态 IP DTO
/// </summary>
public class DynamicIPDto : AuditedEntityDto<Guid>
{
    public string? IP { get; set; }
    public string? Port { get; set; }
}
