using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Formatting.Json;
using SerilogWeb.Classic.Enrichers;

namespace APG.API
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

           Log.Logger = new LoggerConfiguration()
                .Enrich.With<HttpRequestIdEnricher>()
                .Enrich.With<MachineNameEnricher>()
                .Enrich.With<ThreadIdEnricher>()
                .WriteTo.EventCollector()
                .CreateLogger();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }


    public class EventCollectorSink : ILogEventSink
    {
        private readonly int _batchSizeLimit;
        private readonly JsonFormatter _jsonFormatter;
        private readonly ConcurrentQueue<LogEvent> _queue;

        public EventCollectorSink(
            IFormatProvider formatProvider = null,
            bool renderTemplate = true
            )
        {
            _queue = new ConcurrentQueue<LogEvent>();

            _jsonFormatter = new JsonFormatter(renderMessage: true, formatProvider: formatProvider);
            _batchSizeLimit = 1;
            var batchInterval = TimeSpan.FromSeconds(5);

            RepeatAction.OnInterval(batchInterval, () => ProcessQueue().Wait(), new CancellationToken());
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            _queue.Enqueue(logEvent);
        }

        private async Task ProcessQueue()
        {
            try
            {
                do
                {
                    var count = 0;
                    var events = new Queue<LogEvent>();
                    LogEvent next;

                    while (count < _batchSizeLimit && _queue.TryDequeue(out next))
                    {
                        count++;
                        events.Enqueue(next);
                    }

                    if (events.Count == 0)
                        return;

                    var sw = new StringWriter();

                    foreach (var logEvent in events)
                    {
                       // _jsonFormatter.Format(logEvent, sw);


                        var uri = "https://mysplunk:8088/services/collector";

                        var d = new Data { Event = logEvent };

                         _jsonFormatter.Format(logEvent, sw);

                        var d2 = sw.ToString();

                        dynamic d3 = JsonConvert.DeserializeObject(d2);
                        d.Event = d3;

                        using (var client = new HttpClient())
                        {
                            var stringContent = new StringContent(JsonConvert.SerializeObject(d), Encoding.UTF8, "application/json");

                            var request = new HttpRequestMessage
                            {
                                RequestUri = new Uri(uri),
                                Content = stringContent
                            };

                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", "DDCB3B16-9EC4-47ED-B2C1-3775DE291DBF");

                            request.Method = HttpMethod.Post;

                            var response = await client.SendAsync(request);

                            if (response.IsSuccessStatusCode)
                            {
                            }
                        }


                    }


                    //All log message data
                    // var message = Encoding.UTF8.GetBytes(sw.ToString());

                    //  await DoIt(sw.ToString());
                } while (true);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Exception while emitting batch from {0}: {1}", this, ex);
            }
        }

        private static async Task DoIt(string data)
        {
            var uri = "https://mysplunk:8088/services/collector";

            var d = new Data {Event = data};

            using (var client = new HttpClient())
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(d), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Content = stringContent
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk",
                    "DDCB3B16-9EC4-47ED-B2C1-3775DE291DBF");

                request.Method = HttpMethod.Post;

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    //  Log.Information("yeah");
                }
            }
        }
    }


    internal class Data 
    {
        [JsonProperty("event")]
        public object Event { get; set; }
    }

    internal static class RepeatAction
    {
        public static Task OnInterval(TimeSpan pollInterval, Action action, CancellationToken token,
            TaskCreationOptions taskCreationOptions, TaskScheduler taskScheduler)
        {
            return Task.Factory.StartNew(() =>
            {
                for (;;)
                {
                    if (token.WaitCancellationRequested(pollInterval))
                        break;
                    action();
                }
            }, token, taskCreationOptions, taskScheduler);
        }

        public static Task OnInterval(TimeSpan pollInterval, Action action, CancellationToken token)
        {
            return OnInterval(pollInterval, action, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static bool WaitCancellationRequested(this CancellationToken token, TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}