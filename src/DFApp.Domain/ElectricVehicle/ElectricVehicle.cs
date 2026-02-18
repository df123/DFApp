using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.ElectricVehicle
{
    public class ElectricVehicle : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? LicensePlate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? BatteryCapacity { get; set; }
        public decimal TotalMileage { get; set; }
        public string? Remark { get; set; }

        public List<ElectricVehicleCost> Costs { get; set; }
    }
}
