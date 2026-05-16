using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Bookkeeping;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Bookkeeping;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookkeepingExpenditureService = DFApp.Web.Services.Bookkeeping.BookkeepingExpenditureService;
using CompareType = DFApp.Bookkeeping.CompareType;
using NumberType = DFApp.Bookkeeping.NumberType;
using MonthlyExpenditureDto = DFApp.Web.DTOs.Bookkeeping.MonthlyExpenditureDto;

namespace DFApp.Web.Controllers;

/// <summary>
/// 记账支出控制器，提供支出的增删改查、筛选、统计图表等功能
/// </summary>
[ApiController]
[Route("api/app/bookkeeping-expenditure")]
[Authorize]
public class BookkeepingExpenditureController : DFAppControllerBase
{
    private readonly BookkeepingExpenditureService _bookkeepingExpenditureService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="bookkeepingExpenditureService">记账支出服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public BookkeepingExpenditureController(
        BookkeepingExpenditureService bookkeepingExpenditureService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _bookkeepingExpenditureService = bookkeepingExpenditureService;
    }

    /// <summary>
    /// 获取记账支出列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _bookkeepingExpenditureService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取记账支出详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _bookkeepingExpenditureService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取记账支出列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _bookkeepingExpenditureService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 根据过滤条件分页查询支出记录
    /// </summary>
    [HttpGet("filtered")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Default)]
    public async Task<IActionResult> GetFilteredListAsync(
        [FromQuery] string? filter,
        [FromQuery] long? categoryId,
        [FromQuery] bool? isBelongToSelf,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _bookkeepingExpenditureService.GetFilteredListAsync(
            filter, categoryId, isBelongToSelf, pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 获取分类查找列表（用于下拉选择）
    /// </summary>
    [HttpGet("category-lookup")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Default)]
    public async Task<IActionResult> GetCategoryLookupAsync()
    {
        List<BookkeepingCategoryLookupDto> result = await _bookkeepingExpenditureService.GetCategoryLookupDto();
        return Success(result);
    }

    /// <summary>
    /// 获取支出总额
    /// </summary>
    [HttpGet("total")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Default)]
    public async Task<IActionResult> GetTotalExpenditureAsync(
        [FromQuery] string? filter,
        [FromQuery] long? categoryId,
        [FromQuery] bool? isBelongToSelf)
    {
        var result = await _bookkeepingExpenditureService.GetTotalExpenditureAsync(filter, categoryId, isBelongToSelf);
        return Success(result);
    }

    /// <summary>
    /// 获取图表数据（按分类分组统计）
    /// </summary>
    [HttpGet("chart")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Analysis)]
    public async Task<IActionResult> GetChartJSDtoAsync(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] CompareType compareType,
        [FromQuery] NumberType numberType,
        [FromQuery] bool? isBelongToSelf)
    {
        var result = await _bookkeepingExpenditureService.GetChartJSDto(
            start, end, compareType, numberType, isBelongToSelf);
        return Success(result);
    }

    /// <summary>
    /// 获取月度支出统计
    /// </summary>
    [HttpGet("monthly/{year:int}")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Analysis)]
    public async Task<IActionResult> GetMonthlyExpenditureAsync([FromRoute] int year)
    {
        var result = await _bookkeepingExpenditureService.GetMonthlyExpenditureAsync(year);
        return Success(result);
    }

    /// <summary>
    /// 创建记账支出
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateBookkeepingExpenditureDto input)
    {
        var result = await _bookkeepingExpenditureService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新记账支出
    /// </summary>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateBookkeepingExpenditureDto input)
    {
        var result = await _bookkeepingExpenditureService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除记账支出
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.BookkeepingExpenditure.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _bookkeepingExpenditureService.DeleteAsync(id);
        return Success();
    }
}
