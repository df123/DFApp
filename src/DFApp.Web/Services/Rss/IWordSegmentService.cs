using System.Collections.Generic;

namespace DFApp.Web.Services.Rss
{
    /// <summary>
    /// 分词服务接口 - 支持中文、英文、日文及混合语言的分词处理
    /// </summary>
    public interface IWordSegmentService
    {
        /// <summary>
        /// 对文本进行分词
        /// </summary>
        /// <param name="text">要分词的文本</param>
        /// <returns>分词结果列表</returns>
        List<WordSegmentResult> Segment(string text);

        /// <summary>
        /// 对文本进行分词并统计词频
        /// </summary>
        /// <param name="text">要分词的文本</param>
        /// <returns>词语及其出现次数的字典</returns>
        Dictionary<string, int> SegmentAndCount(string text);
    }
}
