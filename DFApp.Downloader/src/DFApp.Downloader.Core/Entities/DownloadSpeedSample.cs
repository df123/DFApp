using SqlSugar;

namespace DFApp.Downloader.Core.Entities;

/// <summary>
/// 全局下载速度采样记录（每分钟一条，仅在有活跃下载时写入，空闲期视为 0 速度）
/// </summary>
[SugarTable("DownloadSpeedSamples")]
[SugarIndex("IX_DownloadSpeedSamples_RecordedAt", nameof(RecordedAt), OrderByType.Asc)]
public class DownloadSpeedSample
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>采样时间（UTC）</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>采样时刻的全局总下载速度（字节/秒）</summary>
    public double SpeedBytesPerSecond { get; set; }
}
