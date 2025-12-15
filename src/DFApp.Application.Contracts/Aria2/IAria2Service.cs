using DFApp.Aria2.Response.TellStatus;
using DFApp.CommonDtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Aria2
{
    public interface IAria2Service: ICrudAppService<
    TellStatusResultDto
    , long
    , FilterAndPagedAndSortedResultRequestDto
    , TellStatusResultDto>
    {
        Task<string> GetExternalLink(long id);
        Task<List<string>> GetAllExternalLinks(bool videoOnly = false);
        Task ClearDownloadDirectoryAsync();
        Task<AddDownloadResponseDto> AddDownloadAsync(AddDownloadRequestDto input);
    }
}
