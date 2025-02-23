using DFApp.Media;
using TL;

namespace DFApp.Queue
{
    public class MediaQueueModel
    {
        public MediaInfo? MediaInfos { get; set; }
        public IObject? TObject { get; set; }
        public bool IsPhoto { get; set; }
    }
}
