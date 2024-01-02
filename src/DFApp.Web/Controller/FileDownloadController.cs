using DFApp.Helper;
using DFApp.Media;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Imaging;
using Volo.Abp.ObjectMapping;

namespace DFApp.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        private readonly IMediaInfoService _mediaInfoService;
        private readonly IObjectMapper _objectMapper;
        private readonly IImageResizer _imageResizer;
        private readonly FileExtensionContentTypeProvider _typeProvider;
        private readonly IQueueBase<MediaInfoDto[]> _mediaQueue;

        public FileDownloadController(IMediaInfoService mediaInfoService,
            IObjectMapper objectMapper,
            IImageResizer imageResizer,
            IQueueBase<MediaInfoDto[]> mediaQueue)
        {
            _mediaInfoService = mediaInfoService;
            _objectMapper = objectMapper;
            _imageResizer = imageResizer;
            _typeProvider = new FileExtensionContentTypeProvider();
            _typeProvider.Mappings[".iso"] = "application/octet-stream";

            _mediaQueue = mediaQueue;
        }

        [HttpGet("thumbnail")]
        [Authorize(DFAppPermissions.Medias.Download)]
        public async Task<IActionResult> GeTthumbnail(int id)
        {
            var dto = await _mediaInfoService.GetAsync(id);

            Check.NotNull(dto, nameof(dto));

            Check.NotNullOrWhiteSpace(dto.SavePath, nameof(dto.SavePath));

            _typeProvider.TryGetContentType(dto.SavePath, out var contentType);
            Check.NotNullOrWhiteSpace(contentType, nameof(contentType));

            if (contentType.Contains("video"))
            {
                return NoContent();
            }

            var fileDownloadName = Path.GetFileName(dto.SavePath);
            Check.NotNullOrWhiteSpace(fileDownloadName, nameof(fileDownloadName));

            using FileStream fs = new FileStream(dto.SavePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var result = await _imageResizer.ResizeAsync(fs, new ImageResizeArgs()
            {
                Width = 256,
                Height = 256,
                Mode = ImageResizeMode.BoxPad
            },
            mimeType: contentType);

            if (result.State == ImageProcessState.Done)
            {
                return new FileStreamResult(result.Result, contentType!)
                {
                    FileDownloadName = fileDownloadName,
                    EnableRangeProcessing = true
                };
            }
            else
            {
                return NoContent();
            }

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


        [HttpGet("ExternalLinkDownload")]
        [Authorize(DFAppPermissions.Medias.Download)]
        public async Task<IActionResult> GetExternalLinkDownload()
        {      
            return Ok(await _mediaInfoService.GetExternalLinkDownload());
        }

    }
}
