using DFApp.Background;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Management
{
    [Authorize(DFAppPermissions.ManagementBackground.Default)]
    public class ManagementBackgroundService : ApplicationService, IManagementBackgroundService
    {
        private readonly IDFAppBackgroundWorkerManagement _backgroundWorkerManager;

        public ManagementBackgroundService(IDFAppBackgroundWorkerManagement backgroundWorkerManager)
        {
            _backgroundWorkerManager = backgroundWorkerManager;
        }

        public int GetBackgroundCount()
        {
            return _backgroundWorkerManager.BackgroundWorkers.Count;
        }

        public List<string> GetModuleName()
        {
            List<string> moduleNames = new List<string>();

            if (_backgroundWorkerManager.BackgroundWorkers == null || _backgroundWorkerManager.BackgroundWorkers.Count <= 0)
            {
                return moduleNames;
            }

            foreach (var item in _backgroundWorkerManager.BackgroundWorkers)
            {
                moduleNames.Add(item.ModuleName);
            }

            return moduleNames;
        }




        public PagedResultDto<BackgroundInfoDto> GetBackgroundStatus()
        {
            if (_backgroundWorkerManager.BackgroundWorkers == null || _backgroundWorkerManager.BackgroundWorkers.Count <= 0)
            {
                return new PagedResultDto<BackgroundInfoDto>();
            }

            List<BackgroundInfoDto> dtos = new List<BackgroundInfoDto>(_backgroundWorkerManager.BackgroundWorkers.Count);

            foreach (var item in _backgroundWorkerManager.BackgroundWorkers)
            {
                dtos.Add(new BackgroundInfoDto()
                {
                    RunStatus = Enum.GetName(typeof(TaskStatus), item.ExecuteTask!.Status),
                    ModuleName = item.ModuleName,
                    StartTime = item.StartTime,
                    RestartTime = item.RestartTime,
                    RestartCount = item.RestartCount,
                    HasError = item.HasError,
                    ErrorCount = item.ErrorCount,
                    ErrorDescription = item.ErrorDescription

                });
            }

            PagedResultDto<BackgroundInfoDto> prdtos = new PagedResultDto<BackgroundInfoDto>();
            prdtos.TotalCount = dtos.Count;
            prdtos.Items = dtos;


            return prdtos;
        }
        [Authorize(DFAppPermissions.ManagementBackground.Restart)]
        public async Task<string> RestartService(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return "需要重启的后台任务不能未空";
            }

            if (_backgroundWorkerManager.BackgroundWorkers == null || _backgroundWorkerManager.BackgroundWorkers.Count <= 0)
            {
                return "没有需要管理的后台任务";
            }

            foreach (var item in _backgroundWorkerManager.BackgroundWorkers)
            {
                if (item.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    await item.RestartAsync();
                    return "已经重启";
                }
            }

            return "未知";
        }
        [Authorize(DFAppPermissions.ManagementBackground.Stop)]
        public async Task<string> StopService(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return "需要停止的后台任务不能未空";
            }

            if (_backgroundWorkerManager.BackgroundWorkers == null || _backgroundWorkerManager.BackgroundWorkers.Count <= 0)
            {
                return "没有需要管理的后台任务";
            }

            foreach (var item in _backgroundWorkerManager.BackgroundWorkers)
            {
                if (item.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    await item.StopAsync();
                    return "已经关闭";
                }
            }

            return "未知";
        }
    }
}
