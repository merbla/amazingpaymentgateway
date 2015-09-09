using Newtonsoft.Json;

namespace APG.API.Serilog
{
    internal class Data 
    {
        [JsonProperty("event")]
        public object Event { get; set; }
    }
}