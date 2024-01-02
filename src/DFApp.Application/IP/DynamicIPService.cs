using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.IP
{
    [Authorize(DFAppPermissions.DynamicIP.Default)]
    public class DynamicIPService : CrudAppService<
        DynamicIP,
        DynamicIPDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateDynamicIPDto>, IDynamicIPService
    {
        public DynamicIPService(IRepository<DynamicIP, Guid> repository) : base(repository)
        {
            GetPolicyName = DFAppPermissions.DynamicIP.Default;
            GetListPolicyName = DFAppPermissions.DynamicIP.Default;
            CreatePolicyName = DFAppPermissions.DynamicIP.Default;
            UpdatePolicyName = DFAppPermissions.DynamicIP.Default;
            DeletePolicyName = DFAppPermissions.DynamicIP.Delete;
        }
    }
}
