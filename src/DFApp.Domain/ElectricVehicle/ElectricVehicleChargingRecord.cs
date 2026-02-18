using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.ElectricVehicle
{
    public class ElectricVehicleChargingRecord : AuditedAggregateRoot<Guid>
    {
        public Guid VehicleId { get; set; }
        public DateTime ChargingDate { get; set; }
        public decimal? Energy { get; set; }
        public decimal Amount { get; set; }
    }
}
