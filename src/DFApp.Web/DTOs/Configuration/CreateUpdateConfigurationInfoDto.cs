namespace DFApp.Web.DTOs.Configuration;

/// <summary>
/// 创建/更新配置信息 DTO
/// </summary>
public class CreateUpdateConfigurationInfoDto
{
    public string? ModuleName { get; set; }
    public string ConfigurationName { get; set; } = default!;
    public string ConfigurationValue { get; set; } = default!;
    public string? Remark { get; set; }
}
