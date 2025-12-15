using BencodeNET.Parsing;
using BencodeNET.Torrents;
using DFApp.Aria2.Repository.Response.TellStatus;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response.TellStatus;
using DFApp.CommonDtos;
using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Aria2
{
    [Authorize(DFAppPermissions.Aria2.Default)]
    public class Aria2Service : CrudAppService<
    TellStatusResult
    , TellStatusResultDto
    , long
    , FilterAndPagedAndSortedResultRequestDto
    , TellStatusResultDto>
        , IAria2Service
    {


        private readonly ITellStatusResultRepository _tellStatusResultRepository;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly IQueueManagement _queueManagement;

        public Aria2Service(ITellStatusResultRepository tellStatusResultRepository
            , IConfigurationInfoRepository configurationInfoRepository
            , IQueueManagement queueManagement)
            : base(tellStatusResultRepository)
        {
            _tellStatusResultRepository = tellStatusResultRepository;
            _configurationInfoRepository = configurationInfoRepository;
            _queueManagement = queueManagement;
            GetPolicyName = DFAppPermissions.Aria2.Default;
            GetListPolicyName = DFAppPermissions.Aria2.Default;
            CreatePolicyName = DFAppPermissions.Aria2.Default;
            UpdatePolicyName = DFAppPermissions.Aria2.Default;
            DeletePolicyName = DFAppPermissions.Aria2.Delete;
        }

        protected override async Task<IQueryable<TellStatusResult>> CreateFilteredQueryAsync(FilterAndPagedAndSortedResultRequestDto input)
        {
            return await ReadOnlyRepository.WithDetailsAsync();
        }

        public override async Task<PagedResultDto<TellStatusResultDto>> GetListAsync(FilterAndPagedAndSortedResultRequestDto input)
        {

            var datas = await base.GetListAsync(input);

            foreach (var data in datas.Items)
            {
                foreach (var file in data.files)
                {
                    if (!string.IsNullOrEmpty(file.Path))
                    {
                        file.Path = Path.GetFileName(file.Path);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(input.Filter))
            {
                return datas;
            }

            List<TellStatusResultDto> resultDtos = new List<TellStatusResultDto>();

            foreach (var data in datas.Items)
            {
                if (data.files != null && data.files.FirstOrDefault(x => x.Path.Contains(input.Filter)) != null)
                {
                    resultDtos.Add(data);
                }
            }

            datas.TotalCount = resultDtos.Count;
            datas.Items = resultDtos;

            return datas;
        }

        [Authorize(DFAppPermissions.Aria2.Link)]
        public async Task<string> GetExternalLink(long id)
        {
            if (id <= 0)
            {
                throw new UserFriendlyException("ID要大于0");
            }

            var data = await _tellStatusResultRepository.GetAsync(id);
            if (data != null && data.Files != null && data.Files.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder(64);
                string reStr = await _configurationInfoRepository.GetConfigurationInfoValue("replaceString", "DFApp.Aria2.Aria2Service");
                string retUrl = await _configurationInfoRepository.GetConfigurationInfoValue("Aria2BtDownloadUrlPrefix", "DFApp.Aria2.Aria2Service");
                foreach (var file in data.Files)
                {
                    if (string.IsNullOrEmpty(file.Path))
                    {
                        continue;
                    }

                    stringBuilder.AppendLine(Path.Combine(retUrl, file.Path.Replace(reStr, string.Empty)));
                }
                return stringBuilder.ToString();
            }
            return string.Empty;
        }

        [Authorize(DFAppPermissions.Aria2.Link)]
        public async Task<List<string>> GetAllExternalLinks(bool videoOnly = false)
        {
            var allResults = await _tellStatusResultRepository.GetListAsync(true);
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

            foreach (var result in allResults)
            {
                if (result.Files != null && result.Files.Count > 0)
                {

                    foreach (var file in result.Files)
                    {
                        if (string.IsNullOrEmpty(file.Path) || file.Path.Contains("[METADATA]", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // 如果启用VideoOnly过滤，检查文件扩展名
                        if (videoOnly)
                        {
                            var extension = Path.GetExtension(file.Path);
                            if (!videoExtensions.Contains(extension))
                            {
                                continue;
                            }
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


        public override Task<TellStatusResultDto> CreateAsync(TellStatusResultDto input)
        {
            throw new UserFriendlyException("此接口不允许使用");
        }

        public override Task<TellStatusResultDto> UpdateAsync(long id, TellStatusResultDto input)
        {
            throw new UserFriendlyException("此接口不允许使用");
        }

        public override async Task DeleteAsync(long id)
        {
            if (id <= 0)
            {
                throw new UserFriendlyException("ID要大于0");
            }

            if (await ReadOnlyRepository.AnyAsync(x => x.Id == id))
            {
                var data = await Repository.GetAsync(id);
                if (data != null && data.Files != null && data.Files.Count > 0)
                {
                    foreach (var file in data.Files)
                    {
                        SpaceHelper.DeleteFile(file.Path!);
                    }
                }

                await base.DeleteAsync(id);
            }
        }

        public async Task DeleteAllAsync()
        {
            var datas = await ReadOnlyRepository.GetListAsync();
            foreach (var data in datas)
            {
                await DeleteAsync(data.Id);
            }
            string reStr = await _configurationInfoRepository.GetConfigurationInfoValue("replaceString", "DFApp.Aria2.Aria2Service");
            SpaceHelper.DeleteEmptyFolders(reStr);
        }

        [Authorize(DFAppPermissions.Aria2.Delete)]
        public async Task ClearDownloadDirectoryAsync()
        {
            string downloadDirectory = await _configurationInfoRepository.GetConfigurationInfoValue("Aria2DownloadPath", "");
            if (string.IsNullOrEmpty(downloadDirectory) || string.IsNullOrWhiteSpace(downloadDirectory))
            {
                throw new UserFriendlyException("下载目录路径未配置");
            }

            if (!Directory.Exists(downloadDirectory))
            {
                throw new UserFriendlyException($"下载目录不存在: {downloadDirectory}");
            }

            SpaceHelper.ClearDirectory(downloadDirectory);
        }

        /// <summary>
        /// 解析torrent文件并返回视频文件索引
        /// </summary>
        /// <param name="torrentUrl">torrent文件URL</param>
        /// <returns>视频文件索引列表（从1开始）</returns>
        private async Task<List<int>> GetVideoFileIndicesFromTorrentAsync(string torrentUrl)
        {
            try
            {
                // 下载torrent文件
                using var httpClient = new HttpClient();
                var torrentBytes = await httpClient.GetByteArrayAsync(torrentUrl);

                // 解析torrent文件
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

                // 查找视频文件索引
                var videoIndices = new List<int>();
                for (int i = 0; i < files.Count(); i++)
                {
                    var file = files[i];
                    var fileName = file.FileName;
                    var extension = Path.GetExtension(fileName);

                    if (videoExtensions.Contains(extension))
                    {
                        // 索引从1开始（Aria2的select-file使用1-based索引）
                        videoIndices.Add(i + 1);
                    }
                }

                return videoIndices;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "解析torrent文件失败: {TorrentUrl}", torrentUrl);
                return new List<int>();
            }
        }

        /// <summary>
        /// 添加选项到Aria2请求
        /// </summary>
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

        [Authorize(DFAppPermissions.Aria2.Default)]
        public async Task<AddDownloadResponseDto> AddDownloadAsync(AddDownloadRequestDto input)
        {
            if (input == null || input.Urls == null || input.Urls.Count == 0)
            {
                throw new UserFriendlyException("URL列表不能为空");
            }

            // 从配置获取aria2secret
            string aria2secret = await _configurationInfoRepository.GetConfigurationInfoValue("aria2secret", "DFApp.Aria2.Aria2Service");

            // 创建Aria2Request - 构造函数会添加token到Params[0]
            var request = new Aria2Request(Guid.NewGuid().ToString(), aria2secret);

            // 判断是否包含torrent文件URL或磁力链接
            bool hasTorrentFile = input.Urls.Any(url => url.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase));
            bool hasMagnet = input.Urls.Any(url => url.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase));
            bool hasTorrent = hasTorrentFile || hasMagnet;

            // 处理VideoOnly选项
            if (input.VideoOnly && hasTorrentFile)
            {
                // 对于.torrent文件，使用addTorrent方法并设置select-file选项
                string torrentUrl = input.Urls.First(url => url.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase));
                var videoIndices = await GetVideoFileIndicesFromTorrentAsync(torrentUrl);

                if (videoIndices.Count > 0)
                {
                    // 构建select-file字符串，例如"1,3,5"
                    string selectFile = string.Join(",", videoIndices);

                    // 下载torrent文件内容
                    using var httpClient = new HttpClient();
                    var torrentBytes = await httpClient.GetByteArrayAsync(torrentUrl);
                    string torrentBase64 = Convert.ToBase64String(torrentBytes);

                    // 使用addTorrent方法
                    request.Method = Aria2Consts.AddTorrent;
                    // 参数：token, torrent内容base64, urls数组（空）, options
                    request.Params.Insert(1, torrentBase64);
                    request.Params.Insert(2, new List<object>()); // 空urls数组

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

                    Logger.LogInformation("VideoOnly过滤已应用，选择文件索引: {SelectFile}", selectFile);
                }
                else
                {
                    Logger.LogWarning("VideoOnly已启用，但未在torrent文件中找到视频文件，将下载全部文件");
                    // 继续使用AddUri方法
                    request.Method = Aria2Consts.AddUri;
                    request.Params.Insert(1, input.Urls);
                    AddOptionsToRequest(request, input.SavePath, input.Options);
                }
            }
            else
            {
                // 普通下载或VideoOnly未启用
                request.Method = hasTorrent ? Aria2Consts.AddUri : Aria2Consts.AddUri;
                request.Params.Insert(1, input.Urls);

                // 如果指定了VideoOnly但不是.torrent文件，记录警告
                if (input.VideoOnly)
                {
                    if (hasMagnet)
                    {
                        Logger.LogWarning("VideoOnly选项暂不支持磁力链接，将下载全部文件");
                    }
                    else if (!hasTorrentFile)
                    {
                        Logger.LogWarning("VideoOnly选项仅支持.torrent文件，将下载全部文件");
                    }
                }

                // 添加选项
                AddOptionsToRequest(request, input.SavePath, input.Options);
            }

            // 将请求转换为DTO并添加到队列
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

    }

}
