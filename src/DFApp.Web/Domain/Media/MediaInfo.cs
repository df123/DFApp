using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Media
{
    /// <summary>
    /// 媒体信息
    /// </summary>
    [SugarTable("MediaInfos")]
    public class MediaInfo : AuditedEntity<long>
    {
        /// <summary>
        /// 媒体ID
        /// </summary>
        public long MediaId { get; set; }

        /// <summary>
        /// 聊天ID
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// 聊天标题
        /// </summary>
        public string ChatTitle { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 保存路径
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// MIME类型
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// 是否已生成外链
        /// </summary>
        public bool IsExternalLinkGenerated { get; set; }

        /// <summary>
        /// 是否下载完成
        /// </summary>
        public bool IsDownloadCompleted { get; set; }

        /// <summary>
        /// 下载耗时（毫秒）
        /// </summary>
        public long DownloadTimeMs { get; set; }

        /// <summary>
        /// 下载速度（字节/秒）
        /// </summary>
        public double DownloadSpeedBps { get; set; }
    }
}
