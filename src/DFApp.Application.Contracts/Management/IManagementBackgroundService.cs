using DFApp.Background;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Management
{
    public interface IManagementBackgroundService : IApplicationService
    {
        int GetBackgroundCount();
        List<string> GetModuleName();
        PagedResultDto<BackgroundInfoDto> GetBackgroundStatus();
        Task<string> RestartService(string moduleName);
        Task<string> StopService(string moduleName);
    }
}
