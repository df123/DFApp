using AutoMapper;
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

namespace DFApp;

public class DFAppApplicationAutoMapperProfile : Profile
{
    public DFAppApplicationAutoMapperProfile()
    {
        CreateMap<MediaInfo, MediaInfoDto>()
        .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.MediaId.ToString()));
        CreateMap<CreateUpdateMediaInfoDto, MediaInfo>();
        CreateMap<MediaInfoDto, MediaInfo>();
        CreateMap<MediaInfoDto, CreateUpdateMediaInfoDto>();


        CreateMap<DynamicIP, DynamicIPDto>();
        CreateMap<CreateUpdateDynamicIPDto, DynamicIP>();

        CreateMap<LotteryInfo, LotteryDto>();
        CreateMap<CreateUpdateLotteryDto, LotteryInfo>();
        CreateMap<LotteryDto, CreateUpdateLotteryDto>();

        CreateMap<LotteryResult, LotteryResultDto>();
        CreateMap<CreateUpdateLotteryResultDto, LotteryResult>();
        CreateMap<LotteryResultDto, CreateUpdateLotteryResultDto>();

        CreateMap<LotteryPrizegrades, LotteryPrizegradesDto>();
        CreateMap<CreateUpdateLotteryPrizegradesDto, LotteryPrizegrades>();
        CreateMap<LotteryPrizegradesDto, CreateUpdateLotteryPrizegradesDto>();

        CreateMap<ResultItemDto, CreateUpdateLotteryResultDto>();
        CreateMap<PrizegradesItemDto, CreateUpdateLotteryPrizegradesDto>();

        CreateMap<ResultItemDto, LotteryResult>();
        CreateMap<PrizegradesItemDto, LotteryPrizegrades>();

        CreateMap<BookkeepingCategory, BookkeepingCategoryDto>();
        CreateMap<BookkeepingCategoryDto, BookkeepingCategory>();
        CreateMap<CreateUpdateBookkeepingCategoryDto, BookkeepingCategory>();
        CreateMap<BookkeepingCategory, BookkeepingCategoryLookupDto>().ForMember(x => x.CategoryId, x => x.MapFrom(y => y.Id));

        CreateMap<BookkeepingExpenditure, BookkeepingExpenditureDto>();
        CreateMap<BookkeepingExpenditureDto, BookkeepingExpenditure>();
        CreateMap<CreateUpdateBookkeepingExpenditureDto, BookkeepingExpenditure>();

        CreateMap<ConfigurationInfo, ConfigurationInfoDto>();
        CreateMap<ConfigurationInfoDto, ConfigurationInfo>();
        CreateMap<CreateUpdateConfigurationInfoDto, ConfigurationInfo>()
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.ModuleName ?? string.Empty))
            .ForMember(dest => dest.Remark, opt => opt.MapFrom(src => src.Remark ?? string.Empty));

        CreateMap<FileUploadInfo, FileUploadInfoDto>();
        CreateMap<CreateUpdateFileUploadInfoDto, FileUploadInfo>();

        CreateMap<MediaExternalLink, ExternalLinkDto>();
        CreateMap<CreateUpdateExternalLinkDto, MediaExternalLink>();

        CreateMap<Aria2NotificationDto, Aria2Notification>();
        CreateMap<ParamsItemDto, ParamsItem>();
        CreateMap<Aria2Request, Aria2RequestDto>();
        CreateMap<ResponseBaseDto, ResponseBase>();
        CreateMap<TellStatusResponseDto, TellStatusResponse>();
        CreateMap<TellStatusResultDto, TellStatusResult>();
        CreateMap<FilesItemDto, FilesItem>();
        CreateMap<UrisItemDto, UrisItem>();

        CreateMap<TellStatusResult, TellStatusResultDto>();
        CreateMap<FilesItem, FilesItemDto>();
        CreateMap<UrisItem, UrisItemDto>();

        CreateMap<ConfigurationInfo, CustomFileTypeDto>();

    }
}
