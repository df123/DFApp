using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Rss
{
    public interface IRssSubscriptionService
    {
        Task<List<RssSubscriptionMatchResult>> MatchSubscriptionsAsync(RssMirrorItem item);
        Task CreateDownloadTaskAsync(long subscriptionId, long rssMirrorItemId);
    }

    public class RssSubscriptionMatchResult
    {
        public long SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public bool Matched { get; set; }
        public string? MatchReason { get; set; }
    }
}
