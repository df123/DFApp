using AutoMapper;
using DFApp.Media;

namespace DFApp.Web;

public class DFAppWebAutoMapperProfile : Profile
{
    public DFAppWebAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Web project.
        CreateMap<MediaInfoDto, CreateUpdateMediaInfoDto>();
    }
}
