using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Media.ExternalLink
{
    public interface IExternalLinkService : ICrudAppService<ExternalLinkDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateExternalLinkDto>
    {
        Task<bool> RemoveFileAsync(long id);
        Task<bool> GetExternalLink();
    }
}
