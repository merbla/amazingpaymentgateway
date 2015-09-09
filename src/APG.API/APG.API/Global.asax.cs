using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Serilog;
using Serilog.Enrichers;
using SerilogWeb.Classic.Enrichers;

namespace APG.API
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var splunkHost = ConfigurationManager.AppSettings["Splunk.EventCollector.Host"];
            var eventCollectorToken = ConfigurationManager.AppSettings["Splunk.EventCollector.Token"];

            Log.Logger = new LoggerConfiguration()

                .Enrich.With<HttpRequestIdEnricher>()
                .Enrich.With<MachineNameEnricher>()
                .Enrich.With<ThreadIdEnricher>()

                .WriteTo.EventCollector(splunkHost, eventCollectorToken)
                .CreateLogger();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}