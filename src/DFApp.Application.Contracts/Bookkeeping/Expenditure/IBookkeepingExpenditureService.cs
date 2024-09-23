﻿using DFApp.Bookkeeping.Expenditure.Analysis;
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
        , FilterAndPagedAndSortedResultRequestDto
        , CreateUpdateBookkeepingExpenditureDto>
    {
        Task<List<BookkeepingCategoryLookupDto>> GetCategoryLookupDto();

        Task<ChartJSDto> GetChartJSDto(DateTime start
            , DateTime end
            , CompareType compareType
            , NumberType numberType
            , bool? isBelongToSelf);

    }
}
