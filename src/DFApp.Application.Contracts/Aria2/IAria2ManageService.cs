using DFApp.Aria2;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Aria2
{
    /// <summary>
    /// Aria2 管理服务接口（直接连接 aria2 RPC）
    /// </summary>
    public interface IAria2ManageService : IApplicationService
    {
        /// <summary>
        /// 获取 Aria2 全局状态
        /// </summary>
        Task<Aria2GlobalStatDto> GetGlobalStatAsync();

        /// <summary>
        /// 获取活跃任务列表
        /// </summary>
        Task<List<Aria2TaskDto>> GetActiveTasksAsync();

        /// <summary>
        /// 获取等待任务列表
        /// </summary>
        Task<List<Aria2TaskDto>> GetWaitingTasksAsync();

        /// <summary>
        /// 获取停止任务列表
        /// </summary>
        Task<List<Aria2TaskDto>> GetStoppedTasksAsync(int offset = 0, int num = 100);

        /// <summary>
        /// 获取任务状态
        /// </summary>
        Task<Aria2TaskDto> GetTaskStatusAsync(string gid);

        /// <summary>
        /// 获取任务详情（包含peers和文件列表）
        /// </summary>
        Task<Aria2TaskDetailDto> GetTaskDetailAsync(string gid);

        /// <summary>
        /// 添加 URI 下载任务
        /// </summary>
        Task<string> AddUriAsync(AddDownloadRequestDto input);

        /// <summary>
        /// 批量添加 URI 下载任务（每条链接创建独立任务）
        /// </summary>
        Task<List<string>> BatchAddUriAsync(BatchAddUriRequestDto input);

        /// <summary>
        /// 添加种子文件下载任务
        /// </summary>
        Task<string> AddTorrentAsync(AddTorrentRequestDto input);

        /// <summary>
        /// 批量添加种子文件下载任务
        /// </summary>
        Task<List<string>> BatchAddTorrentAsync(BatchAddTorrentRequestDto input);

        /// <summary>
        /// 暂停任务
        /// </summary>
        Task<List<string>> PauseTasksAsync(PauseTasksRequestDto input);

        /// <summary>
        /// 暂停所有任务
        /// </summary>
        Task<string> PauseAllTasksAsync();

        /// <summary>
        /// 恢复任务
        /// </summary>
        Task<List<string>> UnpauseTasksAsync(PauseTasksRequestDto input);

        /// <summary>
        /// 恢复所有任务
        /// </summary>
        Task<string> UnpauseAllTasksAsync();

        /// <summary>
        /// 停止任务（移除活跃或等待中的任务）
        /// </summary>
        Task<List<string>> StopTasksAsync(StopTasksRequestDto input);

        /// <summary>
        /// 删除停止的任务
        /// </summary>
        Task<List<string>> RemoveTasksAsync(RemoveTasksRequestDto input);

        /// <summary>
        /// 清空停止的任务
        /// </summary>
        Task<string> PurgeDownloadResultAsync();

        /// <summary>
        /// 获取 Aria2 连接状态
        /// </summary>
        Task<Aria2ConnectionStatusDto> GetConnectionStatusAsync();

        /// <summary>
        /// 批量查询 IP 地理位置（通过后端代理调用 ip-api.com，解决 HTTPS 页面调用 HTTP API 的混合内容问题）
        /// </summary>
        Task<List<IpGeolocationDto>> GetIpGeolocationAsync(List<string> ips);
    }
}
