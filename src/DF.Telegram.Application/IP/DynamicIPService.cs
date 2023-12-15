using DF.Telegram.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DF.Telegram.IP
{
    [Authorize(TelegramPermissions.DynamicIP.Default)]
    public class DynamicIPService : CrudAppService<
        DynamicIP,
        DynamicIPDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateDynamicIPDto>, IDynamicIPService
    {
        public DynamicIPService(IRepository<DynamicIP, Guid> repository) : base(repository)
        {
            GetPolicyName = TelegramPermissions.DynamicIP.Default;
            GetListPolicyName = TelegramPermissions.DynamicIP.Default;
            CreatePolicyName = TelegramPermissions.DynamicIP.Default;
            UpdatePolicyName = TelegramPermissions.DynamicIP.Default;
            DeletePolicyName = TelegramPermissions.DynamicIP.Delete;
        }
    }
}
