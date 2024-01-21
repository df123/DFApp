using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Configuration
{
    public interface IConfigurationInfoService : ICrudAppService<ConfigurationInfoDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateConfigurationInfoDto>
    {
    }
}
