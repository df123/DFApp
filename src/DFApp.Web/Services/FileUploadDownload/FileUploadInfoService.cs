using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DFApp.FileUploadDownload;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;

using IConfigurationInfoRepository = DFApp.Web.Data.Configuration.IConfigurationInfoRepository;
using FileUploadInfoDto = DFApp.Web.DTOs.FileUploadDownload.FileUploadInfoDto;
using CreateUpdateFileUploadInfoDto = DFApp.Web.DTOs.FileUploadDownload.CreateUpdateFileUploadInfoDto;
using CustomFileTypeDto = DFApp.Web.DTOs.FileUploadDownload.CustomFileTypeDto;

namespace DFApp.Web.Services.FileUploadDownload;

/// <summary>
/// 文件上传信息服务
/// </summary>
public class FileUploadInfoService : CrudServiceBase<FileUploadInfo, long, FileUploadInfoDto, CreateUpdateFileUploadInfoDto, CreateUpdateFileUploadInfoDto>
{
    private readonly string _moduleName;
    private readonly IConfigurationInfoRepository _configurationInfoRepository;
    private readonly FileUploadDownloadMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    /// <param name="configurationInfoRepository">配置信息仓储接口</param>
    public FileUploadInfoService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<FileUploadInfo, long> repository,
        IConfigurationInfoRepository configurationInfoRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _moduleName = "DFApp.FileUploadDownload.FileUploadInfoService";
        _configurationInfoRepository = configurationInfoRepository;
    }

    /// <summary>
    /// 创建文件上传信息
    /// 如果已存在相同 SHA1 的文件，则恢复并更新文件信息
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>文件上传信息 DTO</returns>
    public override async Task<FileUploadInfoDto> CreateAsync(CreateUpdateFileUploadInfoDto input)
    {
        // 检查是否已存在相同 SHA1 的文件
        var existing = await Repository.GetFirstOrDefaultAsync(x => x.Sha1 == input.Sha1);
        if (existing != null)
        {
            // 已存在则更新文件信息
            existing.FileName = input.FileName;
            existing.Path = input.Path;
            await Repository.UpdateAsync(existing);
            return MapToGetOutputDto(existing);
        }

        return await base.CreateAsync(input);
    }

    /// <summary>
    /// 删除文件上传信息，同时删除物理文件
    /// </summary>
    /// <param name="id">主键 ID</param>
    public override async Task DeleteAsync(long id)
    {
        var info = await Repository.GetByIdAsync(id);
        if (info != null && !string.IsNullOrWhiteSpace(info.Path) && File.Exists(info.Path))
        {
            File.Delete(info.Path);
        }

        await base.DeleteAsync(id);
    }

    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <param name="configurationName">配置名称</param>
    /// <returns>配置值</returns>
    public async Task<string> GetConfigurationValue(string configurationName)
    {
        return await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, _moduleName);
    }

    /// <summary>
    /// 获取自定义文件类型列表
    /// </summary>
    /// <returns>自定义文件类型 DTO 列表</returns>
    public async Task<List<CustomFileTypeDto>> GetCustomFileTypeDtoAsync()
    {
        var data = await _configurationInfoRepository.GetAllParametersInModule(_moduleName + ".ContentType");

        // 使用 ConfigurationMapper 映射实体到 DTO
        var configMapper = new ConfigurationMapper();
        var result = new List<CustomFileTypeDto>();
        foreach (var item in data)
        {
            result.Add(configMapper.MapToCustomFileTypeDto(item));
        }

        return result;
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">文件上传信息实体</param>
    /// <returns>文件上传信息 DTO</returns>
    protected override FileUploadInfoDto MapToGetOutputDto(FileUploadInfo entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>文件上传信息实体</returns>
    protected override FileUploadInfo MapToEntity(CreateUpdateFileUploadInfoDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">文件上传信息实体</param>
    protected override void MapToEntity(CreateUpdateFileUploadInfoDto input, FileUploadInfo entity)
    {
        var mapped = _mapper.MapToEntity(input);
        entity.FileName = mapped.FileName;
        entity.Path = mapped.Path;
        entity.Sha1 = mapped.Sha1;
        entity.FileSize = mapped.FileSize;
    }
}
