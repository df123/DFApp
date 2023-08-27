using DF.Telegram.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DF.Telegram.IP
{
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
            UpdatePolicyName = TelegramPermissions.DynamicIP.Delete;
            DeletePolicyName = TelegramPermissions.DynamicIP.Delete;
        }
    }
}
