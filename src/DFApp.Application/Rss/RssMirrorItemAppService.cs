using DFApp.Aria2;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Rss
{
    [Authorize(DFAppPermissions.Rss.Default)]
    public class RssMirrorItemAppService : ApplicationService, IRssMirrorItemAppService
    {
        private readonly IRepository<RssMirrorItem, long> _rssMirrorItemRepository;
        private readonly IRepository<RssWordSegment, long> _rssWordSegmentRepository;
        private readonly IRepository<RssSource, long> _rssSourceRepository;
        private readonly IAria2Service _aria2Service;

        public RssMirrorItemAppService(
            IRepository<RssMirrorItem, long> rssMirrorItemRepository,
            IRepository<RssWordSegment, long> rssWordSegmentRepository,
            IRepository<RssSource, long> rssSourceRepository,
            IAria2Service aria2Service)
        {
            _rssMirrorItemRepository = rssMirrorItemRepository;
            _rssWordSegmentRepository = rssWordSegmentRepository;
            _rssSourceRepository = rssSourceRepository;
            _aria2Service = aria2Service;
        }

        public async Task<PagedResultDto<RssMirrorItemDto>> GetListAsync(GetRssMirrorItemsRequestDto input)
        {
            var queryable = await _rssMirrorItemRepository.GetQueryableAsync();

            // 应用过滤条件
            if (input.RssSourceId.HasValue)
            {
                queryable = queryable.Where(x => x.RssSourceId == input.RssSourceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(x => x.Title.Contains(input.Filter) ||
                                               (x.Description != null && x.Description.Contains(input.Filter)));
            }

            if (input.StartTime.HasValue)
            {
                queryable = queryable.Where(x => x.CreationTime >= input.StartTime.Value);
            }

            if (input.EndTime.HasValue)
            {
                queryable = queryable.Where(x => x.CreationTime <= input.EndTime.Value);
            }

            if (input.IsDownloaded.HasValue)
            {
                queryable = queryable.Where(x => x.IsDownloaded == input.IsDownloaded.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.WordToken))
            {
                // 根据分词过滤
                var wordSegmentQueryable = await _rssWordSegmentRepository.GetQueryableAsync();
                var filterItemIds = wordSegmentQueryable
                    .Where(x => x.Word.ToLower() == input.WordToken.ToLower())
                    .Select(x => x.RssMirrorItemId)
                    .Distinct();

                queryable = queryable.Where(x => filterItemIds.Contains(x.Id));
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
            var items = await AsyncExecuter.ToListAsync(queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

            // 转换为DTO并加载关联数据
            var itemIds = items.Select(i => i.Id).ToList();
            var wordSegments = await _rssWordSegmentRepository.GetListAsync(x => itemIds.Contains(x.RssMirrorItemId));
            var sources = await _rssSourceRepository.GetListAsync();

            var dtos = ObjectMapper.Map<List<RssMirrorItem>, List<RssMirrorItemDto>>(items);
            foreach (var dto in dtos)
            {
                dto.RssSourceName = sources.FirstOrDefault(s => s.Id == dto.RssSourceId)?.Name;
                dto.WordSegments = wordSegments
                    .Where(ws => ws.RssMirrorItemId == dto.Id)
                    .GroupBy(ws => ws.Word.ToLower())
                    .Select(g => new RssWordSegmentDto
                    {
                        Word = g.First().Word,
                        LanguageType = g.First().LanguageType,
                        Count = g.Sum(x => x.Count),
                        CreationTime = g.First().CreationTime
                    })
                    .ToList();
            }

            return new PagedResultDto<RssMirrorItemDto>(totalCount, dtos);
        }

        public async Task<RssMirrorItemDto> GetAsync(long id)
        {
            var item = await _rssMirrorItemRepository.GetAsync(id);
            var dto = ObjectMapper.Map<RssMirrorItem, RssMirrorItemDto>(item);

            // 加载分词
            var wordSegments = await _rssWordSegmentRepository.GetListAsync(x => x.RssMirrorItemId == id);
            dto.WordSegments = ObjectMapper.Map<List<RssWordSegment>, List<RssWordSegmentDto>>(wordSegments);

            // 加载RSS源名称
            var source = await _rssSourceRepository.FirstOrDefaultAsync(s => s.Id == dto.RssSourceId);
            dto.RssSourceName = source?.Name;

            return dto;
        }

        [Authorize(DFAppPermissions.Rss.Delete)]
        public async Task DeleteAsync(long id)
        {
            // 先删除关联的分词
            await _rssWordSegmentRepository.DeleteAsync(x => x.RssMirrorItemId == id);

            // 再删除镜像条目
            await _rssMirrorItemRepository.DeleteAsync(id);
        }

        [Authorize(DFAppPermissions.Rss.Delete)]
        public async Task DeleteManyAsync(List<long> ids)
        {
            foreach (var id in ids)
            {
                await DeleteAsync(id);
            }
        }

        public async Task<List<WordSegmentStatisticsDto>> GetWordSegmentStatisticsAsync(
            long? rssSourceId = null,
            int? languageType = null,
            int top = 100)
        {
            var wordSegmentQueryable = await _rssWordSegmentRepository.GetQueryableAsync();
            var mirrorItemQueryable = await _rssMirrorItemRepository.GetQueryableAsync();

            // 应用过滤条件
            if (rssSourceId.HasValue)
            {
                var itemIds = mirrorItemQueryable
                    .Where(x => x.RssSourceId == rssSourceId.Value)
                    .Select(x => x.Id);
                wordSegmentQueryable = wordSegmentQueryable.Where(x => itemIds.Contains(x.RssMirrorItemId));
            }

            if (languageType.HasValue)
            {
                wordSegmentQueryable = wordSegmentQueryable.Where(x => x.LanguageType == languageType.Value);
            }

            // 统计分词
            var statistics = wordSegmentQueryable
                .GroupBy(x => x.Word.ToLower())
                .Select(g => new WordSegmentStatisticsDto
                {
                    Word = g.First().Word,
                    TotalCount = g.Sum(x => x.Count),
                    ItemCount = g.Select(x => x.RssMirrorItemId).Distinct().Count(),
                    LanguageType = g.First().LanguageType
                })
                .OrderByDescending(x => x.TotalCount)
                .Take(top)
                .ToList();

            return statistics;
        }

        public async Task<PagedResultDto<RssMirrorItemDto>> GetByWordTokenAsync(
            string wordToken,
            PagedAndSortedResultRequestDto input)
        {
            var request = new GetRssMirrorItemsRequestDto
            {
                WordToken = wordToken,
                SkipCount = input.SkipCount,
                MaxResultCount = input.MaxResultCount,
                Sorting = input.Sorting
            };

            return await GetListAsync(request);
        }

        [Authorize(DFAppPermissions.Rss.Delete)]
        public async Task ClearAllAsync()
        {
            // 删除所有分词
            await _rssWordSegmentRepository.DeleteAsync(x => true);

            // 删除所有镜像条目
            await _rssMirrorItemRepository.DeleteAsync(x => true);

            Logger.LogInformation("已清空所有RSS镜像数据");
        }

        [Authorize(DFAppPermissions.Rss.Default)]
        public async Task<string> DownloadToAria2Async(long id, bool videoOnly = false, bool enableKeywordFilter = false)
        {
            var item = await _rssMirrorItemRepository.GetAsync(id);

            if (item.IsDownloaded)
            {
                Logger.LogWarning("RSS镜像条目 {Id} 已经下载过", id);
                throw new UserFriendlyException("该条目已经下载过");
            }

            // 创建Aria2下载请求
            var request = new AddDownloadRequestDto
            {
                Urls = new List<string> { item.Link },
                VideoOnly = videoOnly,
                EnableKeywordFilter = enableKeywordFilter
            };

            var result = await _aria2Service.AddDownloadAsync(request);

            // 更新下载状态
            item.IsDownloaded = true;
            item.DownloadTime = DateTime.Now;
            await _rssMirrorItemRepository.UpdateAsync(item);

            Logger.LogInformation("RSS镜像条目 {Id} 已添加到Aria2下载队列", id);

            return result.Id;
        }
    }
}
