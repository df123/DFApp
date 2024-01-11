using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Bookkeeping.Category
{
    public interface IBookkeepingCategoryService : ICrudAppService<
        BookkeepingCategoryDto
        ,long
        ,PagedAndSortedResultRequestDto
        ,CreateUpdateBookkeepingCategoryDto>
    {



    }
}
