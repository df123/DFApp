using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.Services.Rss;
using Microsoft.Extensions.Logging;
using Quartz;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DFApp.Web.Background
{
    /// <summary>
    /// RSS镜像抓取定时任务，定期从启用的RSS源抓取数据并处理订阅匹配
    /// </summary>
    public class RssMirrorFetchJob : IJob
    {
        private readonly ISqlSugarRepository<RssSource, long> _rssSourceRepository;
        private readonly ISqlSugarRepository<RssMirrorItem, long> _rssMirrorItemRepository;
        private readonly ISqlSugarRepository<RssWordSegment, long> _rssWordSegmentRepository;
        private readonly ISqlSugarReadOnlyRepository<RssSubscription, long> _rssSubscriptionRepository;
        private readonly IWordSegmentService _wordSegmentService;
        private readonly ISqlSugarClient _db;
        private readonly IRssSubscriptionService _rssSubscriptionService;
        private readonly ILogger<RssMirrorFetchJob> _logger;

        /// <summary>
        /// RSS标准元素名称集合
        /// </summary>
        private static readonly HashSet<string> StandardRssElements = new()
        {
            "title", "link", "description", "pubDate", "author", "category", "guid", "comments"
        };

        public RssMirrorFetchJob(
            ISqlSugarRepository<RssSource, long> rssSourceRepository,
            ISqlSugarRepository<RssMirrorItem, long> rssMirrorItemRepository,
            ISqlSugarRepository<RssWordSegment, long> rssWordSegmentRepository,
            ISqlSugarReadOnlyRepository<RssSubscription, long> rssSubscriptionRepository,
            IWordSegmentService wordSegmentService,
            ISqlSugarClient db,
            IRssSubscriptionService rssSubscriptionService,
            ILogger<RssMirrorFetchJob> logger)
        {
            _rssSourceRepository = rssSourceRepository;
            _rssMirrorItemRepository = rssMirrorItemRepository;
            _rssWordSegmentRepository = rssWordSegmentRepository;
            _rssSubscriptionRepository = rssSubscriptionRepository;
            _wordSegmentService = wordSegmentService;
            _db = db;
            _rssSubscriptionService = rssSubscriptionService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("开始执行RSS镜像抓取任务");

            try
            {
                var enabledSources = await _rssSourceRepository.GetListAsync(s => s.IsEnabled);

                if (!enabledSources.Any())
                {
                    _logger.LogInformation("没有启用的RSS源");
                    return;
                }

                _logger.LogInformation("找到 {Count} 个启用的RSS源", enabledSources.Count);

                foreach (var source in enabledSources)
                {
                    await FetchRssSource(source);
                }

                _logger.LogInformation("RSS镜像抓取任务完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RSS镜像抓取任务执行失败");
            }
        }

        /// <summary>
        /// 抓取单个RSS源的数据
        /// </summary>
        private async Task FetchRssSource(RssSource source)
        {
            _logger.LogInformation("开始抓取RSS源: {Name} ({Url})", source.Name, source.Url);

            try
            {
                _db.Ado.BeginTran();
                try
                {
                    // 创建HttpClient并配置代理
                    using var handler = new HttpClientHandler();
                    if (!string.IsNullOrWhiteSpace(source.ProxyUrl))
                    {
                        handler.Proxy = new WebProxy(source.ProxyUrl);
                        if (!string.IsNullOrWhiteSpace(source.ProxyUsername) &&
                            !string.IsNullOrWhiteSpace(source.ProxyPassword))
                        {
                            handler.Proxy.Credentials = new NetworkCredential(
                                source.ProxyUsername,
                                source.ProxyPassword);
                        }
                        handler.UseProxy = true;
                    }

                    using var client = new HttpClient(handler);
                    client.Timeout = TimeSpan.FromSeconds(60);

                    // 构建请求URL
                    string requestUrl = source.Url;
                    if (source.MaxItems > 0)
                    {
                        string separator = requestUrl.Contains("?") ? "&" : "?";
                        requestUrl = $"{requestUrl}{separator}n={source.MaxItems}";
                    }

                    if (!string.IsNullOrWhiteSpace(source.Query))
                    {
                        string separator = requestUrl.Contains("?") ? "&" : "?";
                        requestUrl = $"{requestUrl}{separator}q={Uri.EscapeDataString(source.Query)}";
                    }

                    _logger.LogInformation("请求URL: {RequestUrl}", requestUrl);

                    // 发送HTTP请求获取RSS内容
                    var response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("获取到 {Length} 字符的响应内容", content.Length);

                    // 解析RSS XML
                    var items = ParseRssXml(content, source);

                    // 检查重复，准备新条目
                    int newItemCount = 0;
                    var newItems = new List<RssMirrorItem>();

                    foreach (var item in items)
                    {
                        var existing = await _rssMirrorItemRepository.GetFirstOrDefaultAsync(i => i.Link == item.Link);
                        if (existing == null)
                        {
                            item.RssSourceId = source.Id;
                            item.CreationTime = DateTime.Now;
                            newItems.Add(item);
                        }
                    }

                    // 逐个插入镜像条目，确保自增 Id 回填
                    foreach (var item in newItems)
                    {
                        item.Id = await _rssMirrorItemRepository.InsertReturnIdAsync(item);
                    }

                    // 插入分词数据
                    foreach (var item in newItems)
                    {
                        var wordSegments = _wordSegmentService.Segment(item.Title);
                        var segmentDict = _wordSegmentService.SegmentAndCount(item.Title);

                        foreach (var segment in wordSegments)
                        {
                            var rssWordSegment = new RssWordSegment
                            {
                                RssMirrorItemId = item.Id,
                                Word = segment.Word,
                                LanguageType = segment.LanguageType,
                                Count = segmentDict.TryGetValue(segment.Word.ToLower(), out var count) ? count : 1,
                                CreationTime = DateTime.Now
                            };
                            await _rssWordSegmentRepository.InsertAsync(rssWordSegment);
                        }

                        newItemCount++;
                    }

                    // 更新RSS源抓取状态
                    var currentSource = await _rssSourceRepository.GetByIdAsync(source.Id);
                    currentSource.LastFetchTime = DateTime.Now;
                    currentSource.FetchStatus = 1; // 成功
                    currentSource.ErrorMessage = null;
                    await _rssSourceRepository.UpdateAsync(currentSource);

                    _db.Ado.CommitTran();

                    _logger.LogInformation("RSS源 {Name} 抓取完成，新增 {Count} 条记录", source.Name, newItemCount);

                    // 提交事务后处理订阅
                    await ProcessSubscriptionsAsync(newItems);
                }
                catch
                {
                    _db.Ado.RollbackTran();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "抓取RSS源 {Name} 失败: {Message}", source.Name, ex.Message);

                // 在独立操作中更新RSS源的失败状态
                try
                {
                    var currentSource = await _rssSourceRepository.GetByIdAsync(source.Id);
                    currentSource.FetchStatus = 2; // 失败
                    currentSource.ErrorMessage = ex.Message;
                    await _rssSourceRepository.UpdateAsync(currentSource);
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "更新RSS源状态失败");
                }
            }
        }

        /// <summary>
        /// 解析RSS XML内容为镜像条目列表
        /// </summary>
        private List<RssMirrorItem> ParseRssXml(string xmlContent, RssSource source)
        {
            var items = new List<RssMirrorItem>();

            try
            {
                var doc = XDocument.Parse(xmlContent);
                var channel = doc.Root?.Element("channel");
                if (channel == null)
                {
                    _logger.LogWarning("未找到RSS channel元素");
                    return items;
                }

                // 获取nyaa命名空间
                XNamespace nyaaNs = doc.Root?.GetNamespaceOfPrefix("nyaa") ?? "http://www.nyaa.info/xmlns/nyaa";

                var itemElements = channel.Elements("item").Take(source.MaxItems > 0 ? source.MaxItems : int.MaxValue);

                foreach (var itemElement in itemElements)
                {
                    var item = new RssMirrorItem
                    {
                        Title = itemElement.Element("title")?.Value ?? string.Empty,
                        Link = itemElement.Element("link")?.Value ?? string.Empty,
                        Description = itemElement.Element("description")?.Value,
                        Author = itemElement.Element("author")?.Value,
                        Category = itemElement.Element("category")?.Value,
                        CreationTime = DateTime.Now,
                        IsDownloaded = false,
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    // 解析发布时间
                    var pubDateStr = itemElement.Element("pubDate")?.Value;
                    if (!string.IsNullOrEmpty(pubDateStr) && DateTimeOffset.TryParse(pubDateStr, out var pubDate))
                    {
                        item.PublishDate = pubDate;
                    }

                    // 解析nyaa命名空间的种子信息
                    var seedersElement = itemElement.Element(nyaaNs + "seeders");
                    if (seedersElement != null && int.TryParse(seedersElement.Value, out var seeders))
                    {
                        item.Seeders = seeders;
                    }

                    var leechersElement = itemElement.Element(nyaaNs + "leechers");
                    if (leechersElement != null && int.TryParse(leechersElement.Value, out var leechers))
                    {
                        item.Leechers = leechers;
                    }

                    var downloadsElement = itemElement.Element(nyaaNs + "downloads");
                    if (downloadsElement != null && int.TryParse(downloadsElement.Value, out var downloads))
                    {
                        item.Downloads = downloads;
                    }

                    // 收集非标准RSS扩展字段
                    var extensions = new Dictionary<string, string>();
                    foreach (var element in itemElement.Elements())
                    {
                        var name = element.Name.LocalName;
                        if (!StandardRssElements.Contains(name))
                        {
                            extensions[name] = element.Value;
                        }
                    }

                    if (extensions.Any())
                    {
                        item.Extensions = JsonSerializer.Serialize(extensions);
                    }

                    items.Add(item);
                }

                _logger.LogInformation("解析到 {Count} 条RSS条目", items.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析RSS XML失败");
                throw;
            }

            return items;
        }

        /// <summary>
        /// 处理订阅匹配，对新增条目进行关键词匹配和自动下载
        /// </summary>
        private async Task ProcessSubscriptionsAsync(List<RssMirrorItem> newItems)
        {
            var enabledSubscriptions = await _rssSubscriptionRepository.GetListAsync(s => s.IsEnabled);

            if (!enabledSubscriptions.Any())
            {
                return;
            }

            foreach (var item in newItems)
            {
                var matchResults = await _rssSubscriptionService.MatchSubscriptionsAsync(item);

                foreach (var matchResult in matchResults.Where(r => r.Matched))
                {
                    var subscription = enabledSubscriptions.FirstOrDefault(s => s.Id == matchResult.SubscriptionId);
                    if (subscription != null && subscription.AutoDownload)
                    {
                        await _rssSubscriptionService.CreateDownloadTaskAsync(matchResult.SubscriptionId, item.Id);
                        _logger.LogInformation("订阅 {SubscriptionName} 匹配并自动下载: {Title}",
                            subscription.Name, item.Title);
                    }
                }
            }
        }
    }
}
