using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Configuration
{
    public class ConfigurationInfoDto:AuditedEntityDto<long>
    {
        public string ModuleName { get; set; }
        public string ConfigurationName { get; set; }
        public string ConfigurationValue { get; set; }
        public string Remark { get; set; }
    }
}
