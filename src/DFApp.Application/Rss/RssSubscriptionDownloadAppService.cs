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
    [Authorize(DFAppPermissions.RssSubscription.Default)]
    public class RssSubscriptionDownloadAppService : ApplicationService, IRssSubscriptionDownloadAppService
    {
        private readonly IRepository<RssSubscriptionDownload, long> _rssSubscriptionDownloadRepository;
        private readonly IRepository<RssSubscription, long> _rssSubscriptionRepository;
        private readonly IRepository<RssMirrorItem, long> _rssMirrorItemRepository;
        private readonly IRepository<RssSource, long> _rssSourceRepository;
        private readonly IRssSubscriptionService _rssSubscriptionService;

        public RssSubscriptionDownloadAppService(
            IRepository<RssSubscriptionDownload, long> rssSubscriptionDownloadRepository,
            IRepository<RssSubscription, long> rssSubscriptionRepository,
            IRepository<RssMirrorItem, long> rssMirrorItemRepository,
            IRepository<RssSource, long> rssSourceRepository,
            IRssSubscriptionService rssSubscriptionService)
        {
            _rssSubscriptionDownloadRepository = rssSubscriptionDownloadRepository;
            _rssSubscriptionRepository = rssSubscriptionRepository;
            _rssMirrorItemRepository = rssMirrorItemRepository;
            _rssSourceRepository = rssSourceRepository;
            _rssSubscriptionService = rssSubscriptionService;
        }

        public async Task<PagedResultDto<RssSubscriptionDownloadDto>> GetListAsync(GetRssSubscriptionDownloadsRequestDto input)
        {
            var queryable = await _rssSubscriptionDownloadRepository.GetQueryableAsync();

            if (input.SubscriptionId.HasValue)
            {
                queryable = queryable.Where(x => x.SubscriptionId == input.SubscriptionId.Value);
            }

            if (input.RssMirrorItemId.HasValue)
            {
                queryable = queryable.Where(x => x.RssMirrorItemId == input.RssMirrorItemId.Value);
            }

            if (input.DownloadStatus.HasValue)
            {
                queryable = queryable.Where(x => x.DownloadStatus == input.DownloadStatus.Value);
            }

            if (input.StartTime.HasValue)
            {
                queryable = queryable.Where(x => x.CreationTime >= input.StartTime.Value);
            }

            if (input.EndTime.HasValue)
            {
                queryable = queryable.Where(x => x.CreationTime <= input.EndTime.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                queryable = queryable.OrderBy(input.Sorting);
            }
            else
            {
                queryable = queryable.OrderByDescending(x => x.CreationTime);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var items = await AsyncExecuter.ToListAsync(queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

            var dtos = ObjectMapper.Map<List<RssSubscriptionDownload>, List<RssSubscriptionDownloadDto>>(items);

            var subscriptions = await _rssSubscriptionRepository.GetListAsync();
            var mirrorItems = await _rssMirrorItemRepository.GetListAsync();
            var sources = await _rssSourceRepository.GetListAsync();

            foreach (var dto in dtos)
            {
                dto.SubscriptionName = subscriptions.FirstOrDefault(s => s.Id == dto.SubscriptionId)?.Name;
                
                var item = mirrorItems.FirstOrDefault(i => i.Id == dto.RssMirrorItemId);
                dto.RssMirrorItemTitle = item?.Title;
                dto.RssMirrorItemLink = item?.Link;
                dto.RssSourceName = sources.FirstOrDefault(s => s.Id == item?.RssSourceId)?.Name;
                
                dto.DownloadStatusText = GetDownloadStatusText(dto.DownloadStatus);
            }

            return new PagedResultDto<RssSubscriptionDownloadDto>(totalCount, dtos);
        }

        public async Task<RssSubscriptionDownloadDto> GetAsync(long id)
        {
            var download = await _rssSubscriptionDownloadRepository.GetAsync(id);
            var dto = ObjectMapper.Map<RssSubscriptionDownload, RssSubscriptionDownloadDto>(download);

            dto.SubscriptionName = (await _rssSubscriptionRepository.GetAsync(download.SubscriptionId))?.Name;
            
            var item = await _rssMirrorItemRepository.GetAsync(download.RssMirrorItemId);
            dto.RssMirrorItemTitle = item.Title;
            dto.RssMirrorItemLink = item.Link;
            dto.RssSourceName = (await _rssSourceRepository.GetAsync(item.RssSourceId))?.Name;
            
            dto.DownloadStatusText = GetDownloadStatusText(dto.DownloadStatus);

            return dto;
        }

        [Authorize(DFAppPermissions.RssSubscription.Delete)]
        public async Task DeleteAsync(long id)
        {
            await _rssSubscriptionDownloadRepository.DeleteAsync(id);
            Logger.LogInformation("删除订阅下载记录: {Id}", id);
        }

        [Authorize(DFAppPermissions.RssSubscription.Delete)]
        public async Task DeleteManyAsync(List<long> ids)
        {
            foreach (var id in ids)
            {
                await DeleteAsync(id);
            }
        }

        [Authorize(DFAppPermissions.RssSubscription.Delete)]
        public async Task ClearAllAsync()
        {
            await _rssSubscriptionDownloadRepository.DeleteAsync(x => true);
            Logger.LogInformation("清空所有订阅下载记录");
        }

        [Authorize(DFAppPermissions.RssSubscription.Default)]
        public async Task RetryAsync(long id)
        {
            var download = await _rssSubscriptionDownloadRepository.GetAsync(id);

            if (download.DownloadStatus != 3)
            {
                throw new UserFriendlyException("只能重试失败的下载任务");
            }

            // 先删除旧的下载记录，避免与后续创建的记录产生冲突
            await _rssSubscriptionDownloadRepository.DeleteAsync(id);

            await _rssSubscriptionService.CreateDownloadTaskAsync(download.SubscriptionId, download.RssMirrorItemId);

            Logger.LogInformation("重试订阅下载: {Id}", id);
        }

        private string GetDownloadStatusText(int status)
        {
            return status switch
            {
                0 => "待下载",
                1 => "下载中",
                2 => "下载完成",
                3 => "下载失败",
                _ => "未知状态"
            };
        }
    }
}
