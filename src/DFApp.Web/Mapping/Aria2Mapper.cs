using System;
using System.Collections.Generic;
using DFApp.Aria2;
using DFApp.Aria2.Notifications;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Web.DTOs.Aria2;
using Riok.Mapperly.Abstractions;
using TellStatusResultEntity = DFApp.Aria2.Response.TellStatus.TellStatusResult;
using TellStatusResultDtoType = DFApp.Web.DTOs.Aria2.TellStatusResultDto;
using FilesItemEntity = DFApp.Aria2.Response.TellStatus.FilesItem;
using FilesItemDtoType = DFApp.Web.DTOs.Aria2.FilesItemDto;
using UrisItemEntity = DFApp.Aria2.Response.TellStatus.UrisItem;
using UrisItemDtoType = DFApp.Web.DTOs.Aria2.UrisItemDto;
using Aria2NotificationEntity = DFApp.Aria2.Notifications.Aria2Notification;
using Aria2NotificationDtoType = DFApp.Web.DTOs.Aria2.Aria2NotificationDto;
using ParamsItemEntity = DFApp.Aria2.Notifications.ParamsItem;
using ParamsItemDtoType = DFApp.Web.DTOs.Aria2.ParamsItemDto;
using Aria2RequestEntity = DFApp.Aria2.Request.Aria2Request;
using Aria2RequestDtoType = DFApp.Web.DTOs.Aria2.Aria2RequestDto;
using Aria2ResponseEntity = DFApp.Aria2.Response.Aria2Response;
using Aria2ResponseDtoType = DFApp.Web.DTOs.Aria2.Aria2ResponseDto;
using ResponseBaseEntity = DFApp.Aria2.ResponseBase;
using ResponseBaseDtoType = DFApp.Web.DTOs.Aria2.ResponseBaseDto;
using TellStatusResponseEntity = DFApp.Aria2.Response.TellStatus.TellStatusResponse;
using TellStatusResponseDtoType = DFApp.Web.DTOs.Aria2.TellStatusResponseDto;

namespace DFApp.Web.Mapping;

/// <summary>
/// Aria2 模块映射器
/// </summary>
[Mapper]
public partial class Aria2Mapper
{
    /// <summary>
    /// TellStatusResult → TellStatusResultDto
    /// </summary>
    [MapProperty(nameof(TellStatusResultEntity.Files), nameof(TellStatusResultDtoType.Files))]
    public partial TellStatusResultDtoType MapToDto(TellStatusResultEntity entity);

    /// <summary>
    /// TellStatusResultDto → TellStatusResult
    /// </summary>
    public partial TellStatusResultEntity MapToEntity(TellStatusResultDtoType dto);

    /// <summary>
    /// FilesItem → FilesItemDto
    /// </summary>
    public partial FilesItemDtoType MapToDto(FilesItemEntity entity);

    /// <summary>
    /// FilesItemDto → FilesItem
    /// </summary>
    [MapperIgnoreTarget(nameof(FilesItemEntity.Result))]
    [MapperIgnoreTarget(nameof(FilesItemEntity.ResultId))]
    public partial FilesItemEntity MapToEntity(FilesItemDtoType dto);

    /// <summary>
    /// UrisItem → UrisItemDto
    /// </summary>
    public partial UrisItemDtoType MapToDto(UrisItemEntity entity);

    /// <summary>
    /// UrisItemDto → UrisItem
    /// </summary>
    [MapperIgnoreTarget(nameof(UrisItemEntity.FilesItem))]
    [MapperIgnoreTarget(nameof(UrisItemEntity.FilesItemId))]
    public partial UrisItemEntity MapToEntity(UrisItemDtoType dto);

    /// <summary>
    /// Aria2Notification → Aria2NotificationDto
    /// </summary>
    public partial Aria2NotificationDtoType MapToDto(Aria2NotificationEntity entity);

    /// <summary>
    /// Aria2NotificationDto → Aria2Notification
    /// </summary>
    public partial Aria2NotificationEntity MapToEntity(Aria2NotificationDtoType dto);

    /// <summary>
    /// ParamsItem → ParamsItemDto
    /// </summary>
    public partial ParamsItemDtoType MapToDto(ParamsItemEntity entity);

    /// <summary>
    /// ParamsItemDto → ParamsItem
    /// </summary>
    public partial ParamsItemEntity MapToEntity(ParamsItemDtoType dto);

    /// <summary>
    /// Aria2Request → Aria2RequestDto
    /// </summary>
    public partial Aria2RequestDtoType MapToDto(Aria2RequestEntity entity);

    /// <summary>
    /// Aria2Response → Aria2ResponseDto
    /// </summary>
    public partial Aria2ResponseDtoType MapToDto(Aria2ResponseEntity entity);

    /// <summary>
    /// ResponseBase → ResponseBaseDto
    /// </summary>
    public partial ResponseBaseDtoType MapToDto(ResponseBaseEntity entity);

    /// <summary>
    /// ResponseBaseDto → ResponseBase
    /// </summary>
    public partial ResponseBaseEntity MapToEntity(ResponseBaseDtoType dto);

    /// <summary>
    /// TellStatusResponse → TellStatusResponseDto
    /// </summary>
    public partial TellStatusResponseDtoType MapToDto(TellStatusResponseEntity entity);

    /// <summary>
    /// TellStatusResponseDto → TellStatusResponse
    /// </summary>
    public partial TellStatusResponseEntity MapToEntity(TellStatusResponseDtoType dto);

    /// <summary>
    /// 将 long? 转换为 string
    /// </summary>
    private string MapNullableLongToString(long? value) => value?.ToString() ?? string.Empty;

    /// <summary>
    /// 将 string 转换为 long?
    /// </summary>
    private long? MapStringToNullableLong(string value) =>
        long.TryParse(value, out var result) ? result : null;
}
