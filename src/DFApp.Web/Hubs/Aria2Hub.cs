using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using DFApp.Web.Permissions;

namespace DFApp.Web.Hubs
{
    /// <summary>
    /// Aria2 实时状态推送 Hub（加入监控组需持有 Aria2 权限）
    /// </summary>
    [Authorize]
    public class Aria2Hub : Hub
    {
        public const string HubUrl = "/hubs/aria2";

        /// <summary>
        /// 判断连接用户是否持有指定权限（直接读连接建立时的 JWT 权限声明）
        /// </summary>
        private static bool HasPermission(HubCallerContext context, string permissionName)
        {
            return context.User?.FindAll(DFAppClaimTypes.Permission)
                .Any(c => c.Value == permissionName) == true;
        }

        /// <summary>
        /// 加入 Aria2 监控组
        /// </summary>
        public async Task JoinMonitorGroup()
        {
            if (!HasPermission(Context, DFAppPermissions.Aria2.Default))
            {
                throw new HubException("没有加入 Aria2 监控组的权限");
            }

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
