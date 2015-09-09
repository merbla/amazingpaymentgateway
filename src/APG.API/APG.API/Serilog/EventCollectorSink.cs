using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace APG.API.Serilog
{
    public class EventCollectorSink : ILogEventSink
    {
        private readonly int _batchSizeLimit;
        private readonly JsonFormatter _jsonFormatter;
        private readonly ConcurrentQueue<LogEvent> _queue;

        public EventCollectorSink(string splunkHost,
            string eventCollectorToken,
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
}