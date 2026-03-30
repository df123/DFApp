using System;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services.ElectricVehicle;

/// <summary>
/// 电动车服务
/// </summary>
public class ElectricVehicleService : CrudServiceBase<DFApp.ElectricVehicle.ElectricVehicle, Guid, ElectricVehicleDto, CreateUpdateElectricVehicleDto, CreateUpdateElectricVehicleDto>
{
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
        // TODO: 使用 Mapperly 映射实体到 DTO
        return new ElectricVehicleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Brand = entity.Brand,
            Model = entity.Model,
            LicensePlate = entity.LicensePlate,
            PurchaseDate = entity.PurchaseDate,
            BatteryCapacity = entity.BatteryCapacity,
            TotalMileage = entity.TotalMileage,
            Remark = entity.Remark,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <returns>电动车实体</returns>
    protected override DFApp.ElectricVehicle.ElectricVehicle MapToEntity(CreateUpdateElectricVehicleDto input)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        return new DFApp.ElectricVehicle.ElectricVehicle
        {
            Name = input.Name,
            Brand = input.Brand,
            Model = input.Model,
            LicensePlate = input.LicensePlate,
            PurchaseDate = input.PurchaseDate,
            BatteryCapacity = input.BatteryCapacity,
            TotalMileage = input.TotalMileage,
            Remark = input.Remark
        };
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <param name="entity">电动车实体</param>
    protected override void MapToEntity(CreateUpdateElectricVehicleDto input, DFApp.ElectricVehicle.ElectricVehicle entity)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        entity.Name = input.Name;
        entity.Brand = input.Brand;
        entity.Model = input.Model;
        entity.LicensePlate = input.LicensePlate;
        entity.PurchaseDate = input.PurchaseDate;
        entity.BatteryCapacity = input.BatteryCapacity;
        entity.TotalMileage = input.TotalMileage;
        entity.Remark = input.Remark;
    }
}
