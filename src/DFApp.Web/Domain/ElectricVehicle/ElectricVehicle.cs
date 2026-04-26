using System;
using System.Collections.Generic;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.ElectricVehicle
{
    /// <summary>
    /// 电动车实体
    /// </summary>
    [SugarTable("AppElectricVehicle")]
    public class ElectricVehicle : AuditedEntity<Guid>
    {
        /// <summary>
        /// Guid 类型主键不支持数据库自增，覆盖基类属性移除 IsIdentity
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public new Guid Id { get; set; }

        /// <summary>
        /// 车辆名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string? Brand { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 车牌号
        /// </summary>
        public string? LicensePlate { get; set; }

        /// <summary>
        /// 购买日期
        /// </summary>
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// 电池容量（kWh）
        /// </summary>
        public decimal? BatteryCapacity { get; set; }

        /// <summary>
        /// 总里程（km）
        /// </summary>
        public decimal TotalMileage { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 成本记录列表（导航属性）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<ElectricVehicleCost> Costs { get; set; }
    }
}
