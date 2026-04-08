using DFApp.IP;
using DFApp.Web.DTOs.IP;
using Riok.Mapperly.Abstractions;

namespace DFApp.Web.Mapping;

/// <summary>
/// 动态 IP 映射器
/// </summary>
[Mapper]
public partial class IPMapper
{
    /// <summary>
    /// DynamicIP → DynamicIPDto
    /// </summary>
    public partial DynamicIPDto MapToDto(DynamicIP entity);

    /// <summary>
    /// CreateUpdateDynamicIPDto → DynamicIP
    /// </summary>
    [MapperIgnoreTarget(nameof(DynamicIP.ConcurrencyStamp))]
    public partial DynamicIP MapToEntity(CreateUpdateDynamicIPDto dto);
}
