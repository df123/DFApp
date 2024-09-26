using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.FileUploadDownload
{
    public interface IFileUploadInfoService : ICrudAppService<FileUploadInfoDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateFileUploadInfoDto>
    {
        Task<List<CustomFileTypeDto>> GetCustomFileTypeDtoAsync();

        Task<string> GetConfigurationValue(string configurationName);

    }
}
