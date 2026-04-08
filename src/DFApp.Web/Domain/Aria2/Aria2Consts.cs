using System;

namespace DFApp.Aria2;

/// <summary>
/// Aria2 RPC 方法常量
/// </summary>
public static class Aria2Consts
{
    public const string JSONRPC = "2.0";

    /// <summary>
    /// 通知事件：下载开始
    /// </summary>
    public const string OnDownloadStart = "aria2.onDownloadStart";

    /// <summary>
    /// 通知事件：下载暂停
    /// </summary>
    public const string OnDownloadPause = "aria2.onDownloadPause";

    /// <summary>
    /// 通知事件：下载停止
    /// </summary>
    public const string OnDownloadStop = "aria2.onDownloadStop";

    /// <summary>
    /// 通知事件：下载完成
    /// </summary>
    public const string OnDownloadComplete = "aria2.onDownloadComplete";

    /// <summary>
    /// 通知事件：下载错误
    /// </summary>
    public const string OnDownloadError = "aria2.onDownloadError";

    /// <summary>
    /// 通知事件：BT 下载完成
    /// </summary>
    public const string OnBtDownloadComplete = "aria2.onBtDownloadComplete";

    /// <summary>
    /// 添加 URI 下载
    /// </summary>
    public const string AddUri = "aria2.addUri";

    /// <summary>
    /// 添加种子下载
    /// </summary>
    public const string AddTorrent = "aria2.addTorrent";

    /// <summary>
    /// 添加 Metalink 下载
    /// </summary>
    public const string AddMetalink = "aria2.addMetalink";

    /// <summary>
    /// 移除任务
    /// </summary>
    public const string Remove = "aria2.remove";

    /// <summary>
    /// 强制移除任务
    /// </summary>
    public const string ForceRemove = "aria2.forceRemove";

    /// <summary>
    /// 暂停任务
    /// </summary>
    public const string Pause = "aria2.pause";

    /// <summary>
    /// 暂停所有任务
    /// </summary>
    public const string PauseAll = "aria2.pauseAll";

    /// <summary>
    /// 强制暂停任务
    /// </summary>
    public const string ForcePause = "aria2.forcePause";

    /// <summary>
    /// 强制暂停所有任务
    /// </summary>
    public const string ForcePauseAll = "aria2.forcePauseAll";

    /// <summary>
    /// 恢复任务
    /// </summary>
    public const string Unpause = "aria2.unpause";

    /// <summary>
    /// 恢复所有任务
    /// </summary>
    public const string UnpauseAll = "aria2.unpauseAll";

    /// <summary>
    /// 获取任务状态
    /// </summary>
    public const string TellStatus = "aria2.tellStatus";

    /// <summary>
    /// 获取 URI 列表
    /// </summary>
    public const string GetUris = "aria2.getUris";

    /// <summary>
    /// 获取文件列表
    /// </summary>
    public const string GetFiles = "aria2.getFiles";

    /// <summary>
    /// 获取 Peer 列表
    /// </summary>
    public const string GetPeers = "aria2.getPeers";

    /// <summary>
    /// 获取服务器列表
    /// </summary>
    public const string GetServers = "aria2.getServers";

    /// <summary>
    /// 获取活跃任务
    /// </summary>
    public const string TellActive = "aria2.tellActive";

    /// <summary>
    /// 获取等待任务
    /// </summary>
    public const string TellWaiting = "aria2.tellWaiting";

    /// <summary>
    /// 获取停止任务
    /// </summary>
    public const string TellStopped = "aria2.tellStopped";

    /// <summary>
    /// 修改任务位置
    /// </summary>
    public const string ChangePosition = "aria2.changePosition";

    /// <summary>
    /// 修改 URI
    /// </summary>
    public const string ChangeUri = "aria2.changeUri";

    /// <summary>
    /// 获取任务选项
    /// </summary>
    public const string GetOption = "aria2.getOption";

    /// <summary>
    /// 修改任务选项
    /// </summary>
    public const string ChangeOption = "aria2.changeOption";

    /// <summary>
    /// 获取全局选项
    /// </summary>
    public const string GetGlobalOption = "aria2.getGlobalOption";

    /// <summary>
    /// 修改全局选项
    /// </summary>
    public const string ChangeGlobalOption = "aria2.changeGlobalOption";

    /// <summary>
    /// 获取全局统计
    /// </summary>
    public const string GetGlobalStat = "aria2.getGlobalStat";

    /// <summary>
    /// 清空停止的任务记录
    /// </summary>
    public const string PurgeDownloadResult = "aria2.purgeDownloadResult";

    /// <summary>
    /// 移除停止的任务记录
    /// </summary>
    public const string RemoveDownloadResult = "aria2.removeDownloadResult";

    /// <summary>
    /// 获取版本信息
    /// </summary>
    public const string GetVersion = "aria2.getVersion";

    /// <summary>
    /// 获取会话信息
    /// </summary>
    public const string GetSessionInfo = "aria2.getSessionInfo";

    /// <summary>
    /// 关闭 Aria2
    /// </summary>
    public const string Shutdown = "aria2.shutdown";

    /// <summary>
    /// 强制关闭 Aria2
    /// </summary>
    public const string ForceShutdown = "aria2.forceShutdown";

    /// <summary>
    /// 保存会话
    /// </summary>
    public const string SaveSession = "aria2.saveSession";

    /// <summary>
    /// 批量调用
    /// </summary>
    public const string MultiCall = "system.multicall";

    /// <summary>
    /// 列出所有方法
    /// </summary>
    public const string ListMethods = "system.listMethods";

    /// <summary>
    /// 列出所有通知
    /// </summary>
    public const string ListNotifications = "system.listNotifications";

    private static string? _aria2RequestId;

    /// <summary>
    /// Aria2 请求 ID
    /// </summary>
    public static string Aria2RequestId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_aria2RequestId))
            {
                _aria2RequestId = Guid.NewGuid().ToString();
            }
            return _aria2RequestId;
        }
    }
}
