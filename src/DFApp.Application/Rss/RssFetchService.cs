using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            try
            {
                Logger.LogInformation($"开始获取RSS Feed - URL: {input.Url}, 最大条目数: {input.MaxItems}");

                string requestUrl = input.Url;
                response.RequestUrl = requestUrl;

                Logger.LogInformation($"请求URL: {requestUrl}");

                // 创建HTTP客户端
                using var client = _httpClientFactory.CreateClient();

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

        public async Task<RssFetchResponseDto> TestRssFeedConnection(string url)
        {
            Logger.LogInformation($"测试RSS Feed连接 - URL: {url}");

            var request = new RssFetchRequestDto
            {
                Url = url,
                MaxItems = 1 // 测试时只获取1条
            };

            return await FetchRssFeed(request);
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

                var itemElements = channel.Elements("item").Take(maxItems > 0 ? maxItems : int.MaxValue);

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