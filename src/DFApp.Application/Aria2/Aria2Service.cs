using DFApp.Aria2.Repository.Response.TellStatus;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response.TellStatus;
using DFApp.CommonDtos;
using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public async Task<List<string>> GetAllExternalLinks()
        {
            var allResults = await _tellStatusResultRepository.GetListAsync(true);
            List<string> allLinks = new List<string>();

            string reStr = await _configurationInfoRepository.GetConfigurationInfoValue("replaceString", "DFApp.Aria2.Aria2Service");
            string retUrl = await _configurationInfoRepository.GetConfigurationInfoValue("Aria2BtDownloadUrlPrefix", "DFApp.Aria2.Aria2Service");

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
            request.Method = Aria2Consts.AddUri;

            // 添加URLs数组作为第二个参数
            // 注意：Params[0]已经是token，我们需要在索引1插入URLs
            // 但构造函数可能已经添加了token，所以Params.Count为1
            // 我们需要在索引1插入URLs
            request.Params.Insert(1, input.Urls);

            // 如果有保存路径，添加options作为第三个参数
            if (!string.IsNullOrWhiteSpace(input.SavePath))
            {
                var options = new Dictionary<string, object>
                {
                    ["dir"] = input.SavePath
                };
                request.Params.Add(options);
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
