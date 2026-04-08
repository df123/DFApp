using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.IP
{
    [SugarTable("DynamicIP")]
    public class DynamicIP : AuditedEntity<Guid>
    {
        public string IP { get; set; }
        public string Port { get; set; }
    }
}
