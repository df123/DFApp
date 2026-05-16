namespace DFApp.Web.DTOs.Configuration;

/// <summary>
/// 配置信息 DTO
/// </summary>
public class ConfigurationInfoDto : AuditedEntityDto<long>
{
    public string ModuleName { get; set; } = default!;
    public string ConfigurationName { get; set; } = default!;
    public string ConfigurationValue { get; set; } = default!;
    public string Remark { get; set; } = default!;
}
