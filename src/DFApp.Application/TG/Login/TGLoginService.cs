using DFApp.Background;
using DFApp.Permissions;
using DFApp.TG.TGLogin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.TG.Login
{
    [Authorize(DFAppPermissions.Medias.Default)]
    public class TGLoginService : ApplicationService, ITGLoginService
    {
        private readonly ListenTelegramService _listenTelegramService;
        public TGLoginService(IServiceProvider services) 
        {
            _listenTelegramService = services.GetRequiredService<IEnumerable<IHostedService>>()
                .OfType<ListenTelegramService>()
                .FirstOrDefault() ?? throw new InvalidOperationException("ListenTelegramService is not registered.");
        }

        public string Status()
        {
            switch (_listenTelegramService.ConfigNeeded)
            {
                case "connecting":
                    return "WTelegram is connecting...";
                case "start":
                    return "Please start WTelegram background service";
                case null:
                    return $"Connected as {_listenTelegramService.User} Get all chats";
                default:
                    return $@"Enter {_listenTelegramService.ConfigNeeded}: ";
            }
        }
            
        public async Task<string> Config(string value)
        {
            return await _listenTelegramService.DoLogin(value);
        }

        public async Task<object> Chats()
        {
            if (_listenTelegramService.TGClinet == null)
                throw new InvalidOperationException("WTelegram client is not initialized. Please start the background service.");
            
            if (_listenTelegramService.User == null)
                throw new InvalidOperationException("Complete the login first");

            var chats = await _listenTelegramService.TGClinet.Messages_GetAllChats();
            return chats.chats;
        }
    }
}
