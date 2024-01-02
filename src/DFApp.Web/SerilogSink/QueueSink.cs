using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Text;
using System;
using DFApp.SerilogSink;

namespace DFApp.Web.SerilogSink
{
    public class QueueSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly ConcurrentQueue<QueueMessage> _queue;
        private int _bufferTotalSize;
        private readonly int _limitSize;

        public QueueSink(IFormatProvider formatProvider
            , ConcurrentQueue<QueueMessage> queue)
        {
            _formatProvider = formatProvider;
            _queue = queue;
            _bufferTotalSize = 0;
            _limitSize = 256 * 1024;
        }

        public void Emit(LogEvent logEvent)
        {
            string message = logEvent.RenderMessage(_formatProvider);

            int byteCount = Encoding.UTF8.GetByteCount(message);

            QueueMessage mm = new QueueMessage()
            {
                Msg = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}",
                BufferSize = byteCount
            };

            if ((_bufferTotalSize + byteCount) <= _limitSize)
            {
                _queue.Enqueue(mm);
                _bufferTotalSize += byteCount;
            }
            else
            {
                int outSize = 0;
                while (true) 
                {
                    outSize = 0;
                    if (_queue.TryDequeue(out QueueMessage outMM))
                    {
                        outSize = outMM.BufferSize;
                    }

                    _bufferTotalSize -= outSize;

                    if (byteCount >= _limitSize)
                    {
                        break;
                    }

                    if ((_bufferTotalSize + byteCount) <= _limitSize)
                    {
                        _queue.Enqueue(mm);
                        _bufferTotalSize += byteCount;
                        break;
                    }

                } 

            }

        }

    }
}
