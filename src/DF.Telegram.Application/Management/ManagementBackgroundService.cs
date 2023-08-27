using DF.Telegram.Background;
using DF.Telegram.Permissions;
using DF.Telegram.TLConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DF.Telegram.Management
{
    [Authorize(TelegramPermissions.DynamicIP.Default)]
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
