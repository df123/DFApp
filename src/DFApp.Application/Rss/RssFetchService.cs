using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp;

namespace DFApp.Rss
{
    [Authorize(DFAppPermissions.Rss.Default)]
    public class RssFetchService : DFAppAppService, IRssFetchService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RssFetchService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<RssFetchResponseDto> FetchRssFeed(RssFetchRequestDto input)
        {
            var response = new RssFetchResponseDto();
            var stopwatch = Stopwatch.StartNew();

            // 创建HttpClientHandler以配置代理
            var handler = new HttpClientHandler();

            // 配置代理
            if (!string.IsNullOrWhiteSpace(input.ProxyUrl))
            {
                try
                {
                    Logger.LogInformation($"使用代理: {input.ProxyUrl}");

                    // 解析代理URL
                    var proxyUri = new Uri(input.ProxyUrl);
                    var proxy = new WebProxy
                    {
                        Address = proxyUri,
                        BypassProxyOnLocal = false
                    };

                    // 设置代理认证
                    if (!string.IsNullOrWhiteSpace(input.ProxyUsername) &&
                        !string.IsNullOrWhiteSpace(input.ProxyPassword))
                    {
                        proxy.Credentials = new NetworkCredential(
                            input.ProxyUsername,
                            input.ProxyPassword
                        );
                        Logger.LogInformation($"使用代理认证: {input.ProxyUsername}");
                    }

                    handler.Proxy = proxy;
                    handler.UseProxy = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"配置代理失败: {ex.Message}");
                    response.Success = false;
                    response.Message = $"配置代理失败: {ex.Message}";
                    return response;
                }
            }

            try
            {
                Logger.LogInformation($"开始获取RSS Feed - URL: {input.Url}, 最大条目数: {input.MaxItems}");
                if (!string.IsNullOrWhiteSpace(input.Query))
                {
                    Logger.LogInformation($"搜索关键词: {input.Query}");
                }

                string requestUrl = input.Url;

                // 如果URL中没有任何条目数参数，尝试添加
                // Nyaa.si可能使用的参数名：n, limit, count
                bool hasItemCountParam = requestUrl.Contains("&n=") || requestUrl.Contains("?n=") ||
                                        requestUrl.Contains("&limit=") || requestUrl.Contains("?limit=") ||
                                        requestUrl.Contains("&count=") || requestUrl.Contains("?count=");

                if (!hasItemCountParam && input.MaxItems > 0)
                {
                    string separator = requestUrl.Contains("?") ? "&" : "?";
                    // 对于Nyaa.si，使用n参数
                    requestUrl = $"{requestUrl}{separator}n={input.MaxItems}";
                    Logger.LogInformation($"自动添加条目数参数到URL");
                }

                // 添加搜索关键词参数
                if (!string.IsNullOrWhiteSpace(input.Query))
                {
                    string separator = requestUrl.Contains("?") ? "&" : "?";
                    requestUrl = $"{requestUrl}{separator}q={Uri.EscapeDataString(input.Query)}";
                    Logger.LogInformation($"添加搜索参数到URL: q={input.Query}");
                }

                response.RequestUrl = requestUrl;

                Logger.LogInformation($"请求URL: {requestUrl}");

                // 创建HTTP客户端
                using var client = new HttpClient(handler);

                Logger.LogInformation("发送HTTP请求...");

                // 发送请求
                var httpResponse = await client.GetAsync(requestUrl);
                response.StatusCode = (int)httpResponse.StatusCode;

                Logger.LogInformation($"HTTP响应状态码: {response.StatusCode}");

                // 确保请求成功
                httpResponse.EnsureSuccessStatusCode();

                // 读取响应内容
                string responseContent = await httpResponse.Content.ReadAsStringAsync();
                Logger.LogInformation($"响应内容长度: {responseContent.Length} 字符");

                // 记录响应内容（仅前500字符，避免日志过长）
                if (responseContent.Length > 500)
                {
                    Logger.LogInformation($"响应内容前500字符: {responseContent.Substring(0, 500)}...");
                }
                else
                {
                    Logger.LogInformation($"响应内容: {responseContent}");
                }

                response.RawContent = responseContent;

                // 解析RSS XML
                var items = ParseRssXml(responseContent, input.MaxItems);

                response.Items = items;
                response.TotalCount = items.Count;
                response.Success = true;
                response.Message = $"成功获取到 {items.Count} 条RSS条目";

                Logger.LogInformation($"解析到 {items.Count} 条RSS条目");
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, $"HTTP请求异常: {ex.Message}");
                response.Success = false;
                response.Message = $"HTTP请求异常: {ex.Message}";
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogError(ex, $"请求超时: {ex.Message}");
                response.Success = false;
                response.Message = $"请求超时: {ex.Message}";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"解析RSS XML异常: {ex.Message}");
                response.Success = false;
                response.Message = $"解析RSS XML异常: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                response.ResponseTime = stopwatch.ElapsedMilliseconds;
                Logger.LogInformation($"请求完成，耗时: {response.ResponseTime} 毫秒");
            }

            return response;
        }

        private List<RssItemDto> ParseRssXml(string xmlContent, int maxItems)
        {
            var items = new List<RssItemDto>();

            try
            {
                var doc = XDocument.Parse(xmlContent);
                var channel = doc.Root?.Element("channel");
                if (channel == null)
                {
                    // 可能是Atom格式，暂不支持
                    Logger.LogWarning("未找到RSS channel元素，可能是Atom格式或无效RSS");
                    return items;
                }

                // 获取nyaa命名空间
                XNamespace nyaaNs = doc.Root?.GetNamespaceOfPrefix("nyaa") ?? "http://www.nyaa.info/xmlns/nyaa";

                // 先获取所有的item元素
                var allItemElements = channel.Elements("item").ToList();
                Logger.LogInformation($"RSS源返回了 {allItemElements.Count} 个条目，请求的maxItems={maxItems}");

                // 限制数量
                var itemElements = allItemElements.Take(maxItems > 0 ? maxItems : int.MaxValue);

                foreach (var itemElement in itemElements)
                {
                    var item = new RssItemDto
                    {
                        Title = itemElement.Element("title")?.Value ?? string.Empty,
                        Link = itemElement.Element("link")?.Value ?? string.Empty,
                        Description = itemElement.Element("description")?.Value ?? string.Empty,
                        Author = itemElement.Element("author")?.Value ?? string.Empty,
                        Category = itemElement.Element("category")?.Value ?? string.Empty
                    };

                    // 解析发布时间
                    var pubDateStr = itemElement.Element("pubDate")?.Value;
                    if (!string.IsNullOrEmpty(pubDateStr))
                    {
                        if (DateTimeOffset.TryParse(pubDateStr, out var pubDate))
                        {
                            item.PublishDate = pubDate;
                        }
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

                    // 处理扩展字段（如种子信息）
                    var extensions = new Dictionary<string, string>();
                    foreach (var element in itemElement.Elements())
                    {
                        var name = element.Name.LocalName;
                        if (!IsStandardRssElement(name))
                        {
                            extensions[name] = element.Value;
                        }
                    }
                    item.Extensions = extensions;

                    items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "解析RSS XML时发生异常");
                throw;
            }

            return items;
        }

        private bool IsStandardRssElement(string elementName)
        {
            var standardElements = new HashSet<string> { "title", "link", "description", "pubDate", "author", "category", "guid", "comments" };
            return standardElements.Contains(elementName);
        }
    }
}