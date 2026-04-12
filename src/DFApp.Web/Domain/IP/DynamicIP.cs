using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.IP
{
    [SugarTable("AppDynamicIP")]
    public class DynamicIP : AuditedEntity<Guid>
    {
        /// <summary>
        /// Guid 类型主键不支持数据库自增，覆盖基类属性移除 IsIdentity
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public new Guid Id { get; set; }

        public string IP { get; set; }
        public string Port { get; set; }
    }
}
