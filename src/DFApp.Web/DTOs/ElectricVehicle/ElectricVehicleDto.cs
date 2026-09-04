using System;

namespace DFApp.Web.DTOs.ElectricVehicle;

/// <summary>
/// 电动车 DTO
/// </summary>
public class ElectricVehicleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? LicensePlate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? BatteryCapacity { get; set; }
    public decimal TotalMileage { get; set; }
    public DateTime? MileageLastUpdatedTime { get; set; }
    public string? Remark { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

/// <summary>
/// 更新电动车里程 DTO
/// </summary>
public class UpdateElectricVehicleMileageDto
{
    /// <summary>当前总里程（km）</summary>
    public decimal Mileage { get; set; }
}

/// <summary>
/// 电动车里程记录 DTO
/// </summary>
public class ElectricVehicleMileageRecordDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public decimal Mileage { get; set; }
    public DateTime RecordedTime { get; set; }
    public string? Remark { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 创建/更新电动车 DTO
/// </summary>
public class CreateUpdateElectricVehicleDto
{
    public string Name { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? LicensePlate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? BatteryCapacity { get; set; }
    public decimal TotalMileage { get; set; }
    public string? Remark { get; set; }
}
