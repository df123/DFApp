using System;

namespace DFApp.ElectricVehicle
{
    public class ElectricVehicleChargingRecordDto
    {
        public Guid Id { get; set; }
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
        public ElectricVehicleDto? Vehicle { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateUpdateElectricVehicleChargingRecordDto
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
    }
}
