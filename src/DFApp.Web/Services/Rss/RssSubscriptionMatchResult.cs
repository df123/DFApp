namespace DFApp.Web.Services.Rss
{
    /// <summary>
    /// RSS订阅匹配结果
    /// </summary>
    public class RssSubscriptionMatchResult
    {
        /// <summary>
        /// 订阅ID
        /// </summary>
        public long SubscriptionId { get; set; }

        /// <summary>
        /// 订阅名称
        /// </summary>
        public string SubscriptionName { get; set; } = string.Empty;

        /// <summary>
        /// 是否匹配
        /// </summary>
        public bool Matched { get; set; }

        /// <summary>
        /// 匹配原因
        /// </summary>
        public string MatchReason { get; set; } = string.Empty;
    }
}
