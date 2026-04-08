using DFApp.Media;
using TL;

namespace DFApp.Web.Background;

/// <summary>
/// 媒体下载队列模型
/// </summary>
public class MediaQueueModel
{
    /// <summary>
    /// 媒体信息
    /// </summary>
    public MediaInfo? MediaInfos { get; set; }

    /// <summary>
    /// Telegram 媒体对象（Photo 或 Document）
    /// </summary>
    public IObject? TObject { get; set; }

    /// <summary>
    /// 是否为图片
    /// </summary>
    public bool IsPhoto { get; set; }
}
