using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.ElectricVehicle
{
    /// <summary>
    /// 电动车充电记录实体
    /// </summary>
    [SugarTable("AppElectricVehicleChargingRecord")]
    public class ElectricVehicleChargingRecord : AuditedEntity<Guid>
    {
        /// <summary>
        /// Guid 类型主键不支持数据库自增，覆盖基类属性移除 IsIdentity
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public new Guid Id { get; set; }

        /// <summary>
        /// 车辆ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// 充电日期
        /// </summary>
        public DateTime ChargingDate { get; set; }

        /// <summary>
        /// 充电量（kWh）
        /// </summary>
        public decimal? Energy { get; set; }

        /// <summary>
        /// 充电金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 当前里程（km）
        /// </summary>
        public decimal? CurrentMileage { get; set; }

        /// <summary>
        /// 车辆（导航属性）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ElectricVehicle Vehicle { get; set; }
    }
}
