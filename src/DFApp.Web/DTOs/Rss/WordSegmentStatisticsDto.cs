namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// 分词统计DTO
    /// </summary>
    public class WordSegmentStatisticsDto
    {
        /// <summary>
        /// 分词文本
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// 总出现次数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 包含该分词的镜像条目数量
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// 语言类型
        /// </summary>
        public int LanguageType { get; set; }
    }
}
