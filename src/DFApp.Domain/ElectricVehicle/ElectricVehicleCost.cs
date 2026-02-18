using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.ElectricVehicle
{
    public class ElectricVehicleCost : AuditedAggregateRoot<Guid>
    {
        public Guid VehicleId { get; set; }
        public CostType CostType { get; set; }
        public DateTime CostDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsBelongToSelf { get; set; }
        public string? Remark { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(VehicleId))]
        public ElectricVehicle? Vehicle { get; set; }
    }
}
