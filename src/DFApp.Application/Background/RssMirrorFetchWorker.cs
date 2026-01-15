using DFApp.Rss;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace DFApp.Background
{
    /// <summary>
    /// RSS镜像抓取Background Worker
    /// </summary>
    public class RssMirrorFetchWorker : QuartzBackgroundWorkerBase
    {
        private readonly IRepository<RssSource, long> _rssSourceRepository;
        private readonly IRepository<RssMirrorItem, long> _rssMirrorItemRepository;
        private readonly IRepository<RssWordSegment, long> _rssWordSegmentRepository;
        private readonly IWordSegmentService _wordSegmentService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IObjectMapper _objectMapper;

        public RssMirrorFetchWorker(
            IRepository<RssSource, long> rssSourceRepository,
            IRepository<RssMirrorItem, long> rssMirrorItemRepository,
            IRepository<RssWordSegment, long> rssWordSegmentRepository,
            IWordSegmentService wordSegmentService,
            IHttpClientFactory httpClientFactory,
            IUnitOfWorkManager unitOfWorkManager,
            IObjectMapper objectMapper)
        {
            _rssSourceRepository = rssSourceRepository;
            _rssMirrorItemRepository = rssMirrorItemRepository;
            _rssWordSegmentRepository = rssWordSegmentRepository;
            _wordSegmentService = wordSegmentService;
            _httpClientFactory = httpClientFactory;
            _unitOfWorkManager = unitOfWorkManager;
            _objectMapper = objectMapper;

            // 设置Job和Trigger
            JobDetail = JobBuilder
                .Create<RssMirrorFetchWorker>()
                .WithIdentity(nameof(RssMirrorFetchWorker))
                .Build();

            // 默认每5分钟执行一次
            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(RssMirrorFetchWorker))
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(5)
                    .RepeatForever())
                .Build();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            Logger.LogInformation("开始执行RSS镜像抓取任务");

            try
            {
                // 获取所有启用的RSS源
                var enabledSources = await _rssSourceRepository.GetListAsync(s => s.IsEnabled);

                if (!enabledSources.Any())
                {
                    Logger.LogInformation("没有启用的RSS源");
                    return;
                }

                Logger.LogInformation("找到 {Count} 个启用的RSS源", enabledSources.Count);

                foreach (var source in enabledSources)
                {
                    await FetchRssSource(source);
                }

                Logger.LogInformation("RSS镜像抓取任务完成");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "RSS镜像抓取任务执行失败");
            }
        }

        /// <summary>
        /// 抓取单个RSS源
        /// </summary>
        private async Task FetchRssSource(RssSource source)
        {
            Logger.LogInformation("开始抓取RSS源: {Name} ({Url})", source.Name, source.Url);

            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    // 创建HttpClient并配置代理
                    using var handler = new HttpClientHandler();
                    if (!string.IsNullOrWhiteSpace(source.ProxyUrl))
                    {
                        handler.Proxy = new System.Net.WebProxy(source.ProxyUrl);
                        if (!string.IsNullOrWhiteSpace(source.ProxyUsername) &&
                            !string.IsNullOrWhiteSpace(source.ProxyPassword))
                        {
                            handler.Proxy.Credentials = new System.Net.NetworkCredential(
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
                        requestUrl = $"{requestUrl}{separator}q={System.Uri.EscapeDataString(source.Query)}";
                    }

                    Logger.LogInformation("请求URL: {RequestUrl}", requestUrl);

                    // 发送HTTP请求
                    var response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    Logger.LogInformation("获取到 {Length} 字符的响应内容", content.Length);

                    // 解析RSS
                    var items = ParseRssXml(content, source);

                    // 保存到数据库
                    int newItemCount = 0;
                    var newItems = new List<RssMirrorItem>();

                    // 第一步：检查并准备新条目
                    foreach (var item in items)
                    {
                        // 检查是否已存在（根据Link判断）
                        var existing = await _rssMirrorItemRepository.FirstOrDefaultAsync(i => i.Link == item.Link);
                        if (existing == null)
                        {
                            // 设置属性
                            item.RssSourceId = source.Id;
                            item.CreationTime = DateTime.Now;
                            newItems.Add(item);
                        }
                    }

                    // 第二步：批量插入镜像条目
                    foreach (var item in newItems)
                    {
                        await _rssMirrorItemRepository.InsertAsync(item);
                    }

                    // 第三步：保存更改以生成ID
                    await unitOfWork.SaveChangesAsync();

                    // 第四步：插入分词数据
                    foreach (var item in newItems)
                    {
                        // 分词处理
                        var wordSegments = _wordSegmentService.Segment(item.Title);
                        var segmentDict = _wordSegmentService.SegmentAndCount(item.Title);

                        // 保存分词
                        foreach (var segment in wordSegments)
                        {
                            var rssWordSegment = new RssWordSegment
                            {
                                RssMirrorItemId = item.Id, // 现在ID已经生成
                                Word = segment.Word,
                                LanguageType = segment.LanguageType,
                                Count = segmentDict.TryGetValue(segment.Word.ToLower(), out var count) ? count : 1,
                                CreationTime = DateTime.Now
                            };
                            await _rssWordSegmentRepository.InsertAsync(rssWordSegment);
                        }

                        newItemCount++;
                    }

                    // 更新RSS源的抓取状态 - 需要重新获取以避免并发问题
                    var currentSource = await _rssSourceRepository.GetAsync(source.Id);
                    currentSource.LastFetchTime = DateTime.Now;
                    currentSource.FetchStatus = 1; // 成功
                    currentSource.ErrorMessage = null;
                    await _rssSourceRepository.UpdateAsync(currentSource);

                    await unitOfWork.CompleteAsync();

                    Logger.LogInformation("RSS源 {Name} 抓取完成，新增 {Count} 条记录", source.Name, newItemCount);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "抓取RSS源 {Name} 失败: {Message}", source.Name, ex.Message);

                // 更新RSS源的失败状态 - 重新获取以避免并发问题
                try
                {
                    var currentSource = await _rssSourceRepository.GetAsync(source.Id);
                    currentSource.FetchStatus = 2; // 失败
                    currentSource.ErrorMessage = ex.Message;
                    await _rssSourceRepository.UpdateAsync(currentSource);
                }
                catch (Exception updateEx)
                {
                    Logger.LogError(updateEx, "更新RSS源状态失败");
                }
            }
        }

        /// <summary>
        /// 解析RSS XML
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
                    Logger.LogWarning("未找到RSS channel元素");
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

                    // 处理扩展字段
                    var extensions = new Dictionary<string, string>();
                    foreach (var element in itemElement.Elements())
                    {
                        var name = element.Name.LocalName;
                        if (!IsStandardRssElement(name))
                        {
                            extensions[name] = element.Value;
                        }
                    }

                    if (extensions.Any())
                    {
                        item.Extensions = System.Text.Json.JsonSerializer.Serialize(extensions);
                    }

                    items.Add(item);
                }

                Logger.LogInformation("解析到 {Count} 条RSS条目", items.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "解析RSS XML失败");
                throw;
            }

            return items;
        }

        private bool IsStandardRssElement(string elementName)
        {
            var standardElements = new HashSet<string>
            {
                "title", "link", "description", "pubDate", "author", "category", "guid", "comments"
            };
            return standardElements.Contains(elementName);
        }
    }
}
