﻿using DFApp.Configuration;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace DFApp.FileUploadDownload
{
    [Authorize(DFAppPermissions.FileUploadDownload.Default)]
    public class FileUploadInfoService : CrudAppService<FileUploadInfo
        , FileUploadInfoDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateFileUploadInfoDto>, IFileUploadInfoService
    {
        private readonly string _moduleName;
        private readonly IDataFilter<ISoftDelete> _dataFilter;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        
        public FileUploadInfoService(IRepository<FileUploadInfo, long> repository
            , IDataFilter<ISoftDelete> dataFilter
            , IConfigurationInfoRepository configurationInfoRepository) : base(repository)
        {
            _moduleName = "DFApp.FileUploadDownload.FileUploadInfoService";

            _dataFilter = dataFilter;
            _configurationInfoRepository = configurationInfoRepository;

            GetPolicyName = DFAppPermissions.FileUploadDownload.Default;
            GetListPolicyName = DFAppPermissions.FileUploadDownload.Default;
            CreatePolicyName = DFAppPermissions.FileUploadDownload.Default;
            UpdatePolicyName = DFAppPermissions.FileUploadDownload.Default;
            DeletePolicyName = DFAppPermissions.FileUploadDownload.Delete;
        }

        [Authorize(DFAppPermissions.FileUploadDownload.Default)]
        public override async Task<FileUploadInfoDto> CreateAsync(CreateUpdateFileUploadInfoDto input)
        {
            using (_dataFilter.Disable())
            {
                if (await ReadOnlyRepository.AnyAsync(x => x.Sha1 == input.Sha1))
                {
                    var temp = await Repository.FindAsync(x => x.Sha1 == input.Sha1);

                    temp!.IsDeleted = false;
                    temp.FileName = input.FileName;
                    temp.Path = input.Path;
                    return MapToGetOutputDto(await Repository.UpdateAsync(temp));

                }
            }
            

            return await base.CreateAsync(input);
        }

        public override async Task DeleteAsync(long id)
        {
            FileUploadInfo info = await Repository.GetAsync(id);
            if ((!string.IsNullOrWhiteSpace(info.Path)) && System.IO.File.Exists(info.Path))
            {
                System.IO.File.Delete(info.Path);
            }

            await base.DeleteAsync(id);
        }

        public async Task<string> GetConfigurationValue(string configurationName)
        {
            var result = await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, _moduleName);
            return result;
        }

        public async Task<List<CustomFileTypeDto>> GetCustomFileTypeDtoAsync()
        {
            var data = await _configurationInfoRepository.GetAllParametersInModule(_moduleName + ".ContentType");

            var result = ObjectMapper.Map<List<ConfigurationInfo>, List<CustomFileTypeDto>>(data);

            return result;
        }



    }
}
