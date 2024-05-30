using DFApp.Background;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Media.ExternalLink
{
    [Authorize(DFAppPermissions.Medias.Default)]
    public class ExternalLinkService : CrudAppService<MediaExternalLink
        , ExternalLinkDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateExternalLinkDto>, IExternalLinkService
    {

        private readonly IQueueManagement _queueManagement;
        private readonly IQueueBase<int> _queueGenerate;
        private readonly IQueueBase<long> _queueMove;

        public ExternalLinkService(IRepository<MediaExternalLink, long> repository
            , IQueueManagement queueManagement) : base(repository)
        {
            _queueManagement = queueManagement;
            _queueGenerate = _queueManagement.GetQueue<int>(MediaBackgroudConst.QueueGenerate);
            _queueMove = _queueManagement.GetQueue<long>(MediaBackgroudConst.QueueMove);
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
            if (id <= 0)
            {
                throw new UserFriendlyException("ID要大于0");
            }

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
            if (id <= 0)
            {
                throw new UserFriendlyException("ID要大于0");
            }
            _queueMove.AddItem(id);
            return Task.FromResult(true);

        }
    }
}
