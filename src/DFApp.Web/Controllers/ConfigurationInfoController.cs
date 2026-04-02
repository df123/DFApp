using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Configuration;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 配置信息控制器，提供配置信息的增删改查及自定义查询功能
/// </summary>
[ApiController]
[Route("api/app/configuration-info")]
[Authorize]
public class ConfigurationInfoController : DFAppControllerBase
{
    private readonly Services.Configuration.ConfigurationInfoService _configurationInfoService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="configurationInfoService">配置信息服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public ConfigurationInfoController(
        Services.Configuration.ConfigurationInfoService configurationInfoService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _configurationInfoService = configurationInfoService;
    }

    /// <summary>
    /// 获取配置信息列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.ConfigurationInfo.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _configurationInfoService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取配置信息详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.ConfigurationInfo.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _configurationInfoService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取配置信息列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.ConfigurationInfo.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _configurationInfoService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建配置信息
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.ConfigurationInfo.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateConfigurationInfoDto input)
    {
        var result = await _configurationInfoService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新配置信息
    /// </summary>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.ConfigurationInfo.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateConfigurationInfoDto input)
    {
        var result = await _configurationInfoService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除配置信息
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.ConfigurationInfo.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _configurationInfoService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 获取配置信息值
    /// </summary>
    /// <param name="configurationName">配置名称</param>
    /// <param name="moduleName">模块名称</param>
    [HttpGet("value")]
    [Permission(DFAppPermissions.ConfigurationInfo.Default)]
    public async Task<IActionResult> GetConfigurationInfoValueAsync(
        [FromQuery] string configurationName,
        [FromQuery] string moduleName)
    {
        var result = await _configurationInfoService.GetConfigurationInfoValue(configurationName, moduleName);
        return Success(result);
    }

    /// <summary>
    /// 获取指定模块的所有配置参数
    /// </summary>
    /// <param name="moduleName">模块名称</param>
    [HttpGet("module/{moduleName}")]
    [Permission(DFAppPermissions.ConfigurationInfo.Default)]
    public async Task<IActionResult> GetAllParametersInModuleAsync([FromRoute] string moduleName)
    {
        var result = await _configurationInfoService.GetAllParametersInModule(moduleName);
        return Success(result);
    }

    /// <summary>
    /// 获取剩余磁盘空间
    /// </summary>
    [HttpGet("remaining-disk-space")]
    [Permission(DFAppPermissions.ConfigurationInfo.Default)]
    public async Task<IActionResult> GetRemainingDiskSpaceAsync()
    {
        var result = await _configurationInfoService.GetRemainingDiskSpaceAsync();
        return Success(result);
    }
}
