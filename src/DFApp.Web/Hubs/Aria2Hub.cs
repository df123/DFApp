using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DFApp.Web.Hubs
{
    /// <summary>
    /// Aria2 实时状态推送 Hub
    /// </summary>
    public class Aria2Hub : Hub
    {
        public const string HubUrl = "/hubs/aria2";

        /// <summary>
        /// 加入 Aria2 监控组
        /// </summary>
        public async Task JoinMonitorGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Aria2Monitor");
        }

        /// <summary>
        /// 离开 Aria2 监控组
        /// </summary>
        public async Task LeaveMonitorGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Aria2Monitor");
        }
    }
}
