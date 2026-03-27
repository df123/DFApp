using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.IP
{
    [SugarTable("DynamicIP")]
    public class DynamicIP : AuditedEntity<Guid>
    {
        public required string IP { get; set; }
        public required string Port { get; set; }
    }
}
