using System;
using System.Threading.Tasks;
using DFApp.CommonDtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.ElectricVehicle
{
    public interface IElectricVehicleCostService : ICrudAppService<
        ElectricVehicleCostDto,
        Guid,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateElectricVehicleCostDto>
    {
        Task<OilCostComparisonDto> GetOilCostComparisonAsync(OilCostComparisonRequestDto input);
    }
}
