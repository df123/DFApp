using Serilog.Configuration;
using Serilog;
using System;
using DFApp.SerilogSink;

namespace DFApp.Web.SerilogSink
{
    public static class QueueSinkExtensions
    {
        public static LoggerConfiguration QueueSink(
                 this LoggerSinkConfiguration loggerConfiguration,
                 IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new QueueSink(formatProvider, QueueSinkData.Queue));
        }
    }
}
