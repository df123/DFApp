using System.Collections.Generic;

namespace DFApp.Rss
{
    /// <summary>
    /// 分词服务接口
    /// </summary>
    public interface IWordSegmentService
    {
        /// <summary>
        /// 对文本进行分词
        /// </summary>
        /// <param name="text">要分词的文本</param>
        /// <returns>分词结果</returns>
        List<WordSegmentResult> Segment(string text);

        /// <summary>
        /// 对文本进行分词并统计
        /// </summary>
        /// <param name="text">要分词的文本</param>
        /// <returns>分词统计结果</returns>
        Dictionary<string, int> SegmentAndCount(string text);
    }

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

        /// <summary>
        /// 词性（可选）
        /// </summary>
        public string? PartOfSpeech { get; set; }
    }
}
