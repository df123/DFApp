using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.FileFilter;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 关键词过滤规则控制器，提供规则的增删改查及测试功能
/// </summary>
[ApiController]
[Route("api/app/keyword-filter-rule")]
[Authorize]
public class KeywordFilterRuleController : DFAppControllerBase
{
    private readonly Services.FileFilter.KeywordFilterRuleService _keywordFilterRuleService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="keywordFilterRuleService">关键词过滤规则服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public KeywordFilterRuleController(
        Services.FileFilter.KeywordFilterRuleService keywordFilterRuleService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _keywordFilterRuleService = keywordFilterRuleService;
    }

    /// <summary>
    /// 获取关键词过滤规则列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.FileFilter.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _keywordFilterRuleService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取关键词过滤规则详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.FileFilter.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _keywordFilterRuleService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取关键词过滤规则列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.FileFilter.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _keywordFilterRuleService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建关键词过滤规则
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.FileFilter.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateKeywordFilterRuleDto input)
    {
        var result = await _keywordFilterRuleService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新关键词过滤规则
    /// </summary>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.FileFilter.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateKeywordFilterRuleDto input)
    {
        var result = await _keywordFilterRuleService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除关键词过滤规则
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.FileFilter.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _keywordFilterRuleService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 测试文件名过滤（单个文件名）
    /// </summary>
    [HttpPost("test")]
    [Permission(DFAppPermissions.FileFilter.Default)]
    public async Task<IActionResult> TestFilterAsync([FromBody] TestFilterRequestDto input)
    {
        var result = await _keywordFilterRuleService.TestFilterAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 批量测试文件名过滤
    /// </summary>
    [HttpPost("test-batch")]
    [Permission(DFAppPermissions.FileFilter.Default)]
    public async Task<IActionResult> TestFilterBatchAsync([FromBody] List<string> fileNames)
    {
        var result = await _keywordFilterRuleService.TestFilterBatchAsync(fileNames);
        return Success(result);
    }

    /// <summary>
    /// 获取文件名匹配的规则列表
    /// </summary>
    /// <param name="fileName">文件名</param>
    [HttpGet("matching-rules")]
    [Permission(DFAppPermissions.FileFilter.Default)]
    public async Task<IActionResult> GetMatchingRulesAsync([FromQuery] string fileName)
    {
        var result = await _keywordFilterRuleService.GetMatchingRulesAsync(fileName);
        return Success(result);
    }

    /// <summary>
    /// 切换规则启用状态
    /// </summary>
    /// <param name="id">规则 ID</param>
    /// <param name="isEnabled">是否启用</param>
    [HttpPut("{id:long}/toggle")]
    [Permission(DFAppPermissions.FileFilter.Edit)]
    public async Task<IActionResult> ToggleRuleAsync([FromRoute] long id, [FromQuery] bool isEnabled)
    {
        await _keywordFilterRuleService.ToggleRuleAsync(id, isEnabled);
        return Success();
    }
}
