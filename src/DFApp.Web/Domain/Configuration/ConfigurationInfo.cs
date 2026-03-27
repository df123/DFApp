using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Configuration
{
    [SugarTable("ConfigurationInfos")]
    public class ConfigurationInfo : AuditedEntity<long>
    {

        public required string ModuleName { get; set; }
        public required string ConfigurationName { get; set; }
        public required string ConfigurationValue { get; set; }
        public required string Remark { get; set; }
    }
}
