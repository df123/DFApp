using System;
using System.Collections.Generic;
using DFApp.Rss;
using DFApp.Web.DTOs.Rss;
using Riok.Mapperly.Abstractions;
using RssSourceEntity = DFApp.Rss.RssSource;
using RssSourceDtoType = DFApp.Web.DTOs.Rss.RssSourceDto;
using CreateUpdateRssSourceDtoType = DFApp.Web.DTOs.Rss.CreateUpdateRssSourceDto;
using RssSubscriptionEntity = DFApp.Rss.RssSubscription;
using RssSubscriptionDtoType = DFApp.Web.DTOs.Rss.RssSubscriptionDto;
using CreateUpdateRssSubscriptionDtoType = DFApp.Web.DTOs.Rss.CreateUpdateRssSubscriptionDto;
using RssMirrorItemEntity = DFApp.Rss.RssMirrorItem;
using RssMirrorItemDtoType = DFApp.Web.DTOs.Rss.RssMirrorItemDto;
using RssSubscriptionDownloadEntity = DFApp.Rss.RssSubscriptionDownload;
using RssSubscriptionDownloadDtoType = DFApp.Web.DTOs.Rss.RssSubscriptionDownloadDto;
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
    /// RssSubscription → RssSubscriptionDto
    /// 忽略 RssSourceName（由服务层填充）
    /// </summary>
    [MapperIgnoreTarget(nameof(RssSubscriptionDtoType.RssSourceName))]
    public partial RssSubscriptionDtoType MapToDto(RssSubscriptionEntity entity);

    /// <summary>
    /// CreateUpdateRssSubscriptionDto → RssSubscription
    /// 忽略审计字段
    /// </summary>
    [MapperIgnoreTarget(nameof(RssSubscriptionEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(RssSubscriptionEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(RssSubscriptionEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(RssSubscriptionEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(RssSubscriptionEntity.LastModifierId))]
    public partial RssSubscriptionEntity MapToEntity(CreateUpdateRssSubscriptionDtoType dto);

    /// <summary>
    /// RssMirrorItem → RssMirrorItemDto
    /// 忽略 WordSegments（由服务层单独填充）
    /// </summary>
    [MapperIgnoreTarget(nameof(RssMirrorItemDtoType.WordSegments))]
    [MapperIgnoreTarget(nameof(RssMirrorItemDtoType.RssSourceName))]
    public partial RssMirrorItemDtoType MapToDto(RssMirrorItemEntity entity);

    /// <summary>
    /// RssSubscriptionDownload → RssSubscriptionDownloadDto
    /// 忽略由服务层填充的显示字段
    /// </summary>
    [MapperIgnoreTarget(nameof(RssSubscriptionDownloadDtoType.SubscriptionName))]
    [MapperIgnoreTarget(nameof(RssSubscriptionDownloadDtoType.RssMirrorItemTitle))]
    [MapperIgnoreTarget(nameof(RssSubscriptionDownloadDtoType.RssMirrorItemLink))]
    [MapperIgnoreTarget(nameof(RssSubscriptionDownloadDtoType.RssSourceName))]
    [MapperIgnoreTarget(nameof(RssSubscriptionDownloadDtoType.DownloadStatusText))]
    public partial RssSubscriptionDownloadDtoType MapToDto(RssSubscriptionDownloadEntity entity);

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
