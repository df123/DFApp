using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DFApp.Web.Services.Media;

/// <summary>
/// 下载器取回保护：记录已通知下载器取回（或补漏同步已下发）但尚未确认取回的媒体。
/// 空间清理时跳过处于保护期的媒体，避免下载过程中源文件被清理删除导致下载失败。
/// 内存态即可：媒体最终由下载器确认取回（IsExternalLinkGenerated=true）或超过保护时长后自然失效。
/// </summary>
public class MediaRetrievalTracker
{
    /// <summary>保护时长：超过该时长仍未确认取回则视为下载器不再取回，允许清理</summary>
    private static readonly TimeSpan ProtectionTimeout = TimeSpan.FromHours(6);

    private readonly ConcurrentDictionary<long, DateTime> _pending = new();

    /// <summary>
    /// 标记媒体进入取回保护期（刷新保护起始时间）
    /// </summary>
    public void MarkPending(long id)
    {
        _pending[id] = DateTime.Now;
    }

    /// <summary>
    /// 清除保护（下载器确认取回后调用）
    /// </summary>
    public void ClearPending(long id)
    {
        _pending.TryRemove(id, out _);
    }

    /// <summary>
    /// 是否处于取回保护期（已标记且未超过保护时长）
    /// </summary>
    public bool IsProtected(long id)
    {
        if (!_pending.TryGetValue(id, out var since))
        {
            return false;
        }

        if (DateTime.Now - since > ProtectionTimeout)
        {
            _pending.TryRemove(id, out _);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 把一批媒体全部纳入取回保护（服务启动时重建保护，避免后端重启后清理误删未取回媒体）
    /// </summary>
    public void ProtectAll(IEnumerable<long> ids)
    {
        var now = DateTime.Now;
        foreach (var id in ids)
        {
            _pending[id] = now;
        }
    }
}
