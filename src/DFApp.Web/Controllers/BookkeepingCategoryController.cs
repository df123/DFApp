using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Bookkeeping;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Bookkeeping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 记账分类控制器，提供分类的增删改查功能
/// </summary>
[ApiController]
[Route("api/app/bookkeeping-category")]
[Authorize]
public class BookkeepingCategoryController : DFAppControllerBase
{
    private readonly BookkeepingCategoryService _bookkeepingCategoryService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="bookkeepingCategoryService">记账分类服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public BookkeepingCategoryController(
        BookkeepingCategoryService bookkeepingCategoryService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _bookkeepingCategoryService = bookkeepingCategoryService;
    }

    /// <summary>
    /// 获取记账分类列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.BookkeepingCategory.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _bookkeepingCategoryService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取记账分类详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.BookkeepingCategory.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _bookkeepingCategoryService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取记账分类列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.BookkeepingCategory.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _bookkeepingCategoryService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建记账分类
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.BookkeepingCategory.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateBookkeepingCategoryDto input)
    {
        var result = await _bookkeepingCategoryService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新记账分类
    /// </summary>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.BookkeepingCategory.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateBookkeepingCategoryDto input)
    {
        var result = await _bookkeepingCategoryService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除记账分类
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.BookkeepingCategory.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _bookkeepingCategoryService.DeleteAsync(id);
        return Success();
    }
}
