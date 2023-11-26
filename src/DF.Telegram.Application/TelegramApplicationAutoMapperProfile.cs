using AutoMapper;
using DF.Telegram.IP;
using DF.Telegram.Lottery;
using DF.Telegram.Media;
using DF.Telegram.Queue;

namespace DF.Telegram;

public class TelegramApplicationAutoMapperProfile : Profile
{
    public TelegramApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<MediaInfo, MediaInfoDto>();
        CreateMap<CreateUpdateMediaInfoDto, MediaInfo>();
        CreateMap<MediaInfoDto, MediaInfo>();
        CreateMap<MediaInfoDto, CreateUpdateMediaInfoDto>();


        CreateMap<DynamicIP, DynamicIPDto>();
        CreateMap<CreateUpdateDynamicIPDto, DynamicIP>();

        CreateMap<LotteryInfo, LotteryDto>();
        CreateMap<CreateUpdateLotteryDto, LotteryInfo>();
        CreateMap<LotteryDto, CreateUpdateLotteryDto > ();

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
