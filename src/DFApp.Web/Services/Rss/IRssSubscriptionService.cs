using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Rss;

namespace DFApp.Web.Services.Rss
{
    /// <summary>
    /// RSS订阅服务接口 - 负责订阅匹配、下载任务创建和暂存下载处理
    /// </summary>
    public interface IRssSubscriptionService
    {
        /// <summary>
        /// 匹配RSS镜像条目与所有启用的订阅规则
        /// </summary>
        /// <param name="item">RSS镜像条目</param>
        /// <returns>每个订阅的匹配结果列表</returns>
        Task<List<RssSubscriptionMatchResult>> MatchSubscriptionsAsync(RssMirrorItem item);

        /// <summary>
        /// 根据订阅ID和镜像条目ID创建下载任务
        /// </summary>
        /// <param name="subscriptionId">订阅ID</param>
        /// <param name="rssMirrorItemId">RSS镜像条目ID</param>
        Task CreateDownloadTaskAsync(long subscriptionId, long rssMirrorItemId);

        /// <summary>
        /// 处理因磁盘空间不足而暂存的下载任务
        /// </summary>
        Task ProcessPendingDownloadsAsync();
    }
}
