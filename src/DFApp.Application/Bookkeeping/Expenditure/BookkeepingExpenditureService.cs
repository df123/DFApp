using DFApp.Bookkeeping.Expenditure.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Bookkeeping.Expenditure
{
    public class BookkeepingExpenditureService : CrudAppService<
        BookkeepingExpenditure
        , BookkeepingExpenditureDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateBookkeepingExpenditureDto>, IBookkeepingExpenditureService
    {
        private readonly IRepository<BookkeepingCategory, long> _categoryRepository;

        public BookkeepingExpenditureService(IRepository<BookkeepingCategory, long> categoryRepository
            , IRepository<BookkeepingExpenditure, long> repository) : base(repository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<BookkeepingCategoryLookupDto>> GetCategoryLookupDto()
        {
            var categorys = await _categoryRepository.GetListAsync();

            var result = ObjectMapper.Map<List<BookkeepingCategory>, List<BookkeepingCategoryLookupDto>>(categorys);

            return result;
        }
    }
}
