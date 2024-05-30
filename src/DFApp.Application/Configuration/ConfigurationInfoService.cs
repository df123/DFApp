using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Configuration
{
    [Authorize(DFAppPermissions.ConfigurationInfo.Default)]
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
            GetPolicyName = DFAppPermissions.ConfigurationInfo.Default;
            GetListPolicyName = DFAppPermissions.ConfigurationInfo.Default;
            CreatePolicyName = DFAppPermissions.ConfigurationInfo.Create;
            UpdatePolicyName = DFAppPermissions.ConfigurationInfo.Edit;
            DeletePolicyName = DFAppPermissions.ConfigurationInfo.Delete;
        }

        [Authorize(DFAppPermissions.ConfigurationInfo.Create)]
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

        [Authorize(DFAppPermissions.ConfigurationInfo.Default)]
        public async Task<string> GetConfigurationInfoValue(string configurationName, string moduleName)
        {
            return await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, moduleName);
        }

        [Authorize(DFAppPermissions.ConfigurationInfo.Default)]
        public async Task<List<ConfigurationInfoDto>> GetAllParametersInModule(string moduleName)
        {
            var datas = await _configurationInfoRepository.GetAllParametersInModule(moduleName);
            return ObjectMapper.Map<List<ConfigurationInfo>, List<ConfigurationInfoDto>>(datas);
        }

    }
}
