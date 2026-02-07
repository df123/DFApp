using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.ElectricVehicle
{
    public class ElectricVehicleChargingRecord : AuditedAggregateRoot<Guid>
    {
        public Guid VehicleId { get; set; }
        public DateTime ChargingDate { get; set; }
        public string? StationName { get; set; }
        public int? ChargingDuration { get; set; }
        public decimal? Energy { get; set; }
        public decimal Amount { get; set; }
        public int? StartSOC { get; set; }
        public int? EndSOC { get; set; }
        public bool IsBelongToSelf { get; set; }
        public string? Remark { get; set; }
        
        [ForeignKey(nameof(VehicleId))]
        public ElectricVehicle? Vehicle { get; set; }
    }
}
