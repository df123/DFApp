using System.Collections.Generic;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Aria2.Response.TellStatus
{
    /// <summary>
    /// Aria2 文件项实体
    /// </summary>
    [SugarTable("AppAria2FilesItem")]
    public class FilesItem : CreationAuditedEntity<int>
    {
        /// <summary>
        /// 已完成长度
        /// </summary>
        public long? CompletedLength { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public long? Index { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public long? Length { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool? Selected { get; set; }

        /// <summary>
        /// URI列表（导航属性，不映射到数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<UrisItem>? Uris { get; set; }

        /// <summary>
        /// TellStatus结果（导航属性，不映射到数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public TellStatusResult Result { get; set; } = null!;

        /// <summary>
        /// TellStatus结果ID
        /// </summary>
        public long ResultId { get; set; }
    }
}
