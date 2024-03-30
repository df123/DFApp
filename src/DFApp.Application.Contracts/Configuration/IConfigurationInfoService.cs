using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Configuration
{
    public interface IConfigurationInfoService : ICrudAppService<ConfigurationInfoDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateConfigurationInfoDto>
    {
        Task<string> GetConfigurationInfoValue(string configurationName, string moduleName);
        Task<List<ConfigurationInfoDto>> GetAllParametersInModule(string moduleName);
    }
}
