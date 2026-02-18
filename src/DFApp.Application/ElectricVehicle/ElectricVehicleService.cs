using System;
using DFApp.ElectricVehicle;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    [Authorize(DFAppPermissions.ElectricVehicle.Default)]
    public class ElectricVehicleService : CrudAppService<
        DFApp.ElectricVehicle.ElectricVehicle,
        ElectricVehicleDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateElectricVehicleDto>, IElectricVehicleService
    {
        public ElectricVehicleService(IRepository<DFApp.ElectricVehicle.ElectricVehicle, Guid> repository) 
            : base(repository)
        {
            GetPolicyName = DFAppPermissions.ElectricVehicle.Default;
            GetListPolicyName = DFAppPermissions.ElectricVehicle.Default;
            CreatePolicyName = DFAppPermissions.ElectricVehicle.Create;
            UpdatePolicyName = DFAppPermissions.ElectricVehicle.Edit;
            DeletePolicyName = DFAppPermissions.ElectricVehicle.Delete;
        }
    }
}
