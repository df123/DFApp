using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.TG.TGLogin
{
    public interface ITGLoginService : IApplicationService
    {
        string Status();

        Task<string> Config(string value);

        Task<object> Chats();
    }
}
