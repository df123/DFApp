namespace DFApp.Web.DTOs.Media
{
    /// <summary>
    /// 创建/更新媒体信息 DTO
    /// </summary>
    public class CreateUpdateMediaInfoDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public CreateUpdateMediaInfoDto()
        {
        }

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
        public string ChatTitle { get; set; } = null!;

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
        public string SavePath { get; set; } = null!;

        /// <summary>
        /// MD5
        /// </summary>
        public string? MD5 { get; set; }

        /// <summary>
        /// MIME类型
        /// </summary>
        public string MimeType { get; set; } = null!;

        /// <summary>
        /// 是否已生成外链
        /// </summary>
        public bool IsExternalLinkGenerated { get; set; }
    }
}
