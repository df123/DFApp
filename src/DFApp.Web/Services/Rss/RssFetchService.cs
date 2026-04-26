using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Rss;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Rss;

/// <summary>
/// RSS Feed 获取服务
/// </summary>
public class RssFetchService : AppServiceBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RssFetchService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="httpClientFactory">HTTP客户端工厂</param>
    /// <param name="logger">日志记录器</param>
    public RssFetchService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        IHttpClientFactory httpClientFactory,
        ILogger<RssFetchService> logger)
        : base(currentUser, permissionChecker)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取RSS Feed内容
    /// </summary>
    /// <param name="input">请求参数</param>
    /// <returns>获取结果</returns>
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
                _logger.LogInformation("使用代理: {ProxyUrl}", input.ProxyUrl);

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
                    _logger.LogInformation("使用代理认证: {ProxyUsername}", input.ProxyUsername);
                }

                handler.Proxy = proxy;
                handler.UseProxy = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置代理失败: {Message}", ex.Message);
                response.Success = false;
                response.Message = $"配置代理失败: {ex.Message}";
                return response;
            }
        }

        try
        {
            _logger.LogInformation("开始获取RSS Feed - URL: {Url}, 最大条目数: {MaxItems}",
                input.Url, input.MaxItems);
            if (!string.IsNullOrWhiteSpace(input.Query))
            {
                _logger.LogInformation("搜索关键词: {Query}", input.Query);
            }

            string requestUrl = input.Url;

            // 如果URL中没有任何条目数参数，尝试添加
            bool hasItemCountParam = requestUrl.Contains("&n=") || requestUrl.Contains("?n=") ||
                                    requestUrl.Contains("&limit=") || requestUrl.Contains("?limit=") ||
                                    requestUrl.Contains("&count=") || requestUrl.Contains("?count=");

            if (!hasItemCountParam && input.MaxItems > 0)
            {
                string separator = requestUrl.Contains("?") ? "&" : "?";
                requestUrl = $"{requestUrl}{separator}n={input.MaxItems}";
                _logger.LogInformation("自动添加条目数参数到URL");
            }

            // 添加搜索关键词参数
            if (!string.IsNullOrWhiteSpace(input.Query))
            {
                string separator = requestUrl.Contains("?") ? "&" : "?";
                requestUrl = $"{requestUrl}{separator}q={Uri.EscapeDataString(input.Query)}";
                _logger.LogInformation("添加搜索参数到URL: q={Query}", input.Query);
            }

            response.RequestUrl = requestUrl;

            _logger.LogInformation("请求URL: {RequestUrl}", requestUrl);

            // 创建HTTP客户端
            using var client = new HttpClient(handler);

            _logger.LogInformation("发送HTTP请求...");

            // 发送请求
            var httpResponse = await client.GetAsync(requestUrl);
            response.StatusCode = (int)httpResponse.StatusCode;

            _logger.LogInformation("HTTP响应状态码: {StatusCode}", response.StatusCode);

            // 确保请求成功
            httpResponse.EnsureSuccessStatusCode();

            // 读取响应内容
            string responseContent = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("响应内容长度: {Length} 字符", responseContent.Length);

            // 记录响应内容（仅前500字符，避免日志过长）
            if (responseContent.Length > 500)
            {
                _logger.LogInformation("响应内容前500字符: {Content}...", responseContent.Substring(0, 500));
            }
            else
            {
                _logger.LogInformation("响应内容: {Content}", responseContent);
            }

            response.RawContent = responseContent;

            // 解析RSS XML
            var items = ParseRssXml(responseContent, input.MaxItems);

            response.Items = items;
            response.TotalCount = items.Count;
            response.Success = true;
            response.Message = $"成功获取到 {items.Count} 条RSS条目";

            _logger.LogInformation("解析到 {Count} 条RSS条目", items.Count);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP请求异常: {Message}", ex.Message);
            response.Success = false;
            response.Message = $"HTTP请求异常: {ex.Message}";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "请求超时: {Message}", ex.Message);
            response.Success = false;
            response.Message = $"请求超时: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析RSS XML异常: {Message}", ex.Message);
            response.Success = false;
            response.Message = $"解析RSS XML异常: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation("请求完成，耗时: {ResponseTime} 毫秒", response.ResponseTime);
        }

        return response;
    }

    /// <summary>
    /// 解析RSS XML内容
    /// </summary>
    /// <param name="xmlContent">XML内容</param>
    /// <param name="maxItems">最大条目数</param>
    /// <returns>RSS条目列表</returns>
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
                _logger.LogWarning("未找到RSS channel元素，可能是Atom格式或无效RSS");
                return items;
            }

            // 获取nyaa命名空间
            XNamespace nyaaNs = doc.Root?.GetNamespaceOfPrefix("nyaa") ?? "http://www.nyaa.info/xmlns/nyaa";

            // 先获取所有的item元素
            var allItemElements = channel.Elements("item").ToList();
            _logger.LogInformation("RSS源返回了 {Count} 个条目，请求的maxItems={MaxItems}",
                allItemElements.Count, maxItems);

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
            _logger.LogError(ex, "解析RSS XML时发生异常");
            throw;
        }

        return items;
    }

    /// <summary>
    /// 判断是否为标准RSS元素
    /// </summary>
    /// <param name="elementName">元素名称</param>
    /// <returns>是否为标准元素</returns>
    private static bool IsStandardRssElement(string elementName)
    {
        var standardElements = new HashSet<string> { "title", "link", "description", "pubDate", "author", "category", "guid", "comments" };
        return standardElements.Contains(elementName);
    }
}
