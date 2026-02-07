using System;
using System.Threading.Tasks;
using System.Linq;
using DFApp.ElectricVehicle;
using DFApp.Permissions;
using DFApp.CommonDtos;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    [Authorize(DFAppPermissions.ElectricVehicleChargingRecord.Default)]
    public class ElectricVehicleChargingRecordService : CrudAppService<
        ElectricVehicleChargingRecord,
        ElectricVehicleChargingRecordDto,
        Guid,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateElectricVehicleChargingRecordDto>, IElectricVehicleChargingRecordService
    {
        public ElectricVehicleChargingRecordService(IRepository<ElectricVehicleChargingRecord, Guid> repository) 
            : base(repository)
        {
            GetPolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Default;
            GetListPolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Default;
            CreatePolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Create;
            UpdatePolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Edit;
            DeletePolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Delete;
        }

        protected override async Task<System.Linq.IQueryable<ElectricVehicleChargingRecord>> CreateFilteredQueryAsync(FilterAndPagedAndSortedResultRequestDto input)
        {
            var query = await Repository.WithDetailsAsync();
            
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var filter = input.Filter.ToLower();
                query = query.Where(x => 
                    (x.StationName != null && x.StationName.ToLower().Contains(filter)) ||
                    (x.Vehicle.Name != null && x.Vehicle.Name.ToLower().Contains(filter)));
            }
            
            return query;
        }
    }
}
