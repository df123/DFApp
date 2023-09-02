using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DF.Telegram.Media
{
    public interface IMediaRepository : IRepository<MediaInfo, long>
    {
        Task<MediaInfo[]> GetByAccessHashID(long accessHash, long tId, long size);

        Task<MediaInfo[]> GetByValueSHA1([NotNull] string valueSHA1);

        Task<long> GetDownloadsSize();

        Task<MediaInfo[]> GetMediaNotReturn();

        Task<MediaInfo[]> GetAllContainSoftDelete();

        Task<List<MediaInfo>> GetAllTitleNotNullContainSoftDelete();
    }
}
