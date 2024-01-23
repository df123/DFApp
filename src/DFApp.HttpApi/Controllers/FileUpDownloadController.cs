using DFApp.Configuration;
using DFApp.FileUploadDownload;
using DFApp.Helper;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.JSInterop.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace DFApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUpDownloadController : AbpControllerBase
    {
        private readonly long _fileSizeLimit;
        private readonly IFileUploadInfoService _fileInfoService;
        private readonly FileExtensionContentTypeProvider _typeProvider;
        private readonly IConfigurationInfoService _configurationInfoService;
        public FileUpDownloadController(IFileUploadInfoService fileUploadInfoService,
            IConfigurationInfoService configurationInfoService)
        {
            _fileSizeLimit = 10 * 1024 * 1024;
            _fileInfoService = fileUploadInfoService;
            _typeProvider = new FileExtensionContentTypeProvider();
            _typeProvider.Mappings[".iso"] = "application/octet-stream";
            _configurationInfoService = configurationInfoService;
        }

        [HttpPost("upload")]
        [Authorize(DFAppPermissions.FileUploadDownload.Upload)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file.Length > _fileSizeLimit)
            {
                return BadRequest("上传失败：文件超过最大上传值");
            }


            string? userAgent = HttpContext.Request.Headers["FileSHA1"];

            if (userAgent == null)
            {
                return BadRequest("上传失败：缺少本地计算SHA1");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                CreateUpdateFileUploadInfoDto dto = new CreateUpdateFileUploadInfoDto();
                dto.Sha1 = HashHelper.CalculationHash(memoryStream);

                if (!userAgent.Equals(dto.Sha1,StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("上传失败：SHA1不相同");
                }

                dto.FileSize = memoryStream.Length;
                dto.FileName = file.FileName;
                dto.Path = $"{AppsettingsHelper.app("RunConfig", "SaveUplouadFilePath")}/{file.FileName}";

                System.IO.File.WriteAllBytes($"{AppsettingsHelper.app("RunConfig", "SaveUplouadFilePath")}/{file.FileName}", memoryStream.ToArray());
                //System.IO.File.WriteAllBytes($"{file.FileName}", memoryStream.ToArray());

                await _fileInfoService.CreateAsync(dto);
            }

            return Ok($"{file.FileName}成功");

        }

        [HttpGet]
        [Authorize(DFAppPermissions.FileUploadDownload.Download)]
        public async Task<IActionResult> GetFile(int id)
        {

            var dto = await _fileInfoService.GetAsync(id);

            Check.NotNull(dto, nameof(dto));

            Check.NotNullOrWhiteSpace(dto.Path, nameof(dto.Path));

            _typeProvider.TryGetContentType(dto.Path, out var contentType);
            Check.NotNullOrWhiteSpace(contentType, nameof(contentType));

            var fileDownloadName = Path.GetFileName(dto.Path);
            Check.NotNullOrWhiteSpace(fileDownloadName, nameof(fileDownloadName));

            FileStream fs = new FileStream(dto.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var fileStreamReult = new FileStreamResult(fs, contentType!)
            {
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = true
            };

            return fileStreamReult;
        }


    }
}
