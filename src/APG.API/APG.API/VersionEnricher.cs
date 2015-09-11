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
            var environmentVariable = GetApplicationAssembly().GetName().Version;
            _cachedProperty = _cachedProperty ?? propertyFactory.CreateProperty(AssemblyVersionPropertyName, environmentVariable);
            logEvent.AddPropertyIfAbsent(_cachedProperty);
        }

        private const string AspNetNamespace = "ASP";

        private static Assembly GetApplicationAssembly()
        {
           
            Assembly ass = Assembly.GetEntryAssembly();

            HttpContext ctx = HttpContext.Current;
            if (ctx != null)
                ass = GetWebApplicationAssembly(ctx);

            return ass ?? (Assembly.GetExecutingAssembly());
        }

        private static Assembly GetWebApplicationAssembly(HttpContext context)
        {
            
            object app = context.ApplicationInstance;
            if (app == null) return null;

            Type type = app.GetType();
            while (type != null && type != typeof(object) && type.Namespace == AspNetNamespace)
                type = type.BaseType;

            return type.Assembly;
        }
    }
}