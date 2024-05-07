using System;
using System.Collections.Generic;
using System.Text;
using TL;

namespace DFApp.Aria2
{
    public class Aria2Consts
    {
        public const string JSONRPC = "2.0";

        public const string OnDownloadStart = "aria2.onDownloadStart";
        public const string OnDownloadPause = "aria2.onDownloadPause";
        public const string OnDownloadStop = "aria2.onDownloadStop";
        public const string OnDownloadComplete = "aria2.onDownloadComplete";
        public const string OnDownloadError = "aria2.onDownloadError";
        public const string OnBtDownloadComplete = "aria2.onBtDownloadComplete";

        public const string AddUri = "aria2.addUri";
        public const string AddTorrent = "aria2.addTorrent";
        public const string AddMetalink = "aria2.addMetalink";
        public const string Remove = "aria2.remove";
        public const string ForceRemove = "aria2.forceRemove";
        public const string Pause = "aria2.pause";
        public const string PauseAll = "aria2.pauseAll";
        public const string ForcePause = "aria2.forcePause";
        public const string ForcePauseAll = "aria2.forcePauseAll";
        public const string Unpause = "aria2.unpause";
        public const string UnpauseAll = "aria2.unpauseAll";
        public const string TellStatus = "aria2.tellStatus";
        public const string GetUris = "aria2.getUris";
        public const string GetFiles = "aria2.getFiles";
        public const string GetPeers = "aria2.getPeers";
        public const string GetServers = "aria2.getServers";
        public const string TellActive = "aria2.tellActive";
        public const string TellWaiting = "aria2.tellWaiting";
        public const string TellStopped = "aria2.tellStopped";
        public const string ChangePosition = "aria2.changePosition";
        public const string ChangeUri = "aria2.changeUri";
        public const string GetOption = "aria2.getOption";
        public const string ChangeOption = "aria2.changeOption";
        public const string GetGlobalOption = "aria2.getGlobalOption";
        public const string ChangeGlobalOption = "aria2.changeGlobalOption";
        public const string GetGlobalStat = "aria2.getGlobalStat";
        public const string PurgeDownloadResult = "aria2.purgeDownloadResult";
        public const string RemoveDownloadResult = "aria2.removeDownloadResult";
        public const string GetVersion = "aria2.getVersion";
        public const string GetSessionInfo = "aria2.getSessionInfo";
        public const string Shutdown = "aria2.shutdown";
        public const string ForceShutdown = "aria2.forceShutdown";
        public const string SaveSession = "aria2.saveSession";
        public const string MultiCall = "system.multicall";
        public const string ListMethods = "system.listMethods";
        public const string ListNotifications = "system.listNotifications";

        private static string? _aria2RequestId;
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
}
