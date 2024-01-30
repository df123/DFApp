using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Configuration
{
    public interface IConfigurationInfoRepository : IRepository<ConfigurationInfo, long>
    {
        Task<string> GetConfigurationInfoValue(string configurationName, string moduleName);
    }
}
