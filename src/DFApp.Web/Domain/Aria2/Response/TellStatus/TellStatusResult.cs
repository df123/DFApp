using System.Collections.Generic;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Aria2.Response.TellStatus
{
    /// <summary>
    /// Aria2 TellStatus 结果实体
    /// </summary>
    [SugarTable("AppAria2TellStatusResult")]
    public class TellStatusResult : CreationAuditedEntity<long>
    {
        /// <summary>
        /// 位字段
        /// </summary>
        public string? Bitfield { get; set; }

        /// <summary>
        /// 已完成长度
        /// </summary>
        public long? CompletedLength { get; set; }

        /// <summary>
        /// 连接数
        /// </summary>
        public long? Connections { get; set; }

        /// <summary>
        /// 下载目录
        /// </summary>
        public string? Dir { get; set; }

        /// <summary>
        /// 下载速度
        /// </summary>
        public long? DownloadSpeed { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 文件列表（导航属性，不映射到数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<FilesItem>? Files { get; set; }

        /// <summary>
        /// GID
        /// </summary>
        public string? GID { get; set; }

        /// <summary>
        /// 分片数量
        /// </summary>
        public long? NumPieces { get; set; }

        /// <summary>
        /// 分片长度
        /// </summary>
        public long? PieceLength { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 总长度
        /// </summary>
        public long? TotalLength { get; set; }

        /// <summary>
        /// 上传长度
        /// </summary>
        public long? UploadLength { get; set; }

        /// <summary>
        /// 上传速度
        /// </summary>
        public long? UploadSpeed { get; set; }
    }
}
