using System;
using System.Reflection;
using System.Web;
using Serilog.Core;
using Serilog.Events;

namespace APG.API
{
    public class VersionEnricher : ILogEventEnricher
    {
        LogEventProperty _cachedProperty;

        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string AssemblyVersionPropertyName = "AssemblyVersion";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
             var environmentVariable = this.GetType().Assembly.GetName().Version;
            _cachedProperty = _cachedProperty ?? propertyFactory.CreateProperty(AssemblyVersionPropertyName, environmentVariable);
            logEvent.AddPropertyIfAbsent(_cachedProperty);
        } 
    }
}