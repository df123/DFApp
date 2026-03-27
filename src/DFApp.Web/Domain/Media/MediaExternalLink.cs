using System.Collections.Generic;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Media
{
    /// <summary>
    /// 媒体外链
    /// </summary>
    [SugarTable("MediaExternalLinks")]
    public class MediaExternalLink : AuditedEntity<long>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 耗时
        /// </summary>
        public long TimeConsumed { get; set; }

        /// <summary>
        /// 是否移除
        /// </summary>
        public bool IsRemove { get; set; }

        /// <summary>
        /// 链接内容
        /// </summary>
        public required string LinkContent { get; set; }

        /// <summary>
        /// 媒体ID集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public required ICollection<MediaExternalLinkMediaIds> MediaIds { get; set; }
    }
}
