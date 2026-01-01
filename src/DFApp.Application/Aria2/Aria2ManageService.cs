using DFApp.Configuration;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Aria2
{
    [Authorize(DFAppPermissions.Aria2.Default)]
    public class Aria2ManageService : ApplicationService, IAria2ManageService
    {
        private readonly Aria2RpcClient _aria2Client;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;

        public Aria2ManageService(
            Aria2RpcClient aria2Client,
            IConfigurationInfoRepository configurationInfoRepository)
        {
            _aria2Client = aria2Client;
            _configurationInfoRepository = configurationInfoRepository;
        }

        public async Task<Aria2GlobalStatDto> GetGlobalStatAsync()
        {
            try
            {
                return await _aria2Client.GetGlobalStatAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "获取 Aria2 全局状态失败");
                throw;
            }
        }

        public async Task<List<Aria2TaskDto>> GetActiveTasksAsync()
        {
            try
            {
                return await _aria2Client.TellActiveAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "获取活跃任务失败");
                throw;
            }
        }

        public async Task<List<Aria2TaskDto>> GetWaitingTasksAsync()
        {
            try
            {
                return await _aria2Client.TellWaitingAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "获取等待任务失败");
                throw;
            }
        }

        public async Task<List<Aria2TaskDto>> GetStoppedTasksAsync(int offset = 0, int num = 100)
        {
            try
            {
                return await _aria2Client.TellStoppedAsync(offset, num);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "获取停止任务失败");
                throw;
            }
        }

        public async Task<Aria2TaskDto> GetTaskStatusAsync(string gid)
        {
            try
            {
                return await _aria2Client.TellStatusAsync(gid);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "获取任务状态失败: {Gid}", gid);
                throw;
            }
        }

        public async Task<Aria2TaskDetailDto> GetTaskDetailAsync(string gid)
        {
            try
            {
                return await _aria2Client.TellStatusWithDetailAsync(gid);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "获取任务详情失败: {Gid}", gid);
                throw;
            }
        }

        public async Task<string> AddUriAsync(AddDownloadRequestDto input)
        {
            try
            {
                if (input == null || input.Urls == null || input.Urls.Count == 0)
                {
                    throw new ArgumentException("URL 列表不能为空");
                }

                // 构建选项
                var options = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(input.SavePath))
                {
                    options["dir"] = input.SavePath;
                }

                // 合并用户自定义选项
                if (input.Options != null)
                {
                    foreach (var kvp in input.Options)
                    {
                        options[kvp.Key] = kvp.Value;
                    }
                }

                // 调用 RPC 客户端
                return await _aria2Client.AddUriAsync(input.Urls, options.Count > 0 ? options : null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "添加下载任务失败");
                throw;
            }
        }

        public async Task<List<string>> BatchAddUriAsync(BatchAddUriRequestDto input)
        {
            try
            {
                if (input == null || input.Urls == null || input.Urls.Count == 0)
                {
                    throw new ArgumentException("URL 列表不能为空");
                }

                var gids = new List<string>();

                // 为每个 URL 创建独立的下载任务
                foreach (var url in input.Urls)
                {
                    try
                    {
                        // 构建选项
                        var options = new Dictionary<string, object>();

                        if (!string.IsNullOrWhiteSpace(input.SavePath))
                        {
                            options["dir"] = input.SavePath;
                        }

                        // 合并用户自定义选项
                        if (input.Options != null)
                        {
                            foreach (var kvp in input.Options)
                            {
                                options[kvp.Key] = kvp.Value;
                            }
                        }

                        // 为单个 URL 调用 RPC 客户端
                        var gid = await _aria2Client.AddUriAsync(new List<string> { url }, options.Count > 0 ? options : null);
                        gids.Add(gid);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "添加 URL {Url} 失败", url);
                        // 继续处理其他 URL
                    }
                }

                return gids;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "批量添加 URI 下载任务失败");
                throw;
            }
        }

        public async Task<string> AddTorrentAsync(AddTorrentRequestDto input)
        {
            try
            {
                if (input == null || string.IsNullOrWhiteSpace(input.TorrentData))
                {
                    throw new ArgumentException("种子文件数据不能为空");
                }

                // 构建选项
                var options = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(input.SavePath))
                {
                    options["dir"] = input.SavePath;
                }

                // 合并用户自定义选项
                if (input.Options != null)
                {
                    foreach (var kvp in input.Options)
                    {
                        options[kvp.Key] = kvp.Value;
                    }
                }

                // 调用 RPC 客户端
                return await _aria2Client.AddTorrentAsync(input.TorrentData, options.Count > 0 ? options : null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "添加种子文件下载任务失败");
                throw;
            }
        }

        public async Task<List<string>> BatchAddTorrentAsync(BatchAddTorrentRequestDto input)
        {
            try
            {
                if (input == null || input.Torrents == null || input.Torrents.Count == 0)
                {
                    throw new ArgumentException("种子文件列表不能为空");
                }

                var gids = new List<string>();

                foreach (var torrent in input.Torrents)
                {
                    try
                    {
                        // 构建选项
                        var options = new Dictionary<string, object>();

                        if (!string.IsNullOrWhiteSpace(input.SavePath))
                        {
                            options["dir"] = input.SavePath;
                        }

                        // 调用 RPC 客户端添加单个种子
                        var gid = await _aria2Client.AddTorrentAsync(torrent.TorrentData, options.Count > 0 ? options : null);
                        gids.Add(gid);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "添加种子文件 {FileName} 失败", torrent.FileName);
                        // 继续处理其他种子文件
                    }
                }

                return gids;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "批量添加种子文件下载任务失败");
                throw;
            }
        }

        public async Task<List<string>> PauseTasksAsync(PauseTasksRequestDto input)
        {
            try
            {
                if (input == null || input.Gids == null || input.Gids.Count == 0)
                {
                    throw new ArgumentException("GID 列表不能为空");
                }

                var results = new List<string>();
                foreach (var gid in input.Gids)
                {
                    try
                    {
                        var result = await _aria2Client.PauseAsync(gid);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "暂停任务失败: {Gid}", gid);
                        results.Add(gid); // 即使失败也返回 gid
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "批量暂停任务失败");
                throw;
            }
        }

        public async Task<string> PauseAllTasksAsync()
        {
            try
            {
                return await _aria2Client.PauseAllAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "暂停所有任务失败");
                throw;
            }
        }

        public async Task<List<string>> UnpauseTasksAsync(PauseTasksRequestDto input)
        {
            try
            {
                if (input == null || input.Gids == null || input.Gids.Count == 0)
                {
                    throw new ArgumentException("GID 列表不能为空");
                }

                var results = new List<string>();
                foreach (var gid in input.Gids)
                {
                    try
                    {
                        var result = await _aria2Client.UnpauseAsync(gid);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "恢复任务失败: {Gid}", gid);
                        results.Add(gid);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "批量恢复任务失败");
                throw;
            }
        }

        public async Task<string> UnpauseAllTasksAsync()
        {
            try
            {
                return await _aria2Client.UnpauseAllAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "恢复所有任务失败");
                throw;
            }
        }

        public async Task<List<string>> StopTasksAsync(StopTasksRequestDto input)
        {
            try
            {
                if (input == null || input.Gids == null || input.Gids.Count == 0)
                {
                    throw new ArgumentException("GID 列表不能为空");
                }

                var results = new List<string>();
                foreach (var gid in input.Gids)
                {
                    try
                    {
                        var result = await _aria2Client.RemoveAsync(gid);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "停止任务失败: {Gid}", gid);
                        // 尝试强制停止
                        try
                        {
                            var forceResult = await _aria2Client.ForceRemoveAsync(gid);
                            results.Add(forceResult);
                        }
                        catch
                        {
                            results.Add(gid);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "批量停止任务失败");
                throw;
            }
        }

        public async Task<List<string>> RemoveTasksAsync(RemoveTasksRequestDto input)
        {
            try
            {
                if (input == null || input.Gids == null || input.Gids.Count == 0)
                {
                    throw new ArgumentException("GID 列表不能为空");
                }

                var results = new List<string>();
                foreach (var gid in input.Gids)
                {
                    try
                    {
                        var result = await _aria2Client.RemoveAsync(gid);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "删除任务失败: {Gid}", gid);
                        results.Add(gid);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "批量删除任务失败");
                throw;
            }
        }

        public async Task<string> PurgeDownloadResultAsync()
        {
            try
            {
                return await _aria2Client.PurgeDownloadResultAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "清空停止任务失败");
                throw;
            }
        }

        public async Task<Aria2ConnectionStatusDto> GetConnectionStatusAsync()
        {
            try
            {
                var version = await _aria2Client.GetVersionAsync();
                var sessionInfo = await _aria2Client.GetSessionInfoAsync();

                return new Aria2ConnectionStatusDto
                {
                    IsConnected = true,
                    Version = version.Version,
                    SessionInfo = sessionInfo.SessionId
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "获取 Aria2 连接状态失败");
                return new Aria2ConnectionStatusDto
                {
                    IsConnected = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
