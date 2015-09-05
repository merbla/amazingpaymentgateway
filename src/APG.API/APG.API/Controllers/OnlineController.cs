using System;
using System.Web.Http;
using Serilog;

namespace APG.API.Controllers
{
    public class OnlineController : ApiController
    {
        public string Get()
        {
            Log.Information("Yes: {0}", Guid.NewGuid().ToString());
            return "Hello Splunk";
        }
    }

}
