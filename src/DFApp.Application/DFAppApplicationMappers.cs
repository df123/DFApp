using System;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using DFApp.Bookkeeping.Category;
using DFApp.Bookkeeping.Expenditure.Lookup;
using DFApp.Bookkeeping.Expenditure;
using DFApp.IP;
using DFApp.Lottery;
using DFApp.Media;
using DFApp.Bookkeeping;
using DFApp.FileUploadDownload;
using DFApp.Configuration;
using DFApp.Media.ExternalLink;
using DFApp.Aria2.Notifications;
using DFApp.Aria2.Request;
using DFApp.Aria2;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Aria2.Response;
using DFApp.Lottery.Simulation;
using DFApp.FileFilter;

namespace DFApp;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MediaInfoToMediaInfoDtoMapper : MapperBase<MediaInfo, MediaInfoDto>
{
    [MapProperty(nameof(MediaInfo.MediaId), nameof(MediaInfoDto.MediaId))]
    public override partial MediaInfoDto Map(MediaInfo source);

    [MapProperty(nameof(MediaInfo.MediaId), nameof(MediaInfoDto.MediaId))]
    public override partial void Map(MediaInfo source, MediaInfoDto destination);
}

[Mapper]
public partial class CreateUpdateMediaInfoDtoToMediaInfoMapper : MapperBase<CreateUpdateMediaInfoDto, MediaInfo>
{
    [MapperIgnoreTarget(nameof(MediaInfo.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(MediaInfo.CreationTime))]
    [MapperIgnoreTarget(nameof(MediaInfo.LastModificationTime))]
    public override partial MediaInfo Map(CreateUpdateMediaInfoDto source);

    [MapperIgnoreTarget(nameof(MediaInfo.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(MediaInfo.CreationTime))]
    [MapperIgnoreTarget(nameof(MediaInfo.LastModificationTime))]
    public override partial void Map(CreateUpdateMediaInfoDto source, MediaInfo destination);

    public override void AfterMap(CreateUpdateMediaInfoDto source, MediaInfo destination)
    {
        destination.ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}

[Mapper]
public partial class MediaInfoDtoToMediaInfoMapper : MapperBase<MediaInfoDto, MediaInfo>
{
    [MapperIgnoreTarget(nameof(MediaInfo.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(MediaInfo.CreationTime))]
    [MapperIgnoreTarget(nameof(MediaInfo.LastModificationTime))]
    public override partial MediaInfo Map(MediaInfoDto source);

    [MapperIgnoreTarget(nameof(MediaInfo.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(MediaInfo.CreationTime))]
    [MapperIgnoreTarget(nameof(MediaInfo.LastModificationTime))]
    public override partial void Map(MediaInfoDto source, MediaInfo destination);

    public override void AfterMap(MediaInfoDto source, MediaInfo destination)
    {
        destination.ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MediaInfoDtoToCreateUpdateMediaInfoDtoMapper : MapperBase<MediaInfoDto, CreateUpdateMediaInfoDto>
{
    public override partial CreateUpdateMediaInfoDto Map(MediaInfoDto source);
    public override partial void Map(MediaInfoDto source, CreateUpdateMediaInfoDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class DynamicIPToDynamicIPDtoMapper : MapperBase<DynamicIP, DynamicIPDto>
{
    public override partial DynamicIPDto Map(DynamicIP source);
    public override partial void Map(DynamicIP source, DynamicIPDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateDynamicIPDtoToDynamicIPMapper : MapperBase<CreateUpdateDynamicIPDto, DynamicIP>
{
    [MapperIgnoreTarget(nameof(DynamicIP.ConcurrencyStamp))]
    public override partial DynamicIP Map(CreateUpdateDynamicIPDto source);

    [MapperIgnoreTarget(nameof(DynamicIP.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateDynamicIPDto source, DynamicIP destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotteryInfoToLotteryDtoMapper : MapperBase<LotteryInfo, LotteryDto>
{
    public override partial LotteryDto Map(LotteryInfo source);
    public override partial void Map(LotteryInfo source, LotteryDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateLotteryDtoToLotteryInfoMapper : MapperBase<CreateUpdateLotteryDto, LotteryInfo>
{
    [MapperIgnoreTarget(nameof(LotteryInfo.ConcurrencyStamp))]
    public override partial LotteryInfo Map(CreateUpdateLotteryDto source);

    [MapperIgnoreTarget(nameof(LotteryInfo.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateLotteryDto source, LotteryInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotteryDtoToCreateUpdateLotteryDtoMapper : TwoWayMapperBase<LotteryDto, CreateUpdateLotteryDto>
{
    public override partial CreateUpdateLotteryDto Map(LotteryDto source);
    public override partial void Map(LotteryDto source, CreateUpdateLotteryDto destination);

    public override partial LotteryDto ReverseMap(CreateUpdateLotteryDto destination);
    public override partial void ReverseMap(CreateUpdateLotteryDto destination, LotteryDto source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotteryResultToLotteryResultDtoMapper : MapperBase<LotteryResult, LotteryResultDto>
{
    public override partial LotteryResultDto Map(LotteryResult source);
    public override partial void Map(LotteryResult source, LotteryResultDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateLotteryResultDtoToLotteryResultMapper : MapperBase<CreateUpdateLotteryResultDto, LotteryResult>
{
    [MapperIgnoreTarget(nameof(LotteryResult.ConcurrencyStamp))]
    public override partial LotteryResult Map(CreateUpdateLotteryResultDto source);

    [MapperIgnoreTarget(nameof(LotteryResult.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateLotteryResultDto source, LotteryResult destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotteryResultDtoToCreateUpdateLotteryResultDtoMapper : TwoWayMapperBase<LotteryResultDto, CreateUpdateLotteryResultDto>
{
    public override partial CreateUpdateLotteryResultDto Map(LotteryResultDto source);
    public override partial void Map(LotteryResultDto source, CreateUpdateLotteryResultDto destination);

    public override partial LotteryResultDto ReverseMap(CreateUpdateLotteryResultDto destination);
    public override partial void ReverseMap(CreateUpdateLotteryResultDto destination, LotteryResultDto source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotteryPrizegradesToLotteryPrizegradesDtoMapper : MapperBase<LotteryPrizegrades, LotteryPrizegradesDto>
{
    public override partial LotteryPrizegradesDto Map(LotteryPrizegrades source);
    public override partial void Map(LotteryPrizegrades source, LotteryPrizegradesDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateLotteryPrizegradesDtoToLotteryPrizegradesMapper : MapperBase<CreateUpdateLotteryPrizegradesDto, LotteryPrizegrades>
{
    [MapperIgnoreTarget(nameof(LotteryPrizegrades.ConcurrencyStamp))]
    public override partial LotteryPrizegrades Map(CreateUpdateLotteryPrizegradesDto source);

    [MapperIgnoreTarget(nameof(LotteryPrizegrades.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateLotteryPrizegradesDto source, LotteryPrizegrades destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotteryPrizegradesDtoToCreateUpdateLotteryPrizegradesDtoMapper : TwoWayMapperBase<LotteryPrizegradesDto, CreateUpdateLotteryPrizegradesDto>
{
    public override partial CreateUpdateLotteryPrizegradesDto Map(LotteryPrizegradesDto source);
    public override partial void Map(LotteryPrizegradesDto source, CreateUpdateLotteryPrizegradesDto destination);

    public override partial LotteryPrizegradesDto ReverseMap(CreateUpdateLotteryPrizegradesDto destination);
    public override partial void ReverseMap(CreateUpdateLotteryPrizegradesDto destination, LotteryPrizegradesDto source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ResultItemDtoToCreateUpdateLotteryResultDtoMapper : MapperBase<ResultItemDto, CreateUpdateLotteryResultDto>
{
    public override partial CreateUpdateLotteryResultDto Map(ResultItemDto source);
    public override partial void Map(ResultItemDto source, CreateUpdateLotteryResultDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PrizegradesItemDtoToCreateUpdateLotteryPrizegradesDtoMapper : MapperBase<PrizegradesItemDto, CreateUpdateLotteryPrizegradesDto>
{
    public override partial CreateUpdateLotteryPrizegradesDto Map(PrizegradesItemDto source);
    public override partial void Map(PrizegradesItemDto source, CreateUpdateLotteryPrizegradesDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ResultItemDtoToLotteryResultMapper : MapperBase<ResultItemDto, LotteryResult>
{
    [MapperIgnoreTarget(nameof(LotteryResult.Prizegrades))]
    public override partial LotteryResult Map(ResultItemDto source);

    [MapperIgnoreTarget(nameof(LotteryResult.Prizegrades))]
    public override partial void Map(ResultItemDto source, LotteryResult destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PrizegradesItemDtoToLotteryPrizegradesMapper : MapperBase<PrizegradesItemDto, LotteryPrizegrades>
{
    [MapperIgnoreTarget(nameof(LotteryPrizegrades.LotteryResultId))]
    [MapperIgnoreTarget(nameof(LotteryPrizegrades.Result))]
    public override partial LotteryPrizegrades Map(PrizegradesItemDto source);

    [MapperIgnoreTarget(nameof(LotteryPrizegrades.LotteryResultId))]
    [MapperIgnoreTarget(nameof(LotteryPrizegrades.Result))]
    public override partial void Map(PrizegradesItemDto source, LotteryPrizegrades destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BookkeepingCategoryToBookkeepingCategoryDtoMapper : TwoWayMapperBase<BookkeepingCategory, BookkeepingCategoryDto>
{
    public override partial BookkeepingCategoryDto Map(BookkeepingCategory source);
    public override partial void Map(BookkeepingCategory source, BookkeepingCategoryDto destination);

    public override partial BookkeepingCategory ReverseMap(BookkeepingCategoryDto destination);
    public override partial void ReverseMap(BookkeepingCategoryDto destination, BookkeepingCategory source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateBookkeepingCategoryDtoToBookkeepingCategoryMapper : MapperBase<CreateUpdateBookkeepingCategoryDto, BookkeepingCategory>
{
    [MapperIgnoreTarget(nameof(BookkeepingCategory.ConcurrencyStamp))]
    public override partial BookkeepingCategory Map(CreateUpdateBookkeepingCategoryDto source);

    [MapperIgnoreTarget(nameof(BookkeepingCategory.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateBookkeepingCategoryDto source, BookkeepingCategory destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BookkeepingCategoryToBookkeepingCategoryLookupDtoMapper : MapperBase<BookkeepingCategory, BookkeepingCategoryLookupDto>
{
    [MapProperty(nameof(BookkeepingCategory.Id), nameof(BookkeepingCategoryLookupDto.CategoryId))]
    public override partial BookkeepingCategoryLookupDto Map(BookkeepingCategory source);

    [MapProperty(nameof(BookkeepingCategory.Id), nameof(BookkeepingCategoryLookupDto.CategoryId))]
    public override partial void Map(BookkeepingCategory source, BookkeepingCategoryLookupDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BookkeepingExpenditureToBookkeepingExpenditureDtoMapper : TwoWayMapperBase<BookkeepingExpenditure, BookkeepingExpenditureDto>
{
    public override partial BookkeepingExpenditureDto Map(BookkeepingExpenditure source);
    public override partial void Map(BookkeepingExpenditure source, BookkeepingExpenditureDto destination);

    public override partial BookkeepingExpenditure ReverseMap(BookkeepingExpenditureDto destination);
    public override partial void ReverseMap(BookkeepingExpenditureDto destination, BookkeepingExpenditure source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateBookkeepingExpenditureDtoToBookkeepingExpenditureMapper : MapperBase<CreateUpdateBookkeepingExpenditureDto, BookkeepingExpenditure>
{
    [MapperIgnoreTarget(nameof(BookkeepingExpenditure.ConcurrencyStamp))]
    public override partial BookkeepingExpenditure Map(CreateUpdateBookkeepingExpenditureDto source);

    [MapperIgnoreTarget(nameof(BookkeepingExpenditure.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateBookkeepingExpenditureDto source, BookkeepingExpenditure destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ConfigurationInfoToConfigurationInfoDtoMapper : TwoWayMapperBase<ConfigurationInfo, ConfigurationInfoDto>
{
    public override partial ConfigurationInfoDto Map(ConfigurationInfo source);
    public override partial void Map(ConfigurationInfo source, ConfigurationInfoDto destination);

    public override partial ConfigurationInfo ReverseMap(ConfigurationInfoDto destination);
    public override partial void ReverseMap(ConfigurationInfoDto destination, ConfigurationInfo source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateConfigurationInfoDtoToConfigurationInfoMapper : MapperBase<CreateUpdateConfigurationInfoDto, ConfigurationInfo>
{
    public override partial ConfigurationInfo Map(CreateUpdateConfigurationInfoDto source);
    public override partial void Map(CreateUpdateConfigurationInfoDto source, ConfigurationInfo destination);

    public override void AfterMap(CreateUpdateConfigurationInfoDto source, ConfigurationInfo destination)
    {
        destination.ModuleName = source.ModuleName ?? string.Empty;
        destination.Remark = source.Remark ?? string.Empty;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class FileUploadInfoToFileUploadInfoDtoMapper : MapperBase<FileUploadInfo, FileUploadInfoDto>
{
    public override partial FileUploadInfoDto Map(FileUploadInfo source);
    public override partial void Map(FileUploadInfo source, FileUploadInfoDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateFileUploadInfoDtoToFileUploadInfoMapper : MapperBase<CreateUpdateFileUploadInfoDto, FileUploadInfo>
{
    [MapperIgnoreTarget(nameof(FileUploadInfo.ConcurrencyStamp))]
    public override partial FileUploadInfo Map(CreateUpdateFileUploadInfoDto source);

    [MapperIgnoreTarget(nameof(FileUploadInfo.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateFileUploadInfoDto source, FileUploadInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MediaExternalLinkToExternalLinkDtoMapper : MapperBase<MediaExternalLink, ExternalLinkDto>
{
    public override partial ExternalLinkDto Map(MediaExternalLink source);
    public override partial void Map(MediaExternalLink source, ExternalLinkDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateExternalLinkDtoToMediaExternalLinkMapper : MapperBase<CreateUpdateExternalLinkDto, MediaExternalLink>
{
    [MapperIgnoreTarget(nameof(MediaExternalLink.ConcurrencyStamp))]
    public override partial MediaExternalLink Map(CreateUpdateExternalLinkDto source);

    [MapperIgnoreTarget(nameof(MediaExternalLink.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateExternalLinkDto source, MediaExternalLink destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class Aria2NotificationDtoToAria2NotificationMapper : MapperBase<Aria2NotificationDto, Aria2Notification>
{
    public override partial Aria2Notification Map(Aria2NotificationDto source);
    public override partial void Map(Aria2NotificationDto source, Aria2Notification destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ParamsItemDtoToParamsItemMapper : MapperBase<ParamsItemDto, ParamsItem>
{
    public override partial ParamsItem Map(ParamsItemDto source);
    public override partial void Map(ParamsItemDto source, ParamsItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class Aria2RequestToAria2RequestDtoMapper : MapperBase<Aria2Request, Aria2RequestDto>
{
    public override partial Aria2RequestDto Map(Aria2Request source);
    public override partial void Map(Aria2Request source, Aria2RequestDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ResponseBaseDtoToResponseBaseMapper : MapperBase<ResponseBaseDto, ResponseBase>
{
    public override partial ResponseBase Map(ResponseBaseDto source);
    public override partial void Map(ResponseBaseDto source, ResponseBase destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TellStatusResponseDtoToTellStatusResponseMapper : MapperBase<TellStatusResponseDto, TellStatusResponse>
{
    public override partial TellStatusResponse Map(TellStatusResponseDto source);
    public override partial void Map(TellStatusResponseDto source, TellStatusResponse destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TellStatusResultDtoToTellStatusResultMapper : MapperBase<TellStatusResultDto, TellStatusResult>
{
    public override partial TellStatusResult Map(TellStatusResultDto source);
    public override partial void Map(TellStatusResultDto source, TellStatusResult destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class FilesItemDtoToFilesItemMapper : MapperBase<FilesItemDto, FilesItem>
{
    public override partial FilesItem Map(FilesItemDto source);
    public override partial void Map(FilesItemDto source, FilesItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class UrisItemDtoToUrisItemMapper : MapperBase<UrisItemDto, UrisItem>
{
    public override partial UrisItem Map(UrisItemDto source);
    public override partial void Map(UrisItemDto source, UrisItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TellStatusResultToTellStatusResultDtoMapper : MapperBase<TellStatusResult, TellStatusResultDto>
{
    public override partial TellStatusResultDto Map(TellStatusResult source);
    public override partial void Map(TellStatusResult source, TellStatusResultDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class FilesItemToFilesItemDtoMapper : MapperBase<FilesItem, FilesItemDto>
{
    public override partial FilesItemDto Map(FilesItem source);
    public override partial void Map(FilesItem source, FilesItemDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class UrisItemToUrisItemDtoMapper : MapperBase<UrisItem, UrisItemDto>
{
    public override partial UrisItemDto Map(UrisItem source);
    public override partial void Map(UrisItem source, UrisItemDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ConfigurationInfoToCustomFileTypeDtoMapper : MapperBase<ConfigurationInfo, CustomFileTypeDto>
{
    public override partial CustomFileTypeDto Map(ConfigurationInfo source);
    public override partial void Map(ConfigurationInfo source, CustomFileTypeDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotterySimulationToSSQLotterySimulationDtoMapper : MapperBase<LotterySimulation, Lottery.Simulation.SSQ.LotterySimulationDto>
{
    public override partial Lottery.Simulation.SSQ.LotterySimulationDto Map(LotterySimulation source);
    public override partial void Map(LotterySimulation source, Lottery.Simulation.SSQ.LotterySimulationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateSSQLotterySimulationDtoToLotterySimulationMapper : MapperBase<Lottery.Simulation.SSQ.CreateUpdateLotterySimulationDto, LotterySimulation>
{
    [MapperIgnoreTarget(nameof(LotterySimulation.ConcurrencyStamp))]
    public override partial LotterySimulation Map(Lottery.Simulation.SSQ.CreateUpdateLotterySimulationDto source);

    [MapperIgnoreTarget(nameof(LotterySimulation.ConcurrencyStamp))]
    public override partial void Map(Lottery.Simulation.SSQ.CreateUpdateLotterySimulationDto source, LotterySimulation destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LotterySimulationToKL8LotterySimulationDtoMapper : MapperBase<LotterySimulation, Lottery.Simulation.KL8.LotterySimulationDto>
{
    public override partial Lottery.Simulation.KL8.LotterySimulationDto Map(LotterySimulation source);
    public override partial void Map(LotterySimulation source, Lottery.Simulation.KL8.LotterySimulationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateKL8LotterySimulationDtoToLotterySimulationMapper : MapperBase<Lottery.Simulation.KL8.CreateUpdateLotterySimulationDto, LotterySimulation>
{
    [MapperIgnoreTarget(nameof(LotterySimulation.ConcurrencyStamp))]
    public override partial LotterySimulation Map(Lottery.Simulation.KL8.CreateUpdateLotterySimulationDto source);

    [MapperIgnoreTarget(nameof(LotterySimulation.ConcurrencyStamp))]
    public override partial void Map(Lottery.Simulation.KL8.CreateUpdateLotterySimulationDto source, LotterySimulation destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class KeywordFilterRuleToKeywordFilterRuleDtoMapper : TwoWayMapperBase<KeywordFilterRule, KeywordFilterRuleDto>
{
    public override partial KeywordFilterRuleDto Map(KeywordFilterRule source);
    public override partial void Map(KeywordFilterRule source, KeywordFilterRuleDto destination);

    public override partial KeywordFilterRule ReverseMap(KeywordFilterRuleDto destination);
    public override partial void ReverseMap(KeywordFilterRuleDto destination, KeywordFilterRule source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateKeywordFilterRuleDtoToKeywordFilterRuleMapper : MapperBase<CreateUpdateKeywordFilterRuleDto, KeywordFilterRule>
{
    [MapperIgnoreTarget(nameof(KeywordFilterRule.ConcurrencyStamp))]
    public override partial KeywordFilterRule Map(CreateUpdateKeywordFilterRuleDto source);

    [MapperIgnoreTarget(nameof(KeywordFilterRule.ConcurrencyStamp))]
    public override partial void Map(CreateUpdateKeywordFilterRuleDto source, KeywordFilterRule destination);
}
