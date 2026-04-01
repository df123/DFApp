using System;
using DFApp.ElectricVehicle;

namespace DFApp.Web.DTOs.ElectricVehicle;

/// <summary>
/// 电动车费用 DTO
/// </summary>
public class ElectricVehicleCostDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public CostType CostType { get; set; }
    public DateTime CostDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsBelongToSelf { get; set; }
    public string? Remark { get; set; }
    public ElectricVehicleDto? Vehicle { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

/// <summary>
/// 创建/更新电动车费用 DTO
/// </summary>
public class CreateUpdateElectricVehicleCostDto
{
    public Guid VehicleId { get; set; }
    public CostType CostType { get; set; }
    public DateTime CostDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsBelongToSelf { get; set; }
    public string? Remark { get; set; }
}
