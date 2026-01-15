using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS镜像条目应用服务接口
    /// </summary>
    public interface IRssMirrorItemAppService : IApplicationService
    {
        /// <summary>
        /// 获取RSS镜像条目列表
        /// </summary>
        Task<PagedResultDto<RssMirrorItemDto>> GetListAsync(GetRssMirrorItemsRequestDto input);

        /// <summary>
        /// 获取RSS镜像条目详情
        /// </summary>
        Task<RssMirrorItemDto> GetAsync(long id);

        /// <summary>
        /// 删除RSS镜像条目
        /// </summary>
        Task DeleteAsync(long id);

        /// <summary>
        /// 批量删除RSS镜像条目
        /// </summary>
        Task DeleteManyAsync(List<long> ids);

        /// <summary>
        /// 获取分词统计
        /// </summary>
        Task<List<WordSegmentStatisticsDto>> GetWordSegmentStatisticsAsync(
            long? rssSourceId = null,
            int? languageType = null,
            int top = 100);

        /// <summary>
        /// 根据分词查询RSS镜像条目
        /// </summary>
        Task<PagedResultDto<RssMirrorItemDto>> GetByWordTokenAsync(
            string wordToken,
            PagedAndSortedResultRequestDto input);

        /// <summary>
        /// 清空所有RSS镜像条目
        /// </summary>
        Task ClearAllAsync();

        /// <summary>
        /// 下载到Aria2
        /// </summary>
        Task<string> DownloadToAria2Async(long id, bool videoOnly = false, bool enableKeywordFilter = false);
    }

    /// <summary>
    /// RSS源应用服务接口
    /// </summary>
    public interface IRssSourceAppService : IApplicationService
    {
        /// <summary>
        /// 获取RSS源列表
        /// </summary>
        Task<PagedResultDto<RssSourceDto>> GetListAsync(PagedAndSortedResultRequestDto input);

        /// <summary>
        /// 获取RSS源详情
        /// </summary>
        Task<RssSourceDto> GetAsync(long id);

        /// <summary>
        /// 创建RSS源
        /// </summary>
        Task<RssSourceDto> CreateAsync(CreateUpdateRssSourceDto input);

        /// <summary>
        /// 更新RSS源
        /// </summary>
        Task<RssSourceDto> UpdateAsync(long id, CreateUpdateRssSourceDto input);

        /// <summary>
        /// 删除RSS源
        /// </summary>
        Task DeleteAsync(long id);

        /// <summary>
        /// 手动触发RSS源抓取
        /// </summary>
        Task TriggerFetchAsync(long id);
    }
}
