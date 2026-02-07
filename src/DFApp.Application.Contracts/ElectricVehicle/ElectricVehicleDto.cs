using System;

namespace DFApp.ElectricVehicle
{
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
        public string? Remark { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

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
}
