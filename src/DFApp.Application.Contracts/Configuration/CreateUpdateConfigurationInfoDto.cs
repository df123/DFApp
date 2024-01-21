using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Configuration
{
    public class CreateUpdateConfigurationInfoDto
    {
        public string ModuleName { get; set; }
        public string ConfigurationName { get; set; }
        public string ConfigurationValue { get; set; }
    }
}
