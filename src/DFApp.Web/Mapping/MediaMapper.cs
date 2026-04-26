using System;
using DFApp.Media;
using DFApp.Web.DTOs.Media;
using Riok.Mapperly.Abstractions;

namespace DFApp.Web.Mapping;

/// <summary>
/// 媒体信息映射器
/// </summary>
[Mapper]
public partial class MediaMapper
{
    /// <summary>
    /// MediaInfo → MediaInfoDto
    /// MediaId 从 long 转换为 string
    /// </summary>
    [MapProperty(nameof(MediaInfo.MediaId), nameof(MediaInfoDto.MediaId))]
    public partial MediaInfoDto MapToDto(MediaInfo entity);

    /// <summary>
    /// MediaExternalLink → ExternalLinkDto
    /// </summary>
    public partial ExternalLinkDto MapToDto(MediaExternalLink entity);

    /// <summary>
    /// CreateUpdateExternalLinkDto → MediaExternalLink
    /// 忽略审计字段
    /// </summary>
    [MapperIgnoreTarget(nameof(MediaExternalLink.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(MediaExternalLink.CreationTime))]
    [MapperIgnoreTarget(nameof(MediaExternalLink.CreatorId))]
    [MapperIgnoreTarget(nameof(MediaExternalLink.LastModificationTime))]
    [MapperIgnoreTarget(nameof(MediaExternalLink.LastModifierId))]
    [MapperIgnoreTarget(nameof(MediaExternalLink.MediaIds))]
    public partial MediaExternalLink MapToEntity(CreateUpdateExternalLinkDto dto);

    /// <summary>
    /// 将 MediaId 从 long 转换为 string
    /// </summary>
    private string MapLongToString(long value) => value.ToString();
}
