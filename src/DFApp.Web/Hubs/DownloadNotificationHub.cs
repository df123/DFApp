using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DFApp.Web.Hubs;

/// <summary>
/// 下载通知 SignalR Hub，用于向 Downloader 子程序推送媒体下载完成事件
/// </summary>
[Authorize]
public class DownloadNotificationHub : Hub
{
    public const string HubUrl = "/hubs/download-notification";

    /// <summary>
    /// 加入下载通知组
    /// </summary>
    public async Task JoinDownloadGroup()
    {
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
