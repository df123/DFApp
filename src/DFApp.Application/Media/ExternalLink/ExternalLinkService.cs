using DFApp.Helper;
using DFApp.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Media.ExternalLink
{
    public class ExternalLinkService : CrudAppService<MediaExternalLink
        , ExternalLinkDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateExternalLinkDto>, IExternalLinkService
    {

        private readonly IMediaRepository _mediaRepository;
        private readonly IQueueManagement _queueManagement;
        private readonly IQueueBase<int> _queueGenerate;
        private readonly IQueueBase<long> _queueMove;

        public ExternalLinkService(IRepository<MediaExternalLink, long> repository
            , IMediaRepository mediaRepository
            , IQueueManagement queueManagement) : base(repository)
        {
            _mediaRepository = mediaRepository;
            _queueManagement = queueManagement;
            _queueGenerate = _queueManagement.GetQueue<int>(Background.MediaBackgroudService.QueueGenerate);
            _queueMove = _queueManagement.GetQueue<long>(Background.MediaBackgroudService.QueueMove);
        }

        public override Task<ExternalLinkDto> CreateAsync(CreateUpdateExternalLinkDto input)
        {
            throw new UserFriendlyException("此接口不允许使用");
        }

        public override Task<ExternalLinkDto> UpdateAsync(long id, CreateUpdateExternalLinkDto input)
        {
            throw new UserFriendlyException("此接口不允许使用");
        }

        public override async Task DeleteAsync(long id)
        {
            MediaExternalLink mediaExternalLink = await ReadOnlyRepository.FirstAsync(x => x.Id == id);
            if (!mediaExternalLink.IsRemove)
            {
                await this.RemoveFileAsync(id);
            }

            await base.DeleteAsync(id);
        }

        public Task<bool> GetExternalLink()
        {
            _queueGenerate.AddItem(1);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveFileAsync(long id)
        {
            //MediaExternalLink mediaExternalLink = await Repository.GetAsync(x => x.Id == id);

            //string[] ids = mediaExternalLink.MediaIds.Split(',');

            //long[] lids = ids.Select(x => long.Parse(x)).ToArray();

            //List<MediaInfo> mediaInfos = await _mediaRepository.GetListAsync(x => lids.Contains(x.Id));

            //foreach (var item in mediaInfos)
            //{
            //    if (!string.IsNullOrWhiteSpace(item.SavePath))
            //    {
            //        SpaceHelper.DeleteFile(item.SavePath);
            //    }
            //    item.IsFileDeleted = true;
            //}
            //mediaExternalLink.IsRemove = true;
            //await Repository.UpdateAsync(mediaExternalLink);
            //await _mediaRepository.UpdateManyAsync(mediaInfos);
            _queueMove.AddItem(id);
            return Task.FromResult(true);

        }
    }
}
