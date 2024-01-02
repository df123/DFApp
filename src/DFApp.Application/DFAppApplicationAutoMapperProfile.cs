using AutoMapper;
using DFApp.IP;
using DFApp.Lottery;
using DFApp.Media;

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
    }
}
