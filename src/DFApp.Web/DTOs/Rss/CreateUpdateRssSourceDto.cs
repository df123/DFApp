namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// 创建/更新RSS源DTO
    /// </summary>
    public class CreateUpdateRssSourceDto
    {
        /// <summary>
        /// RSS源名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// RSS源URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 抓取间隔（分钟）
        /// </summary>
        public int FetchIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// 最大条目数
        /// </summary>
        public int MaxItems { get; set; } = 50;

        /// <summary>
        /// 查询关键词
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
