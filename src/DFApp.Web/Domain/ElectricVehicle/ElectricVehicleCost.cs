using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.ElectricVehicle
{
    /// <summary>
    /// 电动车成本记录实体
    /// </summary>
    [SugarTable("AppElectricVehicleCost")]
    public class ElectricVehicleCost : AuditedEntity<Guid>
    {
        /// <summary>
        /// 车辆ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// 成本类型
        /// </summary>
        public CostType CostType { get; set; }

        /// <summary>
        /// 成本日期
        /// </summary>
        public DateTime CostDate { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 是否属于自己（个人/家庭）
        /// </summary>
        public bool IsBelongToSelf { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 车辆（导航属性）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ElectricVehicle? Vehicle { get; set; }
    }
}
