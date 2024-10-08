using DFApp.Background;
using DFApp.Bookkeeping.Expenditure;
using DFApp.Configuration;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Media
{
    [Authorize(DFAppPermissions.Medias.Default)]
    public class MediaInfoService : CrudAppService<
        MediaInfo,
        MediaInfoDto,
        long,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateMediaInfoDto>, IMediaInfoService
    {
        private readonly IRepository<MediaInfo, long> _mediaInfoRepository;

        public MediaInfoService(IRepository<MediaInfo, long> repository) : base(repository)
        {
            _mediaInfoRepository = repository;
            GetPolicyName = DFAppPermissions.Medias.Default;
            GetListPolicyName = DFAppPermissions.Medias.Default;
            CreatePolicyName = DFAppPermissions.Medias.Create;
            UpdatePolicyName = DFAppPermissions.Medias.Edit;
            DeletePolicyName = DFAppPermissions.Medias.Delete;
        }

        protected override async Task<IQueryable<MediaInfo>> CreateFilteredQueryAsync(FilterAndPagedAndSortedResultRequestDto input)
        {

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                Expression<Func<MediaInfo, bool>> filter = x => 
                x.ChatTitle.Contains(input.Filter)
                || x.Message!.Contains(input.Filter)
                || x.MimeType.Contains(input.Filter);

                if(long.TryParse(input.Filter, out long mediaId))
                {
                    filter = filter.Or(x => x.MediaId == mediaId);
                }

                var query = await Repository.GetQueryableAsync();
                return query.Where(filter);
            }
            else
            {
                return await Repository.GetQueryableAsync();
            }

        }

        public async Task<ChartDataDto> GetChartData()
        {
            using (_mediaInfoRepository.DisableTracking())
            {
                List<MediaInfo> lms = await _mediaInfoRepository.GetListAsync();
                var temp = lms.GroupBy(item => item.ChatTitle)
                    .Select(item =>
                    new
                    {
                        Title = item.Key,
                        Count = item.Count()
                    });

                ChartDataDto dto = new ChartDataDto();
                dto.Labels = new List<string>(temp.Count());
                dto.Datas = new List<int>(temp.Count());
                foreach (var item in temp)
                {
                    dto.Labels.Add(item.Title!);
                    dto.Datas.Add(item.Count);
                }
                return dto;
            }

        }

    }
}
