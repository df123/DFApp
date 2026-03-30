using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DFApp.Web.Services.ElectricVehicle;

/// <summary>
/// 油价服务
/// </summary>
public class GasolinePriceService : AppServiceBase
{
    private readonly IGasolinePriceRepository _repository;
    private readonly ILogger<GasolinePriceService> _logger;
    private readonly GasolinePriceRefresher _gasolinePriceRefresher;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">油价仓储接口</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="gasolinePriceRefresher">油价刷新器</param>
    public GasolinePriceService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        IGasolinePriceRepository repository,
        ILogger<GasolinePriceService> logger,
        GasolinePriceRefresher gasolinePriceRefresher)
        : base(currentUser, permissionChecker)
    {
        _repository = repository;
        _logger = logger;
        _gasolinePriceRefresher = gasolinePriceRefresher;
    }

    /// <summary>
    /// 获取指定省份的最新汽油价格
    /// </summary>
    /// <param name="province">省份</param>
    /// <returns>汽油价格 DTO</returns>
    public async Task<GasolinePriceDto?> GetLatestPriceAsync(string province)
    {
        var price = await _repository.GetLatestPriceAsync(province);

        if (price == null)
        {
            return null;
        }

        // TODO: 使用 Mapperly 映射实体到 DTO
        return new GasolinePriceDto
        {
            Id = price.Id,
            Province = price.Province,
            Date = price.Date,
            Price0H = price.Price0H,
            Price89H = price.Price89H,
            Price90H = price.Price90H,
            Price92H = price.Price92H,
            Price93H = price.Price93H,
            Price95H = price.Price95H,
            Price97H = price.Price97H,
            Price98H = price.Price98H,
            CreationTime = price.CreationTime
        };
    }

    /// <summary>
    /// 获取指定省份和日期的汽油价格
    /// </summary>
    /// <param name="province">省份</param>
    /// <param name="date">日期</param>
    /// <returns>汽油价格 DTO</returns>
    public async Task<GasolinePriceDto?> GetPriceByDateAsync(string province, DateTime date)
    {
        var price = await _repository.GetPriceByDateAsync(province, date);

        if (price == null)
        {
            return null;
        }

        // TODO: 使用 Mapperly 映射实体到 DTO
        return new GasolinePriceDto
        {
            Id = price.Id,
            Province = price.Province,
            Date = price.Date,
            Price0H = price.Price0H,
            Price89H = price.Price89H,
            Price90H = price.Price90H,
            Price92H = price.Price92H,
            Price93H = price.Price93H,
            Price95H = price.Price95H,
            Price97H = price.Price97H,
            Price98H = price.Price98H,
            CreationTime = price.CreationTime
        };
    }

    /// <summary>
    /// 获取汽油价格列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<GasolinePriceDto>> GetListAsync(GetGasolinePricesDto input)
    {
        var queryable = _repository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(input.Province))
        {
            queryable = queryable.Where(x => x.Province == input.Province);
        }

        if (input.StartDate.HasValue)
        {
            queryable = queryable.Where(x => x.Date >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            queryable = queryable.Where(x => x.Date <= input.EndDate.Value);
        }

        queryable = queryable.OrderByDescending(x => x.Date);

        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        // TODO: 使用 Mapperly 映射实体列表到 DTO 列表
        var dtos = items.Select(item => new GasolinePriceDto
        {
            Id = item.Id,
            Province = item.Province,
            Date = item.Date,
            Price0H = item.Price0H,
            Price89H = item.Price89H,
            Price90H = item.Price90H,
            Price92H = item.Price92H,
            Price93H = item.Price93H,
            Price95H = item.Price95H,
            Price97H = item.Price97H,
            Price98H = item.Price98H,
            CreationTime = item.CreationTime
        }).ToList();

        return new PagedResultDto<GasolinePriceDto>(totalCount, dtos);
    }

    /// <summary>
    /// 刷新汽油价格
    /// </summary>
    public async Task RefreshGasolinePricesAsync()
    {
        await _gasolinePriceRefresher.RefreshGasolinePricesAsync();
    }
}

/// <summary>
/// 分页结果 DTO
/// </summary>
/// <typeparam name="TItem">项目类型</typeparam>
public class PagedResultDto<TItem>
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 项目列表
    /// </summary>
    public List<TItem> Items { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public PagedResultDto()
    {
        Items = new List<TItem>();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="totalCount">总记录数</param>
    /// <param name="items">项目列表</param>
    public PagedResultDto(int totalCount, List<TItem> items)
    {
        TotalCount = totalCount;
        Items = items;
    }
}
