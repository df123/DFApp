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
using Volo.Abp.Uow;

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
        private readonly IRepository<ElectricVehicleCost, Guid> _costRepository;
        private readonly IRepository<ElectricVehicle, Guid> _vehicleRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ElectricVehicleChargingRecordService(
            IRepository<ElectricVehicleChargingRecord, Guid> repository,
            IRepository<ElectricVehicleCost, Guid> costRepository,
            IRepository<ElectricVehicle, Guid> vehicleRepository,
            IUnitOfWorkManager unitOfWorkManager)
            : base(repository)
        {
            _costRepository = costRepository;
            _vehicleRepository = vehicleRepository;
            _unitOfWorkManager = unitOfWorkManager;
            GetPolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Default;
            GetListPolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Default;
            CreatePolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Create;
            UpdatePolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Edit;
            DeletePolicyName = DFAppPermissions.ElectricVehicleChargingRecord.Delete;
        }

        protected override async Task<System.Linq.IQueryable<ElectricVehicleChargingRecord>> CreateFilteredQueryAsync(FilterAndPagedAndSortedResultRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();

            return query;
        }

        public override async Task<ElectricVehicleChargingRecordDto> CreateAsync(CreateUpdateElectricVehicleChargingRecordDto input)
        {
            var chargingRecordDto = await base.CreateAsync(input);
            var chargingRecordId = chargingRecordDto.Id;

            await CreateOrUpdateCostRecordAsync(chargingRecordId, input.ChargingDate, input.Amount, input.VehicleId, input.Energy);

            if (input.CurrentMileage.HasValue)
            {
                await UpdateVehicleTotalMileageAsync(input.VehicleId, input.CurrentMileage.Value);
            }

            var chargingRecordEntity = await Repository.GetAsync(chargingRecordId);
            return await MapToGetOutputDtoAsync(chargingRecordEntity);
        }

        public override async Task<ElectricVehicleChargingRecordDto> UpdateAsync(Guid id, CreateUpdateElectricVehicleChargingRecordDto input)
        {
            await base.UpdateAsync(id, input);

            await CreateOrUpdateCostRecordAsync(id, input.ChargingDate, input.Amount, input.VehicleId, input.Energy);

            if (input.CurrentMileage.HasValue)
            {
                await UpdateVehicleTotalMileageAsync(input.VehicleId, input.CurrentMileage.Value);
            }

            var chargingRecordEntity = await Repository.GetAsync(id);
            return await MapToGetOutputDtoAsync(chargingRecordEntity);
        }

        public override async Task DeleteAsync(Guid id)
        {
            await DeleteRelatedCostRecordAsync(id);
            await base.DeleteAsync(id);
        }

        private async Task CreateOrUpdateCostRecordAsync(Guid chargingRecordId, DateTime chargingDate, decimal amount, Guid vehicleId, decimal? energy)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
            {
                var costQuery = await _costRepository.GetQueryableAsync();
                var existingCost = costQuery.FirstOrDefault(c => c.Remark != null && c.Remark.Contains($"ChargingRecord:{chargingRecordId}"));

                if (existingCost != null)
                {
                    existingCost.CostDate = chargingDate;
                    existingCost.Amount = amount;
                    existingCost.VehicleId = vehicleId;
                    existingCost.Remark = $"ChargingRecord:{chargingRecordId}|充电：{energy?.ToString("0.0")}kWh";
                    await _costRepository.UpdateAsync(existingCost);
                }
                else
                {
                    var cost = new ElectricVehicleCost
                    {
                        VehicleId = vehicleId,
                        CostType = CostType.Charging,
                        CostDate = chargingDate,
                        Amount = amount,
                        IsBelongToSelf = true,
                        Remark = $"ChargingRecord:{chargingRecordId}|充电：{energy?.ToString("0.0")}kWh"
                    };
                    await _costRepository.InsertAsync(cost);
                }

                await uow.CompleteAsync();
            }
        }

        private async Task DeleteRelatedCostRecordAsync(Guid chargingRecordId)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
            {
                var costQuery = await _costRepository.GetQueryableAsync();
                var cost = costQuery.FirstOrDefault(c => c.Remark != null && c.Remark.Contains($"ChargingRecord:{chargingRecordId}"));
                if (cost != null)
                {
                    await _costRepository.DeleteAsync(cost);
                }
                await uow.CompleteAsync();
            }
        }

        private async Task UpdateVehicleTotalMileageAsync(Guid vehicleId, decimal mileage)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
            {
                var vehicle = await _vehicleRepository.GetAsync(vehicleId);
                vehicle.TotalMileage = mileage;
                await _vehicleRepository.UpdateAsync(vehicle);
                await uow.CompleteAsync();
            }
        }
    }
}
