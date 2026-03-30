using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services.Configuration;

/// <summary>
/// 配置信息服务
/// </summary>
public class ConfigurationInfoService : CrudServiceBase<ConfigurationInfo, long, ConfigurationInfoDto, CreateUpdateConfigurationInfoDto, CreateUpdateConfigurationInfoDto>
{
    private readonly IConfigurationInfoRepository _configurationInfoRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    /// <param name="configurationInfoRepository">配置信息仓储接口</param>
    public ConfigurationInfoService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<ConfigurationInfo, long> repository,
        IConfigurationInfoRepository configurationInfoRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _configurationInfoRepository = configurationInfoRepository;
    }

    /// <summary>
    /// 创建配置信息
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>配置信息 DTO</returns>
    public override async Task<ConfigurationInfoDto> CreateAsync(CreateUpdateConfigurationInfoDto input)
    {
        // 检查是否已存在相同模块和配置名的配置
        var exists = await Repository.AnyAsync(x => x.ModuleName == input.ModuleName && x.ConfigurationName == input.ConfigurationName);
        if (exists)
        {
            throw new BusinessException("已经存在无需添加");
        }

        return await base.CreateAsync(input);
    }

    /// <summary>
    /// 获取配置信息值
    /// </summary>
    /// <param name="configurationName">配置名称</param>
    /// <param name="moduleName">模块名称</param>
    /// <returns>配置值</returns>
    public async Task<string> GetConfigurationInfoValue(string configurationName, string moduleName)
    {
        return await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, moduleName);
    }

    /// <summary>
    /// 获取指定模块的所有配置参数
    /// </summary>
    /// <param name="moduleName">模块名称</param>
    /// <returns>配置信息 DTO 列表</returns>
    public async Task<List<ConfigurationInfoDto>> GetAllParametersInModule(string moduleName)
    {
        var datas = await _configurationInfoRepository.GetAllParametersInModule(moduleName);

        // TODO: 使用 Mapperly 映射实体到 DTO
        var dtos = new List<ConfigurationInfoDto>();
        foreach (var data in datas)
        {
            dtos.Add(new ConfigurationInfoDto
            {
                Id = data.Id,
                ModuleName = data.ModuleName,
                ConfigurationName = data.ConfigurationName,
                ConfigurationValue = data.ConfigurationValue,
                Remark = data.Remark,
                CreationTime = data.CreationTime,
                CreatorId = data.CreatorId,
                LastModificationTime = data.LastModificationTime,
                LastModifierId = data.LastModifierId
            });
        }

        return dtos;
    }

    /// <summary>
    /// 获取剩余磁盘空间
    /// </summary>
    /// <returns>剩余磁盘空间（GB）</returns>
    public async Task<string> GetRemainingDiskSpaceAsync()
    {
        string saveDrive = await _configurationInfoRepository.GetConfigurationInfoValue("SaveDrive", string.Empty);
        return StorageUnitConversionHelper.ByteToGB(SpaceHelper.GetAnyDriveAvailable(saveDrive)).ToString("F2") + "GB";
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">配置信息实体</param>
    /// <returns>配置信息 DTO</returns>
    protected override ConfigurationInfoDto MapToGetOutputDto(ConfigurationInfo entity)
    {
        // TODO: 使用 Mapperly 映射实体到 DTO
        return new ConfigurationInfoDto
        {
            Id = entity.Id,
            ModuleName = entity.ModuleName,
            ConfigurationName = entity.ConfigurationName,
            ConfigurationValue = entity.ConfigurationValue,
            Remark = entity.Remark,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>配置信息实体</returns>
    protected override ConfigurationInfo MapToEntity(CreateUpdateConfigurationInfoDto input)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        return new ConfigurationInfo
        {
            ModuleName = input.ModuleName ?? string.Empty,
            ConfigurationName = input.ConfigurationName,
            ConfigurationValue = input.ConfigurationValue,
            Remark = input.Remark ?? string.Empty
        };
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">配置信息实体</param>
    protected override void MapToEntity(CreateUpdateConfigurationInfoDto input, ConfigurationInfo entity)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        entity.ModuleName = input.ModuleName ?? string.Empty;
        entity.ConfigurationName = input.ConfigurationName;
        entity.ConfigurationValue = input.ConfigurationValue;
        entity.Remark = input.Remark ?? string.Empty;
    }
}
