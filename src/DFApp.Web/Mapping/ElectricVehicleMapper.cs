using DFApp.ElectricVehicle;
using Riok.Mapperly.Abstractions;
using EVEntity = DFApp.ElectricVehicle.ElectricVehicle;
using EVCostEntity = DFApp.ElectricVehicle.ElectricVehicleCost;
using EVChargingEntity = DFApp.ElectricVehicle.ElectricVehicleChargingRecord;
using GasPriceEntity = DFApp.ElectricVehicle.GasolinePrice;
using EVDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleDto;
using CreateUpdateEVDto = DFApp.Web.DTOs.ElectricVehicle.CreateUpdateElectricVehicleDto;
using EVCostDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleCostDto;
using CreateUpdateEVCostDto = DFApp.Web.DTOs.ElectricVehicle.CreateUpdateElectricVehicleCostDto;
using EVChargingDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleChargingRecordDto;
using CreateUpdateEVChargingDto = DFApp.Web.DTOs.ElectricVehicle.CreateUpdateElectricVehicleChargingRecordDto;
using GasPriceDto = DFApp.Web.DTOs.ElectricVehicle.GasolinePriceDto;

namespace DFApp.Web.Mapping;

/// <summary>
/// 电动车模块映射器
/// </summary>
[Mapper]
public partial class ElectricVehicleMapper
{
    /// <summary>
    /// ElectricVehicle → ElectricVehicleDto（忽略 Costs 导航属性）
    /// </summary>
    [MapperIgnoreSource(nameof(EVEntity.Costs))]
    public partial EVDto MapToDto(EVEntity entity);

    /// <summary>
    /// CreateUpdateElectricVehicleDto → ElectricVehicle
    /// </summary>
    [MapperIgnoreTarget(nameof(EVEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(EVEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(EVEntity.LastModificationTime))]
    public partial EVEntity MapToEntity(CreateUpdateEVDto dto);

    /// <summary>
    /// ElectricVehicleCost → ElectricVehicleCostDto（忽略 Vehicle 导航属性）
    /// </summary>
    [MapperIgnoreSource(nameof(EVCostEntity.Vehicle))]
    public partial EVCostDto MapToCostDto(EVCostEntity entity);

    /// <summary>
    /// CreateUpdateElectricVehicleCostDto → ElectricVehicleCost
    /// </summary>
    [MapperIgnoreTarget(nameof(EVCostEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(EVCostEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(EVCostEntity.LastModificationTime))]
    public partial EVCostEntity MapToEntity(CreateUpdateEVCostDto dto);

    /// <summary>
    /// ElectricVehicleChargingRecord → ElectricVehicleChargingRecordDto（忽略 Vehicle 导航属性）
    /// </summary>
    [MapperIgnoreSource(nameof(EVChargingEntity.Vehicle))]
    public partial EVChargingDto MapToChargingDto(EVChargingEntity entity);

    /// <summary>
    /// CreateUpdateElectricVehicleChargingRecordDto → ElectricVehicleChargingRecord
    /// </summary>
    [MapperIgnoreTarget(nameof(EVChargingEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(EVChargingEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(EVChargingEntity.LastModificationTime))]
    public partial EVChargingEntity MapToEntity(CreateUpdateEVChargingDto dto);

    /// <summary>
    /// GasolinePrice → GasolinePriceDto
    /// </summary>
    public partial GasPriceDto MapToDto(GasPriceEntity entity);
}
