using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Management
{
    public interface IManagementBackgroundService : IApplicationService
    {
        Task<bool> RestartListenTelegramService();
    }
}
