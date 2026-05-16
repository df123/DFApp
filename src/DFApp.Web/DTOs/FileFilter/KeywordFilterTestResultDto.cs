using System.Collections.Generic;
using DFApp.FileFilter;

namespace DFApp.Web.DTOs.FileFilter
{
    /// <summary>
    /// 关键词过滤测试结果 DTO
    /// </summary>
    public class KeywordFilterTestResultDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public KeywordFilterTestResultDto()
        {
            MatchingRules = new List<KeywordFilterMatchResultDto>();
        }

        /// <summary>
        /// 测试的文件名
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 是否应该过滤该文件
        /// </summary>
        public bool ShouldFilter { get; set; }

        /// <summary>
        /// 过滤原因说明
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 匹配到的规则列表
        /// </summary>
        public List<KeywordFilterMatchResultDto> MatchingRules { get; set; }
    }
}
