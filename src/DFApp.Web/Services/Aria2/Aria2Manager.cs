using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DFApp.Aria2.Notifications;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DFApp.Aria2;

/// <summary>
/// Aria2 管理器，处理 WebSocket 通知和下载完成记录
/// </summary>
public class Aria2Manager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<Aria2Request> _requestsHistory;
    private readonly ILogger<Aria2Manager> _logger;

    public Aria2Manager(
        IServiceScopeFactory scopeFactory,
        ILogger<Aria2Manager> logger)
    {
        _requestsHistory = new List<Aria2Request>();
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 处理 RPC 响应，区分通知和请求响应
    /// </summary>
    /// <param name="response">RPC 响应</param>
    /// <returns>需要发送的请求列表</returns>
    public async Task<List<Aria2Request>> ProcessResponseAsync(ResponseBase? response)
    {
        if (response == null)
        {
            return new List<Aria2Request>();
        }

        if (string.IsNullOrEmpty(response.Id))
        {
            // 通知事件（没有 id 字段）
            return await ProcessNotificationAsync(response as Aria2Notification);
        }
        else
        {
            // 请求响应（有 id 字段）
            var res = _requestsHistory.FirstOrDefault(x => x.Id == response.Id);
            if (res != null)
            {
                switch (res.Method)
                {
                    case Aria2Consts.TellStatus:
                        await SaveTellStatusResultAsync(response);
                        _requestsHistory.Remove(res);
                        break;
                    default:
                        break;
                }
            }
        }
        return new List<Aria2Request>();
    }

    /// <summary>
    /// 处理 WebSocket 通知事件
    /// </summary>
    /// <param name="notification">通知对象</param>
    /// <returns>需要发送的请求列表</returns>
    public async Task<List<Aria2Request>> ProcessNotificationAsync(Aria2Notification? notification)
    {
        if (notification == null)
        {
            return new List<Aria2Request>();
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
                return await DownloadCompleteHandlerAsync(notification.Params);
            default:
                _logger.LogInformation("Aria2 通知: 未知事件 {Method}", notification.Method);
                break;
        }
        return new List<Aria2Request>();
    }

    /// <summary>
    /// 下载完成处理：构建 tellStatus 请求以获取完整状态存入数据库
    /// </summary>
    /// <param name="paramsItems">通知参数列表</param>
    /// <returns>需要发送的 tellStatus 请求列表</returns>
    public async Task<List<Aria2Request>> DownloadCompleteHandlerAsync(List<ParamsItem> paramsItems)
    {
        List<Aria2Request> requests = new List<Aria2Request>();
        string aria2secret;
        using (var scope = _scopeFactory.CreateScope())
        {
            var configRepo = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
            aria2secret = await configRepo.GetConfigurationInfoValue("aria2secret", "DFApp.Aria2.Aria2Service");
        }
        foreach (var item in paramsItems)
        {
            var request = new Aria2Request(Guid.NewGuid().ToString(), aria2secret);
            request.Method = Aria2Consts.TellStatus;
            if (!string.IsNullOrWhiteSpace(aria2secret))
            {
                request.Params.Add($"token:{aria2secret}");
            }
            request.Params.Add(item.GID);
            _requestsHistory.Add(request);
            requests.Add(request);
        }
        return requests;
    }

    /// <summary>
    /// 从 RPC 响应中解析并保存 TellStatus 结果到数据库
    /// </summary>
    /// <param name="response">RPC 响应</param>
    private async Task SaveTellStatusResultAsync(ResponseBase response)
    {
        try
        {
            // 从响应 JSON 中提取 result 数据
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(response));
            if (!jsonElement.TryGetProperty("result", out var resultElement))
            {
                _logger.LogWarning("TellStatus 响应中未找到 result 字段");
                return;
            }

            // 解析 TellStatusResult
            var tellStatusResult = new TellStatusResult
            {
                GID = resultElement.TryGetProperty("gid", out var gid) ? gid.GetString() : null,
                Status = resultElement.TryGetProperty("status", out var status) ? status.GetString() : null,
                TotalLength = resultElement.TryGetProperty("totalLength", out var totalLength) ? GetLongValue(totalLength) : null,
                CompletedLength = resultElement.TryGetProperty("completedLength", out var completedLength) ? GetLongValue(completedLength) : null,
                UploadLength = resultElement.TryGetProperty("uploadLength", out var uploadLength) ? GetLongValue(uploadLength) : null,
                DownloadSpeed = resultElement.TryGetProperty("downloadSpeed", out var downloadSpeed) ? GetLongValue(downloadSpeed) : null,
                UploadSpeed = resultElement.TryGetProperty("uploadSpeed", out var uploadSpeed) ? GetLongValue(uploadSpeed) : null,
                Connections = resultElement.TryGetProperty("connections", out var connections) ? GetLongValue(connections) : null,
                NumPieces = resultElement.TryGetProperty("numPieces", out var numPieces) ? GetLongValue(numPieces) : null,
                PieceLength = resultElement.TryGetProperty("pieceLength", out var pieceLength) ? GetLongValue(pieceLength) : null,
                Bitfield = resultElement.TryGetProperty("bitfield", out var bitfield) ? bitfield.GetString() : null,
                Dir = resultElement.TryGetProperty("dir", out var dir) ? dir.GetString() : null,
                ErrorCode = resultElement.TryGetProperty("errorCode", out var errorCode) ? errorCode.GetString() : null,
                ErrorMessage = resultElement.TryGetProperty("errorMessage", out var errorMessage) ? errorMessage.GetString() : null
            };

            _logger.LogInformation("=== 保存 TellStatus 结果到数据库 ===");
            _logger.LogInformation("GID: {Gid}", tellStatusResult.GID);
            _logger.LogInformation("Dir: {Dir}", tellStatusResult.Dir);
            _logger.LogInformation("Status: {Status}", tellStatusResult.Status);
            _logger.LogInformation("TotalLength: {TotalLength}", tellStatusResult.TotalLength);
            _logger.LogInformation("CompletedLength: {CompletedLength}", tellStatusResult.CompletedLength);

            // 在 scope 中执行数据库操作
            using (var scope = _scopeFactory.CreateScope())
            {
                var resultRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<TellStatusResult, long>>();
                var filesItemRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<FilesItem, int>>();
                var urisItemRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<UrisItem, short>>();

                // 保存主记录，使用 InsertReturnIdAsync 获取自增 Id
                tellStatusResult.Id = await resultRepository.InsertReturnIdAsync(tellStatusResult);

                // 解析并保存文件列表
                if (resultElement.TryGetProperty("files", out var filesElement) && filesElement.ValueKind == JsonValueKind.Array)
                {
                    int fileIndex = 0;
                    foreach (var fileElement in filesElement.EnumerateArray())
                    {
                        var filesItem = new FilesItem
                        {
                            ResultId = tellStatusResult.Id,
                            Index = fileElement.TryGetProperty("index", out var index) ? GetLongValue(index) : fileIndex,
                            Path = fileElement.TryGetProperty("path", out var path) ? path.GetString() : null,
                            Length = fileElement.TryGetProperty("length", out var length) ? GetLongValue(length) : null,
                            CompletedLength = fileElement.TryGetProperty("completedLength", out var fileCompletedLength) ? GetLongValue(fileCompletedLength) : null,
                            Selected = fileElement.TryGetProperty("selected", out var selected) ? GetBoolValue(selected) : null
                        };

                        // 使用 InsertReturnIdAsync 获取自增 Id，用于子表外键关联
                        filesItem.Id = (int)await filesItemRepository.InsertReturnIdAsync(filesItem);

                        _logger.LogInformation("  文件[{Index}]: {Path}, 长度: {Length}, 已完成: {CompletedLength}",
                            filesItem.Index, filesItem.Path, filesItem.Length, filesItem.CompletedLength);

                        // 解析并保存 URI 列表
                        if (fileElement.TryGetProperty("uris", out var urisElement) && urisElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var uriElement in urisElement.EnumerateArray())
                            {
                                var urisItem = new UrisItem
                                {
                                    FilesItemId = filesItem.Id,
                                    Uri = uriElement.TryGetProperty("uri", out var uri) ? uri.GetString() : null,
                                    Status = uriElement.TryGetProperty("status", out var uriStatus) ? uriStatus.GetString() : null
                                };

                                await urisItemRepository.InsertAsync(urisItem);
                            }
                        }

                        fileIndex++;
                    }
                }
            }

            _logger.LogInformation("=== TellStatus 结果保存完成 ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存 TellStatus 结果到数据库失败");
        }
    }

    /// <summary>
    /// 从 JsonElement 安全获取 long 值（支持字符串和数字类型）
    /// </summary>
    private long? GetLongValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var str = element.GetString();
            return long.TryParse(str, out var value) ? value : null;
        }
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.GetInt64();
        }
        return null;
    }

    /// <summary>
    /// 从 JsonElement 安全获取 bool? 值
    /// </summary>
    private bool? GetBoolValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var str = element.GetString();
            return bool.TryParse(str, out var value) ? value : null;
        }
        if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
        {
            return element.GetBoolean();
        }
        return null;
    }

    /// <summary>
    /// 从 JsonElement 安全获取 string 值
    /// </summary>
    private string? GetStringValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }
        return element.ToString();
    }
}
