using System;
using System.Collections.Generic;
using DFApp.Rss;
using DFApp.Web.DTOs.Rss;
using Riok.Mapperly.Abstractions;
using RssSourceEntity = DFApp.Rss.RssSource;
using RssSourceDtoType = DFApp.Web.DTOs.Rss.RssSourceDto;
using CreateUpdateRssSourceDtoType = DFApp.Web.DTOs.Rss.CreateUpdateRssSourceDto;
using RssMirrorItemEntity = DFApp.Rss.RssMirrorItem;
using RssMirrorItemDtoType = DFApp.Web.DTOs.Rss.RssMirrorItemDto;
using RssWordSegmentEntity = DFApp.Rss.RssWordSegment;
using RssWordSegmentDtoType = DFApp.Web.DTOs.Rss.RssWordSegmentDto;
using RssWordSegmentWithItemDtoType = DFApp.Web.DTOs.Rss.RssWordSegmentWithItemDto;

namespace DFApp.Web.Mapping;

/// <summary>
/// RSS 模块映射器
/// </summary>
[Mapper]
public partial class RssMapper
{
    /// <summary>
    /// RssSource → RssSourceDto
    /// </summary>
    public partial RssSourceDtoType MapToDto(RssSourceEntity entity);

    /// <summary>
    /// CreateUpdateRssSourceDto → RssSource
    /// 忽略审计字段
    /// </summary>
    [MapperIgnoreTarget(nameof(RssSourceEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(RssSourceEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(RssSourceEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(RssSourceEntity.ExtraProperties))]
    public partial RssSourceEntity MapToEntity(CreateUpdateRssSourceDtoType dto);

    /// <summary>
    /// RssMirrorItem → RssMirrorItemDto
    /// 忽略 WordSegments（由服务层单独填充）
    /// </summary>
    [MapperIgnoreTarget(nameof(RssMirrorItemDtoType.WordSegments))]
    [MapperIgnoreTarget(nameof(RssMirrorItemDtoType.RssSourceName))]
    public partial RssMirrorItemDtoType MapToDto(RssMirrorItemEntity entity);

    /// <summary>
    /// RssWordSegment → RssWordSegmentDto
    /// </summary>
    public partial RssWordSegmentDtoType MapToDto(RssWordSegmentEntity entity);

    /// <summary>
    /// RssWordSegment → RssWordSegmentWithItemDto
    /// 忽略由服务层填充的关联字段
    /// </summary>
    [MapperIgnoreTarget(nameof(RssWordSegmentWithItemDtoType.RssMirrorItemTitle))]
    [MapperIgnoreTarget(nameof(RssWordSegmentWithItemDtoType.RssMirrorItemLink))]
    [MapperIgnoreTarget(nameof(RssWordSegmentWithItemDtoType.RssSourceId))]
    [MapperIgnoreTarget(nameof(RssWordSegmentWithItemDtoType.RssSourceName))]
    public partial RssWordSegmentWithItemDtoType MapToWithItemDto(RssWordSegmentEntity entity);
}
