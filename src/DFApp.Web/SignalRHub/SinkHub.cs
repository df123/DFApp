using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace DFApp.Web.SignalRHub
{
    [Authorize(DFAppPermissions.LogSink.SignalRSink)]
    [DisableConventionalRegistration]
    public class SinkHub : AbpHub
    {
        private readonly IIdentityRoleRepository _roleRepository;
        private readonly IIdentityUserRepository _identityUserRepository;
        private IReadOnlyList<string> _logUsers;
        public SinkHub(IIdentityRoleRepository roleRepository, IIdentityUserRepository identityUserRepository)
        {
            LogSinkHubService.SinkHub = this;
            _roleRepository = roleRepository;
            _identityUserRepository = identityUserRepository;
        }

        public async Task SendLogMessage(string message)
        {

            if (_logUsers == null)
            {
                var logRole = await _roleRepository.FindByNormalizedNameAsync("LOG");
                var logUsers = (await _identityUserRepository.GetUserIdListByRoleIdAsync(logRole.Id)).Select(x => x.ToString()).ToList();
                _logUsers = logUsers;
            }

            if (_logUsers != null && _logUsers.Count > 0)
            {
                await Clients.Users(_logUsers).SendAsync("SignalRSink", message);
            }

        }
    }

    public static class LogSinkHubService
    {
        public static SinkHub SinkHub { get; set; }
    }

}