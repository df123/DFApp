using DFApp.Bookkeeping.Expenditure.Analysis;
using DFApp.Bookkeeping.Expenditure.Lookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        protected override async Task<IQueryable<BookkeepingExpenditure>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
        {
            return await Repository.WithDetailsAsync();
        }

        public async Task<List<BookkeepingCategoryLookupDto>> GetCategoryLookupDto()
        {
            var categorys = await _categoryRepository.GetListAsync();

            var result = ObjectMapper.Map<List<BookkeepingCategory>, List<BookkeepingCategoryLookupDto>>(categorys);

            return result;
        }

        public async Task<ChartJSDto> GetChartJSDto(DateTime start, DateTime end, bool compare
            , CompareType compareType, NumberType numberType,bool? isBelongToSelf)
        {

            Expression<Func<BookkeepingExpenditure,bool>> expression = x => x.ExpenditureDate >= start && x.ExpenditureDate <= end;

            if (isBelongToSelf.HasValue)
            {
                expression = expression.And(x => x.IsBelongToSelf == isBelongToSelf.Value);
            }

            var expenditures = await ReadOnlyRepository.GetListAsync(expression, true);

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

                Expression<Func<BookkeepingExpenditure, bool>> expression2 = x => x.ExpenditureDate >= startCompare && x.ExpenditureDate <= endCompare;

                if (isBelongToSelf.HasValue)
                {
                    expression2 = expression2.And(x => x.IsBelongToSelf == isBelongToSelf.Value);
                }

                var expendituresCompare = await ReadOnlyRepository.GetListAsync(expression2,true);
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

    }
}
