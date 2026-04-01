using System;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using ElectricVehicleDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleDto;
using CreateUpdateElectricVehicleDto = DFApp.Web.DTOs.ElectricVehicle.CreateUpdateElectricVehicleDto;

namespace DFApp.Web.Services.ElectricVehicle;

/// <summary>
/// 电动车服务
/// </summary>
public class ElectricVehicleService : CrudServiceBase<DFApp.ElectricVehicle.ElectricVehicle, Guid, ElectricVehicleDto, CreateUpdateElectricVehicleDto, CreateUpdateElectricVehicleDto>
{
    private readonly ElectricVehicleMapper _mapper = new();
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    public ElectricVehicleService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<DFApp.ElectricVehicle.ElectricVehicle, Guid> repository)
        : base(currentUser, permissionChecker, repository)
    {
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
}
