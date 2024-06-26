﻿using DFApp.Aria2.Repository.Response.TellStatus;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Permissions;
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
    public class Aria2Service : CrudAppService<TellStatusResult, TellStatusResultDto, long>
        , IAria2Service
    {


        private readonly ITellStatusResultRepository _tellStatusResultRepository;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;

        public Aria2Service(ITellStatusResultRepository tellStatusResultRepository
            , IConfigurationInfoRepository configurationInfoRepository)
            : base(tellStatusResultRepository)
        {
            _tellStatusResultRepository = tellStatusResultRepository;
            _configurationInfoRepository = configurationInfoRepository;
            GetPolicyName = DFAppPermissions.Aria2.Default;
            GetListPolicyName = DFAppPermissions.Aria2.Default;
            CreatePolicyName = DFAppPermissions.Aria2.Default;
            UpdatePolicyName = DFAppPermissions.Aria2.Default;
            DeletePolicyName = DFAppPermissions.Aria2.Delete;
        }

        protected override async Task<IQueryable<TellStatusResult>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
        {
            return await ReadOnlyRepository.WithDetailsAsync();
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
                    stringBuilder.AppendLine(Path.Combine(retUrl, file.Path.Replace(reStr, string.Empty)));
                }
                return stringBuilder.ToString();
            }
            return string.Empty;
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

    }

}
