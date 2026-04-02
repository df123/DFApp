using System.Threading.Tasks;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RssFetchService = DFApp.Web.Services.Rss.RssFetchService;

namespace DFApp.Web.Controllers;

/// <summary>
/// RSS Feed获取控制器，提供手动获取RSS Feed内容的功能
/// </summary>
[ApiController]
[Route("api/app/rss-fetch")]
[Authorize]
public class RssFetchController : DFAppControllerBase
{
    private readonly RssFetchService _rssFetchService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="rssFetchService">RSS Feed获取服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public RssFetchController(
        RssFetchService rssFetchService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _rssFetchService = rssFetchService;
    }

    /// <summary>
    /// 获取RSS Feed内容
    /// </summary>
    /// <param name="input">请求参数</param>
    [HttpPost]
    [Permission(DFAppPermissions.Rss.Download)]
    public async Task<IActionResult> FetchRssFeed([FromBody] RssFetchRequestDto input)
    {
        var result = await _rssFetchService.FetchRssFeed(input);
        return Success(result);
    }
}
