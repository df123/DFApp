using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Text;
using System;
using DFApp.SerilogSink;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.IO;
using DFApp.Web.SignalRHub;

namespace DFApp.Web.SerilogSink
{
    public class QueueSink : ILogEventSink
    {
        private readonly ITextFormatter _formatProvider;
        private readonly ConcurrentQueue<QueueMessage> _queue;
        private int _bufferTotalSize;
        private readonly int _limitSize;

        public QueueSink(ConcurrentQueue<QueueMessage> queue)
        {
            _formatProvider = new MessageTemplateTextFormatter("[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", null);
            _queue = queue;
            _bufferTotalSize = 0;
            _limitSize = 256 * 1024;
        }

        public async void Emit(LogEvent logEvent)
        {

            TextWriter tw = new StringWriter();
            _formatProvider.Format(logEvent, tw);

            string? message = tw.ToString();

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (LogSinkHubService.SinkHub != null)
            {
                await LogSinkHubService.SinkHub.SendLogMessage(message);
            }

            int byteCount = Encoding.UTF8.GetByteCount(message);

            QueueMessage mm = new QueueMessage()
            {
                Msg = message,
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
                    if (_queue.TryDequeue(out QueueMessage? outMM))
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
