using System;

namespace DFApp.ElectricVehicle
{
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

    public class CreateUpdateElectricVehicleCostDto
    {
        public Guid VehicleId { get; set; }
        public CostType CostType { get; set; }
        public DateTime CostDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsBelongToSelf { get; set; }
        public string? Remark { get; set; }
    }
}
