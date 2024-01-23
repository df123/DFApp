using DFApp.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace DFApp.FileUploadDownload
{
    public class FileUploadInfoService : CrudAppService<FileUploadInfo
        , FileUploadInfoDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateFileUploadInfoDto>, IFileUploadInfoService
    {
        private readonly IDataFilter<ISoftDelete> _dataFilter;
        public FileUploadInfoService(IRepository<FileUploadInfo, long> repository
            , IDataFilter<ISoftDelete> dataFilter) : base(repository)
        {
            _dataFilter = dataFilter;
        }

        public override async Task<FileUploadInfoDto> CreateAsync(CreateUpdateFileUploadInfoDto input)
        {
            using (_dataFilter.Disable())
            {
                if (await ReadOnlyRepository.AnyAsync(x => x.Sha1 == input.Sha1))
                {
                    var temp = await Repository.FindAsync(x => x.Sha1 == input.Sha1);

                    temp.IsDeleted = false;
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


    }
}
