using System;

namespace DFApp.Web.DTOs.ElectricVehicle;

/// <summary>
/// 电动车充电记录 DTO
/// </summary>
public class ElectricVehicleChargingRecordDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime ChargingDate { get; set; }
    public decimal? Energy { get; set; }
    public decimal Amount { get; set; }
    public decimal? CurrentMileage { get; set; }
    public ElectricVehicleDto? Vehicle { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

/// <summary>
/// 创建/更新电动车充电记录 DTO
/// </summary>
public class CreateUpdateElectricVehicleChargingRecordDto
{
    public Guid VehicleId { get; set; }
    public DateTime ChargingDate { get; set; }
    public decimal? Energy { get; set; }
    public decimal Amount { get; set; }
    public decimal? CurrentMileage { get; set; }
}
