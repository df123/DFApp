using System;
using System.Collections.Generic;

namespace DFApp.ElectricVehicle
{
    public class OilCostComparisonDto
    {
        // 电车数据
        public decimal ElectricVehicleTotalCost { get; set; }
        public decimal ElectricVehicleMileage { get; set; }
        public decimal ElectricVehicleCostPerKm { get; set; }
        public decimal ElectricChargingCost { get; set; }
        public decimal ElectricOtherCost { get; set; }
        
        // 油车数据
        public decimal OilVehicleCostPerKm { get; set; }
        public decimal OilVehicleTotalCost { get; set; }
        public decimal OilVehicleFuelCost { get; set; }
        
        // 对比数据
        public decimal Savings { get; set; }
        public decimal SavingsPercentage { get; set; }
        public string Province { get; set; }
        public decimal CurrentGasolinePrice { get; set; }
        public GasolineGrade GasolineGrade { get; set; }
        public decimal FuelConsumption { get; set; }
        
        // 时间范围
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class OilCostComparisonRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? VehicleId { get; set; }
        public bool? IsBelongToSelf { get; set; }
    }
}
