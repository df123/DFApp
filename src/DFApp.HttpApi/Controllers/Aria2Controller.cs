using DFApp.Aria2;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;

namespace DFApp.Controllers
{
    [Route("api/app/aria2")]
    [ApiController]
    [Authorize(DFAppPermissions.Aria2.Default)]
    public class Aria2Controller : DFAppController
    {
        private readonly IAria2Service _aria2Service;

        public Aria2Controller(IAria2Service aria2Service)
        {
            _aria2Service = aria2Service;
        }

        [HttpPost("add-download")]
        public async Task<AddDownloadResponseDto> AddDownloadAsync(AddDownloadRequestDto input)
        {
            return await _aria2Service.AddDownloadAsync(input);
        }
    }
}