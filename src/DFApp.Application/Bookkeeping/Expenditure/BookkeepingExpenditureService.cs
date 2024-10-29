using DFApp.Bookkeeping.Expenditure.Analysis;
using DFApp.Bookkeeping.Expenditure.Lookup;
using DFApp.CommonDtos;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Bookkeeping.Expenditure
{
    [Authorize(DFAppPermissions.BookkeepingExpenditure.Default)]
    public class BookkeepingExpenditureService : CrudAppService<
        BookkeepingExpenditure
        , BookkeepingExpenditureDto
        , long
        , FilterAndPagedAndSortedResultRequestDto
        , CreateUpdateBookkeepingExpenditureDto>, IBookkeepingExpenditureService
    {
        private readonly IRepository<BookkeepingCategory, long> _categoryRepository;

        public BookkeepingExpenditureService(IRepository<BookkeepingCategory, long> categoryRepository
            , IRepository<BookkeepingExpenditure, long> repository) : base(repository)
        {
            _categoryRepository = categoryRepository;
            GetPolicyName = DFAppPermissions.BookkeepingExpenditure.Default;
            GetListPolicyName = DFAppPermissions.BookkeepingExpenditure.Default;
            CreatePolicyName = DFAppPermissions.BookkeepingExpenditure.Create;
            UpdatePolicyName = DFAppPermissions.BookkeepingExpenditure.Edit;
            DeletePolicyName = DFAppPermissions.BookkeepingExpenditure.Delete;
        }

        protected override async Task<IQueryable<BookkeepingExpenditure>> CreateFilteredQueryAsync(FilterAndPagedAndSortedResultRequestDto input)
        {
            if(!string.IsNullOrWhiteSpace(input.Filter)){
                var query = await Repository.WithDetailsAsync();
                return query.Where(x => x.Category!.Category.Contains(input.Filter) 
                || x.Remark!.Contains(input.Filter)
                || x.Expenditure.ToString().Contains(input.Filter));
            }
            else{
                return await Repository.WithDetailsAsync();
            }
            
        }

        public async Task<List<BookkeepingCategoryLookupDto>> GetCategoryLookupDto()
        {
            var categorys = await _categoryRepository.GetListAsync();

            var result = ObjectMapper.Map<List<BookkeepingCategory>, List<BookkeepingCategoryLookupDto>>(categorys);

            return result;
        }

        [Authorize(DFAppPermissions.BookkeepingExpenditure.Analysis)]
        public async Task<ChartJSDto> GetChartJSDto(DateTime start, DateTime end
            , CompareType compareType, NumberType numberType, bool? isBelongToSelf)
        {
            var expression = BuildExpression(start, end, isBelongToSelf);
            var expenditures = await ReadOnlyRepository.GetListAsync(expression, true);

            var chartJSDto = new ChartJSDto
            {
                Total = expenditures.Sum(x => x.Expenditure),
                datasets = new List<ChartJSDatasetsItemDto>()
            };

            var chartJSDatasetsItemDto = new ChartJSDatasetsItemDto();
            chartJSDto.datasets.Add(chartJSDatasetsItemDto);

            var expenditureGroups = expenditures.GroupBy(x => x.CategoryId);
            PopulateChartJSDatasetsItemDto(expenditureGroups, chartJSDto, chartJSDatasetsItemDto, start, end, numberType, false);

            if (compareType != CompareType.None)
            {
                var (startCompare, endCompare) = ManipulateDateRange(start, end, compareType);
                var expression2 = BuildExpression(startCompare, endCompare, isBelongToSelf);
                var expendituresCompare = await ReadOnlyRepository.GetListAsync(expression2, true);
                chartJSDto.CompareTotal = expendituresCompare.Sum(x => x.Expenditure);
                var chartJSDatasetsItemCompareDto = new ChartJSDatasetsItemDto();
                chartJSDto.datasets.Add(chartJSDatasetsItemCompareDto);
                var expenditureCompareGroups = expendituresCompare.GroupBy(x => x.CategoryId);
                PopulateChartJSDatasetsItemDto(expenditureCompareGroups, chartJSDto, chartJSDatasetsItemCompareDto, startCompare, endCompare, numberType, true);
                chartJSDto.DifferenceTotal = chartJSDto.Total - chartJSDto.CompareTotal;
            }

            return chartJSDto;
        }

        private Expression<Func<BookkeepingExpenditure, bool>> BuildExpression(DateTime start, DateTime end, bool? isBelongToSelf)
        {
            Expression<Func<BookkeepingExpenditure, bool>> expression = x => x.ExpenditureDate >= start && x.ExpenditureDate <= end;

            if (isBelongToSelf.HasValue)
            {
                expression = expression.And(x => x.IsBelongToSelf == isBelongToSelf.Value);
            }

            return expression;
        }

        private void PopulateChartJSDatasetsItemDto(IEnumerable<IGrouping<long, BookkeepingExpenditure>> expenditureGroups
            , ChartJSDto chartJSDto
            , ChartJSDatasetsItemDto chartJSDatasetsItemDto
            , DateTime start
            , DateTime end
            , NumberType numberType
            , bool isCompare)
        {
            if (expenditureGroups == null)
            {
                return;
            }

            // 初始化一个字典来存储每个类别的总和
            var categorySums = new Dictionary<string, decimal>();

            foreach (var item in expenditureGroups)
            {
                var temp = item.FirstOrDefault();
                if (temp != null && temp.Category != null)
                {
                    var categoryName = temp.Category.Category;

                    // 如果类别不在 chartJSDto.labels 中，则添加
                    if (!chartJSDto.labels.Contains(categoryName))
                    {
                        chartJSDto.labels.Add(categoryName);
                    }

                    // 计算该类别的总和
                    var tempSum = item.Sum(x => x.Expenditure);
                    tempSum = CalculatePercentage(tempSum, chartJSDto, numberType, isCompare);

                    // 将总和添加到字典中
                    if (categorySums.ContainsKey(categoryName))
                    {
                        categorySums[categoryName] += tempSum;
                    }
                    else
                    {
                        categorySums[categoryName] = tempSum;
                    }
                }
            }

            // 将字典中的值添加到 chartJSDatasetsItemDto.data 中
            foreach (var label in chartJSDto.labels)
            {
                if (categorySums.ContainsKey(label))
                {
                    chartJSDatasetsItemDto.data.Add(categorySums[label]);
                }
                else
                {
                    chartJSDatasetsItemDto.data.Add(0); // 如果某个类别没有数据，则添加 0
                }
            }

            chartJSDatasetsItemDto.label = FormatDateRange(start, end);
        }


        private decimal CalculatePercentage(decimal tempSum, ChartJSDto chartJSDto, NumberType numberType, bool isCompare)
        {
            if (numberType == NumberType.PERCENTAGE)
            {
                var total = isCompare ? chartJSDto.CompareTotal : chartJSDto.Total;
                tempSum = Math.Round((tempSum / total), 2) * 100;
            }
            return tempSum;
        }

        private string FormatDateRange(DateTime start, DateTime end)
        {
            return start == end ? start.ToString("yy/MM/dd") : $"{start.ToString("yy/MM/dd")}-{end.ToString("yy/MM/dd")}";
        }

        private (DateTime start, DateTime end) ManipulateDateRange(DateTime start, DateTime end, CompareType compareType)
        {
            var startCompare = ManipulateDate(start, compareType);
            var endCompare = ManipulateDate(end, compareType);
            return (startCompare, endCompare);
        }

        private DateTime ManipulateDate(DateTime dateTime, CompareType compareType)
        {
            switch (compareType)
            {
                case CompareType.DAY:
                    return dateTime.AddDays(-1);
                case CompareType.MONTH:
                    return dateTime.AddMonths(-1);
                case CompareType.YEAR:
                    return dateTime.AddYears(-1);
                default:
                    return dateTime.AddMonths(-1);
            }
        }
    }
}
