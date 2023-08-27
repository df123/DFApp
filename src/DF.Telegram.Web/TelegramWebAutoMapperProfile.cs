using AutoMapper;
using DF.Telegram.Media;

namespace DF.Telegram.Web;

public class TelegramWebAutoMapperProfile : Profile
{
    public TelegramWebAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Web project.
        CreateMap<MediaInfoDto, CreateUpdateMediaInfoDto>();
    }
}
