using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS分词DTO
    /// </summary>
    public class RssWordSegmentDto : EntityDto<long>
    {
        /// <summary>
        /// RSS镜像条目ID
        /// </summary>
        public long RssMirrorItemId { get; set; }

        /// <summary>
        /// 分词文本
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// 语言类型（0=中文，1=英文，2=日文）
        /// </summary>
        public int LanguageType { get; set; }

        /// <summary>
        /// 出现次数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 词性
        /// </summary>
        public string? PartOfSpeech { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
