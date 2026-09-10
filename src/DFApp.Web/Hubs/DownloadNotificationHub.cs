using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using DFApp.Web.Permissions;

namespace DFApp.Web.Hubs;

/// <summary>
/// 下载通知 SignalR Hub，用于向 Downloader 子程序推送媒体下载完成事件
/// （加入通知组需持有媒体权限）
/// </summary>
[Authorize]
public class DownloadNotificationHub : Hub
{
    public const string HubUrl = "/hubs/download-notification";

    /// <summary>
    /// 判断连接用户是否持有指定权限（直接读连接建立时的 JWT 权限声明）
    /// </summary>
    private static bool HasPermission(HubCallerContext context, string permissionName)
    {
        return context.User?.FindAll(DFAppClaimTypes.Permission)
            .Any(c => c.Value == permissionName) == true;
    }

    /// <summary>
    /// 加入下载通知组
    /// </summary>
    public async Task JoinDownloadGroup()
    {
        if (!HasPermission(Context, DFAppPermissions.Medias.Default))
        {
            throw new HubException("没有加入下载通知组的权限");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, "DownloadNotify");
    }

    /// <summary>
    /// 离开下载通知组
    /// </summary>
    public async Task LeaveDownloadGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "DownloadNotify");
    }
}
