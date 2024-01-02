using DFApp.Background;
using DFApp.Permissions;
using DFApp.TLConfig;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Management
{
    [Authorize(DFAppPermissions.DynamicIP.Default)]
    public class ManagementBackgroundService : ApplicationService, IManagementBackgroundService
    {
        private readonly ListenTelegramService _listenTelegramService;

        public ManagementBackgroundService(ListenTelegramService listenTelegramService)
        {
            _listenTelegramService = listenTelegramService;
        }

        public async Task<bool> RestartListenTelegramService()
        {
            await _listenTelegramService.StopAsync();
            _listenTelegramService.StartAsync();
            return true;
        }
    }
}
