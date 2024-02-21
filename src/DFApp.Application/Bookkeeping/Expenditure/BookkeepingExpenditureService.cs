using DFApp.Bookkeeping.Expenditure.Analysis;
using DFApp.Bookkeeping.Expenditure.Lookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

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

        public override async Task<PagedResultDto<BookkeepingExpenditureDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            if (input.SkipCount == 0 && input.Sorting == null)
            {
                input.MaxResultCount = await ReadOnlyRepository.CountAsync();
            }
            var categorys = await _categoryRepository.GetListAsync();
            return await base.GetListAsync(input);
        }

        public async Task<ChartJSDto> GetChartJSDto(DateTime start, DateTime end, bool compare, CompareType compareType, NumberType numberType)
        {
            var categorys = await _categoryRepository.GetListAsync(true);
            var expenditures = await ReadOnlyRepository.GetListAsync(x => x.ExpenditureDate >= start && x.ExpenditureDate <= end, true);

            ChartJSDto chartJSDto = new ChartJSDto();
            chartJSDto.Total = expenditures.Sum(x => x.Expenditure);
            ChartJSDatasetsItemDto chartJSDatasetsItemDto = new ChartJSDatasetsItemDto();
            chartJSDto.datasets.Add(chartJSDatasetsItemDto);
            var expenditureGroups = expenditures.GroupBy(x => x.CategoryId);
            GetChartJSDatasetsItemDto(expenditureGroups
                , chartJSDto
                , chartJSDatasetsItemDto
                , start
                , end
                , numberType
                , false);

            if (compare)
            {
                DateTime startCompare = ManipulateDate(start, compareType);
                DateTime endCompare = ManipulateDate(end, compareType);
                var expendituresCompare = await ReadOnlyRepository.GetListAsync(x => x.ExpenditureDate >= startCompare && x.ExpenditureDate <= endCompare);
                chartJSDto.CompareTotal = expendituresCompare.Sum(x => x.Expenditure);
                ChartJSDatasetsItemDto chartJSDatasetsItemCompareDto = new ChartJSDatasetsItemDto();
                chartJSDto.datasets.Add(chartJSDatasetsItemCompareDto);
                var expenditureCompareGroups = expendituresCompare.GroupBy(x => x.CategoryId);
                GetChartJSDatasetsItemDto(expenditureCompareGroups
                    , chartJSDto
                    , chartJSDatasetsItemCompareDto
                    , startCompare
                    , endCompare
                    , numberType
                    , true);
                chartJSDto.DifferenceTotal = chartJSDto.Total - chartJSDto.CompareTotal;
            }

            return chartJSDto;
        }

        private void GetChartJSDatasetsItemDto(IEnumerable<IGrouping<long, BookkeepingExpenditure>>? expenditureGroups
            , ChartJSDto chartJSDto
            , ChartJSDatasetsItemDto chartJSDatasetsItemDto
            , DateTime start
            , DateTime end
            , NumberType numberType
            , bool isCompare)
        {
            foreach (var item in expenditureGroups)
            {
                var temp = item.FirstOrDefault();
                if (temp != null)
                {
                    if (!chartJSDto.labels.Contains(temp.Category.Category))
                    {
                        chartJSDto.labels.Add(temp.Category.Category);
                    }
                }

                decimal tempSum = item.Sum(x => x.Expenditure);

                if (numberType == NumberType.PERCENTAGE && isCompare)
                {
                    tempSum = Math.Round((tempSum / chartJSDto.CompareTotal),2) * 100;
                }
                else if(numberType == NumberType.PERCENTAGE)
                {
                    tempSum = Math.Round((tempSum / chartJSDto.Total), 2) * 100;
                }

                chartJSDatasetsItemDto.data.Add(tempSum);

                if (start == end)
                {
                    chartJSDatasetsItemDto.label = start.ToString("yy/MM/dd");
                }
                else
                {
                    chartJSDatasetsItemDto.label = $"{start.ToString("yy/MM/dd")}-{end.ToString("yy/MM/dd")}";
                }
            }
        }

        private DateTime ManipulateDate(DateTime dateTime, CompareType compareType)
        {
            DateTime date = dateTime;

            switch (compareType)
            {
                case CompareType.DAY:
                    date = dateTime.AddDays(-1);
                    break;
                case CompareType.MONTH:
                    date = dateTime.AddMonths(-1);
                    break;
                case CompareType.YEAR:
                    date = dateTime.AddYears(-1);
                    break;
                default:
                    date = dateTime.AddMonths(-1);
                    break;
            }

            return date;
        }

        private DateTime SetTimeZero(DateTime dateTime)
        {
            dateTime = dateTime.Date;
            dateTime.AddHours(-12);
            return dateTime;
        }

    }
}
