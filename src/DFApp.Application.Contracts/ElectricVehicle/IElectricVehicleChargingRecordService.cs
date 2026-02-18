using System;
using DFApp.CommonDtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.ElectricVehicle
{
    public interface IElectricVehicleChargingRecordService : ICrudAppService<
        ElectricVehicleChargingRecordDto,
        Guid,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateElectricVehicleChargingRecordDto>
    {
    }
}
