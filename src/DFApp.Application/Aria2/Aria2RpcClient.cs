using DFApp.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFApp.Aria2
{
    /// <summary>
    /// Aria2 RPC HTTP 客户端
    /// </summary>
    public class Aria2RpcClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly ILogger<Aria2RpcClient> _logger;
        private string? _rpcUrl;
        private string? _rpcToken;

        // JSON 序列化选项：不区分大小写，允许字段名尾随下划线
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Aria2RpcClient(
            HttpClient httpClient,
            IConfigurationInfoRepository configurationInfoRepository,
            ILogger<Aria2RpcClient> logger)
        {
            _httpClient = httpClient;
            _configurationInfoRepository = configurationInfoRepository;
            _logger = logger;
        }

        /// <summary>
        /// 初始化客户端配置
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_rpcUrl == null)
            {
                _rpcUrl = await _configurationInfoRepository.GetConfigurationInfoValue("aria2rpc", "DFApp.Aria2.Aria2RpcClient");
                _rpcToken = await _configurationInfoRepository.GetConfigurationInfoValue("aria2secret", "DFApp.Aria2.Aria2RpcClient");

                if (string.IsNullOrWhiteSpace(_rpcUrl))
                {
                    throw new Exception("Aria2 RPC URL 未配置");
                }
            }
        }

        /// <summary>
        /// 发送 RPC 请求
        /// </summary>
        private async Task<T> SendRequestAsync<T>(string method, List<object?> parameters) where T : class
        {
            try
            {
                await EnsureInitializedAsync();

                // 添加 token 到参数
                if (!string.IsNullOrWhiteSpace(_rpcToken))
                {
                    parameters.Insert(0, $"token:{_rpcToken}");
                }

                var requestBody = new
                {
                    jsonrpc = "2.0",
                    id = Guid.NewGuid().ToString(),
                    method = method,
                    @params = parameters
                };

                var json = JsonSerializer.Serialize(requestBody);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_rpcUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var rpcResponse = JsonSerializer.Deserialize<Aria2RpcResponse<T>>(responseContent, _jsonOptions);

                if (rpcResponse?.Error != null)
                {
                    _logger.LogError("Aria2 RPC 错误: {Message}", rpcResponse.Error.Message);
                    throw new Exception($"Aria2 RPC 错误: {rpcResponse.Error.Message}");
                }

                if (rpcResponse?.Result == null)
                {
                    _logger.LogWarning("Aria2 RPC 返回空结果: {Method}, 响应: {Response}", method, responseContent);
                    throw new Exception($"Aria2 RPC 返回空结果: {method}");
                }
                return rpcResponse.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用 Aria2 RPC 失败: {Method}, URL: {Url}", method, _rpcUrl);
                throw;
            }
        }

        /// <summary>
        /// 获取全局统计
        /// </summary>
        public async Task<Aria2GlobalStatDto> GetGlobalStatAsync()
        {
            return await SendRequestAsync<Aria2GlobalStatDto>(Aria2Consts.GetGlobalStat, new List<object?>());
        }

        /// <summary>
        /// 获取活跃任务
        /// </summary>
        public async Task<List<Aria2TaskDto>> TellActiveAsync()
        {
            var result = await SendRequestAsync<List<Dictionary<string, JsonElement>>>(Aria2Consts.TellActive, new List<object?>());
            return ConvertToTasks(result);
        }

        /// <summary>
        /// 获取等待任务
        /// </summary>
        public async Task<List<Aria2TaskDto>> TellWaitingAsync(int offset = 0, int num = 100)
        {
            var result = await SendRequestAsync<List<Dictionary<string, JsonElement>>>(Aria2Consts.TellWaiting, new List<object?> { offset, num });
            return ConvertToTasks(result);
        }

        /// <summary>
        /// 获取停止任务
        /// </summary>
        public async Task<List<Aria2TaskDto>> TellStoppedAsync(int offset = 0, int num = 100)
        {
            var result = await SendRequestAsync<List<Dictionary<string, JsonElement>>>(Aria2Consts.TellStopped, new List<object?> { offset, num });
            return ConvertToTasks(result);
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        public async Task<Aria2TaskDto> TellStatusAsync(string gid)
        {
            var result = await SendRequestAsync<Dictionary<string, JsonElement>>(Aria2Consts.TellStatus, new List<object?> { gid });
            if (result == null) throw new Exception("获取任务状态失败");
            return ConvertToTask(result);
        }

        /// <summary>
        /// 获取任务详情（包含peers信息）
        /// </summary>
        public async Task<Aria2TaskDetailDto> TellStatusWithDetailAsync(string gid)
        {
            // 不指定字段列表，让 Aria2 返回所有可用字段
            var parameters = new List<object?> { gid };

            var result = await SendRequestAsync<Dictionary<string, JsonElement>>(Aria2Consts.TellStatus, parameters);
            if (result == null) throw new Exception("获取任务详情失败");

            // 转换基本信息
            var taskDetail = ConvertToTaskDetail(result);

            // 尝试使用 getPeers 方法获取 peers（仅适用于 BitTorrent 下载）
            try
            {
                var peersParameters = new List<object?> { gid };
                var peersResult = await SendRequestAsync<List<JsonElement>>(Aria2Consts.GetPeers, peersParameters);

                if (peersResult != null && peersResult.Count > 0)
                {
                    taskDetail.Peers = ParsePeersFromGetPeers(peersResult);
                    _logger.LogDebug("成功获取 {Gid} 的 {Count} 个 peers", gid, peersResult.Count);
                }
                else
                {
                    _logger.LogDebug("任务 {Gid} 没有返回 peers 信息", gid);
                    taskDetail.Peers = new List<Aria2PeerDto>();
                }
            }
            catch (Exception ex)
            {
                // getPeers 可能失败（例如非 BT 下载），这不影响基本信息
                _logger.LogWarning(ex, "获取任务 {Gid} 的 peers 信息失败，这可能不是 BitTorrent 下载", gid);
                taskDetail.Peers = new List<Aria2PeerDto>();
            }

            return taskDetail;
        }

        /// <summary>
        /// 从 getPeers 响应解析 peers 列表
        /// </summary>
        private List<Aria2PeerDto> ParsePeersFromGetPeers(List<JsonElement> peersList)
        {
            var peers = new List<Aria2PeerDto>();

            foreach (var peerElement in peersList)
            {
                if (peerElement.ValueKind == JsonValueKind.Object)
                {
                    var peer = new Aria2PeerDto
                    {
                        PeerId = peerElement.TryGetProperty("peerId", out var peerId) ? peerId.GetString() ?? string.Empty : string.Empty,
                        Ip = peerElement.TryGetProperty("ip", out var ip) ? ip.GetString() ?? string.Empty : string.Empty,
                        Port = peerElement.TryGetProperty("port", out var port) ? GetInt32FromElement(port) ?? 0 : 0,
                        Client = peerElement.TryGetProperty("client", out var client) ? client.GetString() : null,
                        AmChoking = peerElement.TryGetProperty("amChoking", out var amChoking) && GetBooleanFromElement(amChoking),
                        PeerChoking = peerElement.TryGetProperty("peerChoking", out var peerChoking) && GetBooleanFromElement(peerChoking),
                        DownloadSpeed = peerElement.TryGetProperty("downloadSpeed", out var downloadSpeed) ? GetInt64FromElement(downloadSpeed) : 0,
                        UploadSpeed = peerElement.TryGetProperty("uploadSpeed", out var uploadSpeed) ? GetInt64FromElement(uploadSpeed) : 0,
                        Seeder = peerElement.TryGetProperty("seeder", out var seeder) ? GetBooleanFromElement(seeder) : false
                    };

                    // Aria2 返回的进度是字符串 "0.1234" 格式
                    if (peerElement.TryGetProperty("progress", out var progress))
                    {
                        if (progress.ValueKind == JsonValueKind.String)
                        {
                            var progressStr = progress.GetString();
                            peer.Progress = decimal.TryParse(progressStr, out var progressValue) ? progressValue : 0;
                        }
                        else
                        {
                            peer.Progress = progress.GetDecimal();
                        }
                    }

                    peers.Add(peer);
                }
            }

            return peers;
        }

        /// <summary>
        /// 添加 URI 下载
        /// </summary>
        public async Task<string> AddUriAsync(List<string> uris, Dictionary<string, object>? options = null)
        {
            var parameters = new List<object?> { uris };
            if (options != null && options.Count > 0)
            {
                parameters.Add(options);
            }

            return await SendRequestAsync<string>(Aria2Consts.AddUri, parameters);
        }

        /// <summary>
        /// 添加种子文件下载
        /// </summary>
        public async Task<string> AddTorrentAsync(string torrentData, Dictionary<string, object>? options = null)
        {
            var parameters = new List<object?> { torrentData };
            if (options != null && options.Count > 0)
            {
                parameters.Add(options);
            }

            return await SendRequestAsync<string>(Aria2Consts.AddTorrent, parameters);
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        public async Task<string> PauseAsync(string gid)
        {
            return await SendRequestAsync<string>(Aria2Consts.Pause, new List<object?> { gid });
        }

        /// <summary>
        /// 暂停所有任务
        /// </summary>
        public async Task<string> PauseAllAsync()
        {
            return await SendRequestAsync<string>(Aria2Consts.PauseAll, new List<object?>());
        }

        /// <summary>
        /// 恢复任务
        /// </summary>
        public async Task<string> UnpauseAsync(string gid)
        {
            return await SendRequestAsync<string>(Aria2Consts.Unpause, new List<object?> { gid });
        }

        /// <summary>
        /// 恢复所有任务
        /// </summary>
        public async Task<string> UnpauseAllAsync()
        {
            return await SendRequestAsync<string>(Aria2Consts.UnpauseAll, new List<object?>());
        }

        /// <summary>
        /// 停止任务（从等待/活跃队列移除）
        /// </summary>
        public async Task<string> RemoveAsync(string gid)
        {
            return await SendRequestAsync<string>(Aria2Consts.Remove, new List<object?> { gid });
        }

        /// <summary>
        /// 强制停止任务
        /// </summary>
        public async Task<string> ForceRemoveAsync(string gid)
        {
            return await SendRequestAsync<string>(Aria2Consts.ForceRemove, new List<object?> { gid });
        }

        /// <summary>
        /// 清空停止的任务记录
        /// </summary>
        public async Task<string> PurgeDownloadResultAsync()
        {
            return await SendRequestAsync<string>(Aria2Consts.PurgeDownloadResult, new List<object?>());
        }

        /// <summary>
        /// 获取版本信息
        /// </summary>
        public async Task<Aria2VersionDto> GetVersionAsync()
        {
            return await SendRequestAsync<Aria2VersionDto>(Aria2Consts.GetVersion, new List<object?>());
        }

        /// <summary>
        /// 获取会话信息
        /// </summary>
        public async Task<Aria2SessionDto> GetSessionInfoAsync()
        {
            return await SendRequestAsync<Aria2SessionDto>(Aria2Consts.GetSessionInfo, new List<object?>());
        }

        /// <summary>
        /// 转换为任务列表
        /// </summary>
        private List<Aria2TaskDto> ConvertToTasks(List<Dictionary<string, JsonElement>>? result)
        {
            if (result == null) return new List<Aria2TaskDto>();

            return result.Select(ConvertToTask).ToList();
        }

        /// <summary>
        /// 从 JsonElement 安全获取 Int64 值
        /// </summary>
        private long GetInt64FromElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                return long.TryParse(str, out var value) ? value : 0;
            }
            return element.GetInt64();
        }

        /// <summary>
        /// 从 JsonElement 安全获取 Int32 值
        /// </summary>
        private int? GetInt32FromElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                return int.TryParse(str, out var value) ? value : (int?)null;
            }
            return element.GetInt32();
        }

        /// <summary>
        /// 从 JsonElement 安全获取 Boolean 值
        /// </summary>
        private bool GetBooleanFromElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                return bool.TryParse(str, out var value) && value;
            }
            return element.GetBoolean();
        }

        /// <summary>
        /// 转换为任务对象
        /// </summary>
        private Aria2TaskDto ConvertToTask(Dictionary<string, JsonElement> dict)
        {
            var completedLength = dict.ContainsKey("completedLength") ? GetInt64FromElement(dict["completedLength"]) : 0;
            var uploadedLength = dict.ContainsKey("uploadLength") ? GetInt64FromElement(dict["uploadLength"]) : 0;

            // 计算分享率：上传量 / 下载量
            var shareRatio = completedLength > 0 ? (decimal)uploadedLength / completedLength : 0;

            return new Aria2TaskDto
            {
                Gid = dict.ContainsKey("gid") ? dict["gid"].GetString() ?? string.Empty : string.Empty,
                Status = dict.ContainsKey("status") ? dict["status"].GetString() ?? "unknown" : "unknown",
                TotalLength = dict.ContainsKey("totalLength") ? GetInt64FromElement(dict["totalLength"]) : 0,
                CompletedLength = completedLength,
                DownloadSpeed = dict.ContainsKey("downloadSpeed") ? GetInt64FromElement(dict["downloadSpeed"]) : 0,
                UploadSpeed = dict.ContainsKey("uploadSpeed") ? GetInt64FromElement(dict["uploadSpeed"]) : 0,
                ErrorCode = dict.ContainsKey("errorCode") ? dict["errorCode"].GetString() : null,
                ErrorMessage = dict.ContainsKey("errorMessage") ? dict["errorMessage"].GetString() : null,
                Dir = dict.ContainsKey("dir") ? dict["dir"].GetString() : null,
                Connections = dict.ContainsKey("connections") ? GetInt32FromElement(dict["connections"]) : (int?)null,
                Files = dict.ContainsKey("files") ? ParseFiles(dict["files"]) : new List<Aria2FileDto>(),
                UploadedLength = uploadedLength,
                ShareRatio = shareRatio,
                BtName = dict.ContainsKey("btName") ? dict["btName"].GetString() : null,
                Peers = dict.ContainsKey("peers") ? ParsePeers(dict["peers"]) : null
            };
        }

        /// <summary>
        /// 转换为任务详情对象
        /// </summary>
        private Aria2TaskDetailDto ConvertToTaskDetail(Dictionary<string, JsonElement> dict)
        {
            var completedLength = dict.ContainsKey("completedLength") ? GetInt64FromElement(dict["completedLength"]) : 0;
            var uploadedLength = dict.ContainsKey("uploadLength") ? GetInt64FromElement(dict["uploadLength"]) : 0;

            // 计算分享率：上传量 / 下载量
            var shareRatio = completedLength > 0 ? (decimal)uploadedLength / completedLength : 0;

            return new Aria2TaskDetailDto
            {
                Gid = dict.ContainsKey("gid") ? dict["gid"].GetString() ?? string.Empty : string.Empty,
                Status = dict.ContainsKey("status") ? dict["status"].GetString() ?? "unknown" : "unknown",
                BtName = dict.ContainsKey("btName") ? dict["btName"].GetString() : null,
                TotalLength = dict.ContainsKey("totalLength") ? GetInt64FromElement(dict["totalLength"]) : 0,
                CompletedLength = completedLength,
                UploadedLength = uploadedLength,
                ShareRatio = shareRatio,
                DownloadSpeed = dict.ContainsKey("downloadSpeed") ? GetInt64FromElement(dict["downloadSpeed"]) : 0,
                UploadSpeed = dict.ContainsKey("uploadSpeed") ? GetInt64FromElement(dict["uploadSpeed"]) : 0,
                Dir = dict.ContainsKey("dir") ? dict["dir"].GetString() : null,
                Connections = dict.ContainsKey("connections") ? GetInt32FromElement(dict["connections"]) : (int?)null,
                Files = dict.ContainsKey("files") ? ParseFiles(dict["files"]) : new List<Aria2FileDto>(),
                Peers = dict.ContainsKey("peers") ? ParsePeers(dict["peers"]) : new List<Aria2PeerDto>()
            };
        }

        /// <summary>
        /// 解析Peers列表
        /// </summary>
        private List<Aria2PeerDto> ParsePeers(JsonElement peersElement)
        {
            var peers = new List<Aria2PeerDto>();

            if (peersElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var peerElement in peersElement.EnumerateArray())
                {
                    var peer = new Aria2PeerDto
                    {
                        PeerId = peerElement.TryGetProperty("peerId", out var peerId) ? peerId.GetString() ?? string.Empty : string.Empty,
                        Ip = peerElement.TryGetProperty("ip", out var ip) ? ip.GetString() ?? string.Empty : string.Empty,
                        Port = peerElement.TryGetProperty("port", out var port) ? GetInt32FromElement(port) ?? 0 : 0,
                        Client = peerElement.TryGetProperty("client", out var client) ? client.GetString() : null,
                        AmChoking = peerElement.TryGetProperty("amChoking", out var amChoking) && GetBooleanFromElement(amChoking),
                        PeerChoking = peerElement.TryGetProperty("peerChoking", out var peerChoking) && GetBooleanFromElement(peerChoking),
                        DownloadSpeed = peerElement.TryGetProperty("downloadSpeed", out var downloadSpeed) ? GetInt64FromElement(downloadSpeed) : 0,
                        UploadSpeed = peerElement.TryGetProperty("uploadSpeed", out var uploadSpeed) ? GetInt64FromElement(uploadSpeed) : 0,
                        Seeder = peerElement.TryGetProperty("seeder", out var seeder) ? GetBooleanFromElement(seeder) : false
                    };

                    // Aria2 返回的进度是字符串 "0.1234" 格式
                    if (peerElement.TryGetProperty("progress", out var progress))
                    {
                        if (progress.ValueKind == JsonValueKind.String)
                        {
                            var progressStr = progress.GetString();
                            peer.Progress = decimal.TryParse(progressStr, out var progressValue) ? progressValue : 0;
                        }
                        else
                        {
                            peer.Progress = progress.GetDecimal();
                        }
                    }

                    peers.Add(peer);
                }
            }

            return peers;
        }

        /// <summary>
        /// 解析文件列表
        /// </summary>
        private List<Aria2FileDto> ParseFiles(JsonElement filesElement)
        {
            var files = new List<Aria2FileDto>();

            if (filesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var fileElement in filesElement.EnumerateArray())
                {
                    var file = new Aria2FileDto
                    {
                        Index = fileElement.TryGetProperty("index", out var index) ? index.GetString() ?? "0" : "0",
                        Path = fileElement.TryGetProperty("path", out var path) ? path.GetString() ?? string.Empty : string.Empty,
                        Length = fileElement.TryGetProperty("length", out var length) ? GetInt64FromElement(length) : 0,
                        CompletedLength = fileElement.TryGetProperty("completedLength", out var completedLength) ? GetInt64FromElement(completedLength) : 0,
                        Selected = fileElement.TryGetProperty("selected", out var selected) && GetBooleanFromElement(selected)
                    };

                    if (fileElement.TryGetProperty("uris", out var urisElement))
                    {
                        file.Uris = new List<Aria2UriDto>();
                        if (urisElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var uriElement in urisElement.EnumerateArray())
                            {
                                file.Uris.Add(new Aria2UriDto
                                {
                                    Uri = uriElement.TryGetProperty("uri", out var uri) ? uri.GetString() ?? string.Empty : string.Empty,
                                    Status = uriElement.TryGetProperty("status", out var status) ? status.GetString() ?? string.Empty : string.Empty
                                });
                            }
                        }
                    }

                    files.Add(file);
                }
            }

            return files;
        }

        /// <summary>
        /// Aria2 RPC 响应
        /// </summary>
        private class Aria2RpcResponse<T>
        {
            public string? JsonRPC { get; set; }
            public string? Id { get; set; }
            public T? Result { get; set; }
            public Aria2RpcError? Error { get; set; }
        }

        /// <summary>
        /// Aria2 RPC 错误
        /// </summary>
        private class Aria2RpcError
        {
            public int Code { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}
