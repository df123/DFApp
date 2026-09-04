using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.ElectricVehicle
{
    /// <summary>
    /// 电动车里程记录实体：某时点的表显里程快照。
    /// 独立更新里程（车辆管理页"里程"按钮）与充电记录联动更新时各留一条，
    /// 供统计计算任意时间段的区间行驶里程（油电对比等）。
    /// </summary>
    [SugarTable("AppElectricVehicleMileageRecord")]
    public class ElectricVehicleMileageRecord : AuditedEntity<Guid>
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
        /// 记录时点的总里程（km）
        /// </summary>
        public decimal Mileage { get; set; }

        /// <summary>
        /// 里程对应的时点
        /// </summary>
        public DateTime RecordedTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
