using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Media
{
    /// <summary>
    /// 媒体外链媒体ID
    /// </summary>
    [SugarTable("MediaExternalLinkMediaIds")]
    public class MediaExternalLinkMediaIds : EntityBase<long>
    {
        /// <summary>
        /// 媒体ID
        /// </summary>
        public long MediaId { get; set; }

        /// <summary>
        /// 媒体外链ID
        /// </summary>
        public long MediaExternalLinkId { get; set; }

        /// <summary>
        /// 外链
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public MediaExternalLink ExternalLink { get; set; } = null!;
    }
}
