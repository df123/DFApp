using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Aria2.Response.TellStatus
{
    /// <summary>
    /// Aria2 URI项实体
    /// </summary>
    [SugarTable("UrisItems")]
    public class UrisItem : CreationAuditedEntity<short>
    {
        /// <summary>
        /// 状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// URI
        /// </summary>
        public string? Uri { get; set; }

        /// <summary>
        /// 文件项（导航属性，不映射到数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public FilesItem FilesItem { get; set; } = null!;

        /// <summary>
        /// 文件项ID
        /// </summary>
        public int FilesItemId { get; set; }
    }
}
