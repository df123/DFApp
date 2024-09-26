using DFApp.FileUploadDownload;
using DFApp.Helper;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;

namespace DFApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadInfoController : DFAppController
    {
        private readonly long _fileSizeLimit;
        private readonly IFileUploadInfoService _fileInfoService;
        private readonly FileExtensionContentTypeProvider _typeProvider;

        public FileUploadInfoController(IFileUploadInfoService fileUploadInfoService)
        {
            _fileSizeLimit = 10 * 1024 * 1024;
            _fileInfoService = fileUploadInfoService;
            _typeProvider = new FileExtensionContentTypeProvider();
        }

        private async Task SetCustomFileTypeDtoAsync()
        {
            List<CustomFileTypeDto> dtos = await _fileInfoService.GetCustomFileTypeDtoAsync();
            
            if (dtos != null && dtos.Count > 0)
            {
                foreach (var dto in dtos)
                {
                    if (dto.ConfigurationName == null || dto.ConfigurationValue == null)
                    {
                        continue;
                    }

                    if(!_typeProvider.Mappings.ContainsKey(dto.ConfigurationName))
                    {
                        _typeProvider.Mappings[dto.ConfigurationName] = dto.ConfigurationValue;
                    }

                }
            }
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

                if (!userAgent.Equals(dto.Sha1, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("上传失败：SHA1不相同");
                }

                string prefix = await _fileInfoService.GetConfigurationValue("SaveUplouadFilePath");

                dto.FileSize = memoryStream.Length;
                dto.FileName = file.FileName;
                dto.Path = Path.Combine(prefix, file.FileName);

                System.IO.File.WriteAllBytes(dto.Path, memoryStream.ToArray());

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

            await SetCustomFileTypeDtoAsync();  

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
