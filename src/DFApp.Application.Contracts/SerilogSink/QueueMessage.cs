using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.SerilogSink
{
    public class QueueMessage
    {
        public QueueMessage()
        {
            Msg = string.Empty;
            BufferSize = 0;
        }
        public string Msg { get; set; }
        public int BufferSize { get; set; }
    }
}
