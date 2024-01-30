using DFApp.Background;
using DFApp.Permissions;
using DFApp.TG.TGLogin;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.TG.Login
{
    [Authorize(DFAppPermissions.Medias.Default)]
    public class TGLoginService : ApplicationService, ITGLoginService
    {
        private readonly ListenTelegramService WT;
        public TGLoginService(ListenTelegramService wt) => WT = wt;

        public string Status()
        {
            switch (WT.ConfigNeeded)
            {
                case "connecting": return "WTelegram is connecting...";
                case null: return $"Connected as {WT.User} Get all chats";
                default: return $@"Enter {WT.ConfigNeeded}: ";
            }
        }
            
        public async Task<string> Config(string value)
        {
            return await WT.DoLogin(value);
        }

        public async Task<object> Chats()
        {
            if (WT.User == null) throw new Exception("Complete the login first");
            var chats = await WT.TGClinet.Messages_GetAllChats();
            return chats.chats;
        }
    }
}
