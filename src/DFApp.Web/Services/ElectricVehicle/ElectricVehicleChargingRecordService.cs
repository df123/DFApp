using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using ElectricVehicleChargingRecordDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleChargingRecordDto;
using CreateUpdateElectricVehicleChargingRecordDto = DFApp.Web.DTOs.ElectricVehicle.CreateUpdateElectricVehicleChargingRecordDto;
using ElectricVehicleDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleDto;

using ElectricVehicleEntity = DFApp.ElectricVehicle.ElectricVehicle;

namespace DFApp.Web.Services.ElectricVehicle;

/// <summary>
/// 电动车充电记录服务
/// </summary>
public class ElectricVehicleChargingRecordService : CrudServiceBase<
    ElectricVehicleChargingRecord,
    Guid,
    ElectricVehicleChargingRecordDto,
    CreateUpdateElectricVehicleChargingRecordDto,
    CreateUpdateElectricVehicleChargingRecordDto>
{
    private readonly ISqlSugarRepository<ElectricVehicleCost, Guid> _costRepository;
    private readonly ISqlSugarRepository<ElectricVehicleEntity, Guid> _vehicleRepository;
    private readonly ElectricVehicleMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">充电记录仓储接口</param>
    /// <param name="costRepository">成本记录仓储接口</param>
    /// <param name="vehicleRepository">车辆仓储接口</param>
    public ElectricVehicleChargingRecordService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<ElectricVehicleChargingRecord, Guid> repository,
        ISqlSugarRepository<ElectricVehicleCost, Guid> costRepository,
        ISqlSugarRepository<ElectricVehicleEntity, Guid> vehicleRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _costRepository = costRepository;
        _vehicleRepository = vehicleRepository;
    }

    /// <summary>
    /// 根据过滤条件分页查询充电记录
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public async Task<(List<ElectricVehicleChargingRecordDto> Items, int TotalCount)> GetFilteredListAsync(
        string? filter, int pageIndex, int pageSize)
    {
        var query = Repository.GetQueryable();

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页查询
        var items = await query
            .OrderByDescending(x => x.ChargingDate)
            .ToPageListAsync(pageIndex, pageSize);

        // 获取关联的车辆信息
        var vehicleIds = items.Select(x => x.VehicleId).Distinct().ToList();
        var vehicles = await _vehicleRepository.GetListAsync(x => vehicleIds.Contains(x.Id));
        var vehicleMap = vehicles.ToDictionary(x => x.Id);

        // 手动映射 DTO
        var dtos = new List<ElectricVehicleChargingRecordDto>();
        foreach (var item in items)
        {
            var dto = MapToGetOutputDto(item);
            if (dto.VehicleId != Guid.Empty && vehicleMap.TryGetValue(dto.VehicleId, out var vehicle))
            {
                dto.Vehicle = MapVehicleToDto(vehicle);
            }
            dtos.Add(dto);
        }

        return (dtos, totalCount);
    }

    /// <summary>
    /// 根据 ID 获取充电记录
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>充电记录 DTO</returns>
    public override async Task<ElectricVehicleChargingRecordDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetByIdAsync(id);
        EnsureEntityExists(entity, id);

        var dto = MapToGetOutputDto(entity!);
        if (dto.VehicleId != Guid.Empty)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle != null)
            {
                dto.Vehicle = MapVehicleToDto(vehicle);
            }
        }

        return dto;
    }

    /// <summary>
    /// 创建充电记录，同时创建或更新关联的成本记录
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>充电记录 DTO</returns>
    public override async Task<ElectricVehicleChargingRecordDto> CreateAsync(CreateUpdateElectricVehicleChargingRecordDto input)
    {
        var entity = MapToEntity(input);
        await Repository.InsertAsync(entity);

        // 创建或更新关联的成本记录
        await CreateOrUpdateCostRecordAsync(entity.Id, input.ChargingDate, input.Amount, input.VehicleId, input.Energy);

        // 更新车辆总里程
        if (input.CurrentMileage.HasValue)
        {
            await UpdateVehicleTotalMileageAsync(input.VehicleId, input.CurrentMileage.Value);
        }

        // 返回包含车辆信息的 DTO
        var dto = MapToGetOutputDto(entity);
        if (dto.VehicleId != Guid.Empty)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle != null)
            {
                dto.Vehicle = MapVehicleToDto(vehicle);
            }
        }

        return dto;
    }

    /// <summary>
    /// 更新充电记录，同时更新关联的成本记录
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <param name="input">更新输入 DTO</param>
    /// <returns>充电记录 DTO</returns>
    public override async Task<ElectricVehicleChargingRecordDto> UpdateAsync(Guid id, CreateUpdateElectricVehicleChargingRecordDto input)
    {
        var entity = await Repository.GetByIdAsync(id);
        EnsureEntityExists(entity, id);

        MapToEntity(input, entity!);
        await Repository.UpdateAsync(entity!);

        // 创建或更新关联的成本记录
        await CreateOrUpdateCostRecordAsync(id, input.ChargingDate, input.Amount, input.VehicleId, input.Energy);

        // 更新车辆总里程
        if (input.CurrentMileage.HasValue)
        {
            await UpdateVehicleTotalMileageAsync(input.VehicleId, input.CurrentMileage.Value);
        }

        // 返回包含车辆信息的 DTO
        var dto = MapToGetOutputDto(entity!);
        if (dto.VehicleId != Guid.Empty)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle != null)
            {
                dto.Vehicle = MapVehicleToDto(vehicle);
            }
        }

        return dto;
    }

    /// <summary>
    /// 删除充电记录，同时删除关联的成本记录
    /// </summary>
    /// <param name="id">主键 ID</param>
    public override async Task DeleteAsync(Guid id)
    {
        await DeleteRelatedCostRecordAsync(id);
        await base.DeleteAsync(id);
    }

    /// <summary>
    /// 创建或更新关联的成本记录
    /// </summary>
    /// <param name="chargingRecordId">充电记录 ID</param>
    /// <param name="chargingDate">充电日期</param>
    /// <param name="amount">金额</param>
    /// <param name="vehicleId">车辆 ID</param>
    /// <param name="energy">充电量</param>
    private async Task CreateOrUpdateCostRecordAsync(Guid chargingRecordId, DateTime chargingDate, decimal amount, Guid vehicleId, decimal? energy)
    {
        var query = _costRepository.GetQueryable();
        var existingCost = await _costRepository.GetFirstOrDefaultAsync(
            c => c.Remark != null && c.Remark.Contains($"ChargingRecord:{chargingRecordId}"));

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
    }

    /// <summary>
    /// 删除关联的成本记录
    /// </summary>
    /// <param name="chargingRecordId">充电记录 ID</param>
    private async Task DeleteRelatedCostRecordAsync(Guid chargingRecordId)
    {
        var cost = await _costRepository.GetFirstOrDefaultAsync(
            c => c.Remark != null && c.Remark.Contains($"ChargingRecord:{chargingRecordId}"));
        if (cost != null)
        {
            await _costRepository.DeleteAsync(cost);
        }
    }

    /// <summary>
    /// 更新车辆总里程
    /// </summary>
    /// <param name="vehicleId">车辆 ID</param>
    /// <param name="mileage">当前里程</param>
    private async Task UpdateVehicleTotalMileageAsync(Guid vehicleId, decimal mileage)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle != null)
        {
            vehicle.TotalMileage = mileage;
            await _vehicleRepository.UpdateAsync(vehicle);
        }
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">充电记录实体</param>
    /// <returns>充电记录 DTO</returns>
    protected override ElectricVehicleChargingRecordDto MapToGetOutputDto(ElectricVehicleChargingRecord entity)
    {
        return _mapper.MapToChargingDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>充电记录实体</returns>
    protected override ElectricVehicleChargingRecord MapToEntity(CreateUpdateElectricVehicleChargingRecordDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">充电记录实体</param>
    protected override void MapToEntity(CreateUpdateElectricVehicleChargingRecordDto input, ElectricVehicleChargingRecord entity)
    {
        var mapped = _mapper.MapToEntity(input);
        entity.VehicleId = mapped.VehicleId;
        entity.ChargingDate = mapped.ChargingDate;
        entity.Energy = mapped.Energy;
        entity.Amount = mapped.Amount;
        entity.CurrentMileage = mapped.CurrentMileage;
    }

    /// <summary>
    /// 将车辆实体映射为 DTO
    /// </summary>
    /// <param name="vehicle">车辆实体</param>
    /// <returns>车辆 DTO</returns>
    private ElectricVehicleDto MapVehicleToDto(ElectricVehicleEntity vehicle)
    {
        return _mapper.MapToDto(vehicle);
    }
}
