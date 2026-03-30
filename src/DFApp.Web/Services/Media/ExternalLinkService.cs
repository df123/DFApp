using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DFApp.Background;
using DFApp.Helper;
using DFApp.Media;
using DFApp.Media.ExternalLink;
using DFApp.Queue;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace DFApp.Web.Services.Media;

/// <summary>
/// 媒体外链服务
/// </summary>
public class ExternalLinkService : CrudServiceBase<
    MediaExternalLink,
    long,
    ExternalLinkDto,
    CreateUpdateExternalLinkDto,
    CreateUpdateExternalLinkDto>
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IConfigurationInfoRepository _configurationInfoRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">外链仓储接口</param>
    /// <param name="backgroundTaskQueue">后台任务队列</param>
    /// <param name="configurationInfoRepository">配置信息仓储接口</param>
    public ExternalLinkService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<MediaExternalLink, long> repository,
        IBackgroundTaskQueue backgroundTaskQueue,
        IConfigurationInfoRepository configurationInfoRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
        _configurationInfoRepository = configurationInfoRepository;
    }

    /// <summary>
    /// 创建操作不允许使用
    /// </summary>
    public override Task<ExternalLinkDto> CreateAsync(CreateUpdateExternalLinkDto input)
    {
        throw new BusinessException("此接口不允许使用");
    }

    /// <summary>
    /// 更新操作不允许使用
    /// </summary>
    public override Task<ExternalLinkDto> UpdateAsync(long id, CreateUpdateExternalLinkDto input)
    {
        throw new BusinessException("此接口不允许使用");
    }

    /// <summary>
    /// 删除外链记录，如果文件未移除则先移除文件
    /// </summary>
    /// <param name="id">外链 ID</param>
    public override async Task DeleteAsync(long id)
    {
        if (id <= 0)
        {
            throw new BusinessException("ID要大于0");
        }

        var mediaExternalLink = await Repository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (mediaExternalLink != null && !mediaExternalLink.IsRemove)
        {
            await RemoveFileAsync(id);
        }

        await base.DeleteAsync(id);
    }

    /// <summary>
    /// 生成外链（后台任务）
    /// 原始代码使用 IBackgroundTaskQueue 在后台执行，依赖 IServiceProvider 解析服务
    /// </summary>
    /// <returns>是否成功加入队列</returns>
    public Task<bool> GetExternalLink()
    {
        _backgroundTaskQueue.EnqueueTask(async (serviceScopeFactory, cancellationToken) =>
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using var scope = serviceScopeFactory.CreateScope();
            var configurationInfoRepository = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
            var mediaInfoRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
            var externalLinkRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaExternalLink, long>>();
            var mediaExternalLinkMediaIdRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaExternalLinkMediaIds, long>>();

            var returnDownloadUrlPrefix = await configurationInfoRepository.GetConfigurationInfoValue("ReturnDownloadUrlPrefix", MediaBackgroudConst.ModuleName);
            if (string.IsNullOrWhiteSpace(returnDownloadUrlPrefix))
            {
                throw new BusinessException(nameof(returnDownloadUrlPrefix));
            }

            string photoSavePath = await configurationInfoRepository.GetConfigurationInfoValue("SavePhotoPathPrefix", MediaBackgroudConst.ModuleName);
            if (string.IsNullOrWhiteSpace(photoSavePath))
            {
                throw new BusinessException(nameof(photoSavePath));
            }

            string zipType = await configurationInfoRepository.GetConfigurationInfoValue("ZipType", MediaBackgroudConst.ModuleName);

            List<MediaInfo> temp = await mediaInfoRepository.GetListAsync(x => !x.IsExternalLinkGenerated
                && x.IsDownloadCompleted);

            if (temp == null || temp.Count <= 0)
            {
                return;
            }

            string datetimeName = DateTime.Now.ToString("yyyyMMddHHmmss");
            string zipPhotoName = $"{datetimeName}.zip";
            string zipPhotoPathName = Path.Combine(Path.GetDirectoryName(photoSavePath)!, zipPhotoName);

            using ZipArchive archive = ZipFile.Open(zipPhotoPathName, ZipArchiveMode.Create);
            long size = 0;

            StringBuilder stringBuilder = new StringBuilder();
            if (File.Exists(zipPhotoPathName))
            {
                stringBuilder.AppendLine(Path.Combine(returnDownloadUrlPrefix, zipPhotoName));
            }

            string replaceUrlPrefix = await configurationInfoRepository.GetConfigurationInfoValue("ReplaceUrlPrefix", MediaBackgroudConst.ModuleName);
            foreach (var mediaInfo in temp)
            {
                if (zipType.Contains(mediaInfo.MimeType) && File.Exists(mediaInfo.SavePath))
                {
                    archive.CreateEntryFromFile(mediaInfo.SavePath, Path.GetFileName(mediaInfo.SavePath), CompressionLevel.NoCompression);
                    mediaInfo.IsExternalLinkGenerated = true;
                    size += mediaInfo.Size;

                    continue;
                }

                stringBuilder.AppendLine($"{Path.Combine(returnDownloadUrlPrefix, mediaInfo.SavePath.Replace(replaceUrlPrefix, string.Empty).Replace("\\", "/"))}");
                mediaInfo.IsExternalLinkGenerated = true;
            }

            if (File.Exists(zipPhotoPathName))
            {
                temp.Add(await mediaInfoRepository.InsertAsync(new MediaInfo
                {
                    MediaId = Random.Shared.NextInt64(),
                    ChatId = Random.Shared.NextInt64(),
                    ChatTitle = "zip",
                    Size = size,
                    SavePath = zipPhotoPathName,
                    MimeType = "zip",
                    IsExternalLinkGenerated = true,
                    IsDownloadCompleted = true,
                }));
            }

            if (temp != null && temp.Count > 0)
            {
                await mediaInfoRepository.UpdateAsync(temp);
                stopwatch.Stop();

                List<MediaExternalLinkMediaIds> mediaExternalLinkMediaIds = new List<MediaExternalLinkMediaIds>();

                var mediaExternalLink = new MediaExternalLink
                {
                    Name = datetimeName,
                    Size = size,
                    TimeConsumed = stopwatch.ElapsedMilliseconds,
                    IsRemove = false,
                    LinkContent = stringBuilder.ToString(),
                    MediaIds = mediaExternalLinkMediaIds
                };

                foreach (var mediaInfo in temp)
                {
                    mediaExternalLinkMediaIds.Add(new MediaExternalLinkMediaIds
                    {
                        MediaId = mediaInfo.Id
                    });
                }

                await externalLinkRepository.InsertAsync(mediaExternalLink);
            }
        });

        return Task.FromResult(true);
    }

    /// <summary>
    /// 移除外链关联的文件（后台任务）
    /// 原始代码使用 IBackgroundTaskQueue 在后台执行，依赖 IServiceProvider 解析服务
    /// </summary>
    /// <param name="id">外链 ID</param>
    /// <returns>是否成功加入队列</returns>
    public Task<bool> RemoveFileAsync(long id)
    {
        if (id <= 0)
        {
            throw new BusinessException("ID要大于0");
        }

        _backgroundTaskQueue.EnqueueTask(async (serviceScopeFactory, cancellationToken) =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var externalLinkRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaExternalLink, long>>();
            var mediaInfoRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
            var configurationInfoRepository = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
            var mediaExternalLinkMediaIdRepository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaExternalLinkMediaIds, long>>();

            var mediaExternalLink = await externalLinkRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (mediaExternalLink != null && !mediaExternalLink.IsRemove)
            {
                var mediaExternalLinkMediaIds = await mediaExternalLinkMediaIdRepository.GetListAsync(x => x.MediaExternalLinkId == mediaExternalLink.Id);

                List<long> ids = mediaExternalLinkMediaIds.Select(x => x.MediaId).ToList();
                List<MediaInfo> medias = await mediaInfoRepository.GetListAsync(x => ids.Contains(x.Id));

                foreach (var item in medias)
                {
                    if (item != null && !string.IsNullOrWhiteSpace(item.SavePath))
                    {
                        SpaceHelper.DeleteFile(item.SavePath!);
                    }
                }

                var savePhotoPathPrefix = await configurationInfoRepository.GetConfigurationInfoValue("SavePhotoPathPrefix", MediaBackgroudConst.ModuleName);
                var saveVideoPathPrefix = await configurationInfoRepository.GetConfigurationInfoValue("SaveVideoPathPrefix", MediaBackgroudConst.ModuleName);

                SpaceHelper.DeleteEmptyFolders(savePhotoPathPrefix);
                SpaceHelper.DeleteEmptyFolders(saveVideoPathPrefix);

                mediaExternalLink.IsRemove = true;
                await externalLinkRepository.UpdateAsync(mediaExternalLink);
                await mediaInfoRepository.UpdateAsync(medias);
            }
        });

        return Task.FromResult(true);
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">外链实体</param>
    /// <returns>外链 DTO</returns>
    protected override ExternalLinkDto MapToGetOutputDto(MediaExternalLink entity)
    {
        // TODO: 使用 Mapperly 映射实体到 DTO
        return new ExternalLinkDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Size = entity.Size.ToString(),
            TimeConsumed = entity.TimeConsumed.ToString(),
            IsRemove = entity.IsRemove,
            LinkContent = entity.LinkContent,
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
    /// <returns>外链实体</returns>
    protected override MediaExternalLink MapToEntity(CreateUpdateExternalLinkDto input)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        return new MediaExternalLink
        {
            Name = input.Name,
            Size = input.Size,
            TimeConsumed = long.Parse(input.TimeConsumed),
            IsRemove = input.IsRemove,
            LinkContent = input.LinkContent,
            MediaIds = new List<MediaExternalLinkMediaIds>()
        };
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">外链实体</param>
    protected override void MapToEntity(CreateUpdateExternalLinkDto input, MediaExternalLink entity)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        entity.Name = input.Name;
        entity.Size = input.Size;
        entity.TimeConsumed = long.Parse(input.TimeConsumed);
        entity.IsRemove = input.IsRemove;
        entity.LinkContent = input.LinkContent;
    }
}
