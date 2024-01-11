using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Bookkeeping.Category
{
    public class BookkeepingCategoryService : CrudAppService<
        BookkeepingCategory
        , BookkeepingCategoryDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateBookkeepingCategoryDto>, IBookkeepingCategoryService
    {
        private readonly IDataFilter _dataFilter;
        private readonly IBookkeepingExpenditureRepository _bookkeepingExpenditureRepository;

        public BookkeepingCategoryService(IDataFilter dataFilter
            , IBookkeepingExpenditureRepository bookkeepingExpenditureRepository
            , IRepository<BookkeepingCategory, long> repository) : base(repository)
        {
            _dataFilter = dataFilter;
            _bookkeepingExpenditureRepository = bookkeepingExpenditureRepository;
        }



        public override async Task<BookkeepingCategoryDto> CreateAsync(CreateUpdateBookkeepingCategoryDto input)
        {
            using (_dataFilter.Disable<ISoftDelete>())
            {
                if (await ReadOnlyRepository.AnyAsync(x => x.Category == input.Category))
                {
                    var ex = await Repository.FirstAsync(x => x.Category == input.Category);

                    if (!ex.IsDeleted)
                    {
                        throw new UserFriendlyException("类型已经存在无需添加");
                    }

                    ex.IsDeleted = false;
                    return MapToGetOutputDto(await Repository.UpdateAsync(ex));
                }
            }

            return await base.CreateAsync(input);
        }

        public override async Task DeleteAsync(long id)
        {

            if (await _bookkeepingExpenditureRepository.AnyAsync(x => x.CategoryId == id))
            {
                throw new UserFriendlyException("不能删除此类型，因为此类型有开支记录");
            }

            await base.DeleteAsync(id);
        }

    }
}
