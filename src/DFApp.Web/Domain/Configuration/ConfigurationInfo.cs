using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Configuration
{
    [SugarTable("AppConfigurationInfo")]
    public class ConfigurationInfo : AuditedEntity<long>
    {

        public string ModuleName { get; set; } = string.Empty;
        public string ConfigurationName { get; set; } = string.Empty;
        public string ConfigurationValue { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }
}
