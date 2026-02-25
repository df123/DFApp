using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Rss
{
    public interface IRssSubscriptionAppService : IApplicationService
    {
        Task<PagedResultDto<RssSubscriptionDto>> GetListAsync(GetRssSubscriptionsRequestDto input);

        Task<RssSubscriptionDto> GetAsync(long id);

        Task<RssSubscriptionDto> CreateAsync(CreateUpdateRssSubscriptionDto input);

        Task<RssSubscriptionDto> UpdateAsync(long id, CreateUpdateRssSubscriptionDto input);

        Task DeleteAsync(long id);

        Task ToggleEnableAsync(long id);
    }

    public interface IRssSubscriptionDownloadAppService : IApplicationService
    {
        Task<PagedResultDto<RssSubscriptionDownloadDto>> GetListAsync(GetRssSubscriptionDownloadsRequestDto input);

        Task<RssSubscriptionDownloadDto> GetAsync(long id);

        Task DeleteAsync(long id);

        Task DeleteManyAsync(List<long> ids);

        Task ClearAllAsync();

        Task RetryAsync(long id);
    }
}
