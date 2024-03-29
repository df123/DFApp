﻿using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Configuration
{
    public class ConfigurationInfoService : CrudAppService<ConfigurationInfo
        , ConfigurationInfoDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateConfigurationInfoDto>, IConfigurationInfoService
    {
        private readonly IDataFilter _dataFilter;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        public ConfigurationInfoService(IConfigurationInfoRepository configurationInfoRepository
            , IDataFilter dataFilter) : base(configurationInfoRepository)
        {
            _dataFilter = dataFilter;
            _configurationInfoRepository = configurationInfoRepository;
        }

        public override async Task<ConfigurationInfoDto> CreateAsync(CreateUpdateConfigurationInfoDto input)
        {
            using (_dataFilter.Disable<ISoftDelete>())
            {
                if (await ReadOnlyRepository.AnyAsync(x => x.ModuleName == input.ModuleName && x.ConfigurationName == input.ConfigurationName))
                {
                    var temp = await Repository.FindAsync(x => x.ModuleName == input.ModuleName && x.ConfigurationName == input.ConfigurationName);

                    if (!temp.IsDeleted)
                    {
                        throw new UserFriendlyException("已经存在无需添加");
                    }

                    temp.IsDeleted = false;
                    temp.ConfigurationValue = input.ConfigurationValue;
                    return MapToGetListOutputDto(await Repository.UpdateAsync(temp));

                }
            }

            return await base.CreateAsync(input);
        }

        public async Task<string> GetConfigurationInfoValue(string configurationName, string moduleName)
        {
            return await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, moduleName);
        }
    }
}
