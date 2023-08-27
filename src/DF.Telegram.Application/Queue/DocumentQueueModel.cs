using DF.Telegram.Media;
using TL;

namespace DF.Telegram.Queue
{
    public class DocumentQueueModel
    {
        public Document? TObject { get; set; }
        public MediaInfo? MediaInfos { get; set; }
    }
}
