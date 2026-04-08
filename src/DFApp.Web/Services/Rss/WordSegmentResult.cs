namespace DFApp.Web.Services.Rss
{
    /// <summary>
    /// 分词结果
    /// </summary>
    public class WordSegmentResult
    {
        /// <summary>
        /// 分词文本
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// 语言类型（0=中文，1=英文，2=日文）
        /// </summary>
        public int LanguageType { get; set; }
    }
}
