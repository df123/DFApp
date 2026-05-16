using DFApp.Configuration;
using DFApp.Web.DTOs.Configuration;
using DFApp.Web.DTOs.FileUploadDownload;
using Riok.Mapperly.Abstractions;

namespace DFApp.Web.Mapping;

/// <summary>
/// 配置信息映射器
/// </summary>
[Mapper]
public partial class ConfigurationMapper
{
    /// <summary>
    /// ConfigurationInfo → ConfigurationInfoDto
    /// </summary>
    public partial ConfigurationInfoDto MapToDto(ConfigurationInfo entity);

    /// <summary>
    /// CreateUpdateConfigurationInfoDto → ConfigurationInfo
    /// </summary>
    [MapperIgnoreTarget(nameof(ConfigurationInfo.ConcurrencyStamp))]
    public partial ConfigurationInfo MapToEntity(CreateUpdateConfigurationInfoDto dto);

    /// <summary>
    /// ConfigurationInfo → CustomFileTypeDto
    /// </summary>
    public partial CustomFileTypeDto MapToCustomFileTypeDto(ConfigurationInfo entity);
}
