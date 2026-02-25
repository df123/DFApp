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
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Rss
{
    [Authorize(DFAppPermissions.RssSubscription.Default)]
    public class RssSubscriptionAppService : ApplicationService, IRssSubscriptionAppService
    {
        private readonly IRepository<RssSubscription, long> _rssSubscriptionRepository;
        private readonly IRepository<RssSource, long> _rssSourceRepository;

        public RssSubscriptionAppService(
            IRepository<RssSubscription, long> rssSubscriptionRepository,
            IRepository<RssSource, long> rssSourceRepository)
        {
            _rssSubscriptionRepository = rssSubscriptionRepository;
            _rssSourceRepository = rssSourceRepository;
        }

        public async Task<PagedResultDto<RssSubscriptionDto>> GetListAsync(GetRssSubscriptionsRequestDto input)
        {
            var queryable = await _rssSubscriptionRepository.GetQueryableAsync();

            if (input.IsEnabled.HasValue)
            {
                queryable = queryable.Where(x => x.IsEnabled == input.IsEnabled.Value);
            }

            if (input.RssSourceId.HasValue)
            {
                queryable = queryable.Where(x => x.RssSourceId == input.RssSourceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(x => x.Name.Contains(input.Filter) || 
                                               x.Keywords.Contains(input.Filter));
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

            var dtos = ObjectMapper.Map<List<RssSubscription>, List<RssSubscriptionDto>>(items);

            var sources = await _rssSourceRepository.GetListAsync();
            foreach (var dto in dtos)
            {
                dto.RssSourceName = sources.FirstOrDefault(s => s.Id == dto.RssSourceId)?.Name;
            }

            return new PagedResultDto<RssSubscriptionDto>(totalCount, dtos);
        }

        public async Task<RssSubscriptionDto> GetAsync(long id)
        {
            var subscription = await _rssSubscriptionRepository.GetAsync(id);
            var dto = ObjectMapper.Map<RssSubscription, RssSubscriptionDto>(subscription);

            if (subscription.RssSourceId.HasValue)
            {
                var source = await _rssSourceRepository.FirstOrDefaultAsync(s => s.Id == subscription.RssSourceId);
                dto.RssSourceName = source?.Name;
            }

            return dto;
        }

        [Authorize(DFAppPermissions.RssSubscription.Create)]
        public async Task<RssSubscriptionDto> CreateAsync(CreateUpdateRssSubscriptionDto input)
        {
            var subscription = ObjectMapper.Map<CreateUpdateRssSubscriptionDto, RssSubscription>(input);
            subscription.CreationTime = DateTime.Now;
            subscription.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _rssSubscriptionRepository.InsertAsync(subscription);

            Logger.LogInformation("创建RSS订阅: {Name}", input.Name);

            return ObjectMapper.Map<RssSubscription, RssSubscriptionDto>(subscription);
        }

        [Authorize(DFAppPermissions.RssSubscription.Update)]
        public async Task<RssSubscriptionDto> UpdateAsync(long id, CreateUpdateRssSubscriptionDto input)
        {
            Logger.LogInformation("开始更新RSS订阅，ID: {Id}", id);

            var subscription = await _rssSubscriptionRepository.GetAsync(id);

            Logger.LogInformation("获取到的实体并发戳: {Stamp}, 最后修改时间: {Time}", 
                subscription.ConcurrencyStamp, subscription.LastModificationTime);

            ObjectMapper.Map(input, subscription);
            subscription.LastModificationTime = DateTime.Now;

            try
            {
                await _rssSubscriptionRepository.UpdateAsync(subscription);
            }
            catch (Volo.Abp.Data.AbpDbConcurrencyException ex)
            {
                Logger.LogWarning("更新RSS订阅时发生并发冲突，重新获取实体后重试: {Name}, 异常: {Message}", input.Name, ex.Message);
                
                // 等待一小段时间，确保获取到最新数据
                await Task.Delay(100);
                
                subscription = await _rssSubscriptionRepository.GetAsync(id);
                Logger.LogInformation("重试获取到的实体并发戳: {Stamp}, 最后修改时间: {Time}", 
                    subscription.ConcurrencyStamp, subscription.LastModificationTime);

                ObjectMapper.Map(input, subscription);
                subscription.LastModificationTime = DateTime.Now;

                await _rssSubscriptionRepository.UpdateAsync(subscription);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "更新RSS订阅时发生未预期的异常: {Name}", input.Name);
                throw;
            }

            Logger.LogInformation("更新RSS订阅成功: {Name}", input.Name);

            return ObjectMapper.Map<RssSubscription, RssSubscriptionDto>(subscription);
        }

        [Authorize(DFAppPermissions.RssSubscription.Delete)]
        public async Task DeleteAsync(long id)
        {
            await _rssSubscriptionRepository.DeleteAsync(id);
            Logger.LogInformation("删除RSS订阅: {Id}", id);
        }

        [Authorize(DFAppPermissions.RssSubscription.Update)]
        public async Task ToggleEnableAsync(long id)
        {
            var subscription = await _rssSubscriptionRepository.GetAsync(id);
            subscription.IsEnabled = !subscription.IsEnabled;
            subscription.LastModificationTime = DateTime.Now;
            
            try
            {
                await _rssSubscriptionRepository.UpdateAsync(subscription);
            }
            catch (Volo.Abp.Data.AbpDbConcurrencyException)
            {
                Logger.LogWarning("切换订阅状态时发生并发冲突，重新获取实体后重试: {Name}", subscription.Name);
                
                // 等待一小段时间，确保获取到最新数据
                await Task.Delay(100);
                
                subscription = await _rssSubscriptionRepository.GetAsync(id);
                subscription.IsEnabled = !subscription.IsEnabled;
                subscription.LastModificationTime = DateTime.Now;
                
                await _rssSubscriptionRepository.UpdateAsync(subscription);
            }
            
            Logger.LogInformation("{Action} RSS订阅: {Name}", 
                subscription.IsEnabled ? "启用" : "禁用", subscription.Name);
        }
    }
}
