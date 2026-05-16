using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DFApp.Aria2.Notifications;
using DFApp.Media;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.DTOs.Media;
using DFApp.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DFApp.Aria2;

/// <summary>
/// Aria2 管理器，处理 WebSocket 通知和下载完成记录
/// </summary>
public class Aria2Manager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Aria2Manager> _logger;

    public Aria2Manager(
        IServiceScopeFactory scopeFactory,
        ILogger<Aria2Manager> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 处理 RPC 响应，区分通知和请求响应
    /// </summary>
    public async Task ProcessResponseAsync(ResponseBase? response)
    {
        if (response == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(response.Id))
        {
            await ProcessNotificationAsync(response as Aria2Notification);
        }
    }

    /// <summary>
    /// 处理 WebSocket 通知事件
    /// </summary>
    public async Task ProcessNotificationAsync(Aria2Notification? notification)
    {
        if (notification == null)
        {
            return;
        }

        switch (notification.Method)
        {
            case Aria2Consts.OnDownloadStart:
                _logger.LogInformation("Aria2 通知: 下载开始");
                break;
            case Aria2Consts.OnDownloadPause:
                _logger.LogInformation("Aria2 通知: 下载暂停");
                break;
            case Aria2Consts.OnDownloadStop:
                _logger.LogInformation("Aria2 通知: 下载停止");
                break;
            case Aria2Consts.OnDownloadError:
                _logger.LogInformation("Aria2 通知: 下载错误");
                break;
            case Aria2Consts.OnDownloadComplete:
            case Aria2Consts.OnBtDownloadComplete:
                await DownloadCompleteHandlerAsync(notification.Params);
                break;
            default:
                _logger.LogInformation("Aria2 通知: 未知事件 {Method}", notification.Method);
                break;
        }
    }

    /// <summary>
    /// 下载完成处理：通过 HTTP RPC 调用 TellStatus 获取完整状态并存入数据库
    /// </summary>
    /// <param name="paramsItems">通知参数列表</param>
    private async Task DownloadCompleteHandlerAsync(List<ParamsItem> paramsItems)
    {
        foreach (var item in paramsItems)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var rpcClient = scope.ServiceProvider.GetRequiredService<Aria2RpcClient>();

                var taskDto = await rpcClient.TellStatusAsync(item.GID);

                _logger.LogInformation("=== 保存 TellStatus 结果到数据库 ===");
                _logger.LogInformation("GID: {Gid}", taskDto.Gid);
                _logger.LogInformation("Dir: {Dir}", taskDto.Dir);
                _logger.LogInformation("Status: {Status}", taskDto.Status);
                _logger.LogInformation("TotalLength: {TotalLength}", taskDto.TotalLength);
                _logger.LogInformation("CompletedLength: {CompletedLength}", taskDto.CompletedLength);

                await SaveTellStatusResultFromDtoAsync(scope, taskDto);

                _logger.LogInformation("=== TellStatus 结果保存完成 ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理下载完成通知失败，GID: {Gid}", item.GID);
            }
        }
    }

    /// <summary>
    /// 从 Aria2TaskDto 保存 TellStatus 结果到数据库
    /// </summary>
    /// <param name="scope">已创建的服务作用域</param>
    /// <param name="taskDto">任务状态 DTO</param>
    private async Task SaveTellStatusResultFromDtoAsync(IServiceScope scope, Aria2TaskDto taskDto)
    {
        var resultRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<Response.TellStatus.TellStatusResult, long>>();
        var filesItemRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<Response.TellStatus.FilesItem, int>>();
        var urisItemRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<Response.TellStatus.UrisItem, short>>();

        var tellStatusResult = new Response.TellStatus.TellStatusResult
        {
            GID = taskDto.Gid,
            Status = taskDto.Status,
            TotalLength = taskDto.TotalLength,
            CompletedLength = taskDto.CompletedLength,
            UploadLength = taskDto.UploadedLength,
            DownloadSpeed = taskDto.DownloadSpeed,
            UploadSpeed = taskDto.UploadSpeed,
            Connections = taskDto.Connections,
            Dir = taskDto.Dir,
            ErrorCode = taskDto.ErrorCode,
            ErrorMessage = taskDto.ErrorMessage
        };

        // 保存主记录
        tellStatusResult.Id = await resultRepository.InsertReturnIdAsync(tellStatusResult);

        // 解析并保存文件列表
        if (taskDto.Files != null)
        {
            int fileIndex = 0;
            foreach (var fileDto in taskDto.Files)
            {
                var filesItem = new Response.TellStatus.FilesItem
                {
                    ResultId = tellStatusResult.Id,
                    Index = long.TryParse(fileDto.Index, out var idx) ? idx : fileIndex,
                    Path = fileDto.Path,
                    Length = fileDto.Length,
                    CompletedLength = fileDto.CompletedLength,
                    Selected = fileDto.Selected
                };

                filesItem.Id = (int)await filesItemRepository.InsertReturnIdAsync(filesItem);

                _logger.LogInformation("  文件[{Index}]: {Path}, 长度: {Length}, 已完成: {CompletedLength}",
                    filesItem.Index, filesItem.Path, filesItem.Length, filesItem.CompletedLength);

                // 解析并保存 URI 列表
                if (fileDto.Uris != null)
                {
                    foreach (var uriDto in fileDto.Uris)
                    {
                        var urisItem = new Response.TellStatus.UrisItem
                        {
                            FilesItemId = filesItem.Id,
                            Uri = uriDto.Uri,
                            Status = uriDto.Status
                        };

                        await urisItemRepository.InsertAsync(urisItem);
                    }
                }

                fileIndex++;
            }
        }

        // 向 Downloader 子程序推送下载完成通知
        await NotifyDownloaderAsync(scope, taskDto, tellStatusResult.Id);
    }

    /// <summary>
    /// 向 Downloader 子程序推送 Aria2 下载完成通知
    /// </summary>
    private async Task NotifyDownloaderAsync(IServiceScope scope, Aria2TaskDto taskDto, long tellStatusResultId)
    {
        try
        {
            var configurationInfoRepository = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
            var downloaderEnabled = await configurationInfoRepository
                .GetConfigurationInfoValue("DownloaderEnabled", MediaBackgroudConst.ModuleName);
            if (!bool.TryParse(downloaderEnabled, out var enabled) || !enabled)
            {
                return;
            }

            var aria2ApachePrefix = await configurationInfoRepository
                .GetConfigurationInfoValue("Aria2ApachePathPrefix", MediaBackgroudConst.ModuleName);
            if (string.IsNullOrWhiteSpace(aria2ApachePrefix))
            {
                return;
            }

            var filePath = taskDto.Files?.Count > 0 ? taskDto.Files[0].Path : "";
            var fileName = Path.GetFileName(filePath);
            var downloadUrl = $"{aria2ApachePrefix.TrimEnd('/')}/{fileName}";

            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DownloadNotificationHub>>();

            var notification = new Aria2DownloadNotificationDto
            {
                FileName = fileName,
                FileSize = taskDto.TotalLength,
                MimeType = "application/octet-stream",
                DownloadUrl = downloadUrl,
                SourceType = "Aria2",
                SourceId = tellStatusResultId,
                Gid = taskDto.Gid,
                TorrentName = Path.GetFileName(taskDto.Dir),
                CompletedAt = DateTime.UtcNow
            };

            await hubContext.Clients.Group("DownloadNotify")
                .SendAsync("DownloadCompleted", notification);

            _logger.LogInformation("已推送 Aria2 下载完成通知到 Downloader: {FileName}", notification.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推送 Aria2 下载完成通知失败，不影响主流程");
        }
    }
}
