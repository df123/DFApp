using AutoMapper;
using DFApp.Bookkeeping.Category;
using DFApp.Bookkeeping.Expenditure;
using DFApp.Media;

namespace DFApp.Web;

public class DFAppWebAutoMapperProfile : Profile
{
    public DFAppWebAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Web project.
        CreateMap<MediaInfoDto, CreateUpdateMediaInfoDto>();

        CreateMap<Pages.Bookkeeping.Category.CreateModalModel.CreateCategoryViewModel, CreateUpdateBookkeepingCategoryDto>();
        CreateMap<BookkeepingCategoryDto, CreateUpdateBookkeepingCategoryDto>();

        CreateMap<Pages.Bookkeeping.Expenditure.CreateModalModel.CreateExpenditureViewModel, CreateUpdateBookkeepingExpenditureDto>();
        CreateMap<BookkeepingExpenditureDto, CreateUpdateBookkeepingExpenditureDto>();

    }
}
