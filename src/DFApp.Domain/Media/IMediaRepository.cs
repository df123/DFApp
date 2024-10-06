using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Media
{
    public interface IMediaRepository : IRepository<MediaInfo, long>
    {

        Task<long> GetDownloadsSize();

        Task<MediaInfo[]> GetMediaNotReturn();

    }
}
