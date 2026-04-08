namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS获取请求DTO
    /// </summary>
    public class RssFetchRequestDto
    {
        /// <summary>
        /// RSS Feed URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 最大条目数（0表示无限制）
        /// </summary>
        public int MaxItems { get; set; } = 50;

        /// <summary>
        /// 搜索关键词（可选）
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 代理服务器地址（例如：http://proxy.example.com:8080 或 socks5://proxy.example.com:1080）
        /// </summary>
        public string? ProxyUrl { get; set; }

        /// <summary>
        /// 代理用户名（可选）
        /// </summary>
        public string? ProxyUsername { get; set; }

        /// <summary>
        /// 代理密码（可选）
        /// </summary>
        public string? ProxyPassword { get; set; }
    }
}
