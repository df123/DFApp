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
    public class RssSourceAppService : ApplicationService, IRssSourceAppService
    {
        private readonly IRepository<RssSource, long> _rssSourceRepository;

        public RssSourceAppService(IRepository<RssSource, long> rssSourceRepository)
        {
            _rssSourceRepository = rssSourceRepository;
        }

        public async Task<PagedResultDto<RssSourceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var queryable = await _rssSourceRepository.GetQueryableAsync();

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

            var dtos = ObjectMapper.Map<List<RssSource>, List<RssSourceDto>>(items);

            return new PagedResultDto<RssSourceDto>(totalCount, dtos);
        }

        public async Task<RssSourceDto> GetAsync(long id)
        {
            var source = await _rssSourceRepository.GetAsync(id);
            return ObjectMapper.Map<RssSource, RssSourceDto>(source);
        }

        [Authorize(DFAppPermissions.Rss.Create)]
        public async Task<RssSourceDto> CreateAsync(CreateUpdateRssSourceDto input)
        {
            // 验证URL是否重复
            var existing = await _rssSourceRepository.FirstOrDefaultAsync(x => x.Url == input.Url);
            if (existing != null)
            {
                throw new UserFriendlyException("该RSS源URL已存在");
            }

            var source = ObjectMapper.Map<CreateUpdateRssSourceDto, RssSource>(input);
            source.CreationTime = DateTime.Now;
            source.FetchStatus = 0;
            source.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _rssSourceRepository.InsertAsync(source);

            Logger.LogInformation("创建RSS源: {Name} ({Url})", input.Name, input.Url);

            return ObjectMapper.Map<RssSource, RssSourceDto>(source);
        }

        [Authorize(DFAppPermissions.Rss.Update)]
        public async Task<RssSourceDto> UpdateAsync(long id, CreateUpdateRssSourceDto input)
        {
            var source = await _rssSourceRepository.GetAsync(id);

            // 检查URL是否与其他源重复
            var existing = await _rssSourceRepository.FirstOrDefaultAsync(x => x.Url == input.Url && x.Id != id);
            if (existing != null)
            {
                throw new UserFriendlyException("该RSS源URL已被其他源使用");
            }

            ObjectMapper.Map(input, source);
            source.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _rssSourceRepository.UpdateAsync(source);

            Logger.LogInformation("更新RSS源: {Name} ({Url})", input.Name, input.Url);

            return ObjectMapper.Map<RssSource, RssSourceDto>(source);
        }

        [Authorize(DFAppPermissions.Rss.Delete)]
        public async Task DeleteAsync(long id)
        {
            await _rssSourceRepository.DeleteAsync(id);
            Logger.LogInformation("删除RSS源: {Id}", id);
        }

        [Authorize(DFAppPermissions.Rss.Default)]
        public async Task TriggerFetchAsync(long id)
        {
            var source = await _rssSourceRepository.GetAsync(id);

            if (!source.IsEnabled)
            {
                throw new UserFriendlyException("该RSS源未启用");
            }

            // 这里可以通过事件或其他机制触发Background Worker立即执行
            // 暂时只记录日志
            Logger.LogInformation("手动触发RSS源抓取: {Name} ({Url})", source.Name, source.Url);

            // 更新最后抓取时间
            source.LastFetchTime = DateTime.Now;
            await _rssSourceRepository.UpdateAsync(source);

            throw new UserFriendlyException("手动触发功能将在Background Worker下次执行时生效，或等待自动调度");
        }
    }
}
