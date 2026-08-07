using System.Threading.Tasks;
using DFApp.Web.DTOs.Media;
using DFApp.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DFApp.Web.Controllers;

/// <summary>
/// 临时调试控制器：模拟推送下载完成通知到 Downloader（本地模拟用，验证后删除）
/// </summary>
[ApiController]
[Route("api/debug")]
public class DebugSimulateController : ControllerBase
{
    private readonly IHubContext<DownloadNotificationHub> _hubContext;

    public DebugSimulateController(IHubContext<DownloadNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// 推送模拟下载完成通知
    /// </summary>
    [HttpPost("notify-download")]
    public async Task<IActionResult> NotifyDownload([FromBody] MediaDownloadNotificationDto notification)
    {
        notification.CompletedAt = System.DateTime.UtcNow;
        await _hubContext.Clients.Group("DownloadNotify")
            .SendAsync("DownloadCompleted", notification);
        return Ok(new { success = true, message = "已推送", fileName = notification.FileName });
    }
}
