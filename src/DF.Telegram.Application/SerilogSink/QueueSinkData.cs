using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DF.Telegram.SerilogSink
{
    public class QueueSinkData
    {
        private static ConcurrentQueue<QueueMessage> _queue = new ConcurrentQueue<QueueMessage>();

        public static ConcurrentQueue<QueueMessage> Queue { get { return _queue; } }
    }
}
