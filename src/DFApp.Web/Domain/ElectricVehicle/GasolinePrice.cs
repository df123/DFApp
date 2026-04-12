using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.ElectricVehicle
{
    /// <summary>
    /// 油价实体
    /// </summary>
    [SugarTable("AppGasolinePrice")]
    public class GasolinePrice : AuditedEntity<Guid>
    {
        /// <summary>
        /// Guid 类型主键不支持数据库自增，覆盖基类属性移除 IsIdentity
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public new Guid Id { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 0号柴油价格
        /// </summary>
        public decimal? Price0H { get; set; }

        /// <summary>
        /// 89号汽油价格
        /// </summary>
        public decimal? Price89H { get; set; }

        /// <summary>
        /// 90号汽油价格
        /// </summary>
        public decimal? Price90H { get; set; }

        /// <summary>
        /// 92号汽油价格
        /// </summary>
        public decimal? Price92H { get; set; }

        /// <summary>
        /// 93号汽油价格
        /// </summary>
        public decimal? Price93H { get; set; }

        /// <summary>
        /// 95号汽油价格
        /// </summary>
        public decimal? Price95H { get; set; }

        /// <summary>
        /// 97号汽油价格
        /// </summary>
        public decimal? Price97H { get; set; }

        /// <summary>
        /// 98号汽油价格
        /// </summary>
        public decimal? Price98H { get; set; }
    }
}
