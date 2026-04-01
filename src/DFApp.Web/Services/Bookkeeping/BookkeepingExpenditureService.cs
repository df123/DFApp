using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DFApp.Bookkeeping;
using DFApp.Bookkeeping.Expenditure;
using DFApp.Bookkeeping.Expenditure.Analysis;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using BookkeepingExpenditureDto = DFApp.Web.DTOs.Bookkeeping.BookkeepingExpenditureDto;
using CreateUpdateBookkeepingExpenditureDto = DFApp.Web.DTOs.Bookkeeping.CreateUpdateBookkeepingExpenditureDto;
using BookkeepingCategoryLookupDto = DFApp.Web.DTOs.Bookkeeping.BookkeepingCategoryLookupDto;
using BookkeepingCategoryDto = DFApp.Web.DTOs.Bookkeeping.BookkeepingCategoryDto;

namespace DFApp.Web.Services.Bookkeeping;

/// <summary>
/// 记账支出服务
/// </summary>
public class BookkeepingExpenditureService : CrudServiceBase<
    BookkeepingExpenditure,
    long,
    BookkeepingExpenditureDto,
    CreateUpdateBookkeepingExpenditureDto,
    CreateUpdateBookkeepingExpenditureDto>
{
    private readonly ISqlSugarRepository<BookkeepingCategory, long> _categoryRepository;
    private readonly BookkeepingMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">支出仓储接口</param>
    /// <param name="categoryRepository">分类仓储接口</param>
    public BookkeepingExpenditureService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<BookkeepingExpenditure, long> repository,
        ISqlSugarRepository<BookkeepingCategory, long> categoryRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// 根据过滤条件分页查询支出记录
    /// 原始代码使用 WithDetailsAsync 导航查询 Category，现改为外键查询
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="categoryId">分类 ID</param>
    /// <param name="isBelongToSelf">是否属于自己</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public async Task<(List<BookkeepingExpenditureDto> Items, int TotalCount)> GetFilteredListAsync(
        string? filter, long? categoryId, bool? isBelongToSelf, int pageIndex, int pageSize)
    {
        var query = Repository.GetQueryable();

        // 应用过滤条件
        if (!string.IsNullOrWhiteSpace(filter))
        {
            // 原始代码通过导航属性 x.Category.Category 过滤，现改为子查询
            var matchingCategoryIds = _categoryRepository.GetQueryable()
                .Where(c => c.Category.Contains(filter))
                .Select(c => c.Id)
                .ToList();

            query = query.Where(x =>
                matchingCategoryIds.Contains(x.CategoryId)
                || x.Remark!.Contains(filter)
                || x.Expenditure.ToString().Contains(filter));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (isBelongToSelf.HasValue)
        {
            query = query.Where(x => x.IsBelongToSelf == isBelongToSelf.Value);
        }

        // 获取总数
        var totalCount = query.Count();

        // 默认按支出日期降序排序
        var items = query
            .OrderByDescending(x => x.ExpenditureDate)
            .ToPageList(pageIndex, pageSize);

        // 获取关联的分类信息（替代导航查询）
        var categoryIds = items.Select(x => x.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetListAsync(x => categoryIds.Contains(x.Id));
        var categoryMap = categories.ToDictionary(x => x.Id);

        // 手动映射 DTO
        var dtos = new List<BookkeepingExpenditureDto>();
        foreach (var item in items)
        {
            var dto = MapToGetOutputDto(item);
            if (categoryMap.TryGetValue(item.CategoryId, out var category))
            {
                dto.Category = MapCategoryToDto(category);
            }
            dtos.Add(dto);
        }

        return (dtos, totalCount);
    }

    /// <summary>
    /// 获取分类查找列表
    /// </summary>
    /// <returns>分类查找 DTO 列表</returns>
    public async Task<List<BookkeepingCategoryLookupDto>> GetCategoryLookupDto()
    {
        var categories = await _categoryRepository.GetListAsync();
        return categories.Select(c => _mapper.MapToLookupDto(c)).ToList();
    }

    /// <summary>
    /// 获取支出总额
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="categoryId">分类 ID</param>
    /// <param name="isBelongToSelf">是否属于自己</param>
    /// <returns>支出总额</returns>
    public async Task<decimal> GetTotalExpenditureAsync(string? filter = null, long? categoryId = null, bool? isBelongToSelf = null)
    {
        var query = Repository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            // 原始代码通过导航属性 x.Category.Category 过滤，现改为子查询
            var matchingCategoryIds = _categoryRepository.GetQueryable()
                .Where(c => c.Category.Contains(filter))
                .Select(c => c.Id)
                .ToList();

            query = query.Where(x =>
                matchingCategoryIds.Contains(x.CategoryId)
                || x.Remark!.Contains(filter)
                || x.Expenditure.ToString().Contains(filter));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (isBelongToSelf.HasValue)
        {
            query = query.Where(x => x.IsBelongToSelf == isBelongToSelf.Value);
        }

        return query.Sum(x => x.Expenditure);
    }

    /// <summary>
    /// 获取图表数据（按分类分组统计）
    /// </summary>
    /// <param name="start">开始日期</param>
    /// <param name="end">结束日期</param>
    /// <param name="compareType">比较类型</param>
    /// <param name="numberType">数值类型</param>
    /// <param name="isBelongToSelf">是否属于自己</param>
    /// <returns>图表数据 DTO</returns>
    public async Task<ChartJSDto> GetChartJSDto(DateTime start, DateTime end,
        CompareType compareType, NumberType numberType, bool? isBelongToSelf)
    {
        var expression = BuildExpression(start, end, isBelongToSelf);
        var expenditures = await Repository.GetListAsync(expression);

        var chartJSDto = new ChartJSDto
        {
            Total = expenditures.Sum(x => x.Expenditure),
            datasets = new List<ChartJSDatasetsItemDto>()
        };

        var chartJSDatasetsItemDto = new ChartJSDatasetsItemDto();
        chartJSDto.datasets.Add(chartJSDatasetsItemDto);

        var expenditureGroups = expenditures.GroupBy(x => x.CategoryId);
        await PopulateChartJSDatasetsItemDto(expenditureGroups, chartJSDto, chartJSDatasetsItemDto, start, end, numberType, false);

        if (compareType != CompareType.None)
        {
            var (startCompare, endCompare) = ManipulateDateRange(start, end, compareType);
            var expression2 = BuildExpression(startCompare, endCompare, isBelongToSelf);
            var expendituresCompare = await Repository.GetListAsync(expression2);
            chartJSDto.CompareTotal = expendituresCompare.Sum(x => x.Expenditure);
            var chartJSDatasetsItemCompareDto = new ChartJSDatasetsItemDto();
            chartJSDto.datasets.Add(chartJSDatasetsItemCompareDto);
            var expenditureCompareGroups = expendituresCompare.GroupBy(x => x.CategoryId);
            await PopulateChartJSDatasetsItemDto(expenditureCompareGroups, chartJSDto, chartJSDatasetsItemCompareDto, startCompare, endCompare, numberType, true);
            chartJSDto.DifferenceTotal = chartJSDto.Total - chartJSDto.CompareTotal;
        }

        return chartJSDto;
    }

    /// <summary>
    /// 获取月度支出统计
    /// </summary>
    /// <param name="year">年份</param>
    /// <returns>月度支出 DTO</returns>
    public async Task<MonthlyExpenditureDto> GetMonthlyExpenditureAsync(int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);

        var expenditures = await Repository.GetListAsync(
            x => x.ExpenditureDate >= startDate && x.ExpenditureDate <= endDate
        );

        var result = new MonthlyExpenditureDto();

        for (int month = 1; month <= 12; month++)
        {
            result.Labels.Add($"{year}/{month}");

            var monthlyData = expenditures.Where(x => x.ExpenditureDate.Month == month);

            result.TotalData.Add(monthlyData.Sum(x => x.Expenditure));
            result.SelfData.Add(monthlyData.Where(x => x.IsBelongToSelf).Sum(x => x.Expenditure));
            result.NonSelfData.Add(monthlyData.Where(x => !x.IsBelongToSelf).Sum(x => x.Expenditure));
        }

        // 计算有记录的月份数
        var monthsWithRecords = result.TotalData.Count(x => x > 0);
        if (monthsWithRecords == 0) monthsWithRecords = 1; // 避免除以零

        // 计算月均
        result.TotalAverage = result.TotalData.Sum() / monthsWithRecords;
        result.SelfAverage = result.SelfData.Sum() / monthsWithRecords;
        result.NonSelfAverage = result.NonSelfData.Sum() / monthsWithRecords;

        return result;
    }

    /// <summary>
    /// 构建日期范围查询表达式
    /// </summary>
    /// <param name="start">开始日期</param>
    /// <param name="end">结束日期</param>
    /// <param name="isBelongToSelf">是否属于自己</param>
    /// <returns>查询表达式</returns>
    private Expression<Func<BookkeepingExpenditure, bool>> BuildExpression(DateTime start, DateTime end, bool? isBelongToSelf)
    {
        Expression<Func<BookkeepingExpenditure, bool>> expression = x => x.ExpenditureDate >= start && x.ExpenditureDate <= end;

        if (isBelongToSelf.HasValue)
        {
            var combined = expression.And(x => x.IsBelongToSelf == isBelongToSelf.Value);
            return combined;
        }

        return expression;
    }

    /// <summary>
    /// 填充图表数据集
    /// 原始代码通过导航属性 temp.Category.Category 获取分类名称，现改为外键查询
    /// </summary>
    private async Task PopulateChartJSDatasetsItemDto(
        IEnumerable<IGrouping<long, BookkeepingExpenditure>> expenditureGroups,
        ChartJSDto chartJSDto,
        ChartJSDatasetsItemDto chartJSDatasetsItemDto,
        DateTime start,
        DateTime end,
        NumberType numberType,
        bool isCompare)
    {
        if (expenditureGroups == null)
        {
            return;
        }

        // 获取所有涉及的分类 ID 并批量查询分类名称
        var categoryIds = expenditureGroups.Select(g => g.Key).Distinct().ToList();
        var categories = await _categoryRepository.GetListAsync(x => categoryIds.Contains(x.Id));
        var categoryNameMap = categories.ToDictionary(x => x.Id);

        // 初始化一个字典来存储每个类别的总和
        var categorySums = new Dictionary<string, decimal>();

        foreach (var item in expenditureGroups)
        {
            // 通过外键查询获取分类名称（替代导航属性）
            if (categoryNameMap.TryGetValue(item.Key, out var category))
            {
                var categoryName = category.Category;

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

    /// <summary>
    /// 计算百分比
    /// </summary>
    private decimal CalculatePercentage(decimal tempSum, ChartJSDto chartJSDto, NumberType numberType, bool isCompare)
    {
        if (numberType == NumberType.PERCENTAGE)
        {
            var total = isCompare ? chartJSDto.CompareTotal : chartJSDto.Total;
            tempSum = Math.Round((tempSum / total), 2) * 100;
        }
        return tempSum;
    }

    /// <summary>
    /// 格式化日期范围
    /// </summary>
    private string FormatDateRange(DateTime start, DateTime end)
    {
        return start == end ? start.ToString("yy/MM/dd") : $"{start:yy/MM/dd}-{end:yy/MM/dd}";
    }

    /// <summary>
    /// 计算比较日期范围
    /// </summary>
    private (DateTime start, DateTime end) ManipulateDateRange(DateTime start, DateTime end, CompareType compareType)
    {
        var startCompare = ManipulateDate(start, compareType);
        var endCompare = ManipulateDate(end, compareType);
        return (startCompare, endCompare);
    }

    /// <summary>
    /// 根据比较类型调整日期
    /// </summary>
    private DateTime ManipulateDate(DateTime dateTime, CompareType compareType)
    {
        return compareType switch
        {
            CompareType.DAY => dateTime.AddDays(-1),
            CompareType.MONTH => dateTime.AddMonths(-1),
            CompareType.YEAR => dateTime.AddYears(-1),
            _ => dateTime.AddMonths(-1)
        };
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">支出实体</param>
    /// <returns>支出 DTO</returns>
    protected override BookkeepingExpenditureDto MapToGetOutputDto(BookkeepingExpenditure entity)
    {
        return _mapper.MapToExpenditureDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>支出实体</returns>
    protected override BookkeepingExpenditure MapToEntity(CreateUpdateBookkeepingExpenditureDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">支出实体</param>
    protected override void MapToEntity(CreateUpdateBookkeepingExpenditureDto input, BookkeepingExpenditure entity)
    {
        var mapped = _mapper.MapToEntity(input);
        entity.ExpenditureDate = mapped.ExpenditureDate;
        entity.Expenditure = mapped.Expenditure;
        entity.Remark = mapped.Remark;
        entity.IsBelongToSelf = mapped.IsBelongToSelf;
        entity.CategoryId = mapped.CategoryId;
    }

    /// <summary>
    /// 将分类实体映射为 DTO
    /// </summary>
    /// <param name="category">分类实体</param>
    /// <returns>分类 DTO</returns>
    private BookkeepingCategoryDto MapCategoryToDto(BookkeepingCategory category)
    {
        return _mapper.MapToDto(category);
    }
}
