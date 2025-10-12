using DFApp.Media;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.ObjectMapping;

namespace DFApp.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        private readonly IMediaInfoService _mediaInfoService;
        private readonly IObjectMapper _objectMapper;
        private readonly FileExtensionContentTypeProvider _typeProvider;

        public FileDownloadController(IMediaInfoService mediaInfoService,
            IObjectMapper objectMapper)
        {
            _mediaInfoService = mediaInfoService;
            _objectMapper = objectMapper;
            _typeProvider = new FileExtensionContentTypeProvider();
            _typeProvider.Mappings[".iso"] = "application/octet-stream";
        }

        [HttpGet]
        [Authorize(DFAppPermissions.Medias.Download)]
        public async Task<IActionResult> GetFile(int id)
        {

            var dto = await _mediaInfoService.GetAsync(id);

            Check.NotNull(dto, nameof(dto));

            Check.NotNullOrWhiteSpace(dto.SavePath, nameof(dto.SavePath));

            _typeProvider.TryGetContentType(dto.SavePath, out var contentType);
            Check.NotNullOrWhiteSpace(contentType, nameof(contentType));

            var fileDownloadName = Path.GetFileName(dto.SavePath);
            Check.NotNullOrWhiteSpace(fileDownloadName, nameof(fileDownloadName));

            FileStream fs = new FileStream(dto.SavePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var fileStreamReult = new FileStreamResult(fs, contentType!)
            {
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = true
            };

            var updateDto = _objectMapper.Map<MediaInfoDto, CreateUpdateMediaInfoDto>(dto);

            Check.NotNull(updateDto, nameof(updateDto));

            await _mediaInfoService.UpdateAsync(dto.Id, updateDto);

            return fileStreamReult;
        }

    }
}
