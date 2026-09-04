using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using ElectricVehicleDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleDto;
using CreateUpdateElectricVehicleDto = DFApp.Web.DTOs.ElectricVehicle.CreateUpdateElectricVehicleDto;
using UpdateElectricVehicleMileageDto = DFApp.Web.DTOs.ElectricVehicle.UpdateElectricVehicleMileageDto;
using ElectricVehicleMileageRecordDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleMileageRecordDto;

namespace DFApp.Web.Services.ElectricVehicle;

/// <summary>
/// 电动车服务
/// </summary>
public class ElectricVehicleService : CrudServiceBase<DFApp.ElectricVehicle.ElectricVehicle, Guid, ElectricVehicleDto, CreateUpdateElectricVehicleDto, CreateUpdateElectricVehicleDto>
{
    private readonly ElectricVehicleMapper _mapper = new();
    private readonly ISqlSugarRepository<ElectricVehicleMileageRecord, Guid> _mileageRecordRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    /// <param name="mileageRecordRepository">里程记录仓储接口</param>
    public ElectricVehicleService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<DFApp.ElectricVehicle.ElectricVehicle, Guid> repository,
        ISqlSugarRepository<ElectricVehicleMileageRecord, Guid> mileageRecordRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _mileageRecordRepository = mileageRecordRepository;
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">电动车实体</param>
    /// <returns>电动车 DTO</returns>
    protected override ElectricVehicleDto MapToGetOutputDto(DFApp.ElectricVehicle.ElectricVehicle entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <returns>电动车实体</returns>
    protected override DFApp.ElectricVehicle.ElectricVehicle MapToEntity(CreateUpdateElectricVehicleDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <param name="entity">电动车实体</param>
    protected override void MapToEntity(CreateUpdateElectricVehicleDto input, DFApp.ElectricVehicle.ElectricVehicle entity)
    {
        var mapped = _mapper.MapToEntity(input);
        entity.Name = mapped.Name;
        entity.Brand = mapped.Brand;
        entity.Model = mapped.Model;
        entity.LicensePlate = mapped.LicensePlate;
        entity.PurchaseDate = mapped.PurchaseDate;
        entity.BatteryCapacity = mapped.BatteryCapacity;
        entity.TotalMileage = mapped.TotalMileage;
        entity.Remark = mapped.Remark;
    }

    /// <summary>
    /// 独立更新车辆当前总里程（不依赖充电/成本记录，随时可单独记录当前表显里程）
    /// </summary>
    /// <param name="id">车辆 ID</param>
    /// <param name="input">里程输入</param>
    public async Task<ElectricVehicleDto> UpdateMileageAsync(Guid id, UpdateElectricVehicleMileageDto input)
    {
        if (input.Mileage < 0)
        {
            throw new BusinessException("里程不能为负数");
        }

        var entity = await Repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new BusinessException("车辆不存在");
        }

        entity.TotalMileage = input.Mileage;
        entity.MileageLastUpdatedTime = DateTime.Now;
        await Repository.UpdateAsync(entity);

        // 每次更新留一条里程快照，供统计计算任意时间段的区间行驶里程
        await _mileageRecordRepository.InsertAsync(new ElectricVehicleMileageRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = id,
            Mileage = input.Mileage,
            RecordedTime = DateTime.Now
        });

        return MapToGetOutputDto(entity);
    }

    /// <summary>
    /// 获取车辆的里程记录（按记录时间倒序）
    /// </summary>
    /// <param name="vehicleId">车辆 ID</param>
    public async Task<List<ElectricVehicleMileageRecordDto>> GetMileageRecordsAsync(Guid vehicleId)
    {
        var records = await _mileageRecordRepository.GetListAsync(x => x.VehicleId == vehicleId);
        return records
            .OrderByDescending(x => x.RecordedTime)
            .Select(x => new ElectricVehicleMileageRecordDto
            {
                Id = x.Id,
                VehicleId = x.VehicleId,
                Mileage = x.Mileage,
                RecordedTime = x.RecordedTime,
                Remark = x.Remark,
                CreationTime = x.CreationTime
            })
            .ToList();
    }

    /// <summary>
    /// 删除里程记录（用于清理误录的快照；不影响车辆当前总里程）
    /// </summary>
    /// <param name="recordId">里程记录 ID</param>
    public async Task DeleteMileageRecordAsync(Guid recordId)
    {
        var record = await _mileageRecordRepository.GetByIdAsync(recordId);
        if (record == null)
        {
            throw new BusinessException("里程记录不存在");
        }
        await _mileageRecordRepository.DeleteAsync(recordId);
    }
}
