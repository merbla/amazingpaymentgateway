using System;
using APG.API.Serilog;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace APG.API
{
    public static class SerilogExtensions
    {
        public const string DefaultOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";

        public static LoggerConfiguration EventCollector(
            this LoggerSinkConfiguration sinkConfiguration,
            string splunkHost,
            string eventCollectorToken,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (outputTemplate == null) throw new ArgumentNullException(nameof(outputTemplate));
            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return sinkConfiguration.Sink(new EventCollectorSink(splunkHost, eventCollectorToken), restrictedToMinimumLevel);
        }
    }
}