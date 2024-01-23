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

namespace DFApp;

public class DFAppApplicationAutoMapperProfile : Profile
{
    public DFAppApplicationAutoMapperProfile()
    {
        CreateMap<MediaInfo, MediaInfoDto>();
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
        CreateMap<CreateUpdateConfigurationInfoDto, ConfigurationInfo>();

        CreateMap<FileUploadInfo, FileUploadInfoDto>();
        CreateMap<CreateUpdateFileUploadInfoDto, FileUploadInfo>();


    }
}
