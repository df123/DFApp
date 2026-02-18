using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.ElectricVehicle
{
    public interface IElectricVehicleService : ICrudAppService<
        ElectricVehicleDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateElectricVehicleDto>
    {
    }
}
