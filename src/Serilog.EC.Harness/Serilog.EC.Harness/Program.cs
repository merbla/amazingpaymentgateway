using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Enrichers;

namespace Serilog.EC.Harness
{
    internal class Data
    {
        [JsonProperty("event")]
        public string Event { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With(new MachineNameEnricher())
                .Enrich.With(new ProcessIdEnricher())
                .Enrich.With(new ThreadIdEnricher())
                .WriteTo.LiterateConsole()
                .CreateLogger();

            ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            DoIt().Wait();

            Console.ReadLine();
        }

        private static async Task DoIt()
        {
            var uri = "https://mysplunk:8088/services/collector";

            var d = new Data {Event = Guid.NewGuid().ToString()};

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
                    Log.Information("yeah");
                }
                else
                {
                    Log.Information("boo");
                    Log.Information("{0}", response.Content.ReadAsStringAsync().Result);
                }
            }
        }
    }
}