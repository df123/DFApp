using DFApp.Bookkeeping.Expenditure.Analysis;
using DFApp.Bookkeeping.Expenditure.Lookup;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Bookkeeping.Expenditure
{
    public interface IBookkeepingExpenditureService : ICrudAppService<
        BookkeepingExpenditureDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateBookkeepingExpenditureDto>
    {
        Task<List<BookkeepingCategoryLookupDto>> GetCategoryLookupDto();

        Task<ChartJSDto> GetChartJSDto(DateTime start, DateTime end, bool compare, CompareType compareType, NumberType numberType);

    }
}
