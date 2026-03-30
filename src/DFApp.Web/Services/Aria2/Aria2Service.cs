using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Parsing;
using BencodeNET.Torrents;
using DFApp.Aria2;
using DFApp.Aria2.Repository.Response.TellStatus;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response.TellStatus;
using DFApp.FileFilter;
using DFApp.Helper;
using DFApp.Queue;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Aria2;

/// <summary>
/// Aria2 下载管理服务
/// </summary>
public class Aria2Service : CrudServiceBase<
    TellStatusResult,
    long,
    TellStatusResultDto,
    TellStatusResultDto,
    TellStatusResultDto>
{
    private readonly ISqlSugarRepository<FilesItem, int> _filesItemRepository;
    private readonly IConfigurationInfoRepository _configurationInfoRepository;
    private readonly IQueueManagement _queueManagement;
    private readonly IKeywordFilterRuleRepository _keywordFilterRuleRepository;
    private readonly ILogger<Aria2Service> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">TellStatusResult 仓储接口</param>
    /// <param name="filesItemRepository">FilesItem 仓储接口</param>
    /// <param name="configurationInfoRepository">配置信息仓储接口</param>
    /// <param name="queueManagement">队列管理接口</param>
    /// <param name="keywordFilterRuleRepository">关键词过滤规则仓储接口</param>
    /// <param name="logger">日志记录器</param>
    public Aria2Service(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<TellStatusResult, long> repository,
        ISqlSugarRepository<FilesItem, int> filesItemRepository,
        IConfigurationInfoRepository configurationInfoRepository,
        IQueueManagement queueManagement,
        IKeywordFilterRuleRepository keywordFilterRuleRepository,
        ILogger<Aria2Service> logger)
        : base(currentUser, permissionChecker, repository)
    {
        _filesItemRepository = filesItemRepository;
        _configurationInfoRepository = configurationInfoRepository;
        _queueManagement = queueManagement;
        _keywordFilterRuleRepository = keywordFilterRuleRepository;
        _logger = logger;
    }

    /// <summary>
    /// 根据过滤条件分页查询下载记录
    /// 原始代码使用 WithDetailsAsync 导航查询 Files，现改为外键查询
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public async Task<(List<TellStatusResultDto> Items, int TotalCount)> GetFilteredListAsync(
        string? filter, int pageIndex, int pageSize)
    {
        var query = Repository.GetQueryable();

        // 获取总数
        var totalCount = query.Count();

        // 分页查询
        var items = query
            .OrderByDescending(x => x.CreationTime)
            .ToPageList(pageIndex, pageSize);

        // 获取关联的文件信息（替代导航查询）
        var resultIds = items.Select(x => x.Id).ToList();
        var filesItems = await _filesItemRepository.GetListAsync(f => resultIds.Contains(f.ResultId));
        var filesGrouped = filesItems.GroupBy(f => f.ResultId).ToDictionary(g => g.Key, g => g.ToList());

        // 手动映射 DTO
        var dtos = new List<TellStatusResultDto>();
        foreach (var entity in items)
        {
            var dto = MapToGetOutputDto(entity);

            // 附加文件信息
            if (filesGrouped.TryGetValue(entity.Id, out var files))
            {
                dto.Files = files.Select(MapFilesItemToDto).ToList();

                // 截断路径为文件名
                foreach (var file in dto.Files)
                {
                    if (!string.IsNullOrEmpty(file.Path))
                    {
                        file.Path = Path.GetFileName(file.Path);
                    }
                }
            }

            dtos.Add(dto);
        }

        // 应用文件名过滤
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var filteredDtos = new List<TellStatusResultDto>();
            foreach (var dto in dtos)
            {
                if (dto.Files != null && dto.Files.Any(f => f.Path != null && f.Path.Contains(filter)))
                {
                    filteredDtos.Add(dto);
                }
            }

            return (filteredDtos, filteredDtos.Count);
        }

        return (dtos, totalCount);
    }

    /// <summary>
    /// 获取指定下载记录的外链
    /// </summary>
    /// <param name="id">下载记录 ID</param>
    /// <returns>外链字符串</returns>
    public async Task<string> GetExternalLinkAsync(long id)
    {
        if (id <= 0)
        {
            throw new BusinessException("ID要大于0");
        }

        var data = await Repository.GetByIdAsync(id);
        if (data != null)
        {
            // 通过外键查询文件列表
            var files = await _filesItemRepository.GetListAsync(f => f.ResultId == data.Id);
            if (files != null && files.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder(64);
                string reStr = await _configurationInfoRepository.GetConfigurationInfoValue("replaceString", "DFApp.Aria2.Aria2Service");
                string retUrl = await _configurationInfoRepository.GetConfigurationInfoValue("Aria2BtDownloadUrlPrefix", "DFApp.Aria2.Aria2Service");
                foreach (var file in files)
                {
                    if (string.IsNullOrEmpty(file.Path))
                    {
                        continue;
                    }

                    stringBuilder.AppendLine(Path.Combine(retUrl, file.Path.Replace(reStr, string.Empty)));
                }
                return stringBuilder.ToString();
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取所有下载记录的外链列表
    /// </summary>
    /// <param name="videoOnly">是否只获取视频文件</param>
    /// <returns>外链列表</returns>
    public async Task<List<string>> GetAllExternalLinksAsync(bool videoOnly = true)
    {
        var allResults = await Repository.GetListAsync();
        List<string> allLinks = new List<string>();

        string reStr = await _configurationInfoRepository.GetConfigurationInfoValue("replaceString", "DFApp.Aria2.Aria2Service");
        string retUrl = await _configurationInfoRepository.GetConfigurationInfoValue("Aria2BtDownloadUrlPrefix", "DFApp.Aria2.Aria2Service");

        // 视频文件扩展名列表
        var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm",
            ".m4v", ".mpg", ".mpeg", ".3gp", ".ogg", ".ts", ".m2ts",
            ".vob", ".rm", ".rmvb", ".asf", ".divx", ".xvid"
        };

        // 批量获取所有文件
        var allResultIds = allResults.Select(r => r.Id).ToList();
        var allFiles = await _filesItemRepository.GetListAsync(f => allResultIds.Contains(f.ResultId));
        var filesByResult = allFiles.GroupBy(f => f.ResultId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var result in allResults)
        {
            if (filesByResult.TryGetValue(result.Id, out var files) && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (string.IsNullOrEmpty(file.Path) || file.Path.Contains("[METADATA]", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // 如果启用 VideoOnly 过滤，检查文件扩展名
                    if (videoOnly)
                    {
                        var extension = Path.GetExtension(file.Path);
                        if (!videoExtensions.Contains(extension))
                        {
                            continue;
                        }
                    }

                    // 应用关键词过滤规则
                    bool shouldFilter = await _keywordFilterRuleRepository.ShouldFilterFileAsync(file.Path);
                    if (shouldFilter)
                    {
                        continue;
                    }

                    string filePath = Path.Combine(retUrl, file.Path.Replace(reStr, string.Empty));
                    if (!allLinks.Contains(filePath))
                    {
                        allLinks.Add(filePath);
                    }
                }
            }
        }

        return allLinks;
    }

    /// <summary>
    /// 创建操作不允许使用
    /// </summary>
    public override Task<TellStatusResultDto> CreateAsync(TellStatusResultDto input)
    {
        throw new BusinessException("此接口不允许使用");
    }

    /// <summary>
    /// 更新操作不允许使用
    /// </summary>
    public override Task<TellStatusResultDto> UpdateAsync(long id, TellStatusResultDto input)
    {
        throw new BusinessException("此接口不允许使用");
    }

    /// <summary>
    /// 删除下载记录及关联文件
    /// </summary>
    /// <param name="id">下载记录 ID</param>
    public override async Task DeleteAsync(long id)
    {
        if (id <= 0)
        {
            throw new BusinessException("ID要大于0");
        }

        var exists = Repository.GetQueryable().Any(x => x.Id == id);
        if (exists)
        {
            var data = await Repository.GetByIdAsync(id);
            if (data != null)
            {
                // 通过外键查询文件列表
                var files = await _filesItemRepository.GetListAsync(f => f.ResultId == data.Id);
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        SpaceHelper.DeleteFile(file.Path!);
                    }
                }
            }

            await base.DeleteAsync(id);
        }
    }

    /// <summary>
    /// 删除所有下载记录
    /// </summary>
    public async Task DeleteAllAsync()
    {
        var datas = await Repository.GetListAsync();
        foreach (var data in datas)
        {
            await DeleteAsync(data.Id);
        }
        string reStr = await _configurationInfoRepository.GetConfigurationInfoValue("replaceString", "DFApp.Aria2.Aria2Service");
        SpaceHelper.DeleteEmptyFolders(reStr);
    }

    /// <summary>
    /// 清空下载目录
    /// </summary>
    public async Task ClearDownloadDirectoryAsync()
    {
        string downloadDirectory = await _configurationInfoRepository.GetConfigurationInfoValue("Aria2DownloadPath", "");
        if (string.IsNullOrEmpty(downloadDirectory) || string.IsNullOrWhiteSpace(downloadDirectory))
        {
            throw new BusinessException("下载目录路径未配置");
        }

        if (!Directory.Exists(downloadDirectory))
        {
            throw new BusinessException($"下载目录不存在: {downloadDirectory}");
        }

        SpaceHelper.ClearDirectory(downloadDirectory);
    }

    /// <summary>
    /// 添加下载任务
    /// </summary>
    /// <param name="input">下载请求 DTO</param>
    /// <returns>下载响应 DTO</returns>
    public async Task<AddDownloadResponseDto> AddDownloadAsync(AddDownloadRequestDto input)
    {
        if (input == null || input.Urls == null || input.Urls.Count == 0)
        {
            throw new BusinessException("URL列表不能为空");
        }

        // 从配置获取 aria2secret
        string aria2secret = await _configurationInfoRepository.GetConfigurationInfoValue("aria2secret", "DFApp.Aria2.Aria2Service");

        // 创建 Aria2Request - 构造函数会添加 token 到 Params[0]
        var request = new Aria2Request(Guid.NewGuid().ToString(), aria2secret);

        // 判断是否包含 torrent 文件 URL 或磁力链接
        bool hasTorrentFile = input.Urls.Any(url => url.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase));
        bool hasMagnet = input.Urls.Any(url => url.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase));
        bool hasTorrent = hasTorrentFile || hasMagnet;

        // 处理过滤选项（VideoOnly 和关键词过滤）
        bool shouldFilterTorrent = (input.VideoOnly || input.EnableKeywordFilter) && hasTorrentFile;

        if (shouldFilterTorrent)
        {
            // 对于 .torrent 文件，使用 addTorrent 方法并设置 select-file 选项
            string torrentUrl = input.Urls.First(url => url.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase));
            var filteredIndices = await GetFilteredFileIndicesFromTorrentAsync(torrentUrl, input.VideoOnly, input.EnableKeywordFilter);

            if (filteredIndices.Count > 0)
            {
                // 构建 select-file 字符串，例如"1,3,5"
                string selectFile = string.Join(",", filteredIndices);

                // 下载 torrent 文件内容
                using var httpClient = new HttpClient();
                var torrentBytes = await httpClient.GetByteArrayAsync(torrentUrl);
                string torrentBase64 = Convert.ToBase64String(torrentBytes);

                // 使用 addTorrent 方法
                request.Method = Aria2Consts.AddTorrent;
                // 参数：token, torrent 内容 base64, urls 数组（空）, options
                request.Params.Insert(1, torrentBase64);
                request.Params.Insert(2, new List<object>());

                // 构建选项
                var options = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(input.SavePath))
                {
                    options["dir"] = input.SavePath;
                }
                options["select-file"] = selectFile;

                // 合并用户自定义选项
                if (input.Options != null)
                {
                    foreach (var kvp in input.Options)
                    {
                        options[kvp.Key] = kvp.Value;
                    }
                }

                if (options.Count > 0)
                {
                    request.Params.Add(options);
                }

                string filterDescription = GetFilterDescription(input.VideoOnly, input.EnableKeywordFilter);
                _logger.LogInformation("{FilterDescription}过滤已应用，选择文件索引: {SelectFile}", filterDescription, selectFile);
            }
            else
            {
                _logger.LogWarning("过滤已启用，但未在 torrent 文件中找到符合条件的文件，将下载全部文件");
                // 继续使用 AddUri 方法
                request.Method = Aria2Consts.AddUri;
                request.Params.Insert(1, input.Urls);
                AddOptionsToRequest(request, input.SavePath, input.Options);
            }
        }
        else
        {
            // 普通下载或没有过滤选项启用
            // 对于非 .torrent 文件，应用关键词过滤（如果启用）
            List<string> filteredUrls = input.Urls;
            if (input.EnableKeywordFilter && !hasTorrentFile)
            {
                filteredUrls = await FilterUrlsByKeywordsAsync(input.Urls);
                if (filteredUrls.Count == 0)
                {
                    _logger.LogWarning("关键词过滤已启用，但所有 URL 都被过滤，将取消下载");
                    throw new BusinessException("所有URL都被关键词过滤规则过滤，没有文件可下载");
                }
                else if (filteredUrls.Count < input.Urls.Count)
                {
                    _logger.LogInformation("关键词过滤已应用，从{OriginalCount}个URL中过滤掉{FilteredCount}个，剩余{RemainingCount}个",
                        input.Urls.Count, input.Urls.Count - filteredUrls.Count, filteredUrls.Count);
                }
            }

            request.Method = Aria2Consts.AddUri;
            request.Params.Insert(1, filteredUrls);

            // 如果指定了 VideoOnly 但不是 .torrent 文件，记录警告
            if (input.VideoOnly)
            {
                if (hasMagnet)
                {
                    _logger.LogWarning("VideoOnly选项暂不支持磁力链接，将下载全部文件");
                }
                else if (!hasTorrentFile)
                {
                    _logger.LogWarning("VideoOnly选项仅支持.torrent文件，将下载全部文件");
                }
            }

            // 如果指定了 EnableKeywordFilter 但不是 .torrent 文件，记录信息
            if (input.EnableKeywordFilter && !hasTorrentFile)
            {
                _logger.LogInformation("关键词过滤已应用于URL列表");
            }

            // 添加选项
            AddOptionsToRequest(request, input.SavePath, input.Options);
        }

        // 将请求转换为 DTO 并添加到队列
        var requestDto = new Aria2RequestDto
        {
            JSONRPC = request.JSONRPC,
            Method = request.Method,
            Id = request.Id,
            Params = request.Params
        };

        // 添加到队列
        _queueManagement.AddQueueValue("Aria2RequestQueue", new List<Aria2RequestDto> { requestDto });

        return new AddDownloadResponseDto { Id = request.Id };
    }

    /// <summary>
    /// 根据 VideoOnly 和关键词过滤获取 torrent 文件中过滤后的文件索引
    /// </summary>
    /// <param name="torrentUrl">torrent 文件 URL</param>
    /// <param name="videoOnly">是否只下载视频</param>
    /// <param name="enableKeywordFilter">是否启用关键词过滤</param>
    /// <returns>过滤后的文件索引列表</returns>
    private async Task<List<int>> GetFilteredFileIndicesFromTorrentAsync(string torrentUrl, bool videoOnly, bool enableKeywordFilter)
    {
        try
        {
            // 下载 torrent 文件
            using var httpClient = new HttpClient();
            var torrentBytes = await httpClient.GetByteArrayAsync(torrentUrl);

            // 解析 torrent 文件
            using var stream = new MemoryStream(torrentBytes);
            var parser = new TorrentParser();
            var torrent = parser.Parse(stream);

            // 获取文件列表
            var files = torrent.Files;

            // 视频文件扩展名列表
            var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm",
                ".m4v", ".mpg", ".mpeg", ".3gp", ".ogg", ".ts", ".m2ts",
                ".vob", ".rm", ".rmvb", ".asf", ".divx", ".xvid"
            };

            // 查找符合条件的文件索引
            var filteredIndices = new List<int>();
            for (int i = 0; i < files.Count(); i++)
            {
                var file = files[i];
                var fileName = file.FileName;
                var extension = Path.GetExtension(fileName);

                // 检查 VideoOnly 条件
                if (videoOnly && !videoExtensions.Contains(extension))
                {
                    continue;
                }

                // 检查关键词过滤条件
                if (enableKeywordFilter)
                {
                    bool shouldFilter = await _keywordFilterRuleRepository.ShouldFilterFileAsync(fileName);
                    if (shouldFilter)
                    {
                        continue;
                    }
                }

                // 索引从 1 开始（Aria2 的 select-file 使用 1-based 索引）
                filteredIndices.Add(i + 1);
            }

            return filteredIndices;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析torrent文件失败: {TorrentUrl}", torrentUrl);
            return new List<int>();
        }
    }

    /// <summary>
    /// 获取过滤描述字符串
    /// </summary>
    /// <param name="videoOnly">是否只下载视频</param>
    /// <param name="enableKeywordFilter">是否启用关键词过滤</param>
    /// <returns>过滤描述</returns>
    private string GetFilterDescription(bool videoOnly, bool enableKeywordFilter)
    {
        if (videoOnly && enableKeywordFilter)
        {
            return "VideoOnly和关键词";
        }
        else if (videoOnly)
        {
            return "VideoOnly";
        }
        else if (enableKeywordFilter)
        {
            return "关键词";
        }
        else
        {
            return "无";
        }
    }

    /// <summary>
    /// 根据关键词过滤规则过滤 URL 列表
    /// </summary>
    /// <param name="urls">URL 列表</param>
    /// <returns>过滤后的 URL 列表</returns>
    private async Task<List<string>> FilterUrlsByKeywordsAsync(List<string> urls)
    {
        var filteredUrls = new List<string>();

        foreach (var url in urls)
        {
            // 从 URL 中提取文件名
            string fileName = ExtractFileNameFromUrl(url);
            if (string.IsNullOrEmpty(fileName))
            {
                // 无法提取文件名，保留 URL
                filteredUrls.Add(url);
                continue;
            }

            // 检查是否应该过滤
            bool shouldFilter = await _keywordFilterRuleRepository.ShouldFilterFileAsync(fileName);
            if (!shouldFilter)
            {
                filteredUrls.Add(url);
            }
        }

        return filteredUrls;
    }

    /// <summary>
    /// 从 URL 中提取文件名
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>文件名</returns>
    private string ExtractFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            string path = uri.AbsolutePath;
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            // 获取路径的最后一部分作为文件名
            string fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            // 移除查询参数（如果有）
            int queryIndex = fileName.IndexOf('?');
            if (queryIndex > 0)
            {
                fileName = fileName.Substring(0, queryIndex);
            }

            // 移除片段（如果有）
            int fragmentIndex = fileName.IndexOf('#');
            if (fragmentIndex > 0)
            {
                fileName = fileName.Substring(0, fragmentIndex);
            }

            return fileName;
        }
        catch (UriFormatException)
        {
            // URL 格式无效，尝试简单提取
            int lastSlash = url.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < url.Length - 1)
            {
                string fileName = url.Substring(lastSlash + 1);

                // 移除查询参数和片段
                int queryIndex = fileName.IndexOf('?');
                if (queryIndex > 0)
                {
                    fileName = fileName.Substring(0, queryIndex);
                }

                int fragmentIndex = fileName.IndexOf('#');
                if (fragmentIndex > 0)
                {
                    fileName = fileName.Substring(0, fragmentIndex);
                }

                return fileName;
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// 添加选项到 Aria2 请求
    /// </summary>
    /// <param name="request">Aria2 请求</param>
    /// <param name="savePath">保存路径</param>
    /// <param name="customOptions">自定义选项</param>
    private void AddOptionsToRequest(Aria2Request request, string? savePath, Dictionary<string, object>? customOptions)
    {
        var options = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(savePath))
        {
            options["dir"] = savePath;
        }

        if (customOptions != null)
        {
            foreach (var kvp in customOptions)
            {
                options[kvp.Key] = kvp.Value;
            }
        }

        if (options.Count > 0)
        {
            request.Params.Add(options);
        }
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">TellStatusResult 实体</param>
    /// <returns>TellStatusResult DTO</returns>
    protected override TellStatusResultDto MapToGetOutputDto(TellStatusResult entity)
    {
        // TODO: 使用 Mapperly 映射实体到 DTO
        return new TellStatusResultDto
        {
            Id = entity.Id,
            Bitfield = entity.Bitfield,
            CompletedLength = entity.CompletedLength?.ToString(),
            Connections = entity.Connections?.ToString(),
            Dir = entity.Dir,
            DownloadSpeed = entity.DownloadSpeed?.ToString(),
            ErrorCode = entity.ErrorCode,
            ErrorMessage = entity.ErrorMessage,
            Gid = entity.GID,
            NumPieces = entity.NumPieces?.ToString(),
            PieceLength = entity.PieceLength?.ToString(),
            Status = entity.Status,
            TotalLength = entity.TotalLength?.ToString(),
            UploadLength = entity.UploadLength?.ToString(),
            UploadSpeed = entity.UploadSpeed?.ToString(),
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId
        };
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体（不允许使用）
    /// </summary>
    protected override TellStatusResult MapToEntity(TellStatusResultDto input)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        return new TellStatusResult
        {
            Bitfield = input.Bitfield,
            CompletedLength = long.TryParse(input.CompletedLength, out var cl) ? cl : null,
            Connections = long.TryParse(input.Connections, out var conn) ? conn : null,
            Dir = input.Dir,
            DownloadSpeed = long.TryParse(input.DownloadSpeed, out var ds) ? ds : null,
            ErrorCode = input.ErrorCode,
            ErrorMessage = input.ErrorMessage,
            GID = input.Gid,
            NumPieces = long.TryParse(input.NumPieces, out var np) ? np : null,
            PieceLength = long.TryParse(input.PieceLength, out var pl) ? pl : null,
            Status = input.Status,
            TotalLength = long.TryParse(input.TotalLength, out var tl) ? tl : null,
            UploadLength = long.TryParse(input.UploadLength, out var ul) ? ul : null,
            UploadSpeed = long.TryParse(input.UploadSpeed, out var us) ? us : null
        };
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体（不允许使用）
    /// </summary>
    protected override void MapToEntity(TellStatusResultDto input, TellStatusResult entity)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        entity.Bitfield = input.Bitfield;
        entity.CompletedLength = long.TryParse(input.CompletedLength, out var cl) ? cl : null;
        entity.Connections = long.TryParse(input.Connections, out var conn) ? conn : null;
        entity.Dir = input.Dir;
        entity.DownloadSpeed = long.TryParse(input.DownloadSpeed, out var ds) ? ds : null;
        entity.ErrorCode = input.ErrorCode;
        entity.ErrorMessage = input.ErrorMessage;
        entity.GID = input.Gid;
        entity.NumPieces = long.TryParse(input.NumPieces, out var np) ? np : null;
        entity.PieceLength = long.TryParse(input.PieceLength, out var pl) ? pl : null;
        entity.Status = input.Status;
        entity.TotalLength = long.TryParse(input.TotalLength, out var tl) ? tl : null;
        entity.UploadLength = long.TryParse(input.UploadLength, out var ul) ? ul : null;
        entity.UploadSpeed = long.TryParse(input.UploadSpeed, out var us) ? us : null;
    }

    /// <summary>
    /// 将 FilesItem 实体映射为 DTO
    /// </summary>
    /// <param name="file">文件项实体</param>
    /// <returns>文件项 DTO</returns>
    private FilesItemDto MapFilesItemToDto(FilesItem file)
    {
        // TODO: 使用 Mapperly 映射实体到 DTO
        return new FilesItemDto
        {
            CompletedLength = file.CompletedLength?.ToString(),
            Index = file.Index?.ToString(),
            Length = file.Length?.ToString(),
            Path = file.Path,
            Selected = file.Selected?.ToString()
        };
    }
}
