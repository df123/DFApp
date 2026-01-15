using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS分词应用服务
    /// </summary>
    [Authorize(DFAppPermissions.Rss.Default)]
    public class RssWordSegmentAppService : ApplicationService
    {
        private readonly IRepository<RssWordSegment, long> _rssWordSegmentRepository;
        private readonly IRepository<RssMirrorItem, long> _rssMirrorItemRepository;
        private readonly IRepository<RssSource, long> _rssSourceRepository;

        public RssWordSegmentAppService(
            IRepository<RssWordSegment, long> rssWordSegmentRepository,
            IRepository<RssMirrorItem, long> rssMirrorItemRepository,
            IRepository<RssSource, long> rssSourceRepository)
        {
            _rssWordSegmentRepository = rssWordSegmentRepository;
            _rssMirrorItemRepository = rssMirrorItemRepository;
            _rssSourceRepository = rssSourceRepository;
        }

        /// <summary>
        /// 获取分词列表（分页）
        /// </summary>
        public async Task<PagedResultDto<RssWordSegmentWithItemDto>> GetListAsync(GetRssWordSegmentsRequestDto input)
        {
            var queryable = await _rssWordSegmentRepository.GetQueryableAsync();

            // 应用过滤条件
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(x => x.Word.Contains(input.Filter));
            }

            if (input.RssSourceId.HasValue)
            {
                var mirrorItemQueryable = await _rssMirrorItemRepository.GetQueryableAsync();
                var filterItemIds = mirrorItemQueryable
                    .Where(x => x.RssSourceId == input.RssSourceId.Value)
                    .Select(x => x.Id);
                queryable = queryable.Where(x => filterItemIds.Contains(x.RssMirrorItemId));
            }

            if (input.LanguageType.HasValue)
            {
                queryable = queryable.Where(x => x.LanguageType == input.LanguageType.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Word))
            {
                queryable = queryable.Where(x => x.Word.ToLower() == input.Word.ToLower());
            }

            // 排序
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                queryable = queryable.OrderBy(input.Sorting);
            }
            else
            {
                queryable = queryable.OrderByDescending(x => x.CreationTime);
            }

            // 分页
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var items = await AsyncExecuter.ToListAsync(
                queryable.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            // 加载关联数据
            var itemIds = items.Select(i => i.RssMirrorItemId).Distinct().ToList();
            var mirrorItems = await _rssMirrorItemRepository.GetListAsync(x => itemIds.Contains(x.Id));
            var sources = await _rssSourceRepository.GetListAsync();

            var dtos = ObjectMapper.Map<List<RssWordSegment>, List<RssWordSegmentWithItemDto>>(items);
            foreach (var dto in dtos)
            {
                var mirrorItem = mirrorItems.FirstOrDefault(mi => mi.Id == dto.RssMirrorItemId);
                if (mirrorItem != null)
                {
                    dto.RssMirrorItemTitle = mirrorItem.Title;
                    dto.RssMirrorItemLink = mirrorItem.Link;
                    dto.RssSourceId = mirrorItem.RssSourceId;
                    dto.RssSourceName = sources.FirstOrDefault(s => s.Id == mirrorItem.RssSourceId)?.Name;
                }
            }

            return new PagedResultDto<RssWordSegmentWithItemDto>(totalCount, dtos);
        }

        /// <summary>
        /// 获取分词统计（带分页）
        /// </summary>
        public async Task<PagedResultDto<WordSegmentStatisticsDto>> GetStatisticsAsync(
            GetRssWordSegmentsRequestDto input)
        {
            var wordSegmentQueryable = await _rssWordSegmentRepository.GetQueryableAsync();
            var mirrorItemQueryable = await _rssMirrorItemRepository.GetQueryableAsync();

            // 应用过滤条件
            if (input.RssSourceId.HasValue)
            {
                var filterItemIds = mirrorItemQueryable
                    .Where(x => x.RssSourceId == input.RssSourceId.Value)
                    .Select(x => x.Id);
                wordSegmentQueryable = wordSegmentQueryable.Where(x => filterItemIds.Contains(x.RssMirrorItemId));
            }

            if (input.LanguageType.HasValue)
            {
                wordSegmentQueryable = wordSegmentQueryable.Where(x => x.LanguageType == input.LanguageType.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                wordSegmentQueryable = wordSegmentQueryable.Where(x => x.Word.Contains(input.Filter));
            }

            // 统计分词（在内存中执行，因为需要 GroupBy）
            var allSegments = await AsyncExecuter.ToListAsync(wordSegmentQueryable);

            var statisticsQuery = allSegments
                .GroupBy(x => x.Word.ToLower())
                .Select(g => new WordSegmentStatisticsDto
                {
                    Word = g.First().Word,
                    TotalCount = g.Sum(x => x.Count),
                    ItemCount = g.Select(x => x.RssMirrorItemId).Distinct().Count(),
                    LanguageType = g.First().LanguageType
                })
                .AsQueryable();

            // 排序
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                statisticsQuery = statisticsQuery.OrderBy(input.Sorting);
            }
            else
            {
                statisticsQuery = statisticsQuery.OrderByDescending(x => x.TotalCount);
            }

            // 获取总数
            var totalCount = statisticsQuery.Count();

            // 分页
            var items = statisticsQuery
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            Logger.LogInformation("获取分词统计，来源：{RssSourceId}，语言：{LanguageType}，总数：{TotalCount}", input.RssSourceId, input.LanguageType, totalCount);

            return new PagedResultDto<WordSegmentStatisticsDto>(totalCount, items);
        }

        /// <summary>
        /// 删除指定RSS镜像条目的所有分词
        /// </summary>
        [Authorize(DFAppPermissions.Rss.Delete)]
        public async Task DeleteByItemAsync(long rssMirrorItemId)
        {
            await _rssWordSegmentRepository.DeleteAsync(x => x.RssMirrorItemId == rssMirrorItemId);
            Logger.LogInformation("已删除RSS镜像条目 {ItemId} 的所有分词", rssMirrorItemId);
        }

        /// <summary>
        /// 删除指定RSS源的所有分词
        /// </summary>
        [Authorize(DFAppPermissions.Rss.Delete)]
        public async Task DeleteBySourceAsync(long rssSourceId)
        {
            var mirrorItemQueryable = await _rssMirrorItemRepository.GetQueryableAsync();
            var itemIds = mirrorItemQueryable
                .Where(x => x.RssSourceId == rssSourceId)
                .Select(x => x.Id);

            await _rssWordSegmentRepository.DeleteAsync(x => itemIds.Contains(x.RssMirrorItemId));
            Logger.LogInformation("已删除RSS源 {SourceId} 的所有分词", rssSourceId);
        }
    }
}
