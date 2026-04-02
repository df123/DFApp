using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DFApp.Helper;
using DFApp.Web.Data;
using DFApp.Web.DTOs.FileUploadDownload;
using DFApp.Web.Permissions;
using DFApp.Web.Services.FileUploadDownload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

using CreateUpdateFileUploadInfoInput = DFApp.FileUploadDownload.CreateUpdateFileUploadInfoDto;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 文件上传信息控制器，提供文件上传信息的增删改查、文件上传下载及配置查询功能
/// </summary>
[ApiController]
[Route("api/app/file-upload-info")]
[Authorize]
public class FileUploadInfoController : DFAppControllerBase
{
    private readonly long _fileSizeLimit = 10 * 1024 * 1024;
    private readonly FileUploadInfoService _fileUploadInfoService;
    private readonly FileExtensionContentTypeProvider _typeProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileUploadInfoService">文件上传信息服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public FileUploadInfoController(
        FileUploadInfoService fileUploadInfoService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _fileUploadInfoService = fileUploadInfoService;
        _typeProvider = new FileExtensionContentTypeProvider();
    }

    /// <summary>
    /// 获取文件上传信息列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.FileUploadDownload.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _fileUploadInfoService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取文件上传信息详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.FileUploadDownload.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _fileUploadInfoService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取文件上传信息列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.FileUploadDownload.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _fileUploadInfoService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 删除文件上传信息（同时删除物理文件）
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.FileUploadDownload.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _fileUploadInfoService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 获取文件上传模块的配置值
    /// </summary>
    /// <param name="configurationName">配置名称</param>
    [HttpGet("configuration")]
    [Permission(DFAppPermissions.FileUploadDownload.Default)]
    public async Task<IActionResult> GetConfigurationValueAsync([FromQuery] string configurationName)
    {
        var result = await _fileUploadInfoService.GetConfigurationValue(configurationName);
        return Success(result);
    }

    /// <summary>
    /// 获取自定义文件类型列表
    /// </summary>
    [HttpGet("custom-file-types")]
    [Permission(DFAppPermissions.FileUploadDownload.Default)]
    public async Task<IActionResult> GetCustomFileTypeDtoAsync()
    {
        var result = await _fileUploadInfoService.GetCustomFileTypeDtoAsync();
        return Success(result);
    }

    /// <summary>
    /// 上传文件
    /// 接收文件后校验大小和 SHA1，保存到服务器并创建上传记录
    /// </summary>
    /// <param name="file">上传的文件</param>
    [HttpPost("upload")]
    [Permission(DFAppPermissions.FileUploadDownload.Upload)]
    public async Task<IActionResult> UploadAsync(IFormFile file)
    {
        if (file.Length > _fileSizeLimit)
        {
            return Fail("上传失败：文件超过最大上传值（10MB）");
        }

        string? clientSha1 = HttpContext.Request.Headers["FileSHA1"];
        if (string.IsNullOrWhiteSpace(clientSha1))
        {
            return Fail("上传失败：缺少本地计算SHA1");
        }

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var dto = new CreateUpdateFileUploadInfoInput
        {
            Sha1 = HashHelper.CalculateHash(memoryStream)
        };

        if (!clientSha1.Equals(dto.Sha1, StringComparison.OrdinalIgnoreCase))
        {
            return Fail("上传失败：SHA1不相同");
        }

        string savePath = await _fileUploadInfoService.GetConfigurationValue("SaveUplouadFilePath");

        dto.FileSize = memoryStream.Length;
        dto.FileName = file.FileName;
        dto.Path = Path.Combine(savePath, file.FileName);

        // 确保保存目录存在
        var directory = Path.GetDirectoryName(dto.Path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await System.IO.File.WriteAllBytesAsync(dto.Path, memoryStream.ToArray());
        await _fileUploadInfoService.CreateAsync(dto);

        return Success($"{file.FileName}上传成功");
    }

    /// <summary>
    /// 下载文件
    /// 根据文件记录 ID 返回文件流
    /// </summary>
    /// <param name="id">文件记录 ID</param>
    [HttpGet("download")]
    [Permission(DFAppPermissions.FileUploadDownload.Download)]
    public async Task<IActionResult> DownloadAsync([FromQuery] long id)
    {
        var dto = await _fileUploadInfoService.GetAsync(id);
        if (dto == null)
        {
            ThrowBusinessException("文件记录不存在");
            return Fail("文件记录不存在");
        }

        if (string.IsNullOrWhiteSpace(dto.Path))
        {
            ThrowBusinessException("文件路径为空");
            return Fail("文件路径为空");
        }

        await LoadCustomFileTypesAsync();

        if (!_typeProvider.TryGetContentType(dto.Path, out var contentType) || string.IsNullOrWhiteSpace(contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileDownloadName = Path.GetFileName(dto.Path);

        var fs = new FileStream(dto.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        return new FileStreamResult(fs, contentType)
        {
            FileDownloadName = fileDownloadName,
            EnableRangeProcessing = true
        };
    }

    /// <summary>
    /// 从配置中加载自定义文件类型映射
    /// </summary>
    private async Task LoadCustomFileTypesAsync()
    {
        var dtos = await _fileUploadInfoService.GetCustomFileTypeDtoAsync();
        if (dtos == null || dtos.Count == 0) return;

        foreach (var dto in dtos)
        {
            if (dto.ConfigurationName == null || dto.ConfigurationValue == null) continue;
            if (!_typeProvider.Mappings.ContainsKey(dto.ConfigurationName))
            {
                _typeProvider.Mappings[dto.ConfigurationName] = dto.ConfigurationValue;
            }
        }
    }
}
