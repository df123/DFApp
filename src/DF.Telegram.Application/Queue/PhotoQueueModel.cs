using DF.Telegram.Media;
using TL;

namespace DF.Telegram.Queue
{
    public class PhotoQueueModel
    {
        public Photo? TObject { get; set; }
        public MediaInfo? MediaInfos { get; set; }
    }
}
